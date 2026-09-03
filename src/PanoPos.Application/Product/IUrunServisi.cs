using PanoPos.Application.Common;

namespace PanoPos.Application.Product;

public interface IStokKartServisi
{
    Task<StokKartDto> StokKartOlusturAsync(StokKartOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<StokKartDto> StokKartGuncelleAsync(long id, StokKartGuncelleRequestDto request, CancellationToken cancellationToken = default);
    Task<StokKartDto> StokKartDetayGetirAsync(long id, CancellationToken cancellationToken = default);
    Task<SayfaliSonucDto<StokKartListeItemDto>> StokKartListeleAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<StokKartVaryantDto> StokKartVaryantOlusturAsync(long urunId, StokKartVaryantOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<List<StokKartVaryantDto>> StokKartVaryantlariGetirAsync(long urunId, CancellationToken cancellationToken = default);
}
