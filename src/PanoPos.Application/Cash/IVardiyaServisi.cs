namespace PanoPos.Application.Cash;

public interface IVardiyaServisi
{
    Task<VardiyaResponseDto> VardiyaAcAsync(long kullaniciId, long cihazId, long kasaId, decimal acilisNakit, CancellationToken cancellationToken = default);
    Task<VardiyaKapanisResponseDto> VardiyaKapatAsync(long vardiyaId, decimal sayilanNakit, string? aciklama, CancellationToken cancellationToken = default);
    Task<VardiyaResponseDto?> AktifVardiyaGetirAsync(long cihazId, CancellationToken cancellationToken = default);
}
