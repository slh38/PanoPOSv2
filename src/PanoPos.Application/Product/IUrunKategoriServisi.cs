namespace PanoPos.Application.Product;

public interface IUrunKategoriServisi
{
    Task<UrunKategoriDto> OlusturAsync(UrunKategoriOlusturRequestDto request, CancellationToken cancellationToken = default);
    Task<List<UrunKategoriDto>> ListeleAsync(CancellationToken cancellationToken = default);
}
