namespace PanoPos.Desktop.Models;

public sealed class TahsilatResponseModel
{
    public long Id { get; set; }
    public decimal FaturaOdenenTutar { get; set; }
    public decimal FaturaKalanTutar { get; set; }
    public short FaturaDurumu { get; set; }
}
