using PanoPos.Application.Common;

namespace PanoPos.Application.Product;

public interface IStokKartServisi
{
    Task<StokKartDto> StokKartOlusturAsync(StokKartOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<StokKartDto> StokKartGuncelleAsync(long id, StokKartGuncelleRequestDto request, CancellationToken cancellationToken = default);
    Task<StokKartDto> StokKartDetayGetirAsync(long id, CancellationToken cancellationToken = default);
    Task<SayfaliSonucDto<StokKartListeItemDto>> StokKartListeleAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default, long? kategoriId = null, long? grupId = null, bool? aktifMi = null);
    Task<StokKartVaryantDto> StokKartVaryantOlusturAsync(long stokKartId, StokKartVaryantOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<List<StokKartVaryantDto>> StokKartVaryantlariGetirAsync(long stokKartId, CancellationToken cancellationToken = default);
}
