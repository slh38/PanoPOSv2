using PanoPos.Application.Tax;
using Dapper;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Order;
using PanoPos.Application.Outbox;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Outbox;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Order;

public sealed class SiparisServisi : ISiparisServisi
{
    private readonly PanoPosDbContext _dbContext;
    private readonly IOutboxServisi _outboxServisi;
    private readonly IVergiHesaplamaServisi _vergi;

    public SiparisServisi(PanoPosDbContext dbContext)
        : this(dbContext, new BosOutboxServisi())
    {
    }

    public SiparisServisi(PanoPosDbContext dbContext, IOutboxServisi outboxServisi)
        : this(dbContext, outboxServisi, new VergiHesaplamaServisi())
    {
    }

    public SiparisServisi(PanoPosDbContext dbContext, IOutboxServisi outboxServisi, IVergiHesaplamaServisi vergi)
    {
        _dbContext = dbContext;
        _outboxServisi = outboxServisi;
        _vergi = vergi;
    }

    public async Task<SiparisDto> SiparisOlusturAsync(SiparisOlusturRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.SubeId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "SubeId zorunludur.", "sube_required");
        }

        if (string.IsNullOrWhiteSpace(request.ParaBirimKodu))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "ParaBirimKodu bos olamaz.", "currency_required");
        }

        if (request.Kur <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Kur 0'dan buyuk olmalidir.", "currency_rate_invalid");
        }

        SiparisGenelIndirimKontrolu(request.GenelIndirimOrani, request.GenelIndirimTutari);

        var sube = await _dbContext.Subeler.SingleOrDefaultAsync(x => x.Id == request.SubeId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Sube bulunamadi", "Sube bulunamadi.", "sube_not_found");

        if (request.SiparisTipi == SiparisTipi.Masa && !request.AdisyonId.HasValue)
        {
            throw new UygulamaHatasi(400, "Gecersiz siparis", "Masa siparisinde AdisyonId zorunludur.", "adisyon_required_for_table_order");
        }

        if (request.AdisyonId.HasValue)
        {
            var adisyonVar = await _dbContext.Adisyonlar.AnyAsync(x => x.Id == request.AdisyonId.Value && x.TenantId == sube.TenantId && x.SubeId == sube.Id && x.Durum == AdisyonDurumu.Acik, cancellationToken);
            if (!adisyonVar)
            {
                throw new UygulamaHatasi(404, "Adisyon bulunamadi", "Acik adisyon bulunamadi.", "open_adisyon_not_found");
            }
        }

        if (request.CariId.HasValue)
        {
            var cariVar = await _dbContext.Cariler.AnyAsync(x => x.Id == request.CariId.Value && x.TenantId == sube.TenantId && x.AktifMi && x.SubeId == request.SubeId, cancellationToken);
            if (!cariVar)
            {
                throw new UygulamaHatasi(404, "Cari bulunamadi", "Cari bulunamadi.", "cari_not_found");
            }
        }

        var dahil = await _dbContext.TenantAyarlari.Where(x => x.TenantId == sube.TenantId).Select(x => (bool?)x.SatisFiyatlariKdvDahilMi).SingleOrDefaultAsync(cancellationToken) ?? true;
        var siparis = new Siparis
        {
            KdvDahilMi = dahil,
            TenantId = sube.TenantId,
            SubeId = sube.Id,
            SiparisNo = await SiparisNoUretAsync(sube.TenantId, cancellationToken),
            SiparisTipi = request.SiparisTipi,
            AdisyonId = request.AdisyonId,
            CariId = request.CariId,
            Aciklama = NormalizeOptional(request.Aciklama),
            ParaBirimKodu = request.ParaBirimKodu.Trim().ToUpperInvariant(),
            Kur = request.ParaBirimKodu.Trim().ToUpperInvariant() == "TRY" ? 1m : request.Kur,
            AraToplam = 0,
            GenelIndirimOrani = request.GenelIndirimOrani,
            GenelIndirimTutari = request.GenelIndirimTutari ?? 0,
            NetToplam = 0,
            ToplamTutar = 0,
            Durum = SiparisDurumu.Bekliyor,
            AktifMi = true,
            SilindiMi = false
        };

        SiparisToplamlariniHesapla(siparis, []);

        _dbContext.Siparisler.Add(siparis);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await SiparisGetirAsync(siparis.Id, cancellationToken);
    }

    public async Task<SiparisDto> SiparisSatirEkleAsync(long id, SiparisSatirEkleRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.StokKartId <= 0 || request.Miktar <= 0 || request.BirimFiyat < 0 ||
            decimal.Round(request.BirimFiyat, 4) != request.BirimFiyat || decimal.Round(request.Miktar, 3) != request.Miktar)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "StokKartId, Miktar ve BirimFiyat gecersiz.", "siparis_line_invalid");
        }

        SiparisSatirIndirimKontrolu(request.IndirimOrani, request.IndirimTutari);

        var siparis = await _dbContext.Siparisler.Include(x => x.Detaylar).SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Siparis bulunamadi", "Siparis bulunamadi.", "siparis_not_found");

        if (siparis.Durum != SiparisDurumu.Bekliyor)
        {
            throw new UygulamaHatasi(409, "Siparis guncellenemedi", "Sadece bekleyen siparise satir eklenebilir.", "siparis_not_editable");
        }

        var stokKart = await _dbContext.StokKartler.SingleOrDefaultAsync(x => x.Id == request.StokKartId && x.TenantId == siparis.TenantId && x.AktifMi, cancellationToken)
            ?? throw new UygulamaHatasi(404, "StokKart bulunamadi", "StokKart bulunamadi.", "stok_kart_not_found");

        if (request.StokKartVaryantId.HasValue)
        {
            var varyantVar = await _dbContext.StokKartVaryantlari.AnyAsync(x => x.Id == request.StokKartVaryantId.Value && x.StokKartId == request.StokKartId && x.TenantId == siparis.TenantId && x.AktifMi, cancellationToken);
            if (!varyantVar)
            {
                throw new UygulamaHatasi(404, "Varyant bulunamadi", "Varyant bulunamadi.", "variant_not_found");
            }
        }

        var kdv = await _dbContext.Kdvler.SingleOrDefaultAsync(x => x.Id == stokKart.KdvId && x.TenantId == siparis.TenantId && x.AktifMi, cancellationToken)
            ?? throw new UygulamaHatasi(400, "Gecersiz KDV", "Stok kartinin aktif KDV kaydi bulunamadi.", "kdv_invalid");
        StokKartSatisBirimi? birim = null;
        if (request.StokKartSatisBirimiId.HasValue)
            birim = await _dbContext.StokKartSatisBirimleri.SingleOrDefaultAsync(x => x.Id == request.StokKartSatisBirimiId &&
                x.StokKartId == stokKart.Id && x.TenantId == siparis.TenantId && x.AktifMi, cancellationToken)
                ?? throw new UygulamaHatasi(400, "Gecersiz birim", "Satis birimi bulunamadi.", "sales_unit_invalid");
        else
        {
            var varsayilanlar = await _dbContext.StokKartSatisBirimleri.Where(x => x.StokKartId == stokKart.Id &&
                x.TenantId == siparis.TenantId && x.AktifMi && x.VarsayilanMi).Take(2).ToListAsync(cancellationToken);
            if (varsayilanlar.Count > 1)
                throw new UygulamaHatasi(400, "Gecersiz birim", "Birden fazla varsayilan satis birimi var.", "sales_unit_invalid");
            birim = varsayilanlar.SingleOrDefault();
        }
        var fiyatParaBirimi = (request.FiyatParaBirimKodu ?? siparis.ParaBirimKodu).Trim().ToUpperInvariant();
        var fiyatKur = fiyatParaBirimi == "TRY" ? 1m : request.FiyatKur ?? siparis.Kur;
        if (fiyatParaBirimi.Length is < 1 or > 10 || fiyatKur <= 0)
            throw new UygulamaHatasi(400, "Gecersiz kur", "Fiyat para birimi veya kuru gecersiz.", "price_currency_invalid");
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var detay = new SiparisDetay
        {
            TenantId = siparis.TenantId,
            SubeId = siparis.SubeId,
            SiparisId = siparis.Id,
            StokKartId = request.StokKartId,
            KdvId = kdv.Id, KdvOrani = kdv.Oran, KdvDahilMi = siparis.KdvDahilMi,
            StokKartSatisBirimiId = birim?.Id, BirimKodu = birim?.BirimKodu, BirimAdi = birim?.BirimAdi, BirimKatsayi = birim?.Katsayi,
            FiyatParaBirimKodu = fiyatParaBirimi, FiyatKur = fiyatKur,
            StokKartVaryantId = request.StokKartVaryantId,
            Miktar = request.Miktar,
            BirimFiyat = request.BirimFiyat,
            IndirimOrani = request.IndirimOrani,
            IndirimTutari = request.IndirimTutari ?? 0,
            Aciklama = NormalizeOptional(request.Aciklama),
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.SiparisDetaylari.Add(detay);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var detaylar = await _dbContext.SiparisDetaylari.Where(x => x.SiparisId == siparis.Id && x.AktifMi).OrderBy(x => x.Id).ToListAsync(cancellationToken);
        SiparisToplamlariniHesapla(siparis, detaylar);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await SiparisGetirAsync(siparis.Id, cancellationToken);
    }

    public async Task<SiparisDto> SiparisGetirAsync(long id, CancellationToken cancellationToken = default)
    {
        var siparis = await _dbContext.Siparisler
            .Include(x => x.Detaylar.Where(y => y.AktifMi)).ThenInclude(x => x.StokKart)
            .Include(x => x.Detaylar.Where(y => y.AktifMi)).ThenInclude(x => x.StokKartVaryant)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Siparis bulunamadi", "Siparis bulunamadi.", "siparis_not_found");

        return new SiparisDto
        {
            Id = siparis.Id,
            SiparisNo = siparis.SiparisNo,
            SiparisTipi = siparis.SiparisTipi,
            AdisyonId = siparis.AdisyonId,
            CariId = siparis.CariId,
            Aciklama = siparis.Aciklama,
            ParaBirimKodu = siparis.ParaBirimKodu,
            Kur = siparis.Kur,
            AraToplam = siparis.AraToplam,
            ToplamMatrah = siparis.ToplamMatrah, ToplamKdv = siparis.ToplamKdv,
            GenelIndirimOrani = siparis.GenelIndirimOrani,
            GenelIndirimTutari = siparis.GenelIndirimTutari,
            NetToplam = siparis.NetToplam,
            ToplamTutar = siparis.ToplamTutar,
            Durum = siparis.Durum,
            AktifMi = siparis.AktifMi,
            Detaylar = siparis.Detaylar.OrderBy(x => x.Id).Select(x => new SiparisDetayDto
            {
                Id = x.Id,
                KdvId = x.KdvId, KdvOrani = x.KdvOrani, KdvDahilMi = x.KdvDahilMi,
                Matrah = x.Matrah, KdvTutari = x.KdvTutari, GenelIndirimPayi = x.GenelIndirimPayi,
                StokKartId = x.StokKartId,
                StokKartAd = x.StokKart.Ad,
                StokKartVaryantId = x.StokKartVaryantId,
                VaryantKodu = x.StokKartVaryant != null ? x.StokKartVaryant.VaryantKodu : null,
                Miktar = x.Miktar,
                BirimFiyat = x.BirimFiyat,
                SatirAraToplam = x.SatirAraToplam,
                IndirimOrani = x.IndirimOrani,
                IndirimTutari = x.IndirimTutari,
                SatirNetToplam = x.SatirNetToplam,
                SatirToplam = x.SatirToplam,
                Aciklama = x.Aciklama
            }).ToList()
        };
    }

    public async Task<SayfaliSonucDto<SiparisListeItemDto>> SiparisListeleAsync(long subeId, int? durum, int page, int pageSize, CancellationToken cancellationToken = default)
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
FROM Siparis
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@Durum IS NULL OR Durum = @Durum);";

        var provider = _dbContext.Database.ProviderName ?? string.Empty;
        var listSql = provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)
            ? @"SELECT Id, SiparisNo, SiparisTipi, Durum, ParaBirimKodu, Kur, AraToplam, GenelIndirimTutari, NetToplam, OlusturmaTarihi
FROM Siparis
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@Durum IS NULL OR Durum = @Durum)
ORDER BY Id DESC
LIMIT @Take OFFSET @Skip;"
            : @"SELECT Id, SiparisNo, SiparisTipi, Durum, ParaBirimKodu, Kur, AraToplam, GenelIndirimTutari, NetToplam, OlusturmaTarihi
FROM Siparis
WHERE TenantId = @TenantId
  AND SubeId = @SubeId
  AND SilindiMi = 0
  AND (@Durum IS NULL OR Durum = @Durum)
ORDER BY Id DESC
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

        var parameters = new { TenantId = tenantId, SubeId = subeId, Durum = durum, Skip = (page - 1) * pageSize, Take = pageSize };
        var toplamKayit = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
        var kayitlar = (await connection.QueryAsync<SiparisListeItemDto>(new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken))).ToList();

        return new SayfaliSonucDto<SiparisListeItemDto>
        {
            ToplamKayit = toplamKayit,
            Sayfa = page,
            SayfaBoyutu = pageSize,
            Kayitlar = kayitlar
        };
    }

    public async Task<SiparisDto> SiparisIptalAsync(long id, SiparisIptalRequestDto? request = null, CancellationToken cancellationToken = default)
    {
        var siparis = await _dbContext.Siparisler.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Siparis bulunamadi", "Siparis bulunamadi.", "siparis_not_found");

        if (siparis.Durum == SiparisDurumu.Iptal)
        {
            return await SiparisGetirAsync(id, cancellationToken);
        }

        if (siparis.Durum == SiparisDurumu.Tamamlandi)
        {
            throw new UygulamaHatasi(409, "Siparis iptal edilemedi", "Tamamlanmis siparis iptal edilemez.", "siparis_completed");
        }

        siparis.Durum = SiparisDurumu.Iptal;
        siparis.AktifMi = false;
        if (!string.IsNullOrWhiteSpace(request?.Aciklama))
        {
            siparis.Aciklama = request!.Aciklama!.Trim();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await SiparisGetirAsync(id, cancellationToken);
    }

    private void SiparisToplamlariniHesapla(Siparis siparis, IEnumerable<SiparisDetay> detaylar)
    {
        var lines = detaylar.ToList();
        if (lines.Count == 0) { siparis.AraToplam = siparis.NetToplam = siparis.ToplamTutar = siparis.ToplamMatrah = siparis.ToplamKdv = 0; return; }
        var sonuc = _vergi.Hesapla(lines.Select(x => new VergiSatir(x.Miktar, x.BirimFiyat, x.KdvOrani,
            x.KdvDahilMi, x.IndirimOrani, x.IndirimTutari)).ToList(), siparis.GenelIndirimOrani,
            siparis.GenelIndirimOrani.HasValue ? 0m : siparis.GenelIndirimTutari);
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i]; var r = sonuc.Satirlar[i];
            line.SatirAraToplam = r.AraToplam; line.IndirimTutari = r.IndirimTutari;
            line.GenelIndirimPayi = r.GenelIndirimPayi; line.Matrah = r.Matrah; line.KdvTutari = r.KdvTutari;
            line.SatirNetToplam = line.SatirToplam = r.NetToplam;
        }
        siparis.AraToplam = sonuc.AraToplam; siparis.GenelIndirimTutari = sonuc.GenelIndirimTutari;
        siparis.ToplamMatrah = sonuc.ToplamMatrah; siparis.ToplamKdv = sonuc.ToplamKdv;
        siparis.NetToplam = siparis.ToplamTutar = sonuc.NetToplam;
    }

    private static void SiparisSatirIndirimKontrolu(decimal? indirimOrani, decimal? indirimTutari)
    {
        if (indirimOrani.HasValue && indirimTutari.HasValue)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Ayni satirda hem IndirimOrani hem IndirimTutari dolu olamaz.", "line_discount_conflict");
        }

        if (indirimOrani.HasValue && (indirimOrani < 0 || indirimOrani > 100))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "IndirimOrani 0-100 arasi olmali.", "line_discount_rate_invalid");
        }

        if (indirimTutari.HasValue && indirimTutari < 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "IndirimTutari negatif olamaz.", "line_discount_amount_invalid");
        }
    }

    private static void SiparisGenelIndirimKontrolu(decimal? genelIndirimOrani, decimal? genelIndirimTutari)
    {
        if (genelIndirimOrani.HasValue && genelIndirimTutari.HasValue)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "Sipariste hem GenelIndirimOrani hem GenelIndirimTutari dolu olamaz.", "order_discount_conflict");
        }

        if (genelIndirimOrani.HasValue && (genelIndirimOrani < 0 || genelIndirimOrani > 100))
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "GenelIndirimOrani 0-100 arasi olmali.", "order_discount_rate_invalid");
        }

        if (genelIndirimTutari.HasValue && genelIndirimTutari < 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "GenelIndirimTutari negatif olamaz.", "order_discount_amount_invalid");
        }
    }

    private async Task<string> SiparisNoUretAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var bugun = DateTime.UtcNow.ToString("yyyyMMdd");
        var oncekiSayac = await _dbContext.Siparisler
            .Where(x => x.TenantId == tenantId && x.SiparisNo.StartsWith($"SIP-{bugun}-"))
            .Select(x => x.SiparisNo)
            .ToListAsync(cancellationToken);

        var sonraki = oncekiSayac
            .Select(x => x.Split('-').LastOrDefault())
            .Select(x => int.TryParse(x, out var sayi) ? sayi : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"SIP-{bugun}-{sonraki:000000}";
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}




