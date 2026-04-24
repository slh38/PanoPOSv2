using PanoPos.Desktop.Models;

namespace PanoPos.Desktop.Services;

public interface IHizliSatisService
{
    Task<List<UrunKartModel>> GetUrunlerAsync(CancellationToken cancellationToken = default);
    Task<SepetSatirModel?> GetSepetSatiriByBarcodeAsync(string barkodNo, CancellationToken cancellationToken = default);
    Task<long> BekletAsync(IEnumerable<SepetSatirModel> satirlar, CancellationToken cancellationToken = default);
    Task<FaturaResponseModel> FaturaOlusturAsync(IEnumerable<SepetSatirModel> satirlar, CancellationToken cancellationToken = default);
}
