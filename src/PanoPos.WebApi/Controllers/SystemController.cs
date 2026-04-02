using Microsoft.AspNetCore.Mvc;

namespace PanoPos.WebApi.Controllers;

[ApiController]
[Route("api/v1/system")]
public sealed class SystemController : ControllerBase
{
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "PanoPos.WebApi",
            utcTime = DateTime.UtcNow
        });
    }
}
