using System.Security.Cryptography;
using System.Text;
using HinduPanchang.Api.Models;
using HinduPanchang.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HinduPanchang.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoHubController : ControllerBase
{
    private readonly VideoCurationService _videoService;
    private readonly IConfiguration _config;

    public VideoHubController(VideoCurationService videoService, IConfiguration config)
    {
        _videoService = videoService;
        _config = config;
    }

    [HttpGet("videos")]
    public async Task<ActionResult<List<SpiritualVideo>>> GetVideos(
        [FromQuery] string? category = null,
        [FromQuery] string? date = null,
        [FromQuery] bool allDates = true,
        [FromQuery] string? channel = null)
    {
        var videos = await _videoService.GetVideosAsync(category, date, allDates, channel);
        return Ok(videos);
    }

    [HttpGet("feed")]
    public async Task<ActionResult<VideoFeedResponse>> GetVideoFeed(
        [FromQuery] string? category = null,
        [FromQuery] string? date = null,
        [FromQuery] bool allDates = false,
        [FromQuery] string? channel = null)
    {
        var feed = await _videoService.GetVideoFeedAsync(category, date, allDates, channel);
        return Ok(feed);
    }

    [HttpGet("featured")]
    public async Task<ActionResult<SpiritualVideo>> GetFeaturedVideo()
    {
        var video = await _videoService.GetFeaturedVideoAsync();
        if (video == null) return NotFound();
        return Ok(video);
    }

    [HttpPost("manage")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SpiritualVideo>> AddOrUpdateVideo([FromBody] SpiritualVideo video)
    {
        var result = await _videoService.AddOrUpdateVideoAsync(video);
        return Ok(result);
    }

    /// <summary>
    /// Accepts video registrations from sister ecosystem apps (e.g. AnimStudio Auto-Post).
    /// Requires EITHER an authenticated Admin SSO role OR a constant-time matching
    /// X-Ecosystem-Key header configured in Ecosystem:SyncKey.
    /// </summary>
    [HttpPost("automation-sync")]
    [AllowAnonymous]
    public async Task<ActionResult<SpiritualVideo>> SyncFromAutomation([FromBody] AutomationVideoSyncRequest request)
    {
        if (!IsAuthorizedForAutomationSync())
            return Unauthorized(new { error = "Admin role or valid X-Ecosystem-Key required." });

        var video = VideoCurationService.MapAutomationSyncRequest(request);
        var saved = await _videoService.AddOrUpdateVideoAsync(video);
        return Ok(saved);
    }

    /// <summary>Removes a video that AnimStudio Auto-Post published here. Same auth as <see cref="SyncFromAutomation"/>.</summary>
    [HttpDelete("automation-sync/{automationPostId}")]
    [AllowAnonymous]
    public async Task<IActionResult> RemoveFromAutomation(string automationPostId)
    {
        if (!IsAuthorizedForAutomationSync())
            return Unauthorized(new { error = "Admin role or valid X-Ecosystem-Key required." });
        if (string.IsNullOrWhiteSpace(automationPostId) || automationPostId.Length > 128)
            return BadRequest(new { error = "A valid automation post id is required." });

        var removed = await _videoService.RemoveByAutomationPostIdAsync(automationPostId);
        return removed ? NoContent() : NotFound(new { error = "No Panchang video is linked to that post." });
    }

    private bool IsAuthorizedForAutomationSync()
    {
        if (User?.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            return true;

        var configuredKey = (_config["Ecosystem:SyncKey"] ?? Environment.GetEnvironmentVariable("ECOSYSTEM_SYNC_KEY"))?.Trim();
        if (string.IsNullOrWhiteSpace(configuredKey))
            return false;

        if (!Request.Headers.TryGetValue("X-Ecosystem-Key", out var providedValues))
            return false;

        var provided = providedValues.ToString().Trim();
        var a = Encoding.UTF8.GetBytes(configuredKey);
        var b = Encoding.UTF8.GetBytes(provided);
        return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
    }
}
