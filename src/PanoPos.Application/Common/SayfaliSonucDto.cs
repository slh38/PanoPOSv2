namespace PanoPos.Application.Common;

public sealed class SayfaliSonucDto<T>
{
    public int ToplamKayit { get; set; }
    public int Sayfa { get; set; }
    public int SayfaBoyutu { get; set; }
    public List<T> Kayitlar { get; set; } = new();
}
