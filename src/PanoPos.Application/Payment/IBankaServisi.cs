namespace PanoPos.Application.Payment;

public interface IBankaServisi
{
    Task<BankaDto> BankaOlusturAsync(BankaOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<List<BankaDto>> BankaListeleAsync(long subeId, CancellationToken cancellationToken = default);
}
