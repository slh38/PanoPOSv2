using Microsoft.AspNetCore.Mvc;
using PanoPos.Application.Auth;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthServisi _authServisi;

    public AuthController(IAuthServisi authServisi)
    {
        _authServisi = authServisi;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authServisi.LoginAsync(request.Pin, request.CihazId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request, CancellationToken cancellationToken)
    {
        await _authServisi.LogoutAsync(request.KullaniciOturumId, cancellationToken);
        return NoContent();
    }
}
