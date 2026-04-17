namespace PanoPos.Application.Restaurant;

public interface IMasaGrupServisi
{
    Task<MasaGrupDto> OlusturAsync(MasaGrupOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<List<MasaGrupDto>> ListeleAsync(CancellationToken cancellationToken = default);
}
