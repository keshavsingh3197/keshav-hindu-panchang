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

    public CulturalContentService(HybridDataStore store)
    {
        _store = store;
    }

    public string StorageMode => _store.StorageMode;

    // ---- Historical Incidents ----

    public async Task<List<HistoricalIncident>> GetIncidentsForDateAsync(DateTime date)
    {
        var all = await GetAllIncidentsAsync();
        string key = date.ToString("MM-dd");
        var matches = all.Where(i => i.DateKey == key).OrderByDescending(i => i.Year).ToList();
        if (matches.Count > 0) return matches;

        // If no exact MM-dd entry exists yet, rotate deterministically by date instead of identical top 3
        if (all.Count == 0) return [];
        int offset = Math.Abs(date.DayOfYear) % all.Count;
        return all.Skip(offset).Concat(all.Take(offset)).Take(Math.Min(3, all.Count)).ToList();
    }

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

    // ---- Festivals ----

    public async Task<List<FestivalItem>> GetUpcomingFestivalsAsync()
    {
        var list = await _store.GetListAsync(FestivalsCollection, PanchangSeedData.DefaultFestivals);
        return list.OrderBy(f => f.Date).ToList();
    }

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

    public Task<PanchangSiteConfig> GetSiteConfigAsync() =>
        _store.GetSingletonAsync(SiteConfigCollection, "default", PanchangSeedData.DefaultSiteConfig);

    public Task<PanchangSiteConfig> SaveSiteConfigAsync(PanchangSiteConfig config)
    {
        config.Id = "default";
        config.UpdatedAt = DateTime.UtcNow;
        config.VideoCategories ??= [];
        return _store.SaveSingletonAsync(SiteConfigCollection, "default", config);
    }
}
