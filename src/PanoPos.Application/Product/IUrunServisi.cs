using PanoPos.Application.Common;

namespace PanoPos.Application.Product;

public interface IUrunServisi
{
    Task<UrunDto> UrunOlusturAsync(UrunOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<UrunDto> UrunGuncelleAsync(long id, UrunGuncelleRequestDto request, CancellationToken cancellationToken = default);
    Task<UrunDto> UrunDetayGetirAsync(long id, CancellationToken cancellationToken = default);
    Task<SayfaliSonucDto<UrunListeItemDto>> UrunListeleAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<UrunVaryantDto> UrunVaryantOlusturAsync(long urunId, UrunVaryantOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<List<UrunVaryantDto>> UrunVaryantlariGetirAsync(long urunId, CancellationToken cancellationToken = default);
}
