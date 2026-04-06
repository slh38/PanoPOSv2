using System.Text.Json;
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
        ValidateRequest(request);

        var fatura = await _dbContext.Faturalar.SingleOrDefaultAsync(x => x.Id == request.FaturaId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Fatura bulunamadi", "Fatura bulunamadi.", "invoice_not_found");

        if (fatura.SubeId != request.SubeId)
        {
            throw new UygulamaHatasi(409, "Tahsilat olusturulamadi", "Fatura ile sube bilgisi uyusmuyor.", "payment_branch_mismatch");
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

        var mevcutToplamTahsilat = await FaturaTahsilatToplaminiHesaplaAsync(request.FaturaId, cancellationToken);
        var yeniToplamTahsilat = mevcutToplamTahsilat + request.Tutar;
        if (yeniToplamTahsilat > fatura.NetToplam)
        {
            throw new UygulamaHatasi(409, "Tahsilat olusturulamadi", "Tahsilat toplami fatura net toplamini gecemez.", "payment_total_exceeds_invoice");
        }

        var kullaniciVar = await _dbContext.Kullanicilar.AnyAsync(x => x.Id == request.KullaniciId, cancellationToken);
        if (!kullaniciVar)
        {
            throw new UygulamaHatasi(404, "Kullanici bulunamadi", "Kullanici bulunamadi.", "kullanici_not_found");
        }

        var cihazVar = await _dbContext.Cihazlar.AnyAsync(x => x.Id == request.CihazId && x.SubeId == request.SubeId, cancellationToken);
        if (!cihazVar)
        {
            throw new UygulamaHatasi(404, "Cihaz bulunamadi", "Cihaz bulunamadi.", "cihaz_not_found");
        }

        var tahsilatTarihi = request.TahsilatTarihi ?? DateTime.UtcNow;
        var yerelTutar = HesaplaYerelTutar(request.Tutar, request.Kur);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var tahsilat = new Tahsilat
        {
            TenantId = fatura.TenantId,
            SubeId = fatura.SubeId,
            FaturaId = fatura.Id,
            TahsilatFisNo = await TahsilatFisNoUretAsync(fatura.TenantId, cancellationToken),
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

        var toplamTahsilat = await FaturaTahsilatToplaminiHesaplaAsync(fatura.Id, cancellationToken);
        FaturaDurumunuGuncelle(fatura, toplamTahsilat, tahsilatTarihi, request.KullaniciId);

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

        await transaction.CommitAsync(cancellationToken);

        return await TahsilatGetirAsync(tahsilat.Id, cancellationToken);
    }

    public async Task<TahsilatDto> TahsilatGetirAsync(long id, CancellationToken cancellationToken = default)
    {
        var tahsilat = await _dbContext.Tahsilatlar
            .AsNoTracking()
            .Include(x => x.Fatura)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Tahsilat bulunamadi", "Tahsilat bulunamadi.", "payment_not_found");

        return new TahsilatDto
        {
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

    public async Task<SayfaliSonucDto<TahsilatListeItemDto>> TahsilatListeleAsync(long subeId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (subeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        if (page <= 0 || pageSize <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Page ve pageSize 0'dan buyuk olmalidir.", "pagination_invalid");
        }

        var tenantId = await _dbContext.Subeler.Where(x => x.Id == subeId).Select(x => x.TenantId).SingleOrDefaultAsync(cancellationToken);
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
  AND SilindiMi = 0;";

        var provider = _dbContext.Database.ProviderName ?? string.Empty;
        var listSql = provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)
            ? @"SELECT Id, TahsilatFisNo, FaturaId, OdemeTipi, ParaBirimKodu, Kur, Tutar, YerelTutar, TahsilatTarihi
FROM Tahsilat
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
ORDER BY Id DESC
LIMIT @Take OFFSET @Skip;"
            : @"SELECT Id, TahsilatFisNo, FaturaId, OdemeTipi, ParaBirimKodu, Kur, Tutar, YerelTutar, TahsilatTarihi
FROM Tahsilat
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
ORDER BY Id DESC
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

        var parameters = new { TenantId = tenantId, SubeId = subeId, Skip = (page - 1) * pageSize, Take = pageSize };
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

        var kasaVar = await _dbContext.Kasalar.AnyAsync(x => x.Id == request.KasaId.Value && x.SubeId == request.SubeId, cancellationToken);
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

        var bankaVar = await _dbContext.Bankalar.AnyAsync(x => x.Id == request.BankaId.Value && x.SubeId == request.SubeId, cancellationToken);
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

        var cariVar = await _dbContext.Cariler.AnyAsync(x => x.Id == fatura.CariId.Value && x.SubeId == request.SubeId, cancellationToken);
        if (!cariVar)
        {
            throw new UygulamaHatasi(404, "Cari bulunamadi", "Cari bulunamadi.", "cari_not_found");
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
        if (request.SubeId <= 0 || request.FaturaId <= 0 || request.KullaniciId <= 0 || request.CihazId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId, FaturaId, KullaniciId ve CihazId zorunludur.", "payment_required_fields");
        }

        if (string.IsNullOrWhiteSpace(request.ParaBirimKodu))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "ParaBirimKodu zorunludur.", "payment_currency_required");
        }

        if (request.Tutar <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Tahsilat tutari 0'dan buyuk olmalidir.", "payment_amount_invalid");
        }

        if (request.Kur <= 0)
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

    private async Task<decimal> FaturaTahsilatToplaminiHesaplaAsync(long faturaId, CancellationToken cancellationToken)
    {
        var tahsilatlar = await _dbContext.Tahsilatlar
            .Where(x => x.FaturaId == faturaId && x.AktifMi)
            .Select(x => x.Tutar)
            .ToListAsync(cancellationToken);

        return tahsilatlar.Sum();
    }

    private static void FaturaDurumunuGuncelle(Fatura fatura, decimal toplamTahsilat, DateTime kapanisTarihi, long kullaniciId)
    {
        toplamTahsilat = Math.Round(toplamTahsilat, 2, MidpointRounding.AwayFromZero);
        var kalanTutar = Math.Round(fatura.NetToplam - toplamTahsilat, 2, MidpointRounding.AwayFromZero);
        if (kalanTutar < 0)
        {
            throw new UygulamaHatasi(409, "Tahsilat olusturulamadi", "Tahsilat toplami fatura net toplamini gecemez.", "payment_total_exceeds_invoice");
        }

        fatura.OdenenTutar = toplamTahsilat;
        fatura.KalanTutar = kalanTutar;

        if (kalanTutar == 0)
        {
            fatura.Durum = FaturaDurumu.Kapali;
            fatura.KapanisTarihi = kapanisTarihi;
            fatura.KapatanKullaniciId = kullaniciId;
            fatura.AktifMi = false;
            return;
        }

        fatura.Durum = FaturaDurumu.Acik;
        fatura.KapanisTarihi = null;
        fatura.KapatanKullaniciId = null;
        fatura.AktifMi = true;
    }

    private async Task<string> TahsilatFisNoUretAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var bugun = DateTime.UtcNow.ToString("yyyyMMdd");
        var oncekiler = await _dbContext.Tahsilatlar
            .Where(x => x.TenantId == tenantId && x.TahsilatFisNo.StartsWith($"TAH-{bugun}-"))
            .Select(x => x.TahsilatFisNo)
            .ToListAsync(cancellationToken);

        var sonraki = oncekiler
            .Select(x => x.Split('-').LastOrDefault())
            .Select(x => int.TryParse(x, out var sayi) ? sayi : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"TAH-{bugun}-{sonraki:000000}";
    }

    private static decimal HesaplaYerelTutar(decimal tutar, decimal kur)
        => Math.Round(tutar * kur, 2, MidpointRounding.AwayFromZero);

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

