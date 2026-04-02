namespace PanoPos.Domain.Entities;

public sealed class MasaDurum
{
    public long Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public bool AktifMi { get; set; }

    public ICollection<Masa> Masalar { get; set; } = new List<Masa>();
}
