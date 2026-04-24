namespace PanoPos.Desktop.Models;

public sealed class LoginRequestModel
{
    public string Pin { get; set; } = string.Empty;
    public long CihazId { get; set; }
}
