using Microsoft.AspNetCore.Mvc;
using HinduPanchang.Api.Models;
using HinduPanchang.Api.Services;

namespace HinduPanchang.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CulturalHistoryController : ControllerBase
{
    private readonly CulturalContentService _contentService;

    public CulturalHistoryController(CulturalContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet("aaj-ka-itihaas")]
    public async Task<ActionResult<List<HistoricalIncident>>> GetAajKaItihaas([FromQuery] string? date = null)
    {
        DateTime targetDate = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var parsed))
        {
            targetDate = parsed;
        }

        var incidents = await _contentService.GetIncidentsForDateAsync(targetDate);
        return Ok(incidents);
    }

    [HttpGet("daily-katha")]
    public async Task<ActionResult<DailyKatha>> GetDailyKatha([FromQuery] string? date = null)
    {
        DateTime targetDate = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var parsed))
        {
            targetDate = parsed;
        }

        var katha = await _contentService.GetDailyKathaAsync(targetDate);
        return Ok(katha);
    }
}
