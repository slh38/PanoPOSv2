namespace PanoPos.Application.Auth;

public sealed class LoginResponseDto
{
    public string OturumToken { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public long SubeId { get; set; }
    public long KullaniciId { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public long VarsayilanSubeId { get; set; }
    public long CihazId { get; set; }
    public long OturumId { get; set; }
    public List<string> Roller { get; set; } = new();
    public List<SubeBilgisiDto> Subeler { get; set; } = new();
}
