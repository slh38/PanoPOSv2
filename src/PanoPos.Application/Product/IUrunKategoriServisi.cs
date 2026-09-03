namespace PanoPos.Application.Product;

public interface IStokKategoriServisi
{
    Task<StokKategoriDto> OlusturAsync(StokKategoriOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<List<StokKategoriDto>> ListeleAsync(CancellationToken cancellationToken = default);
}
