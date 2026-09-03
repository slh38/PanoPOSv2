namespace PanoPos.Application.Restaurant;

public interface IMasaServisi
{
    Task<MasaDto> MasaOlusturAsync(MasaOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<List<MasaDto>> MasaListeleAsync(long subeId, CancellationToken cancellationToken = default);
}
