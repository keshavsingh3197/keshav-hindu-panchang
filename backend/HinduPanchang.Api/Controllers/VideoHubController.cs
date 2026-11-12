using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HinduPanchang.Api.Models;
using HinduPanchang.Api.Services;

namespace HinduPanchang.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoHubController : ControllerBase
{
    private readonly VideoCurationService _videoService;

    public VideoHubController(VideoCurationService videoService)
    {
        _videoService = videoService;
    }

    [HttpGet("videos")]
    public async Task<ActionResult<List<SpiritualVideo>>> GetVideos([FromQuery] string? category = null)
    {
        var videos = await _videoService.GetVideosAsync(category);
        return Ok(videos);
    }

    [HttpGet("featured")]
    public async Task<ActionResult<SpiritualVideo>> GetFeaturedVideo()
    {
        var video = await _videoService.GetFeaturedVideoAsync();
        if (video == null) return NotFound();
        return Ok(video);
    }

    [HttpPost("manage")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult<SpiritualVideo>> AddOrUpdateVideo([FromBody] SpiritualVideo video)
    {
        var result = await _videoService.AddOrUpdateVideoAsync(video);
        return Ok(result);
    }

    // Public demo fallback endpoint for frontend client updates when running without central IdP in dev
    [HttpPost("demo-add")]
    public async Task<ActionResult<SpiritualVideo>> DemoAddVideo([FromBody] SpiritualVideo video)
    {
        var result = await _videoService.AddOrUpdateVideoAsync(video);
        return Ok(result);
    }
}
