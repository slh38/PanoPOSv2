using PanoPos.Application.Common;
using PanoPos.Domain.Enums;
namespace PanoPos.Application.Purchase;
public sealed class AlisFaturaKaydetRequest
{
    public long SubeId { get; set; }
    public long CariId { get; set; }
    public string FaturaNo { get; set; } = string.Empty;
    public DateTime FaturaTarihi { get; set; }
    public string ParaBirimKodu { get; set; } = "TRY";
    public decimal Kur { get; set; } = 1m;
    public decimal? GenelIndirimOrani { get; set; }
    public decimal? GenelIndirimTutari { get; set; }
    public string? Aciklama { get; set; }
    public List<AlisFaturaSatirRequest> Detaylar { get; set; } = new();
}
public sealed class AlisFaturaSatirRequest
{
    public long? Id { get; set; }
    public long StokKartId { get; set; }
    public long? StokKartVaryantId { get; set; }
    public long StokKartSatisBirimiId { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public string FiyatParaBirimKodu { get; set; } = "TRY";
    public decimal FiyatKur { get; set; } = 1m;
    public decimal? IndirimOrani { get; set; }
    public decimal? IndirimTutari { get; set; }
}
public class AlisFaturaListeDto
{
    public long Id { get; set; }
    public long CariId { get; set; }
    public string CariAd { get; set; } = string.Empty;
    public string FaturaNo { get; set; } = string.Empty;
    public DateTime FaturaTarihi { get; set; }
    public string ParaBirimKodu { get; set; } = string.Empty;
    public decimal Kur { get; set; }
    public decimal AraToplam { get; set; }
    public decimal? GenelIndirimOrani { get; set; }
    public decimal GenelIndirimTutari { get; set; }
    public decimal ToplamMatrah { get; set; }
    public decimal ToplamKdv { get; set; }
    public decimal NetToplam { get; set; }
    public AlisFaturaDurumu Durum { get; set; }
}
public sealed class AlisFaturaDto : AlisFaturaListeDto
{
    public bool KdvDahilMi { get; set; }
    public string? Aciklama { get; set; }
    public List<AlisFaturaDetayDto> Detaylar { get; set; } = new();
}
public sealed class AlisFaturaDetayDto
{
    public long Id { get; set; }
    public long StokKartId { get; set; }
    public long? StokKartVaryantId { get; set; }
    public long StokKartSatisBirimiId { get; set; }
    public string BirimKodu { get; set; } = string.Empty;
    public string BirimAdi { get; set; } = string.Empty;
    public decimal Katsayi { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public string FiyatParaBirimKodu { get; set; } = string.Empty;
    public decimal FiyatKur { get; set; }
    public decimal SatirAraToplam { get; set; }
    public decimal? IndirimOrani { get; set; }
    public decimal IndirimTutari { get; set; }
    public decimal GenelIndirimPayi { get; set; }
    public long KdvId { get; set; }
    public decimal KdvOrani { get; set; }
    public bool KdvDahilMi { get; set; }
    public decimal Matrah { get; set; }
    public decimal KdvTutari { get; set; }
    public decimal SatirNetToplam { get; set; }
}
public sealed class AlisFaturaFiltre
{
    public long SubeId { get; set; }
    public long? CariId { get; set; }
    public AlisFaturaDurumu? Durum { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public string? Arama { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
public interface IAlisFaturaServisi
{
    Task<AlisFaturaDto> CreateAsync(AlisFaturaKaydetRequest request, CancellationToken ct = default);
    Task<AlisFaturaDto> UpdateAsync(long id, AlisFaturaKaydetRequest request, CancellationToken ct = default);
    Task<AlisFaturaDto> GetByIdAsync(long id, long subeId, CancellationToken ct = default);
    Task<SayfaliSonucDto<AlisFaturaListeDto>> GetPagedAsync(AlisFaturaFiltre filtre, CancellationToken ct = default);
    Task DeleteAsync(long id, long subeId, CancellationToken ct = default);
    Task<AlisFaturaDto> KesinlestirAsync(long id, long subeId, CancellationToken ct = default);
    Task<AlisFaturaDto> IptalAsync(long id, long subeId, CancellationToken ct = default);
}
