using System.ComponentModel.DataAnnotations;

namespace PanoPos.Application.Auth;

public sealed class LoginRequestDto
{
    [Required]
    public string Pin { get; set; } = string.Empty;

    [Range(1, long.MaxValue)]
    public long CihazId { get; set; }
}
