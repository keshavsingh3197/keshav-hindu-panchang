using System.Text.RegularExpressions;
using HinduPanchang.Api.Models;

namespace HinduPanchang.Api.Services;

public class CulturalContentService
{
    private const string IncidentsCollection = "historical_incidents";
    private const string KathasCollection = "daily_kathas";
    private const string FestivalsCollection = "festivals";
    private const string KitsCollection = "devotional_kits";
    private const string SiteConfigCollection = "site_config";

    private readonly HybridDataStore _store;
    private readonly CentralAdminConfigReader? _centralReader;

    public CulturalContentService(HybridDataStore store, CentralAdminConfigReader? centralReader = null)
    {
        _store = store;
        _centralReader = centralReader;
    }

    public string StorageMode => _store.StorageMode;

    // ---- Historical Incidents ----

    public async Task<List<HistoricalIncident>> GetIncidentsForDateAsync(DateTime date)
    {
        var all = await GetAllIncidentsAsync();
        string key = date.ToString("MM-dd");
        var matches = all
            .Where(i => i.DateKey == key)
            .OrderByDescending(i => i.Year)
            .Select(i => CloneIncident(i, exact: true))
            .ToList();
        if (matches.Count > 0) return matches;

        // If no exact MM-dd entry exists, return a deterministic heritage archive selection explicitly flagged as non-exact
        if (all.Count == 0) return [];
        int offset = Math.Abs(date.DayOfYear) % all.Count;
        return all.Skip(offset).Concat(all.Take(offset))
            .Take(Math.Min(4, all.Count))
            .Select(i => CloneIncident(i, exact: false))
            .ToList();
    }

    private static HistoricalIncident CloneIncident(HistoricalIncident src, bool exact) => new()
    {
        Id = src.Id,
        DateKey = src.DateKey,
        Year = src.Year,
        Title = src.Title,
        Category = src.Category,
        Description = src.Description,
        CulturalSignificance = src.CulturalSignificance,
        RelatedVideoUrl = src.RelatedVideoUrl,
        IsExactDateMatch = exact
    };

    public async Task<List<HistoricalIncident>> GetAllIncidentsAsync()
    {
        var list = await _store.GetListAsync(IncidentsCollection, PanchangSeedData.DefaultIncidents);
        return list.OrderBy(i => i.DateKey).ThenByDescending(i => i.Year).ToList();
    }

    public async Task<HistoricalIncident> UpsertIncidentAsync(HistoricalIncident incident)
    {
        if (string.IsNullOrWhiteSpace(incident.Id))
            incident.Id = $"inc-{Guid.NewGuid():N}"[..10];
        return await _store.UpsertItemAsync(IncidentsCollection, incident.Id, x => x.Id, incident, PanchangSeedData.DefaultIncidents);
    }

    public Task<bool> DeleteIncidentAsync(string id) =>
        _store.DeleteItemAsync(IncidentsCollection, id, x => x.Id, PanchangSeedData.DefaultIncidents);

    // ---- Daily Kathas ----

    public Task<List<DailyKatha>> GetAllKathasAsync() =>
        _store.GetListAsync(KathasCollection, PanchangSeedData.DefaultKathas);

    public async Task<DailyKatha> GetDailyKathaAsync(DateTime date)
    {
        var kathas = await GetAllKathasAsync();
        if (kathas.Count == 0)
            return PanchangSeedData.DefaultKathas()[0];

        int idx = Math.Abs(date.DayOfYear + date.Year) % kathas.Count;
        return kathas[idx];
    }

    public async Task<DailyKatha> UpsertKathaAsync(DailyKatha katha)
    {
        if (string.IsNullOrWhiteSpace(katha.Id))
            katha.Id = $"katha-{Guid.NewGuid():N}"[..10];
        return await _store.UpsertItemAsync(KathasCollection, katha.Id, x => x.Id, katha, PanchangSeedData.DefaultKathas);
    }

    public Task<bool> DeleteKathaAsync(string id) =>
        _store.DeleteItemAsync(KathasCollection, id, x => x.Id, PanchangSeedData.DefaultKathas);

    // ---- Festivals (Astronomically Computed + Admin Custom Merges) ----

    public async Task<List<FestivalItem>> GetUpcomingFestivalsAsync()
    {
        var customSaved = await _store.GetListAsync(FestivalsCollection, PanchangSeedData.DefaultFestivals);
        var computed = VedicPanchangCalculator.ComputeDynamicFestivalsForWindow(DateTime.UtcNow.Date.AddDays(-2), 365);

        // Filter out legacy fake f-1..f-4 seed entries from older Mongo deployments that used relative today.AddDays()
        var customClean = customSaved
            .Where(f => !IsLegacyFakeSeedFestival(f))
            .ToList();

        var merged = new List<FestivalItem>(computed);
        foreach (var c in customClean)
        {
            merged.RemoveAll(m => m.Id == c.Id || (m.Date.Date == c.Date.Date && m.NameEnglish.Equals(c.NameEnglish, StringComparison.OrdinalIgnoreCase)));
            merged.Add(c);
        }

        return merged.OrderBy(f => f.Date).ToList();
    }

    private static bool IsLegacyFakeSeedFestival(FestivalItem f) =>
        f.Id is "f-1" or "f-2" or "f-3" or "f-4" && !f.IsDynamicCalculated &&
        (f.Description.StartsWith("Marks the awakening of Lord Vishnu", StringComparison.Ordinal) ||
         f.Description.StartsWith("Sacred ceremonial union of Vrinda", StringComparison.Ordinal) ||
         f.Description.StartsWith("Celebrates Lord Shiva's victory", StringComparison.Ordinal) ||
         f.Description.StartsWith("Commemorates the sacred day Bhagavan", StringComparison.Ordinal));

    public async Task<FestivalItem> UpsertFestivalAsync(FestivalItem festival)
    {
        if (string.IsNullOrWhiteSpace(festival.Id))
            festival.Id = $"f-{Guid.NewGuid():N}"[..10];
        return await _store.UpsertItemAsync(FestivalsCollection, festival.Id, x => x.Id, festival, PanchangSeedData.DefaultFestivals);
    }

    public Task<bool> DeleteFestivalAsync(string id) =>
        _store.DeleteItemAsync(FestivalsCollection, id, x => x.Id, PanchangSeedData.DefaultFestivals);

    // ---- Devotional Kits ----

    public async Task<List<DevotionalKitItem>> GetDevotionalKitsAsync(bool includeInactive = false)
    {
        var list = await _store.GetListAsync(KitsCollection, PanchangSeedData.DefaultDevotionalKits);
        return (includeInactive ? list : list.Where(k => k.IsActive))
            .OrderBy(k => k.SortOrder)
            .ToList();
    }

    public async Task<DevotionalKitItem> UpsertDevotionalKitAsync(DevotionalKitItem kit)
    {
        if (string.IsNullOrWhiteSpace(kit.Id))
            kit.Id = $"kit-{Guid.NewGuid():N}"[..10];
        return await _store.UpsertItemAsync(KitsCollection, kit.Id, x => x.Id, kit, PanchangSeedData.DefaultDevotionalKits);
    }

    public Task<bool> DeleteDevotionalKitAsync(string id) =>
        _store.DeleteItemAsync(KitsCollection, id, x => x.Id, PanchangSeedData.DefaultDevotionalKits);

    // ---- Site Config ----

    public async Task<PanchangSiteConfig> GetSiteConfigAsync()
    {
        var cfg = await _store.GetSingletonAsync(SiteConfigCollection, "default", PanchangSeedData.DefaultSiteConfig);
        cfg.Ads ??= new AdMonetizationConfig();

        var centralId = await ResolveCentralAdSenseClientIdAsync();
        if (!string.IsNullOrEmpty(centralId))
            cfg.Ads.AdSenseClientId = centralId;

        return cfg;
    }

    public async Task<PanchangSiteConfig> SaveSiteConfigAsync(PanchangSiteConfig config)
    {
        config.Id = "default";
        config.UpdatedAt = DateTime.UtcNow;
        config.VideoCategories ??= [];

        var centralId = await ResolveCentralAdSenseClientIdAsync();
        config.Ads = SanitizeAdConfig(config.Ads, centralId);

        var saved = await _store.SaveSingletonAsync(SiteConfigCollection, "default", config);
        if (!string.IsNullOrEmpty(centralId))
            saved.Ads.AdSenseClientId = centralId;

        return saved;
    }

    private async Task<string> ResolveCentralAdSenseClientIdAsync()
    {
        if (_centralReader is null) return string.Empty;
        var snap = await _centralReader.GetSnapshotAsync();
        var id = (snap.AdSenseClientId ?? string.Empty).Trim();
        return Regex.IsMatch(id, @"^ca-pub-\d{10,20}$") ? id : string.Empty;
    }

    private static AdMonetizationConfig SanitizeAdConfig(AdMonetizationConfig? input, string centralAdSenseClientId = "")
    {
        if (input is null) return new AdMonetizationConfig();

        string clientId = (input.AdSenseClientId ?? string.Empty).Trim();
        if (!Regex.IsMatch(clientId, @"^ca-pub-\d{10,20}$"))
            clientId = string.Empty;

        // Avoid duplicating Central Admin's publisher ID in Panchang's local database.
        if (!string.IsNullOrEmpty(centralAdSenseClientId) &&
            string.Equals(clientId, centralAdSenseClientId, StringComparison.OrdinalIgnoreCase))
        {
            clientId = string.Empty;
        }

        static string CleanSlot(string? raw)
        {
            var s = (raw ?? string.Empty).Trim();
            return Regex.IsMatch(s, @"^\d{6,15}$") ? s : string.Empty;
        }

        string sponsorUrl = (input.SponsorUrl ?? string.Empty).Trim();
        if (!Uri.TryCreate(sponsorUrl, UriKind.Absolute, out var parsed) ||
            (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps))
        {
            sponsorUrl = string.Empty;
        }

        bool hasEffectiveClient = !string.IsNullOrEmpty(clientId) || !string.IsNullOrEmpty(centralAdSenseClientId);

        return new AdMonetizationConfig
        {
            AdsEnabled = input.AdsEnabled && hasEffectiveClient,
            AdSenseClientId = clientId,
            HeaderAdSlotId = CleanSlot(input.HeaderAdSlotId),
            InFeedAdSlotId = CleanSlot(input.InFeedAdSlotId),
            SidebarAdSlotId = CleanSlot(input.SidebarAdSlotId),
            FooterAdSlotId = CleanSlot(input.FooterAdSlotId),
            AdLabel = string.IsNullOrWhiteSpace(input.AdLabel) ? "Advertisement" : input.AdLabel.Trim(),
            SponsorBannerEnabled = input.SponsorBannerEnabled && !string.IsNullOrEmpty(sponsorUrl),
            SponsorBadge = string.IsNullOrWhiteSpace(input.SponsorBadge) ? "Featured" : input.SponsorBadge.Trim(),
            SponsorTitle = (input.SponsorTitle ?? string.Empty).Trim(),
            SponsorDescription = (input.SponsorDescription ?? string.Empty).Trim(),
            SponsorCtaText = string.IsNullOrWhiteSpace(input.SponsorCtaText) ? "Learn More" : input.SponsorCtaText.Trim(),
            SponsorUrl = sponsorUrl
        };
    }
}
