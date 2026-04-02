using System.ComponentModel.DataAnnotations;

namespace PanoPos.Application.Auth;

public sealed class LogoutRequestDto
{
    [Range(1, long.MaxValue)]
    public long KullaniciOturumId { get; set; }
}
