using System.Security.Claims;
using HinduPanchang.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HinduPanchang.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class I18nController : ControllerBase
{
    private readonly CentralLocalizationService _localization;

    public I18nController(CentralLocalizationService localization)
    {
        _localization = localization;
    }

    [HttpGet("i18n/manifest")]
    [AllowAnonymous]
    public async Task<ActionResult<LocalizationManifestResponse>> Manifest(CancellationToken ct)
    {
        Response.Headers.CacheControl = "public, max-age=30";
        return Ok(await _localization.GetManifestAsync(ct));
    }

    [HttpGet("i18n/bundle/{locale}")]
    [AllowAnonymous]
    public async Task<ActionResult<LocalizationBundleResponse>> Bundle(string locale, CancellationToken ct)
    {
        Response.Headers.CacheControl = "public, max-age=30";
        return Ok(await _localization.GetBundleAsync(locale, ct));
    }

    [HttpGet("settings/localization")]
    [AllowAnonymous]
    public async Task<ActionResult<SiteLocalizationAdminStateResponse>> GetSiteLocalization(CancellationToken ct)
    {
        return Ok(await _localization.GetAdminStateAsync(ct));
    }

    [HttpPut("settings/localization")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SiteLocalizationAdminStateResponse>> UpdateSiteLocalization(
        [FromBody] UpdateSiteLocalizationRequest req,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? "admin";
        return Ok(await _localization.UpdateLocalSettingsAsync(req, userId, ct));
    }
}
