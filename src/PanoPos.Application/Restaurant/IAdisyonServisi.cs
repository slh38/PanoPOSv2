namespace PanoPos.Application.Restaurant;

public interface IAdisyonServisi
{
    Task<AdisyonDto> AdisyonAcAsync(AdisyonAcRequestDto request, CancellationToken cancellationToken = default);
    Task<AdisyonDto> AdisyonKapatAsync(AdisyonKapatRequestDto request, CancellationToken cancellationToken = default);
    Task<AdisyonDto?> AcikAdisyonGetirAsync(long masaId, CancellationToken cancellationToken = default);
}
