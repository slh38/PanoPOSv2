namespace PanoPos.Desktop.Models;

public sealed class LoginResponseModel
{
    public long KullaniciId { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public long CihazId { get; set; }
    public long OturumId { get; set; }
    public long VarsayilanSubeId { get; set; }
    public List<string> Roller { get; set; } = [];
    public List<SubeModel> Subeler { get; set; } = [];
}
