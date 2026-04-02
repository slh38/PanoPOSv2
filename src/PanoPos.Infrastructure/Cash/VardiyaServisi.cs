using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Application.Cash;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Infrastructure.Cash;

public sealed class VardiyaServisi : IVardiyaServisi
{
    private readonly PanoPosDbContext _dbContext;

    public VardiyaServisi(PanoPosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VardiyaResponseDto> VardiyaAcAsync(long kullaniciId, long cihazId, long kasaId, decimal acilisNakit, CancellationToken cancellationToken = default)
    {
        if (kullaniciId <= 0 || cihazId <= 0 || kasaId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "KullaniciId, CihazId ve KasaId zorunludur.", "vardiya_open_invalid_request");
        }

        var cihaz = await _dbContext.Cihazlar.SingleOrDefaultAsync(x => x.Id == cihazId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Cihaz bulunamadi", "Cihaz bulunamadi.", "cihaz_not_found");

        var kasa = await _dbContext.Kasalar.SingleOrDefaultAsync(x => x.Id == kasaId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Kasa bulunamadi", "Kasa bulunamadi.", "kasa_not_found");

        var kullanici = await _dbContext.Kullanicilar.SingleOrDefaultAsync(x => x.Id == kullaniciId, cancellationToken)
            ?? throw new UygulamaHatasi(404, "Kullanici bulunamadi", "Kullanici bulunamadi.", "kullanici_not_found");

        var aktifOturumVar = await _dbContext.KullaniciOturumlari
            .AnyAsync(x => x.KullaniciId == kullaniciId && x.CihazId == cihazId && x.AktifMi && x.CikisTarihi == null, cancellationToken);

        if (!aktifOturumVar)
        {
            throw new UygulamaHatasi(403, "Vardiya acilamadi", "Kullanicinin aktif oturumu yok.", "aktif_oturum_required");
        }

        var cihazdaAktifVardiyaVar = await _dbContext.Vardiyalar
            .AnyAsync(x => x.CihazId == cihazId && x.AktifMi, cancellationToken);

        if (cihazdaAktifVardiyaVar)
        {
            throw new UygulamaHatasi(409, "Vardiya acilamadi", "Ayni cihazda aktif vardiya zaten var.", "aktif_vardiya_device_exists");
        }

        var kasadaAktifVardiyaVar = await _dbContext.Vardiyalar
            .AnyAsync(x => x.KasaId == kasaId && x.AktifMi, cancellationToken);

        if (kasadaAktifVardiyaVar)
        {
            throw new UygulamaHatasi(409, "Vardiya acilamadi", "Ayni kasada aktif vardiya zaten var.", "aktif_vardiya_cash_exists");
        }

        var simdi = DateTime.UtcNow;
        var vardiya = new Vardiya
        {
            TenantId = kullanici.TenantId,
            SubeId = cihaz.SubeId,
            KasaId = kasaId,
            CihazId = cihazId,
            KullaniciId = kullaniciId,
            AcilisTarihi = simdi,
            AcilisNakit = acilisNakit,
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.Vardiyalar.Add(vardiya);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _dbContext.KasaHareketleri.Add(new KasaHareket
        {
            TenantId = vardiya.TenantId,
            SubeId = vardiya.SubeId,
            KasaId = vardiya.KasaId,
            VardiyaId = vardiya.Id,
            KullaniciId = vardiya.KullaniciId,
            CihazId = vardiya.CihazId,
            IslemTipi = KasaIslemTipi.Acilis,
            Tutar = acilisNakit,
            Aciklama = "Vardiya acilis nakdi",
            ReferansTip = nameof(Vardiya),
            ReferansId = vardiya.Id,
            Tarih = simdi,
            AktifMi = true,
            SilindiMi = false
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(vardiya);
    }

    public async Task<VardiyaKapanisResponseDto> VardiyaKapatAsync(long vardiyaId, decimal sayilanNakit, string? aciklama, CancellationToken cancellationToken = default)
    {
        if (vardiyaId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "VardiyaId zorunludur.", "vardiya_id_required");
        }

        var vardiya = await _dbContext.Vardiyalar
            .Include(x => x.KasaHareketleri.Where(y => y.AktifMi))
            .SingleOrDefaultAsync(x => x.Id == vardiyaId, cancellationToken);

        if (vardiya is null || !vardiya.AktifMi)
        {
            throw new UygulamaHatasi(404, "Vardiya bulunamadi", "Aktif vardiya bulunamadi.", "aktif_vardiya_not_found");
        }

        var beklenenNakit = HesaplaBeklenenNakit(vardiya);
        var farkTutar = sayilanNakit - beklenenNakit;
        var simdi = DateTime.UtcNow;

        var kapanis = new VardiyaKapanis
        {
            TenantId = vardiya.TenantId,
            SubeId = vardiya.SubeId,
            VardiyaId = vardiya.Id,
            BeklenenNakit = beklenenNakit,
            SayilanNakit = sayilanNakit,
            FarkTutar = farkTutar,
            KartToplam = 0,
            VeresiyeToplam = 0,
            Aciklama = string.IsNullOrWhiteSpace(aciklama) ? null : aciklama.Trim(),
            AktifMi = true,
            SilindiMi = false
        };

        vardiya.AktifMi = false;
        vardiya.KapanisTarihi = simdi;

        _dbContext.VardiyaKapanislari.Add(kapanis);
        _dbContext.KasaHareketleri.Add(new KasaHareket
        {
            TenantId = vardiya.TenantId,
            SubeId = vardiya.SubeId,
            KasaId = vardiya.KasaId,
            VardiyaId = vardiya.Id,
            KullaniciId = vardiya.KullaniciId,
            CihazId = vardiya.CihazId,
            IslemTipi = KasaIslemTipi.VardiyaKapanis,
            Tutar = sayilanNakit,
            Aciklama = "Vardiya kapanis nakdi",
            ReferansTip = nameof(VardiyaKapanis),
            ReferansId = vardiya.Id,
            Tarih = simdi,
            AktifMi = true,
            SilindiMi = false
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new VardiyaKapanisResponseDto
        {
            VardiyaId = vardiya.Id,
            BeklenenNakit = beklenenNakit,
            SayilanNakit = sayilanNakit,
            FarkTutar = farkTutar,
            KapanisTarihi = simdi
        };
    }

    public async Task<VardiyaResponseDto?> AktifVardiyaGetirAsync(long cihazId, CancellationToken cancellationToken = default)
    {
        if (cihazId <= 0)
        {
            throw new UygulamaHatasi(400, "Gecersiz istek", "CihazId zorunludur.", "cihaz_required");
        }

        var vardiya = await _dbContext.Vardiyalar
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.CihazId == cihazId && x.AktifMi, cancellationToken);

        return vardiya is null ? null : ToDto(vardiya);
    }

    private static decimal HesaplaBeklenenNakit(Vardiya vardiya)
    {
        var hareketToplami = vardiya.KasaHareketleri
            .Where(x => x.IslemTipi != KasaIslemTipi.Acilis && x.IslemTipi != KasaIslemTipi.VardiyaKapanis)
            .Sum(x => IslemTutari(x.IslemTipi, x.Tutar));

        return vardiya.AcilisNakit + hareketToplami;
    }

    private static decimal IslemTutari(KasaIslemTipi islemTipi, decimal tutar)
    {
        return islemTipi switch
        {
            KasaIslemTipi.SatisTahsilat => tutar,
            KasaIslemTipi.NakitGiris => tutar,
            KasaIslemTipi.Duzeltme => tutar,
            KasaIslemTipi.NakitCikis => -tutar,
            KasaIslemTipi.Masraf => -tutar,
            _ => 0
        };
    }

    private static VardiyaResponseDto ToDto(Vardiya vardiya)
    {
        return new VardiyaResponseDto
        {
            VardiyaId = vardiya.Id,
            KasaId = vardiya.KasaId,
            CihazId = vardiya.CihazId,
            KullaniciId = vardiya.KullaniciId,
            AcilisNakit = vardiya.AcilisNakit,
            AcilisTarihi = vardiya.AcilisTarihi,
            AktifMi = vardiya.AktifMi
        };
    }
}
