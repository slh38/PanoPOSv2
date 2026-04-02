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

    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetInfo()
    {
        var environment = HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();

        return Ok(new
        {
            service = "PanoPos.WebApi",
            version = "v1",
            environment = environment.EnvironmentName,
            databaseProvider = "SqlServer",
            connectionStringName = "PanoPos"
        });
    }
}
