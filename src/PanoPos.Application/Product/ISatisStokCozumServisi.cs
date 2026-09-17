using PanoPos.Application.Common;

namespace PanoPos.Application.Product;

public interface ISatisStokCozumServisi
{
    Task<SatisStokDto> BarkodCozAsync(string barkodNo, long fiyatTipiId, CancellationToken ct = default);
    Task<SatisStokDto> FiyatCozAsync(long satisBirimiId, long fiyatTipiId, long? varyantId = null, CancellationToken ct = default);
    Task<SayfaliSonucDto<FiyatTipiListeDto>> FiyatTipleriAsync(int page, int pageSize, CancellationToken ct = default);
    Task<SayfaliSonucDto<SatisBirimiListeDto>> SatisBirimleriAsync(long stokKartId, int page, int pageSize, CancellationToken ct = default);
}

public sealed record FiyatTipiListeDto(long Id, string Kod, string Ad);
public sealed record SatisBirimiListeDto(long Id, string BirimKodu, string BirimAdi, decimal Katsayi, bool VarsayilanMi);
