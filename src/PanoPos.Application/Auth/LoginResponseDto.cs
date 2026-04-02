namespace PanoPos.Application.Auth;

public sealed class LoginResponseDto
{
    public long KullaniciId { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public long VarsayilanSubeId { get; set; }
    public long CihazId { get; set; }
    public long OturumId { get; set; }
    public List<string> Roller { get; set; } = new();
    public List<SubeBilgisiDto> Subeler { get; set; } = new();
}
