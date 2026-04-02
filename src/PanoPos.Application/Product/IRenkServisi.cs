namespace PanoPos.Application.Product;

public interface IRenkServisi
{
    Task<RenkDto> OlusturAsync(RenkOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<List<RenkDto>> ListeleAsync(CancellationToken cancellationToken = default);
}
