namespace PanoPos.Application.Product;

public interface IBarkodServisi
{
    Task<BarkodDto> BarkodOlusturAsync(BarkodOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<BarkodDto?> BarkodIleBulAsync(string barkodNo, CancellationToken cancellationToken = default);
    Task<BarkodDto> BarkodGuncelleAsync(long id, BarkodOlusturRequestDto request, CancellationToken cancellationToken = default);
}
