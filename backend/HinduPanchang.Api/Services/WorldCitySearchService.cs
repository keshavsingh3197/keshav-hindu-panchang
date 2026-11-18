using System.Collections.Concurrent;
using System.Text.Json;
using HinduPanchang.Api.Models;

namespace HinduPanchang.Api.Services;

/// <summary>
/// Fast worldwide city resolution + automatic IANA timezone / DST offset lookup:
/// merges the embedded offline catalog with the free Open-Meteo Geocoding API (cached 24h)
/// so any city, town, or custom (lat, lon) works with sub-millisecond repeat latency and $0 cost.
/// </summary>
public sealed class WorldCitySearchService
{
    private static readonly ConcurrentDictionary<string, CachedSearch> SearchCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, PanchangCity> ResolvedCities = new(StringComparer.OrdinalIgnoreCase);
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory? _httpFactory;

    public WorldCitySearchService(IHttpClientFactory? httpFactory = null)
    {
        _httpFactory = httpFactory;
    }

    public async Task<IReadOnlyList<PanchangCity>> SearchAsync(
        string? query,
        IReadOnlyCollection<PanchangCity> savedCities,
        int maxResults = 15,
        CancellationToken ct = default)
    {
        var q = (query ?? string.Empty).Trim();
        var merged = new List<PanchangCity>();
        var seenCoords = new HashSet<string>(StringComparer.Ordinal);

        void TryAdd(PanchangCity c)
        {
            var coordKey = $"{c.Latitude:F2},{c.Longitude:F2}";
            if (!seenCoords.Add(coordKey)) return;
            merged.Add(c);
        }

        var localPool = savedCities
            .Concat(WorldCityCatalog.All)
            .ToList();

        if (string.IsNullOrEmpty(q))
        {
            foreach (var c in localPool.Take(maxResults)) TryAdd(c);
            return merged;
        }

        var scoredLocal = localPool
            .Select(c => (City: c, Score: ScoreMatch(c, q)))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.City.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var item in scoredLocal)
            TryAdd(item.City);

        if (q.Length >= 2 && merged.Count < maxResults && _httpFactory is not null)
        {
            var remote = await FetchOpenMeteoCitiesAsync(q, ct).ConfigureAwait(false);
            foreach (var c in remote)
            {
                ResolvedCities[c.Name] = c;
                ResolvedCities[c.Id] = c;
                TryAdd(c);
                if (merged.Count >= maxResults) break;
            }
        }

        return merged.Take(maxResults).ToList();
    }

    public async Task<PanchangCity> ResolveCityAsync(
        string? location,
        IReadOnlyCollection<PanchangCity> savedCities,
        double? lat = null,
        double? lon = null,
        string? tz = null,
        CancellationToken ct = default)
    {
        if (lat is { } customLat && lon is { } customLon)
        {
            var estOffset = Math.Round(customLon / 15.0 * 2.0) / 2.0;
            var tzId = !string.IsNullOrWhiteSpace(tz) ? tz.Trim() : EstimateIanaOrFixedTz(customLat, customLon, estOffset);
            var cleanLabel = !string.IsNullOrWhiteSpace(location)
                ? location.Trim()
                : $"{customLat:F3}°, {customLon:F3}°";

            var (_, usesDst) = ResolveOffsetForDate(tzId, estOffset, DateTime.Today);
            return new PanchangCity
            {
                Id = $"custom-{customLat:F3}-{customLon:F3}",
                Name = cleanLabel,
                LabelHindi = cleanLabel,
                Country = "Custom Coordinates",
                Latitude = Math.Clamp(customLat, -89.9, 89.9),
                Longitude = Math.Clamp(customLon, -180.0, 180.0),
                TimeZoneId = tzId,
                TimezoneOffsetHours = estOffset,
                UsesDst = usesDst,
                IsActive = true
            };
        }

        var q = (location ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(q)) q = "New Delhi, India";

        var savedMatch = MatchCityFromList(savedCities, q);
        if (savedMatch is not null)
            return EnrichWithKnownIanaTz(savedMatch);

        var catalogMatch = MatchCityFromList(WorldCityCatalog.All, q);
        if (catalogMatch is not null)
            return catalogMatch;

        if (ResolvedCities.TryGetValue(q, out var cachedCity))
            return cachedCity;

        if (_httpFactory is not null)
        {
            var token = q.Split(',')[0].Trim();
            var remote = await FetchOpenMeteoCitiesAsync(token, ct).ConfigureAwait(false);
            if (remote.Count > 0)
            {
                var best = MatchCityFromList(remote, q) ?? remote[0];
                ResolvedCities[q] = best;
                return best;
            }
        }

        return EnrichWithKnownIanaTz(savedCities.FirstOrDefault() ?? WorldCityCatalog.All[0]);
    }

    public static (double OffsetHours, bool IsDaylightSaving) ResolveOffsetForDate(
        string? timeZoneId,
        double fallbackOffsetHours,
        DateTime localDate)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            try
            {
                var tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId.Trim());
                var localNoon = new DateTime(localDate.Year, localDate.Month, localDate.Day, 12, 0, 0, DateTimeKind.Unspecified);
                var offset = tzi.GetUtcOffset(localNoon);
                return (offset.TotalHours, tzi.IsDaylightSavingTime(localNoon));
            }
            catch
            {
                // Ignore unrecognized IDs cleanly and fall through to numeric offset.
            }
        }

        return (fallbackOffsetHours, false);
    }

    private async Task<IReadOnlyList<PanchangCity>> FetchOpenMeteoCitiesAsync(string query, CancellationToken ct)
    {
        var key = query.Trim().ToLowerInvariant();
        var now = DateTimeOffset.UtcNow;
        if (SearchCache.TryGetValue(key, out var cached) && cached.ExpiresAtUtc > now)
            return cached.Results;

        try
        {
            using var client = _httpFactory!.CreateClient("WorldCityGeoClient");
            var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(query.Trim())}&count=10&language=en&format=json";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.UserAgent.ParseAdd("HinduPanchangApp/2.0");
            using var res = await client.SendAsync(req, ct).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode) return [];

            await using var stream = await res.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            var payload = await JsonSerializer.DeserializeAsync<OpenMeteoGeoResponse>(stream, JsonOpts, ct).ConfigureAwait(false);
            if (payload?.Results is not { Count: > 0 }) return [];

            var mapped = new List<PanchangCity>(payload.Results.Count);
            foreach (var r in payload.Results)
            {
                if (string.IsNullOrWhiteSpace(r.Name)) continue;
                var statePart = !string.IsNullOrWhiteSpace(r.Admin1) && !r.Admin1.Equals(r.Name, StringComparison.OrdinalIgnoreCase)
                    ? $", {r.Admin1}"
                    : string.Empty;
                var countryPart = !string.IsNullOrWhiteSpace(r.Country) ? $", {r.Country}" : string.Empty;
                var tzId = !string.IsNullOrWhiteSpace(r.Timezone) ? r.Timezone! : "UTC";
                var (offset, dst) = ResolveOffsetForDate(tzId, 0.0, DateTime.UtcNow.Date);

                mapped.Add(new PanchangCity
                {
                    Id = $"geo-{r.Id}",
                    Name = $"{r.Name}{countryPart}",
                    LabelHindi = $"{r.Name}{statePart}{countryPart}",
                    State = r.Admin1 ?? string.Empty,
                    Country = r.Country ?? string.Empty,
                    Latitude = Math.Round(r.Latitude, 5),
                    Longitude = Math.Round(r.Longitude, 5),
                    ElevationMeters = Math.Round(r.Elevation ?? 0, 1),
                    TimeZoneId = tzId,
                    TimezoneOffsetHours = offset,
                    UsesDst = dst,
                    IsActive = true
                });
            }

            SearchCache[key] = new CachedSearch(mapped, now.AddHours(24));
            return mapped;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return cached?.Results ?? [];
        }
    }

    private static PanchangCity? MatchCityFromList(IEnumerable<PanchangCity> pool, string query)
    {
        var list = pool as IList<PanchangCity> ?? pool.ToList();
        var primaryToken = query.Split(',')[0].Trim();

        return list.FirstOrDefault(c => c.Id.Equals(query, StringComparison.OrdinalIgnoreCase))
            ?? list.FirstOrDefault(c => c.Name.Equals(query, StringComparison.OrdinalIgnoreCase))
            ?? list.FirstOrDefault(c => c.Name.Split(',')[0].Trim().Equals(primaryToken, StringComparison.OrdinalIgnoreCase))
            ?? list.FirstOrDefault(c => c.Name.Contains(primaryToken, StringComparison.OrdinalIgnoreCase));
    }

    private static PanchangCity EnrichWithKnownIanaTz(PanchangCity saved)
    {
        var catalog = MatchCityFromList(WorldCityCatalog.All, saved.Name);
        if (catalog is null || !string.IsNullOrWhiteSpace(saved.TimeZoneId) && !string.Equals(saved.TimeZoneId, "Asia/Kolkata", StringComparison.OrdinalIgnoreCase))
            return saved;

        return new PanchangCity
        {
            Id = saved.Id,
            Name = saved.Name,
            LabelHindi = string.IsNullOrWhiteSpace(saved.LabelHindi) ? catalog.LabelHindi : saved.LabelHindi,
            State = string.IsNullOrWhiteSpace(saved.State) ? catalog.State : saved.State,
            Country = string.IsNullOrWhiteSpace(saved.Country) ? catalog.Country : saved.Country,
            Latitude = saved.Latitude,
            Longitude = saved.Longitude,
            ElevationMeters = saved.ElevationMeters != 0 ? saved.ElevationMeters : catalog.ElevationMeters,
            TimeZoneId = catalog.TimeZoneId,
            TimezoneOffsetHours = saved.TimezoneOffsetHours,
            UsesDst = catalog.UsesDst,
            IsActive = saved.IsActive
        };
    }

    private static int ScoreMatch(PanchangCity c, string q)
    {
        var shortName = c.Name.Split(',')[0].Trim();
        if (shortName.Equals(q, StringComparison.OrdinalIgnoreCase) || c.Name.Equals(q, StringComparison.OrdinalIgnoreCase))
            return 100;
        if (shortName.StartsWith(q, StringComparison.OrdinalIgnoreCase))
            return 80;
        if (c.Name.StartsWith(q, StringComparison.OrdinalIgnoreCase))
            return 70;
        if (c.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrWhiteSpace(c.State) && c.State.Contains(q, StringComparison.OrdinalIgnoreCase))
            || c.Country.Contains(q, StringComparison.OrdinalIgnoreCase)
            || c.LabelHindi.Contains(q, StringComparison.OrdinalIgnoreCase))
            return 50;
        return 0;
    }

    private static string EstimateIanaOrFixedTz(double lat, double lon, double offsetHours)
    {
        // Fast India/Nepal bounding heuristic for custom coords in South Asia; otherwise use standard UTC offset string
        if (lat is >= 6.0 and <= 37.5 && lon is >= 68.0 and <= 97.5)
        {
            if (lat is >= 26.3 and <= 30.5 && lon is >= 80.0 and <= 88.2)
                return "Asia/Kathmandu";
            return "Asia/Kolkata";
        }
        return offsetHours == 0 ? "UTC" : $"UTC{(offsetHours >= 0 ? "+" : "")}{offsetHours:0.##}";
    }

    private sealed record CachedSearch(IReadOnlyList<PanchangCity> Results, DateTimeOffset ExpiresAtUtc);

    private sealed record OpenMeteoGeoResponse(List<OpenMeteoGeoItem>? Results);

    private sealed record OpenMeteoGeoItem(
        long Id,
        string? Name,
        double Latitude,
        double Longitude,
        double? Elevation,
        string? Country,
        string? Admin1,
        string? Timezone);
}
