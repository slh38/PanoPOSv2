using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Order;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Order;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Tests;

internal static class SatisFiyatFixture
{
    // Existing tax/ledger tests now arrange a persisted master price before adding a line.
    public static async Task<SiparisDto> KayitliFiyatlaSatirEkleAsync(this SiparisServisi service,
        PanoPosDbContext db, long orderId, SiparisSatirEkleRequestDto request)
    {
        var stok = await db.StokKartler.SingleAsync(x => x.Id == request.StokKartId);
        var birim = request.StokKartSatisBirimiId.HasValue
            ? await db.StokKartSatisBirimleri.SingleAsync(x => x.Id == request.StokKartSatisBirimiId)
            : await db.StokKartSatisBirimleri.FirstOrDefaultAsync(x => x.StokKartId == stok.Id && x.AktifMi);
        if (birim == null)
        {
            birim = new() { TenantId = stok.TenantId, SubeId = stok.SubeId, StokKartId = stok.Id,
                BirimKodu = "AD", BirimAdi = "Adet", Katsayi = 1, VarsayilanMi = true };
            db.Add(birim); await db.SaveChangesAsync();
        }
        var type = request.FiyatTipiId ?? 1;
        var price = await db.StokKartFiyatlari.SingleOrDefaultAsync(x => x.StokKartSatisBirimiId == birim.Id && x.FiyatTipiId == type && x.AktifMi);
        if (price == null)
        {
            price = new() { TenantId = stok.TenantId, SubeId = stok.SubeId, StokKartSatisBirimiId = birim.Id, FiyatTipiId = type };
            db.Add(price);
        }
        price.Fiyat = request.BirimFiyat;
        price.ParaBirimKodu = request.FiyatParaBirimKodu ?? await db.Siparisler.Where(x => x.Id == orderId).Select(x => x.ParaBirimKodu).SingleAsync();
        await db.SaveChangesAsync();
        request.StokKartSatisBirimiId = birim.Id;
        request.FiyatTipiId = type;
        return await service.SiparisSatirEkleAsync(orderId, request);
    }
}
