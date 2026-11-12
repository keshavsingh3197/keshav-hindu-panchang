using Microsoft.AspNetCore.Mvc;
using HinduPanchang.Api.Models;
using HinduPanchang.Api.Services;

namespace HinduPanchang.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PanchangController : ControllerBase
{
    private readonly VedicPanchangCalculator _calculator;

    public PanchangController(VedicPanchangCalculator calculator)
    {
        _calculator = calculator;
    }

    [HttpGet("daily")]
    public ActionResult<PanchangData> GetDailyPanchang([FromQuery] string? date = null, [FromQuery] string location = "New Delhi, India")
    {
        DateTime targetDate = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var parsed))
        {
            targetDate = parsed;
        }

        var data = _calculator.CalculateForDate(targetDate, location);
        return Ok(data);
    }
}
