using Microsoft.AspNetCore.Mvc;
using HinduPanchang.Api.Models;
using HinduPanchang.Api.Services;

namespace HinduPanchang.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FestivalsController : ControllerBase
{
    private readonly CulturalContentService _contentService;

    public FestivalsController(CulturalContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<List<FestivalItem>>> GetUpcomingFestivals([FromQuery] string? date = null)
    {
        DateTime? anchor = null;
        if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var parsed))
            anchor = parsed.Date;

        var festivals = await _contentService.GetUpcomingFestivalsAsync(anchor);
        return Ok(festivals);
    }

    [HttpGet("kits")]
    public async Task<ActionResult<List<DevotionalKitItem>>> GetDevotionalKits()
    {
        var kits = await _contentService.GetDevotionalKitsAsync();
        return Ok(kits);
    }
}
