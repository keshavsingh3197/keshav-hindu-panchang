using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HinduPanchang.Api.Models;
using HinduPanchang.Api.Services;

namespace HinduPanchang.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class PanchangAdminController : ControllerBase
{
    private readonly VedicPanchangCalculator _calculator;
    private readonly CulturalContentService _culturalService;
    private readonly VideoCurationService _videoService;
    private readonly CentralEcosystemSyncService _syncService;
    private readonly IntegrationConfigService _integrationConfig;

    public PanchangAdminController(
        VedicPanchangCalculator calculator,
        CulturalContentService culturalService,
        VideoCurationService videoService,
        CentralEcosystemSyncService syncService,
        IntegrationConfigService integrationConfig)
    {
        _calculator = calculator;
        _culturalService = culturalService;
        _videoService = videoService;
        _syncService = syncService;
        _integrationConfig = integrationConfig;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var videos = await _videoService.GetVideosAsync();
        var festivals = await _culturalService.GetUpcomingFestivalsAsync();
        var incidents = await _culturalService.GetAllIncidentsAsync();
        var cities = await _calculator.GetCitiesAsync(includeInactive: true);
        return Ok(new
        {
            storageMode = _culturalService.StorageMode,
            videoCount = videos.Count,
            festivalCount = festivals.Count,
            incidentCount = incidents.Count,
            cityCount = cities.Count,
            adminUser = User.FindFirst("email")?.Value ?? User.Identity?.Name ?? "Admin"
        });
    }

    // ---- Videos CRUD ----

    [HttpGet("videos")]
    public async Task<ActionResult<List<SpiritualVideo>>> GetVideos() =>
        Ok(await _videoService.GetVideosAsync());

    [HttpPost("videos")]
    public async Task<ActionResult<SpiritualVideo>> CreateVideo([FromBody] SpiritualVideo video) =>
        Ok(await _videoService.AddOrUpdateVideoAsync(video));

    [HttpPut("videos/{id}")]
    public async Task<ActionResult<SpiritualVideo>> UpdateVideo(string id, [FromBody] SpiritualVideo video)
    {
        video.Id = id;
        return Ok(await _videoService.AddOrUpdateVideoAsync(video));
    }

    [HttpDelete("videos/{id}")]
    public async Task<IActionResult> DeleteVideo(string id) =>
        await _videoService.DeleteVideoAsync(id) ? NoContent() : NotFound();

    // ---- Festivals CRUD ----

    [HttpGet("festivals")]
    public async Task<ActionResult<List<FestivalItem>>> GetFestivals() =>
        Ok(await _culturalService.GetUpcomingFestivalsAsync());

    [HttpPost("festivals")]
    public async Task<ActionResult<FestivalItem>> CreateFestival([FromBody] FestivalItem festival) =>
        Ok(await _culturalService.UpsertFestivalAsync(festival));

    [HttpPut("festivals/{id}")]
    public async Task<ActionResult<FestivalItem>> UpdateFestival(string id, [FromBody] FestivalItem festival)
    {
        festival.Id = id;
        return Ok(await _culturalService.UpsertFestivalAsync(festival));
    }

    [HttpDelete("festivals/{id}")]
    public async Task<IActionResult> DeleteFestival(string id) =>
        await _culturalService.DeleteFestivalAsync(id) ? NoContent() : NotFound();

    // ---- Devotional Kits CRUD ----

    [HttpGet("kits")]
    public async Task<ActionResult<List<DevotionalKitItem>>> GetKits() =>
        Ok(await _culturalService.GetDevotionalKitsAsync(includeInactive: true));

    [HttpPost("kits")]
    public async Task<ActionResult<DevotionalKitItem>> CreateKit([FromBody] DevotionalKitItem kit) =>
        Ok(await _culturalService.UpsertDevotionalKitAsync(kit));

    [HttpPut("kits/{id}")]
    public async Task<ActionResult<DevotionalKitItem>> UpdateKit(string id, [FromBody] DevotionalKitItem kit)
    {
        kit.Id = id;
        return Ok(await _culturalService.UpsertDevotionalKitAsync(kit));
    }

    [HttpDelete("kits/{id}")]
    public async Task<IActionResult> DeleteKit(string id) =>
        await _culturalService.DeleteDevotionalKitAsync(id) ? NoContent() : NotFound();

    // ---- Historical Incidents CRUD ----

    [HttpGet("incidents")]
    public async Task<ActionResult<List<HistoricalIncident>>> GetIncidents() =>
        Ok(await _culturalService.GetAllIncidentsAsync());

    [HttpPost("incidents")]
    public async Task<ActionResult<HistoricalIncident>> CreateIncident([FromBody] HistoricalIncident incident) =>
        Ok(await _culturalService.UpsertIncidentAsync(incident));

    [HttpPut("incidents/{id}")]
    public async Task<ActionResult<HistoricalIncident>> UpdateIncident(string id, [FromBody] HistoricalIncident incident)
    {
        incident.Id = id;
        return Ok(await _culturalService.UpsertIncidentAsync(incident));
    }

    [HttpDelete("incidents/{id}")]
    public async Task<IActionResult> DeleteIncident(string id) =>
        await _culturalService.DeleteIncidentAsync(id) ? NoContent() : NotFound();

    // ---- Daily Kathas CRUD ----

    [HttpGet("kathas")]
    public async Task<ActionResult<List<DailyKatha>>> GetKathas() =>
        Ok(await _culturalService.GetAllKathasAsync());

    [HttpPost("kathas")]
    public async Task<ActionResult<DailyKatha>> CreateKatha([FromBody] DailyKatha katha) =>
        Ok(await _culturalService.UpsertKathaAsync(katha));

    [HttpPut("kathas/{id}")]
    public async Task<ActionResult<DailyKatha>> UpdateKatha(string id, [FromBody] DailyKatha katha)
    {
        katha.Id = id;
        return Ok(await _culturalService.UpsertKathaAsync(katha));
    }

    [HttpDelete("kathas/{id}")]
    public async Task<IActionResult> DeleteKatha(string id) =>
        await _culturalService.DeleteKathaAsync(id) ? NoContent() : NotFound();

    // ---- Daily Sutras CRUD ----

    [HttpGet("sutras")]
    public async Task<ActionResult<List<DailySutra>>> GetSutras() =>
        Ok(await _calculator.GetSutrasAsync());

    [HttpPost("sutras")]
    public async Task<ActionResult<DailySutra>> CreateSutra([FromBody] DailySutra sutra) =>
        Ok(await _calculator.UpsertSutraAsync(sutra));

    [HttpPut("sutras/{id}")]
    public async Task<ActionResult<DailySutra>> UpdateSutra(string id, [FromBody] DailySutra sutra)
    {
        sutra.Id = id;
        return Ok(await _calculator.UpsertSutraAsync(sutra));
    }

    [HttpDelete("sutras/{id}")]
    public async Task<IActionResult> DeleteSutra(string id) =>
        await _calculator.DeleteSutraAsync(id) ? NoContent() : NotFound();

    // ---- Panchang Date Overrides CRUD ----

    [HttpGet("overrides")]
    public async Task<ActionResult<List<PanchangOverride>>> GetOverrides() =>
        Ok(await _calculator.GetOverridesAsync());

    [HttpPost("overrides")]
    public async Task<ActionResult<PanchangOverride>> SaveOverride([FromBody] PanchangOverride ov) =>
        Ok(await _calculator.UpsertOverrideAsync(ov));

    [HttpDelete("overrides/{dateKey}")]
    public async Task<IActionResult> DeleteOverride(string dateKey) =>
        await _calculator.DeleteOverrideAsync(dateKey) ? NoContent() : NotFound();

    // ---- Cities CRUD ----

    [HttpGet("cities")]
    public async Task<ActionResult<List<PanchangCity>>> GetCities() =>
        Ok(await _calculator.GetCitiesAsync(includeInactive: true));

    [HttpPost("cities")]
    public async Task<ActionResult<PanchangCity>> CreateCity([FromBody] PanchangCity city) =>
        Ok(await _calculator.UpsertCityAsync(city));

    [HttpPut("cities/{id}")]
    public async Task<ActionResult<PanchangCity>> UpdateCity(string id, [FromBody] PanchangCity city)
    {
        city.Id = id;
        return Ok(await _calculator.UpsertCityAsync(city));
    }

    [HttpDelete("cities/{id}")]
    public async Task<IActionResult> DeleteCity(string id) =>
        await _calculator.DeleteCityAsync(id) ? NoContent() : NotFound();

    // ---- Site Config ----

    [HttpGet("site-config")]
    public async Task<ActionResult<PanchangSiteConfig>> GetSiteConfig() =>
        Ok(await _culturalService.GetSiteConfigAsync());

    [HttpPut("site-config")]
    public async Task<ActionResult<PanchangSiteConfig>> SaveSiteConfig([FromBody] PanchangSiteConfig config) =>
        Ok(await _culturalService.SaveSiteConfigAsync(config));

    // ---- Central Ecosystem Sync, Live YouTube Feeds & Masked Integrations ----

    [HttpPost("sync/ecosystem")]
    public async Task<ActionResult<PanchangSyncSummary>> SyncEcosystem(CancellationToken ct) =>
        Ok(await _syncService.SyncAllAsync(syncVideos: true, ct));

    [HttpPost("sync/videos")]
    public async Task<ActionResult<PanchangSyncSummary>> SyncVideos(CancellationToken ct) =>
        Ok(await _syncService.SyncAllAsync(syncVideos: true, ct));

    [HttpPost("purge-legacy-videos")]
    public async Task<IActionResult> PurgeLegacyVideos()
    {
        var removed = await _videoService.PurgeLegacyDemoVideosAsync();
        return Ok(new { removed });
    }

    [HttpGet("integrations")]
    public async Task<ActionResult<PanchangIntegrationSettingsView>> GetIntegrations() =>
        Ok(await _integrationConfig.GetMaskedViewAsync());

    [HttpPut("integrations")]
    public async Task<ActionResult<PanchangIntegrationSettingsView>> UpdateIntegrations([FromBody] UpdatePanchangIntegrationsRequest request) =>
        Ok(await _integrationConfig.UpdateAsync(request));

    [HttpGet("export")]
    public async Task<ActionResult<PanchangExportBundle>> ExportAll()
    {
        var bundle = new PanchangExportBundle
        {
            ExportedAtUtc = DateTime.UtcNow,
            SiteConfig = await _culturalService.GetSiteConfigAsync(),
            Videos = await _videoService.GetVideosAsync(),
            Festivals = await _culturalService.GetUpcomingFestivalsAsync(),
            Kits = await _culturalService.GetDevotionalKitsAsync(includeInactive: true),
            Incidents = await _culturalService.GetAllIncidentsAsync(),
            Kathas = await _culturalService.GetAllKathasAsync(),
            Sutras = await _calculator.GetSutrasAsync()
        };
        return Ok(bundle);
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportBundle([FromBody] PanchangExportBundle bundle)
    {
        int imported = 0;
        if (bundle.SiteConfig != null && !string.IsNullOrWhiteSpace(bundle.SiteConfig.SiteNameHindi))
        {
            await _culturalService.SaveSiteConfigAsync(bundle.SiteConfig);
            imported++;
        }
        foreach (var v in bundle.Videos ?? [])
        {
            if (!VideoCurationService.IsLegacyDemoVideo(v))
            {
                await _videoService.AddOrUpdateVideoAsync(v);
                imported++;
            }
        }
        foreach (var f in bundle.Festivals ?? []) { await _culturalService.UpsertFestivalAsync(f); imported++; }
        foreach (var k in bundle.Kits ?? []) { await _culturalService.UpsertDevotionalKitAsync(k); imported++; }
        foreach (var i in bundle.Incidents ?? []) { await _culturalService.UpsertIncidentAsync(i); imported++; }
        foreach (var kt in bundle.Kathas ?? []) { await _culturalService.UpsertKathaAsync(kt); imported++; }
        foreach (var s in bundle.Sutras ?? []) { await _calculator.UpsertSutraAsync(s); imported++; }

        return Ok(new { imported });
    }
}
