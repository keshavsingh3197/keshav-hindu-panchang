using Microsoft.AspNetCore.Mvc;

namespace HinduPanchang.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "HinduPanchang.Api",
            version = "1.0.0",
            time = DateTime.UtcNow
        });
    }
}
