using PanoPos.Domain.Enums;

namespace PanoPos.Application.Invoice;

public sealed class FaturaDto
{
    public DateTime FaturaTarihi { get; set; }
    public Guid TenantId { get; set; }
    public string? TenantAdi { get; set; }
    public long SubeId { get; set; }
    public string? SubeAdi { get; set; }
    public string? CariKodu { get; set; }
    public string? CariAdi { get; set; }
    public long? KasiyerId { get; set; }
    public string? KasiyerAdi { get; set; }
    public long? CihazId { get; set; }
    public string? CihazAdi { get; set; }
    public string? DepoAdi { get; set; }
    public decimal NakitToplam { get; set; }
    public decimal KartToplam { get; set; }
    public decimal VeresiyeToplam { get; set; }
    public List<FaturaOdemeDto> Odemeler { get; set; } = new();
    public decimal ToplamMatrah { get; set; }
    public decimal ToplamKdv { get; set; }
    public long Id { get; set; }
    public long DepoId { get; set; }
    public string FaturaNo { get; set; } = string.Empty;
    public long? SiparisId { get; set; }
    public long? CariId { get; set; }
    public string? Aciklama { get; set; }
    public string ParaBirimKodu { get; set; } = string.Empty;
    public decimal Kur { get; set; }
    public decimal AraToplam { get; set; }
    public decimal? GenelIndirimOrani { get; set; }
    public decimal GenelIndirimTutari { get; set; }
    public decimal NetToplam { get; set; }
    public decimal OdenenTutar { get; set; }
    public decimal KalanTutar { get; set; }
    public decimal ToplamTutar { get; set; }
    public FaturaDurumu Durum { get; set; }
    public DateTime? KapanisTarihi { get; set; }
    public long? KapatanKullaniciId { get; set; }
    public bool AktifMi { get; set; }
    public List<FaturaDetayDto> Detaylar { get; set; } = new();
}
