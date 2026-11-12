using Microsoft.AspNetCore.Mvc;
using HinduPanchang.Api.Models;
using HinduPanchang.Api.Services;

namespace HinduPanchang.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PanchangController : ControllerBase
{
    private readonly VedicPanchangCalculator _calculator;
    private readonly CulturalContentService _culturalService;

    public PanchangController(VedicPanchangCalculator calculator, CulturalContentService culturalService)
    {
        _calculator = calculator;
        _culturalService = culturalService;
    }

    [HttpGet("daily")]
    public async Task<ActionResult<PanchangData>> GetDailyPanchang(
        [FromQuery] string? date = null,
        [FromQuery] string location = "New Delhi, India")
    {
        DateTime targetDate = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var parsed))
            targetDate = parsed;

        var data = await _calculator.CalculateForDateAsync(targetDate, location);
        return Ok(data);
    }

    [HttpGet("cities")]
    public async Task<ActionResult<List<PanchangCity>>> GetCities()
    {
        return Ok(await _calculator.GetCitiesAsync());
    }

    [HttpGet("config")]
    public async Task<ActionResult<PanchangSiteConfig>> GetSiteConfig()
    {
        return Ok(await _culturalService.GetSiteConfigAsync());
    }
}
