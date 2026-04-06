namespace PanoPos.Application.Audit;

public sealed class IslemLogListeItemDto
{
    public long Id { get; set; }
    public string ModulAdi { get; set; } = string.Empty;
    public string? EkranAdi { get; set; }
    public string? ButonAdi { get; set; }
    public string IslemTipi { get; set; } = string.Empty;
    public string? HedefTablo { get; set; }
    public long? HedefId { get; set; }
    public bool BasariliMi { get; set; }
    public string? HataKodu { get; set; }
    public long? KullaniciId { get; set; }
    public long? CihazId { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
}
