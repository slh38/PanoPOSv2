namespace PanoPos.Application.Product;

public interface IStokGrupServisi
{
    Task<StokGrupDto> OlusturAsync(StokGrupOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<List<StokGrupDto>> ListeleAsync(CancellationToken cancellationToken = default);
}
