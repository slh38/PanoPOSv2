namespace PanoPos.Application.Audit;

public sealed class IslemLogDto
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public long SubeId { get; set; }
    public long? CihazId { get; set; }
    public long? KullaniciId { get; set; }
    public string ModulAdi { get; set; } = string.Empty;
    public string? EkranAdi { get; set; }
    public string? ButonAdi { get; set; }
    public string IslemTipi { get; set; } = string.Empty;
    public string? HedefTablo { get; set; }
    public long? HedefId { get; set; }
    public string? Aciklama { get; set; }
    public bool BasariliMi { get; set; }
    public string? HataKodu { get; set; }
    public string? HataMesaji { get; set; }
    public long? SureMs { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
}
