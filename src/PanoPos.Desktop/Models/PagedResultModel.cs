namespace PanoPos.Desktop.Models;

public sealed class PagedResultModel<T>
{
    public int ToplamKayit { get; set; }
    public int Sayfa { get; set; }
    public int SayfaBoyutu { get; set; }
    public List<T> Kayitlar { get; set; } = [];
}
