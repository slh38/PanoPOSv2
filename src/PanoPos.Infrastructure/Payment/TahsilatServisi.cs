using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Outbox;
using PanoPos.Application.Payment;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Outbox;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Payment;

public sealed class TahsilatServisi : ITahsilatServisi
{
    private readonly PanoPosDbContext _dbContext;
    private readonly IOutboxServisi _outboxServisi;

    public TahsilatServisi(PanoPosDbContext dbContext)
        : this(dbContext, new BosOutboxServisi())
    {
    }

    public TahsilatServisi(PanoPosDbContext dbContext, IOutboxServisi outboxServisi)
    {
        _dbContext = dbContext;
        _outboxServisi = outboxServisi;
    }

    public async Task<TahsilatDto> TahsilatOlusturAsync(TahsilatOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Baglam() is { } context)
        {
            request.SubeId = IslemKapsami.Kimlik(request.SubeId, context.SubeId);
            request.KullaniciId = IslemKapsami.Kimlik(request.KullaniciId, context.KullaniciId);
            request.CihazId = IslemKapsami.Kimlik(request.CihazId, context.CihazId);
        }
        ValidateRequest(request);
        request.ParaBirimKodu = request.ParaBirimKodu.Trim().ToUpperInvariant();
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        Guid? tenantId = null;
        try
        {
            var fatura = await FaturaOdemeButunlugu.KilitleAsync(_dbContext, request.FaturaId, cancellationToken);
            tenantId = fatura.TenantId;

            if (fatura.SubeId != request.SubeId)
            {
                throw new UygulamaHatasi(409, "Tahsilat olusturulamadi", "Fatura ile sube bilgisi uyusmuyor.", "payment_branch_mismatch");
            }

            var tekrar = await TekrarGetirAsync(request, fatura.TenantId, cancellationToken);
            if (tekrar != null)
            {
                await transaction.CommitAsync(cancellationToken);
                return tekrar;
            }

            if (fatura.Durum == FaturaDurumu.Iptal || fatura.Durum == FaturaDurumu.Iade)
            {
                throw new UygulamaHatasi(409, "Tahsilat olusturulamadi", "Iptal veya iade faturadan tahsilat alinamaz.", "invoice_not_collectible");
            }

            if (fatura.Durum != FaturaDurumu.Acik)
            {
                throw new UygulamaHatasi(409, "Tahsilat olusturulamadi", "Sadece acik faturadan tahsilat alinabilir.", "invoice_not_open");
            }

            FaturaUyumlulukKontrolu(fatura, request);

            var mevcutToplamTahsilat = await FaturaOdemeButunlugu.ToplamAsync(_dbContext, fatura, cancellationToken);
            var yeniToplamTahsilat = mevcutToplamTahsilat + request.Tutar;
            if (yeniToplamTahsilat > fatura.NetToplam)
            {
                throw new UygulamaHatasi(409, "Tahsilat olusturulamadi", "Tahsilat toplami fatura net toplamini gecemez.", "payment_total_exceeds_invoice");
            }

            var kullaniciVar = await _dbContext.Kullanicilar.TenantKapsami(_dbContext).AnyAsync(x => x.Id == request.KullaniciId && x.TenantId == fatura.TenantId && x.AktifMi, cancellationToken);
            if (!kullaniciVar)
            {
                throw new UygulamaHatasi(404, "Kullanici bulunamadi", "Kullanici bulunamadi.", "kullanici_not_found");
            }

            var cihazVar = await _dbContext.Cihazlar.SubeKapsami(_dbContext).AnyAsync(x => x.Id == request.CihazId && x.SubeId == request.SubeId && x.TenantId == fatura.TenantId && x.AktifMi, cancellationToken);
            if (!cihazVar)
            {
                throw new UygulamaHatasi(404, "Cihaz bulunamadi", "Cihaz bulunamadi.", "cihaz_not_found");
            }

            var tahsilatTarihi = request.TahsilatTarihi ?? DateTime.UtcNow;
            var yerelTutar = HesaplaYerelTutar(request.Tutar, request.Kur);

            var tahsilat = new Tahsilat
            {
                IslemAnahtari = request.IslemAnahtari,
                IstekOzeti = IstekOzeti(request),
                TenantId = fatura.TenantId,
                SubeId = fatura.SubeId,
                FaturaId = fatura.Id,
                TahsilatFisNo = $"TAH-{DateTime.UtcNow:yyyyMMdd}-{request.IslemAnahtari:N}",
                OdemeTipi = request.OdemeTipi,
                ParaBirimKodu = request.ParaBirimKodu.Trim(),
                Kur = request.Kur,
                Tutar = request.Tutar,
                YerelTutar = yerelTutar,
                Aciklama = NormalizeOptional(request.Aciklama),
                TahsilatTarihi = tahsilatTarihi,
                AktifMi = true,
                SilindiMi = false,
                OlusturanKullaniciId = request.KullaniciId,
                GuncelleyenKullaniciId = request.KullaniciId
            };

            _dbContext.Tahsilatlar.Add(tahsilat);
            await _dbContext.SaveChangesAsync(cancellationToken);

            switch (request.OdemeTipi)
            {
                case OdemeTipi.Nakit:
                    await NakitHareketiOlusturAsync(request, tahsilat, cancellationToken);
                    break;
                case OdemeTipi.KrediKarti:
                    await BankaHareketiOlusturAsync(request, tahsilat, cancellationToken);
                    break;
                case OdemeTipi.Veresiye:
                    await CariHareketiOlusturAsync(fatura, request, tahsilat, cancellationToken);
                    break;
                default:
                    throw new UygulamaHatasi(400, "Gecersiz istek", "Desteklenmeyen odeme tipi.", "payment_type_invalid");
            }

            var toplamTahsilat = await FaturaOdemeButunlugu.ToplamAsync(_dbContext, fatura, cancellationToken);
            FaturaOdemeButunlugu.Guncelle(fatura, toplamTahsilat, tahsilatTarihi, request.KullaniciId);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _outboxServisi.OlayEkleAsync(new OutboxOlayEkleRequestDto
            {
                TenantId = tahsilat.TenantId,
                SubeId = tahsilat.SubeId,
                CihazId = request.CihazId,
                OlayTipi = "TahsilatOlusturuldu",
                KaynakTablo = nameof(Tahsilat),
                KaynakId = tahsilat.Id,
                PayloadJson = JsonSerializer.Serialize(new
                {
                    tahsilat.Id,
                    tahsilat.FaturaId,
                    tahsilat.TahsilatFisNo,
                    tahsilat.OdemeTipi,
                    tahsilat.ParaBirimKodu,
                    tahsilat.Kur,
                    tahsilat.Tutar,
                    tahsilat.YerelTutar,
                    fatura.OdenenTutar,
                    fatura.KalanTutar,
                    fatura.Durum
                })
            }, cancellationToken);

            var result = await TahsilatGetirAsync(tahsilat.Id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 } && tenantId.HasValue)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            await transaction.DisposeAsync();
            _dbContext.ChangeTracker.Clear();
            return await TekrarGetirAsync(request, tenantId.Value, cancellationToken)
                ?? throw new UygulamaHatasi(409, "Tahsilat cakismasi", "Islem cakisti. Ayni islem anahtariyla tekrar deneyiniz.", "payment_conflict");
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            _dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task<TahsilatDto> TahsilatGetirAsync(long id, CancellationToken cancellationToken = default)
    {
        var tahsilat = await _dbContext.Tahsilatlar.SubeKapsami(_dbContext)
            .AsNoTracking()
            .Include(x => x.Fatura)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Tahsilat bulunamadi", "Tahsilat bulunamadi.", "payment_not_found");

        return new TahsilatDto
        {
            IslemAnahtari = tahsilat.IslemAnahtari,
            Id = tahsilat.Id,
            FaturaId = tahsilat.FaturaId,
            TahsilatFisNo = tahsilat.TahsilatFisNo,
            OdemeTipi = tahsilat.OdemeTipi,
            ParaBirimKodu = tahsilat.ParaBirimKodu,
            Kur = tahsilat.Kur,
            Tutar = tahsilat.Tutar,
            YerelTutar = tahsilat.YerelTutar,
            FaturaOdenenTutar = tahsilat.Fatura?.OdenenTutar ?? 0m,
            FaturaKalanTutar = tahsilat.Fatura?.KalanTutar ?? 0m,
            FaturaDurumu = tahsilat.Fatura?.Durum ?? FaturaDurumu.Acik,
            Aciklama = tahsilat.Aciklama,
            TahsilatTarihi = tahsilat.TahsilatTarihi,
            AktifMi = tahsilat.AktifMi
        };
    }

    public async Task<SayfaliSonucDto<TahsilatListeItemDto>> TahsilatListeleAsync(long subeId, int page, int pageSize, CancellationToken cancellationToken = default, long? faturaId = null)
    {
        if (subeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        if (page <= 0 || pageSize is < 1 or > 200 || (long)(page - 1) * pageSize > int.MaxValue || faturaId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Page ve pageSize 0'dan buyuk olmalidir.", "pagination_invalid");
        }

        var tenantId = await _dbContext.Subeler.YetkiliSube(_dbContext).Where(x => x.Id == subeId).Select(x => x.TenantId).SingleOrDefaultAsync(cancellationToken);
        if (tenantId == Guid.Empty)
        {
            throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");
        }

        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var countSql = @"SELECT COUNT(1)
FROM Tahsilat
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0 AND AktifMi = 1
  AND (@FaturaId IS NULL OR FaturaId = @FaturaId);";

        var provider = _dbContext.Database.ProviderName ?? string.Empty;
        var listSql = provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)
            ? @"SELECT Id, TahsilatFisNo, FaturaId, OdemeTipi, ParaBirimKodu, Kur, Tutar, YerelTutar, TahsilatTarihi
FROM Tahsilat
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0 AND AktifMi = 1
  AND (@FaturaId IS NULL OR FaturaId = @FaturaId)
ORDER BY Id DESC
LIMIT @Take OFFSET @Skip;"
            : @"SELECT Id, TahsilatFisNo, FaturaId, OdemeTipi, ParaBirimKodu, Kur, Tutar, YerelTutar, TahsilatTarihi
FROM Tahsilat
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0 AND AktifMi = 1
  AND (@FaturaId IS NULL OR FaturaId = @FaturaId)
ORDER BY Id DESC
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

        var parameters = new { TenantId = tenantId, SubeId = subeId, FaturaId = faturaId, Skip = (page - 1) * pageSize, Take = pageSize };
        var toplamKayit = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
        var kayitlar = (await connection.QueryAsync<TahsilatListeItemDto>(new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken))).ToList();

        return new SayfaliSonucDto<TahsilatListeItemDto>
        {
            ToplamKayit = toplamKayit,
            Sayfa = page,
            SayfaBoyutu = pageSize,
            Kayitlar = kayitlar
        };
    }

    private async Task NakitHareketiOlusturAsync(TahsilatOlusturRequestDto request, Tahsilat tahsilat, CancellationToken cancellationToken)
    {
        if (!request.KasaId.HasValue || request.KasaId.Value <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Nakit tahsilatta KasaId zorunludur.", "cash_register_required");
        }

        var kasaVar = await _dbContext.Kasalar.SubeKapsami(_dbContext).AnyAsync(x => x.Id == request.KasaId.Value && x.SubeId == request.SubeId && x.TenantId == tahsilat.TenantId && x.AktifMi, cancellationToken);
        if (!kasaVar)
        {
            throw new UygulamaHatasi(404, "Kasa bulunamadi", "Kasa bulunamadi.", "kasa_not_found");
        }

        _dbContext.KasaHareketleri.Add(new KasaHareket
        {
            TenantId = tahsilat.TenantId,
            SubeId = tahsilat.SubeId,
            KasaId = request.KasaId.Value,
            VardiyaId = null,
            KullaniciId = request.KullaniciId,
            CihazId = request.CihazId,
            IslemTipi = KasaIslemTipi.SatisTahsilat,
            Tutar = tahsilat.YerelTutar,
            Aciklama = tahsilat.Aciklama,
            ReferansTip = nameof(Tahsilat),
            ReferansId = tahsilat.Id,
            Tarih = tahsilat.TahsilatTarihi,
            AktifMi = true,
            SilindiMi = false,
            OlusturanKullaniciId = request.KullaniciId,
            GuncelleyenKullaniciId = request.KullaniciId
        });
    }

    private async Task BankaHareketiOlusturAsync(TahsilatOlusturRequestDto request, Tahsilat tahsilat, CancellationToken cancellationToken)
    {
        if (!request.BankaId.HasValue || request.BankaId.Value <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Kredi karti tahsilatta BankaId zorunludur.", "bank_required");
        }

        var bankaVar = await _dbContext.Bankalar.SubeKapsami(_dbContext).AnyAsync(x => x.Id == request.BankaId.Value && x.SubeId == request.SubeId && x.TenantId == tahsilat.TenantId && x.AktifMi, cancellationToken);
        if (!bankaVar)
        {
            throw new UygulamaHatasi(404, "Banka bulunamadi", "Banka bulunamadi.", "bank_not_found");
        }

        _dbContext.BankaHareketleri.Add(new BankaHareket
        {
            TenantId = tahsilat.TenantId,
            SubeId = tahsilat.SubeId,
            BankaId = request.BankaId.Value,
            FaturaId = tahsilat.FaturaId,
            TahsilatId = tahsilat.Id,
            Tutar = tahsilat.Tutar,
            ParaBirimKodu = tahsilat.ParaBirimKodu,
            Kur = tahsilat.Kur,
            YerelTutar = tahsilat.YerelTutar,
            HareketTarihi = tahsilat.TahsilatTarihi,
            Aciklama = tahsilat.Aciklama,
            AktifMi = true,
            SilindiMi = false,
            OlusturanKullaniciId = request.KullaniciId,
            GuncelleyenKullaniciId = request.KullaniciId
        });
    }

    private async Task CariHareketiOlusturAsync(Fatura fatura, TahsilatOlusturRequestDto request, Tahsilat tahsilat, CancellationToken cancellationToken)
    {
        if (!fatura.CariId.HasValue || fatura.CariId.Value <= 0)
        {
            throw new UygulamaHatasi(409, "Tahsilat olusturulamadi", "Veresiye tahsilatta faturada CariId zorunludur.", "invoice_customer_required");
        }

        var cariVar = await _dbContext.CariKartlar.SubeKapsami(_dbContext).AnyAsync(x => x.Id == fatura.CariId.Value && x.SubeId == request.SubeId && x.TenantId == tahsilat.TenantId && x.AktifMi, cancellationToken);
        if (!cariVar)
        {
            throw new UygulamaHatasi(404, "Cari bulunamadi", "Cari bulunamadi.", "cari_kart_not_found");
        }

        _dbContext.CariHareketleri.Add(new CariHareket
        {
            TenantId = tahsilat.TenantId,
            SubeId = tahsilat.SubeId,
            CariId = fatura.CariId.Value,
            FaturaId = tahsilat.FaturaId,
            TahsilatId = tahsilat.Id,
            HareketTipi = CariHareketTipi.Borc,
            Tutar = tahsilat.Tutar,
            ParaBirimKodu = tahsilat.ParaBirimKodu,
            Kur = tahsilat.Kur,
            YerelTutar = tahsilat.YerelTutar,
            HareketTarihi = tahsilat.TahsilatTarihi,
            Aciklama = tahsilat.Aciklama,
            AktifMi = true,
            SilindiMi = false,
            OlusturanKullaniciId = request.KullaniciId,
            GuncelleyenKullaniciId = request.KullaniciId
        });
    }

    private static void ValidateRequest(TahsilatOlusturRequestDto request)
    {
        if (request.IslemAnahtari == Guid.Empty)
            throw new UygulamaHatasi(400, "Gecersiz istek", "IslemAnahtari zorunludur.", "payment_key_required");
        if (request.SubeId <= 0 || request.FaturaId <= 0 || request.KullaniciId <= 0 || request.CihazId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId, FaturaId, KullaniciId ve CihazId zorunludur.", "payment_required_fields");
        }

        if (string.IsNullOrWhiteSpace(request.ParaBirimKodu) || request.ParaBirimKodu.Trim().Length > 10)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "ParaBirimKodu zorunludur.", "payment_currency_required");
        }

        if (request.Tutar <= 0 || request.Tutar > 9999999999999999.99m || decimal.Round(request.Tutar, 2) != request.Tutar)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Tahsilat tutari 0'dan buyuk olmalidir.", "payment_amount_invalid");
        }

        if (request.Kur <= 0 || request.Kur > 999999999999.999999m || decimal.Round(request.Kur, 6) != request.Kur)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Kur 0'dan buyuk olmalidir.", "payment_rate_invalid");
        }
    }

    private static void FaturaUyumlulukKontrolu(Fatura fatura, TahsilatOlusturRequestDto request)
    {
        if (!string.Equals(fatura.ParaBirimKodu, request.ParaBirimKodu?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new UygulamaHatasi(409, "Tahsilat olusturulamadi", "Tahsilat para birimi fatura ile ayni olmalidir.", "payment_currency_mismatch");
        }

        if (fatura.Kur != request.Kur)
        {
            throw new UygulamaHatasi(409, "Tahsilat olusturulamadi", "Tahsilat kuru fatura ile ayni olmalidir.", "payment_rate_mismatch");
        }
    }

    private async Task<TahsilatDto?> TekrarGetirAsync(TahsilatOlusturRequestDto request, Guid tenant, CancellationToken ct)
    {
        var existing = await _dbContext.Tahsilatlar.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(x => x.TenantId == tenant && x.IslemAnahtari == request.IslemAnahtari, ct);
        if (existing == null) return null;
        if (existing.SubeId != request.SubeId || existing.SilindiMi || !existing.AktifMi || existing.IstekOzeti != IstekOzeti(request))
            throw new UygulamaHatasi(409, "Islem anahtari kullanilmis", "Ayni anahtar farkli veya gecersiz bir odemeye ait.", "payment_key_conflict");
        return await TahsilatGetirAsync(existing.Id, ct);
    }

    private static string IstekOzeti(TahsilatOlusturRequestDto request)
        => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new
        {
            request.SubeId,
            request.FaturaId,
            request.OdemeTipi,
            Tutar = request.Tutar.ToString("G29", System.Globalization.CultureInfo.InvariantCulture),
            request.ParaBirimKodu,
            Kur = request.Kur.ToString("G29", System.Globalization.CultureInfo.InvariantCulture),
            request.KasaId,
            request.BankaId,
            Aciklama = NormalizeOptional(request.Aciklama),
            Tarih = request.TahsilatTarihi?.ToUniversalTime()
        })));

    private static decimal HesaplaYerelTutar(decimal tutar, decimal kur)
    {
        try
        {
            var result = Math.Round(tutar * kur, 2, MidpointRounding.AwayFromZero);
            if (result <= 9999999999999999.99m) return result;
        }
        catch (OverflowException) { }
        throw new UygulamaHatasi(400, "Gecersiz tutar", "Yerel tutar desteklenen siniri asiyor.", "payment_amount_invalid");
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

