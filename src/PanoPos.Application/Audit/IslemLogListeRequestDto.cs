namespace PanoPos.Application.Audit;

public sealed class IslemLogListeRequestDto
{
    public long SubeId { get; set; }
    public long? KullaniciId { get; set; }
    public string? IslemTipi { get; set; }
    public bool? BasariliMi { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
