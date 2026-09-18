using System.Text.Json;
using Dapper;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Invoice;
using PanoPos.Application.Outbox;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Outbox;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Payment;

namespace PanoPos.Infrastructure.Invoice;

public sealed partial class FaturaServisi : IFaturaServisi
{
    private readonly PanoPosDbContext _dbContext;
    private readonly IOutboxServisi _outboxServisi;
    private readonly PanoPos.Application.Stock.IStokMaliyetServisi _maliyet;

    public FaturaServisi(PanoPosDbContext dbContext)
        : this(dbContext, new BosOutboxServisi())
    {
    }

    public FaturaServisi(PanoPosDbContext dbContext, IOutboxServisi outboxServisi)
        : this(dbContext, outboxServisi, new PanoPos.Infrastructure.Stock.StokMaliyetServisi(dbContext))
    {
    }

    public FaturaServisi(PanoPosDbContext dbContext, IOutboxServisi outboxServisi,
        PanoPos.Application.Stock.IStokMaliyetServisi maliyet)
    {
        _dbContext = dbContext;
        _outboxServisi = outboxServisi;
        _maliyet = maliyet;
    }

    public async Task<FaturaDto> SiparistenFaturaOlusturAsync(SiparistenFaturaOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.SiparisId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SiparisId zorunludur.", "siparis_required");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        await LockOrderAsync(request.SiparisId, cancellationToken);
        // Keep invalid masters visible for validation; a deleted product must not silently remove an order line.
        var siparis = await _dbContext.Siparisler.SubeKapsami(_dbContext).IgnoreQueryFilters().AsNoTracking()
            .Include(x => x.Detaylar.Where(y => y.AktifMi && !y.SilindiMi))
                .ThenInclude(x => x.StokKart)
            .Include(x => x.Detaylar.Where(y => y.AktifMi && !y.SilindiMi))
                .ThenInclude(x => x.StokKartVaryant)
            .SingleOrDefaultAsync(x => x.Id == request.SiparisId && !x.SilindiMi, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Siparis bulunamadi", "Siparis bulunamadi.", "siparis_not_found");

        if (siparis.Durum != SiparisDurumu.Bekliyor)
        {
            throw new UygulamaHatasi(409, "Fatura olusturulamadi", "Sadece bekleyen siparisten fatura olusturulabilir.", "siparis_not_invoiceable");
        }

        if (siparis.Detaylar.Count == 0)
        {
            throw new UygulamaHatasi(409, "Fatura olusturulamadi", "Detaysiz siparisten fatura olusturulamaz.", "siparis_has_no_lines");
        }

        FaturaKaynakSiparisKontrolu(siparis);

        var mevcutFaturaVar = await _dbContext.Faturalar.SubeKapsami(_dbContext).AnyAsync(x => x.SiparisId == siparis.Id && x.Durum != FaturaDurumu.Iptal, cancellationToken);
        if (mevcutFaturaVar)
        {
            throw new UygulamaHatasi(409, "Fatura olusturulamadi", "Bu siparisten zaten fatura olusturulmus.", "invoice_already_exists");
        }

        var depoId = await ResolveDepoAsync(siparis, request.DepoId, cancellationToken);

        var fatura = new Fatura
        {
            TenantId = siparis.TenantId,
            SubeId = siparis.SubeId,
            DepoId = depoId,
            CihazId = _dbContext.Baglam()?.CihazId,
            FaturaNo = await FaturaNoUretAsync(siparis.TenantId, cancellationToken),
            SiparisId = siparis.Id,
            CariId = siparis.CariId,
            Aciklama = NormalizeOptional(request.Aciklama) ?? siparis.Aciklama,
            ParaBirimKodu = siparis.ParaBirimKodu,
            Kur = siparis.Kur,
            AraToplam = siparis.AraToplam,
            KdvDahilMi = siparis.KdvDahilMi, ToplamMatrah = siparis.ToplamMatrah, ToplamKdv = siparis.ToplamKdv,
            GenelIndirimOrani = siparis.GenelIndirimOrani,
            GenelIndirimTutari = siparis.GenelIndirimTutari,
            NetToplam = siparis.NetToplam,
            OdenenTutar = 0m,
            KalanTutar = siparis.NetToplam,
            ToplamTutar = siparis.ToplamTutar,
            Durum = FaturaDurumu.Acik,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Faturalar.Add(fatura);
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var detay in siparis.Detaylar)
        {
            _dbContext.FaturaDetaylari.Add(new FaturaDetay
            {
                TenantId = detay.TenantId,
                SubeId = detay.SubeId,
                FaturaId = fatura.Id,
                StokKartId = detay.StokKartId,
                StokKartAd = detay.StokKart.TenantId == siparis.TenantId ? detay.StokKart.Ad
                    : throw new UygulamaHatasi(409, "Gecersiz stok", "Stok karti kapsam disinda.", "stock_scope_mismatch"),
                VaryantKodu = detay.StokKartVaryant?.TenantId == siparis.TenantId ? detay.StokKartVaryant.VaryantKodu : null,
                KdvId = detay.KdvId, KdvOrani = detay.KdvOrani, KdvDahilMi = detay.KdvDahilMi,
                Matrah = detay.Matrah, KdvTutari = detay.KdvTutari, GenelIndirimPayi = detay.GenelIndirimPayi,
                StokKartSatisBirimiId = detay.StokKartSatisBirimiId, BirimKodu = detay.BirimKodu, BirimAdi = detay.BirimAdi, BirimKatsayi = detay.BirimKatsayi,
                FiyatParaBirimKodu = detay.FiyatParaBirimKodu, FiyatKur = detay.FiyatKur,
                StokKartVaryantId = detay.StokKartVaryantId,
                Miktar = detay.Miktar,
                BirimFiyat = detay.BirimFiyat,
                FiyatTipiId = detay.FiyatTipiId, FiyatTipiAdi = detay.FiyatTipiAdi,
                SatirAraToplam = detay.SatirAraToplam,
                IndirimOrani = detay.IndirimOrani,
                IndirimTutari = detay.IndirimTutari,
                SatirNetToplam = detay.SatirNetToplam,
                SatirToplam = detay.SatirToplam,
                Aciklama = detay.Aciklama,
                AktifMi = true,
                SilindiMi = false
            });
        }

        await _maliyet.SatisSnapshotAsync(fatura, fatura.Detaylar.ToList(), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await CreateSalesStockAsync(fatura, cancellationToken);

        var trackedOrder = await _dbContext.Siparisler.SubeKapsami(_dbContext).SingleAsync(x => x.Id == siparis.Id, cancellationToken);
        await _dbContext.Entry(trackedOrder).ReloadAsync(cancellationToken);
        trackedOrder.Durum = SiparisDurumu.Tamamlandi;
        trackedOrder.AktifMi = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _outboxServisi.OlayEkleAsync(new OutboxOlayEkleRequestDto
        {
            TenantId = fatura.TenantId,
            SubeId = fatura.SubeId,
            CihazId = _dbContext.Baglam()?.CihazId ?? await _dbContext.Cihazlar
                .Where(x => x.TenantId == fatura.TenantId && x.SubeId == fatura.SubeId && x.AktifMi)
                .OrderBy(x => x.Id).Select(x => x.Id).FirstAsync(cancellationToken),
            OlayTipi = "FaturaSiparistenOlusturuldu",
            KaynakTablo = nameof(Fatura),
            KaynakId = fatura.Id,
            PayloadJson = JsonSerializer.Serialize(new
            {
                fatura.Id,
                fatura.FaturaNo,
                fatura.SiparisId,
                fatura.CariId,
                fatura.ParaBirimKodu,
                fatura.Kur,
                fatura.NetToplam
            })
        }, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return await FaturaGetirAsync(fatura.Id, cancellationToken);
    }

    public async Task<FaturaDto> FaturaGetirAsync(long id, CancellationToken cancellationToken = default)
    {
        // Reuse an outer write transaction; otherwise hold the invoice lock until the read is complete.
        await using var transaction = _dbContext.Database.CurrentTransaction == null
            ? await _dbContext.Database.BeginTransactionAsync(cancellationToken) : null;
        var fatura = await FaturaOdemeButunlugu.KilitleAsync(_dbContext, id, cancellationToken);
        var names = await _dbContext.Faturalar.AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == fatura.TenantId && x.SubeId == fatura.SubeId)
            .Select(x => new
            {
                TenantAdi = _dbContext.Set<Tenant>().IgnoreQueryFilters().Where(t => t.TenantId == x.TenantId).Select(t => t.Ad).FirstOrDefault(),
                SubeAdi = _dbContext.Subeler.IgnoreQueryFilters().Where(s => s.Id == x.SubeId && s.TenantId == x.TenantId).Select(s => s.Ad).FirstOrDefault(),
                CariKodu = _dbContext.CariKartlar.IgnoreQueryFilters().Where(c => c.Id == x.CariId && c.TenantId == x.TenantId && c.SubeId == x.SubeId).Select(c => c.CariKodu).FirstOrDefault(),
                CariAdi = _dbContext.CariKartlar.IgnoreQueryFilters().Where(c => c.Id == x.CariId && c.TenantId == x.TenantId && c.SubeId == x.SubeId).Select(c => c.Ad).FirstOrDefault(),
                KasiyerAdi = _dbContext.Kullanicilar.IgnoreQueryFilters().Where(k => k.Id == x.OlusturanKullaniciId && k.TenantId == x.TenantId).Select(k => k.Ad + " " + k.Soyad).FirstOrDefault(),
                CihazAdi = _dbContext.Cihazlar.IgnoreQueryFilters().Where(c => c.Id == x.CihazId && c.TenantId == x.TenantId && c.SubeId == x.SubeId).Select(c => c.Ad).FirstOrDefault(),
                DepoAdi = _dbContext.Depolar.IgnoreQueryFilters().Where(d => d.Id == x.DepoId && d.TenantId == x.TenantId && d.SubeId == x.SubeId).Select(d => d.Ad).FirstOrDefault()
            }).SingleAsync(cancellationToken);
        var details = await _dbContext.FaturaDetaylari.AsNoTracking()
            .Where(x => x.FaturaId == id && x.TenantId == fatura.TenantId && x.SubeId == fatura.SubeId && x.AktifMi)
            .OrderBy(x => x.Id).ToListAsync(cancellationToken);
        var payments = await FaturaOdemeleriAsync(fatura, cancellationToken);
        var result = new FaturaDto
        {
            Id = fatura.Id,
            FaturaTarihi = fatura.OlusturmaTarihi,
            TenantId = fatura.TenantId, TenantAdi = names.TenantAdi,
            SubeId = fatura.SubeId, SubeAdi = names.SubeAdi,
            CariKodu = names.CariKodu, CariAdi = names.CariAdi,
            KasiyerId = fatura.OlusturanKullaniciId, KasiyerAdi = names.KasiyerAdi,
            CihazId = fatura.CihazId, CihazAdi = names.CihazAdi, DepoAdi = names.DepoAdi,
            Odemeler = payments,
            NakitToplam = payments.Where(p => p.OdemeTipi == OdemeTipi.Nakit).Sum(p => p.Tutar),
            KartToplam = payments.Where(p => p.OdemeTipi == OdemeTipi.KrediKarti).Sum(p => p.Tutar),
            VeresiyeToplam = payments.Where(p => p.OdemeTipi == OdemeTipi.Veresiye).Sum(p => p.Tutar),
            DepoId = fatura.DepoId,
            FaturaNo = fatura.FaturaNo,
            SiparisId = fatura.SiparisId,
            CariId = fatura.CariId,
            Aciklama = fatura.Aciklama,
            ParaBirimKodu = fatura.ParaBirimKodu,
            Kur = fatura.Kur,
            AraToplam = fatura.AraToplam,
            ToplamMatrah = fatura.ToplamMatrah, ToplamKdv = fatura.ToplamKdv,
            GenelIndirimOrani = fatura.GenelIndirimOrani,
            GenelIndirimTutari = fatura.GenelIndirimTutari,
            NetToplam = fatura.NetToplam,
            OdenenTutar = fatura.OdenenTutar,
            KalanTutar = fatura.KalanTutar,
            ToplamTutar = fatura.ToplamTutar,
            Durum = fatura.Durum,
            KapanisTarihi = fatura.KapanisTarihi,
            KapatanKullaniciId = fatura.KapatanKullaniciId,
            AktifMi = fatura.AktifMi,
            Detaylar = details.Select(x => new FaturaDetayDto
            {
                BirimMaliyet = x.BirimMaliyet, MaliyetYontemi = x.MaliyetYontemi,
                Id = x.Id,
                KdvId = x.KdvId, KdvOrani = x.KdvOrani, KdvDahilMi = x.KdvDahilMi,
                Matrah = x.Matrah, KdvTutari = x.KdvTutari, GenelIndirimPayi = x.GenelIndirimPayi,
                StokKartId = x.StokKartId,
                StokKartAd = x.StokKartAd ?? string.Empty,
                StokKartVaryantId = x.StokKartVaryantId,
                VaryantKodu = x.VaryantKodu,
                Miktar = x.Miktar,
                BirimFiyat = x.BirimFiyat,
                FiyatTipiId = x.FiyatTipiId, FiyatTipiAdi = x.FiyatTipiAdi,
                StokKartSatisBirimiId = x.StokKartSatisBirimiId, BirimKodu = x.BirimKodu,
                BirimAdi = x.BirimAdi, BirimKatsayi = x.BirimKatsayi,
                FiyatParaBirimKodu = x.FiyatParaBirimKodu, FiyatKur = x.FiyatKur,
                SatirAraToplam = x.SatirAraToplam,
                IndirimOrani = x.IndirimOrani,
                IndirimTutari = x.IndirimTutari,
                SatirNetToplam = x.SatirNetToplam,
                SatirToplam = x.SatirToplam,
                Aciklama = x.Aciklama
            }).ToList()
        };
        if (transaction != null) await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task<SayfaliSonucDto<FaturaListeItemDto>> FaturaListeleAsync(long subeId, int? durum, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (subeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        if (page <= 0 || pageSize <= 0)
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
FROM Fatura
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@Durum IS NULL OR Durum = @Durum);";

        var provider = _dbContext.Database.ProviderName ?? string.Empty;
        var listSql = provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)
            ? @"SELECT Id, DepoId, FaturaNo, SiparisId, ParaBirimKodu, Kur, AraToplam, GenelIndirimTutari, NetToplam, OdenenTutar, KalanTutar, ToplamTutar, Durum, KapanisTarihi
FROM Fatura
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@Durum IS NULL OR Durum = @Durum)
ORDER BY Id DESC
LIMIT @Take OFFSET @Skip;"
            : @"SELECT Id, DepoId, FaturaNo, SiparisId, ParaBirimKodu, Kur, AraToplam, GenelIndirimTutari, NetToplam, OdenenTutar, KalanTutar, ToplamTutar, Durum, KapanisTarihi
FROM Fatura
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@Durum IS NULL OR Durum = @Durum)
ORDER BY Id DESC
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

        var parameters = new { TenantId = tenantId, SubeId = subeId, Durum = durum, Skip = (page - 1) * pageSize, Take = pageSize };
        var toplamKayit = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
        var kayitlar = (await connection.QueryAsync<FaturaListeItemDto>(new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken))).ToList();

        return new SayfaliSonucDto<FaturaListeItemDto>
        {
            ToplamKayit = toplamKayit,
            Sayfa = page,
            SayfaBoyutu = pageSize,
            Kayitlar = kayitlar
        };
    }

    public async Task<FaturaDto> FaturaKapatAsync(long id, FaturaKapatRequestDto request, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Baglam() is { } context)
            request.KapatanKullaniciId = IslemKapsami.Kimlik(request.KapatanKullaniciId, context.KullaniciId);
        if (request.KapatanKullaniciId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "KapatanKullaniciId zorunludur.", "closing_user_required");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var fatura = await FaturaOdemeButunlugu.KilitleAsync(_dbContext, id, cancellationToken);
        if (fatura.Durum is FaturaDurumu.Iptal or FaturaDurumu.Iade)
        {
            throw new UygulamaHatasi(409, "Fatura kapatilamadi", "Sadece acik fatura kapatilabilir.", "invoice_not_open");
        }

        var toplam = await FaturaOdemeButunlugu.ToplamAsync(_dbContext, fatura, cancellationToken);
        if (toplam < fatura.NetToplam)
            throw new UygulamaHatasi(409, "Fatura kapatilamadi", "Fatura icin yeterli gercek tahsilat bulunmuyor.", "invoice_not_fully_paid");
        FaturaOdemeButunlugu.Guncelle(fatura, toplam, DateTime.UtcNow, request.KapatanKullaniciId);

        await _dbContext.SaveChangesAsync(cancellationToken);
        var result = await FaturaGetirAsync(id, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task<FaturaDto> FaturaIptalAsync(long id, FaturaIptalRequestDto? request = null, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.StokFisleri.SubeKapsami(_dbContext).IgnoreQueryFilters().AnyAsync(x => x.FaturaId == id, cancellationToken))
            throw new UygulamaHatasi(409, "Fatura iptal edilemedi", "Stok cikisi bulunan fatura icin ters stok hareketi gereklidir.", "sales_stock_reversal_required");
        var fatura = await _dbContext.Faturalar.SubeKapsami(_dbContext).SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Fatura bulunamadi", "Fatura bulunamadi.", "invoice_not_found");

        if (fatura.Durum == FaturaDurumu.Iptal)
        {
            return await FaturaGetirAsync(id, cancellationToken);
        }

        if (fatura.Durum == FaturaDurumu.Iade)
        {
            throw new UygulamaHatasi(409, "Fatura iptal edilemedi", "Iade durumundaki fatura iptal edilemez.", "invoice_refund_state");
        }

        fatura.Durum = FaturaDurumu.Iptal;
        fatura.AktifMi = false;
        if (!string.IsNullOrWhiteSpace(request?.Aciklama))
        {
            fatura.Aciklama = request!.Aciklama!.Trim();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await FaturaGetirAsync(id, cancellationToken);
    }

    private async Task<string> FaturaNoUretAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var bugun = DateTime.UtcNow.ToString("yyyyMMdd");
        var oncekiler = await _dbContext.Faturalar.TenantKapsami(_dbContext)
            .Where(x => x.TenantId == tenantId && x.FaturaNo.StartsWith($"FTR-{bugun}-"))
            .Select(x => x.FaturaNo)
            .ToListAsync(cancellationToken);

        var sonraki = oncekiler
            .Select(x => x.Split('-').LastOrDefault())
            .Select(x => int.TryParse(x, out var sayi) ? sayi : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"FTR-{bugun}-{sonraki:000000}";
    }

    private static void FaturaKaynakSiparisKontrolu(Siparis siparis)
    {
        if (string.IsNullOrWhiteSpace(siparis.ParaBirimKodu))
        {
            throw new UygulamaHatasi(409, "Fatura olusturulamadi", "Siparis para birimi bos olamaz.", "invoice_currency_required");
        }

        if (siparis.Kur <= 0)
        {
            throw new UygulamaHatasi(409, "Fatura olusturulamadi", "Siparis kuru 0'dan buyuk olmalidir.", "invoice_currency_rate_invalid");
        }

        if (siparis.NetToplam < 0)
        {
            throw new UygulamaHatasi(409, "Fatura olusturulamadi", "Siparis net toplam negatif olamaz.", "invoice_net_total_negative");
        }
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
