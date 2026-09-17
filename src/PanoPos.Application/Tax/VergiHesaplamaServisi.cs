using PanoPos.Application.Common;
namespace PanoPos.Application.Tax;

public sealed record VergiSatir(decimal Miktar, decimal BirimFiyat, decimal KdvOrani,
    bool KdvDahilMi, decimal? IndirimOrani = null, decimal IndirimTutari = 0m);
public sealed record VergiSatirSonuc(decimal AraToplam, decimal IndirimTutari,
    decimal GenelIndirimPayi, decimal Matrah, decimal KdvTutari, decimal NetToplam);
public sealed record VergiSonuc(IReadOnlyList<VergiSatirSonuc> Satirlar)
{
    public decimal AraToplam => Satirlar.Sum(x => x.AraToplam);
    public decimal GenelIndirimTutari => Satirlar.Sum(x => x.GenelIndirimPayi);
    public decimal ToplamMatrah => Satirlar.Sum(x => x.Matrah);
    public decimal ToplamKdv => Satirlar.Sum(x => x.KdvTutari);
    public decimal NetToplam => Satirlar.Sum(x => x.NetToplam);
}
public interface IVergiHesaplamaServisi
{
    VergiSonuc Hesapla(IReadOnlyList<VergiSatir> satirlar, decimal? genelIndirimOrani = null, decimal genelIndirimTutari = 0m);
}
public sealed class VergiHesaplamaServisi : IVergiHesaplamaServisi
{
    public static decimal Yuvarla(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    public VergiSonuc Hesapla(IReadOnlyList<VergiSatir> satirlar, decimal? genelIndirimOrani = null, decimal genelIndirimTutari = 0m)
    {
        if (genelIndirimOrani is < 0 or > 100 || genelIndirimTutari < 0 ||
            (genelIndirimOrani.HasValue && genelIndirimTutari != 0))
            throw Error("Genel indirim gecersiz.");
        var brutler = new decimal[satirlar.Count];
        var indirimler = new decimal[satirlar.Count];
        var kalanlar = new decimal[satirlar.Count];
        for (var i = 0; i < satirlar.Count; i++)
        {
            var s = satirlar[i];
            if (s.Miktar <= 0 || s.BirimFiyat < 0 || s.KdvOrani is < 0 or > 100 ||
                s.IndirimOrani is < 0 or > 100 || s.IndirimTutari < 0)
                throw Error("Satir miktari, fiyati, KDV veya indirimi gecersiz.");
            brutler[i] = Yuvarla(s.Miktar * s.BirimFiyat);
            indirimler[i] = Yuvarla(s.IndirimOrani.HasValue ? brutler[i] * s.IndirimOrani.Value / 100m : s.IndirimTutari);
            kalanlar[i] = brutler[i] - indirimler[i];
            if (kalanlar[i] < 0) throw Error("Indirim satir tutarini asamaz.");
        }
        var toplam = kalanlar.Sum();
        var indirim = Yuvarla(genelIndirimOrani.HasValue ? toplam * genelIndirimOrani.Value / 100m : genelIndirimTutari);
        if (indirim > toplam) throw Error("Genel indirim belge tutarini asamaz.");
        var paylar = new decimal[satirlar.Count];
        if (toplam > 0)
        {
            // Largest remainder allocation: stable input order resolves equal remainders.
            var ham = kalanlar.Select(x => indirim * x / toplam).ToArray();
            for (var i = 0; i < paylar.Length; i++) paylar[i] = decimal.Floor(ham[i] * 100m) / 100m;
            var kuruslar = (int)((indirim - paylar.Sum()) * 100m);
            foreach (var i in Enumerable.Range(0, paylar.Length).OrderByDescending(i => ham[i] - paylar[i]).ThenBy(i => i).Take(kuruslar))
                paylar[i] += 0.01m;
        }
        var sonuc = new List<VergiSatirSonuc>();
        for (var i = 0; i < satirlar.Count; i++)
        {
            var s = satirlar[i];
            var tutar = kalanlar[i] - paylar[i];
            var matrah = s.KdvDahilMi ? Yuvarla(tutar / (1m + s.KdvOrani / 100m)) : tutar;
            var kdv = s.KdvDahilMi ? tutar - matrah : Yuvarla(matrah * s.KdvOrani / 100m);
            sonuc.Add(new(brutler[i], indirimler[i], paylar[i], matrah, kdv, matrah + kdv));
        }
        return new(sonuc);
    }
    private static UygulamaHatasi Error(string message) => new(400, "Hesaplama hatasi", message, "tax_calculation_invalid");
}
