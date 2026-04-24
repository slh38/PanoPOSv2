using PanoPos.Desktop.Models;
using PanoPos.Desktop.Session;

namespace PanoPos.Desktop.Services;

public sealed class HizliSatisService : IHizliSatisService
{
    private const short HizliSatisBekleyen = 2;

    private readonly IApiClient _apiClient;
    private readonly AppSession _session;

    public HizliSatisService(IApiClient apiClient, AppSession session)
    {
        _apiClient = apiClient;
        _session = session;
    }

    public async Task<List<UrunKartModel>> GetUrunlerAsync(CancellationToken cancellationToken = default)
    {
        var endpoint = $"/api/v1/urun?subeId={_session.VarsayilanSubeId}&search=&page=1&pageSize=50";
        var response = await _apiClient.GetAsync<PagedResultModel<UrunListItemApiModel>>(endpoint, cancellationToken);

        return response.Kayitlar
            .Where(x => x.AktifMi)
            .Select(x => new UrunKartModel
            {
                UrunId = x.Id,
                UrunAdi = x.Ad,
                Fiyat = 0m,
                KategoriId = x.UrunKategoriId,
                KategoriAdi = x.UrunKategoriAd ?? string.Empty
            })
            .ToList();
    }

    public async Task<SepetSatirModel?> GetSepetSatiriByBarcodeAsync(string barkodNo, CancellationToken cancellationToken = default)
    {
        var barkod = await _apiClient.GetAsync<BarkodApiModel>($"/api/v1/barkod/{Uri.EscapeDataString(barkodNo)}", cancellationToken);
        if (barkod.UrunId is null)
        {
            return null;
        }

        var urun = await _apiClient.GetAsync<UrunDetayApiModel>($"/api/v1/urun/{barkod.UrunId.Value}", cancellationToken);

        return new SepetSatirModel
        {
            UrunId = barkod.UrunId.Value,
            UrunVaryantId = barkod.UrunVaryantId,
            UrunAdi = barkod.UrunAd ?? urun.Ad,
            Miktar = 1,
            BirimFiyat = 0m,
            IndirimTutari = 0m,
            SatirNetToplam = 0m,
            BarkodNo = barkod.BarkodNo
        };
    }

    public async Task<long> BekletAsync(IEnumerable<SepetSatirModel> satirlar, CancellationToken cancellationToken = default)
    {
        var satirList = satirlar.ToList();
        if (satirList.Count == 0)
        {
            throw new InvalidOperationException("Bekletmek icin sepette urun olmalidir.");
        }

        var siparis = await _apiClient.PostAsync<SiparisOlusturRequestModel, SiparisResponseModel>(
            "/api/v1/siparis",
            new SiparisOlusturRequestModel
            {
                SubeId = _session.VarsayilanSubeId,
                SiparisTipi = HizliSatisBekleyen,
                ParaBirimKodu = "TRY",
                Kur = 1,
                GenelIndirimTutari = 0m
            },
            cancellationToken);

        if (siparis is null || siparis.Id <= 0)
        {
            throw new InvalidOperationException("Siparis olusturulamadi.");
        }

        foreach (var satir in satirList)
        {
            await _apiClient.PostAsync<SiparisSatirEkleRequestModel, SiparisResponseModel>(
                $"/api/v1/siparis/{siparis.Id}/satir",
                new SiparisSatirEkleRequestModel
                {
                    UrunId = satir.UrunId,
                    UrunVaryantId = satir.UrunVaryantId,
                    Miktar = satir.Miktar,
                    BirimFiyat = satir.BirimFiyat,
                    IndirimTutari = satir.IndirimTutari
                },
                cancellationToken);
        }

        return siparis.Id;
    }
}
