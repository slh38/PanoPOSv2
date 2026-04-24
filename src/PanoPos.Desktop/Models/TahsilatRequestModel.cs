namespace PanoPos.Desktop.Models;

public sealed class TahsilatRequestModel
{
    public long SubeId { get; set; }
    public long FaturaId { get; set; }
    public OdemeTipiModel OdemeTipi { get; set; }
    public long KullaniciId { get; set; }
    public long CihazId { get; set; }
    public decimal Tutar { get; set; }
    public string ParaBirimKodu { get; set; } = string.Empty;
    public decimal Kur { get; set; }
}
