namespace PanoPos.Application.Product;

public interface IUrunGrupServisi
{
    Task<UrunGrupDto> OlusturAsync(UrunGrupOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<List<UrunGrupDto>> ListeleAsync(CancellationToken cancellationToken = default);
}
