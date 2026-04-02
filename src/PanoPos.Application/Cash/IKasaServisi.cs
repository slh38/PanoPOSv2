namespace PanoPos.Application.Cash;

public interface IKasaServisi
{
    Task<List<KasaDto>> ListeleAsync(CancellationToken cancellationToken = default);
    Task<KasaDto> OlusturAsync(KasaOlusturRequestDto request, CancellationToken cancellationToken = default);
}
