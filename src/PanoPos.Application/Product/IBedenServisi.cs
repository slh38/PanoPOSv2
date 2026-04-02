namespace PanoPos.Application.Product;

public interface IBedenServisi
{
    Task<BedenDto> OlusturAsync(BedenOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<List<BedenDto>> ListeleAsync(CancellationToken cancellationToken = default);
}
