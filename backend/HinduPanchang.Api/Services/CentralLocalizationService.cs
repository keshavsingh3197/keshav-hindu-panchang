using System.Collections.Concurrent;
using HinduPanchang.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HinduPanchang.Api.Services;

public sealed class PanchangLocalizationConfig
{
    public string Id { get; set; } = "default";
    public bool OverrideCentralLocalization { get; set; } = false;
    public string DefaultLocale { get; set; } = "hi";
    public List<string> EnabledLocales { get; set; } = ["hi", "en"];
    public bool AllowVisitorLanguageSwitch { get; set; } = true;
    public bool ShowLanguagePicker { get; set; } = true;
    public bool ForceLocale { get; set; } = false;
    public Dictionary<string, Dictionary<string, string>> LocalOverrides { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public string? LastUpdatedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed record PanchangLocaleDto(
    string Code,
    string NativeName,
    string EnglishName,
    string Direction = "ltr",
    string Icon = "",
    bool IsDefault = false);

public sealed record EffectiveLocalizationPolicyDto(
    string SiteKey,
    string DefaultLocale,
    IReadOnlyList<string> EnabledLocales,
    bool AllowVisitorLanguageSwitch,
    bool ShowLanguagePicker,
    bool ForceLocale,
    string ControlledBy,
    bool OverrideCentralLocalization);

public sealed record LocalizationManifestResponse(
    EffectiveLocalizationPolicyDto Policy,
    IReadOnlyList<PanchangLocaleDto> Locales,
    IReadOnlyDictionary<string, string> Versions,
    string Source);

public sealed record LocalizationBundleResponse(
    string Locale,
    string Version,
    EffectiveLocalizationPolicyDto Policy,
    IReadOnlyDictionary<string, string> Entries,
    string Source);

public sealed record SiteLocalizationAdminStateResponse(
    EffectiveLocalizationPolicyDto CentralAdminPolicy,
    PanchangLocalizationConfig LocalSettings,
    EffectiveLocalizationPolicyDto EffectivePolicy);

public sealed record UpdateSiteLocalizationRequest(
    bool OverrideCentralLocalization,
    string DefaultLocale,
    IReadOnlyList<string>? EnabledLocales,
    bool AllowVisitorLanguageSwitch,
    bool ShowLanguagePicker,
    bool ForceLocale,
    Dictionary<string, Dictionary<string, string>>? LocalOverrides = null);

/// <summary>
/// Direct AdminDb MongoDB localization + ecosystem config reader for Hindu Panchang (default: Hindi),
/// supporting central Admin control plus optional per-site override.
/// </summary>
public sealed class CentralLocalizationService
{
    private const string SiteKey = "panchang";
    private const string SettingsCollection = "localization_settings";
    private static readonly string[] Namespaces = ["ecosystem", "common", "panchang"];
    private static readonly ConcurrentDictionary<string, MongoClient> Clients = new(StringComparer.Ordinal);

    private readonly HybridDataStore _store;
    private readonly string? _centralConnectionString;
    private readonly string _centralDbName;
    private readonly ILogger<CentralLocalizationService> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private CentralSnapshot? _cached;
    private DateTime _cacheExpiresAtUtc = DateTime.MinValue;

    public CentralLocalizationService(
        HybridDataStore store,
        IConfiguration configuration,
        ILogger<CentralLocalizationService> logger)
    {
        _store = store;
        _centralConnectionString = FirstNonEmpty(
            configuration["CentralMongo:ConnectionString"],
            Environment.GetEnvironmentVariable("CentralMongo__ConnectionString"),
            configuration["Mongo:ConnectionString"],
            Environment.GetEnvironmentVariable("Mongo__ConnectionString"),
            configuration["MongoDbSettings:ConnectionString"],
            Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING"));
        _centralDbName = FirstNonEmpty(
            configuration["CentralMongo:DatabaseName"],
            Environment.GetEnvironmentVariable("CentralMongo__DatabaseName")) ?? "AdminDb";
        _logger = logger;
    }

    public async Task<LocalizationManifestResponse> GetManifestAsync(CancellationToken ct = default)
    {
        var snap = await GetSnapshotAsync(ct);
        var local = await GetLocalConfigAsync();
        var policy = ResolvePolicy(snap.CentralPolicy, local);
        var locales = FilterLocales(snap.Locales, policy);
        var versions = locales.ToDictionary(l => l.Code, l => $"{l.Code}:{snap.LoadedAtUtc.Ticks:x}:{local.UpdatedAtUtc.Ticks:x}", StringComparer.OrdinalIgnoreCase);
        return new LocalizationManifestResponse(policy, locales, versions, snap.LoadedFromCentralMongo ? "mongodb-central" : "builtin-fallback");
    }

    public async Task<LocalizationBundleResponse> GetBundleAsync(string? requestedLocale, CancellationToken ct = default)
    {
        var snap = await GetSnapshotAsync(ct);
        var local = await GetLocalConfigAsync();
        var policy = ResolvePolicy(snap.CentralPolicy, local);

        var requested = NormalizeLocale(requestedLocale, policy.DefaultLocale);
        var effectiveLocale = policy.ForceLocale
            ? policy.DefaultLocale
            : (policy.EnabledLocales.Contains(requested, StringComparer.OrdinalIgnoreCase)
                ? requested
                : policy.DefaultLocale);

        var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        ApplyEntries(merged, BuiltInFallback(effectiveLocale));
        if (snap.TranslationsByLocale.TryGetValue("en", out var centralEn))
            ApplyEntries(merged, centralEn);
        if (snap.TranslationsByLocale.TryGetValue(effectiveLocale, out var centralTarget))
            ApplyEntries(merged, centralTarget);
        if (local.OverrideCentralLocalization && local.LocalOverrides.TryGetValue(effectiveLocale, out var localMap))
            ApplyEntries(merged, localMap);

        var version = $"{effectiveLocale}:{snap.LoadedAtUtc.Ticks:x}:{local.UpdatedAtUtc.Ticks:x}";
        return new LocalizationBundleResponse(effectiveLocale, version, policy, merged, snap.LoadedFromCentralMongo ? "mongodb-central" : "builtin-fallback");
    }

    public async Task<SiteLocalizationAdminStateResponse> GetAdminStateAsync(CancellationToken ct = default)
    {
        var snap = await GetSnapshotAsync(ct);
        var local = await GetLocalConfigAsync();
        return new SiteLocalizationAdminStateResponse(snap.CentralPolicy, local, ResolvePolicy(snap.CentralPolicy, local));
    }

    public async Task<SiteLocalizationAdminStateResponse> UpdateLocalSettingsAsync(
        UpdateSiteLocalizationRequest req, string adminUserId, CancellationToken ct = default)
    {
        var existing = await GetLocalConfigAsync();
        var defaultLocale = NormalizeLocale(req.DefaultLocale, "hi");
        var enabled = NormalizeEnabledLocales(req.EnabledLocales, defaultLocale);

        var updated = new PanchangLocalizationConfig
        {
            Id = "default",
            OverrideCentralLocalization = req.OverrideCentralLocalization,
            DefaultLocale = defaultLocale,
            EnabledLocales = enabled.ToList(),
            AllowVisitorLanguageSwitch = req.AllowVisitorLanguageSwitch,
            ShowLanguagePicker = req.ShowLanguagePicker,
            ForceLocale = req.ForceLocale,
            LocalOverrides = req.LocalOverrides ?? existing.LocalOverrides,
            LastUpdatedByUserId = adminUserId,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        await _store.SaveSingletonAsync(SettingsCollection, "default", updated);
        InvalidateCache();
        return await GetAdminStateAsync(ct);
    }

    public async Task<CentralConfigEnvelope?> TryLoadCentralEnvelopeFromMongoAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_centralConnectionString))
            return null;

        try
        {
            var client = Clients.GetOrAdd(_centralConnectionString, cs => new MongoClient(cs));
            var db = client.GetDatabase(_centralDbName);

            var siteDocs = await db.GetCollection<BsonDocument>("websites")
                .Find(Builders<BsonDocument>.Filter.Eq("IsEnabled", true))
                .ToListAsync(ct);

            var channelDocs = await db.GetCollection<BsonDocument>("creator_channels")
                .Find(Builders<BsonDocument>.Filter.Eq("IsEnabled", true))
                .ToListAsync(ct);

            var cfgDocs = await db.GetCollection<BsonDocument>("config_entries")
                .Find(Builders<BsonDocument>.Filter.Ne("IsSecret", true))
                .ToListAsync(ct);

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var d in cfgDocs)
            {
                var k = d.GetValue("Key", "").AsString;
                var val = d.TryGetValue("Value", out var v) && v.IsString && !string.IsNullOrWhiteSpace(v.AsString)
                    ? v.AsString
                    : d.GetValue("DefaultValue", "").AsString;
                if (!string.IsNullOrWhiteSpace(k))
                    values[k] = val;
            }

            return new CentralConfigEnvelope
            {
                SiteTitle = "Keshav Ecosystem",
                Values = values,
                Websites = siteDocs.Select(d => new CentralWebsiteConfig
                {
                    Key = d.GetValue("Key", "").AsString,
                    Name = d.GetValue("Name", "").AsString,
                    ShortLabel = d.GetValue("Name", "").AsString,
                    Url = d.GetValue("Url", "").AsString,
                    Description = d.GetValue("Description", "").AsString,
                    Icon = d.GetValue("Icon", "🌐").AsString,
                    ShowInEcosystemBar = !d.TryGetValue("ShowInEcosystemBar", out var sb) || !sb.IsBoolean || sb.AsBoolean,
                }).ToList(),
                Channels = channelDocs.Select(d => new CentralChannelConfig
                {
                    Id = d.TryGetValue("_id", out var id) ? id.ToString() ?? string.Empty : string.Empty,
                    Key = d.GetValue("Key", "").AsString,
                    Name = d.GetValue("Name", "").AsString,
                    Handle = d.GetValue("Handle", "").AsString,
                    Platform = d.GetValue("Platform", "YouTube").AsString,
                    ChannelId = d.GetValue("ChannelId", "").AsString,
                    Url = d.GetValue("Url", "").AsString,
                    Niche = d.GetValue("Niche", "").AsString,
                    Tagline = d.GetValue("Tagline", "").AsString,
                    ShowInAnalytics = d.TryGetValue("ShowInAnalytics", out var sa) && sa.IsBoolean && sa.AsBoolean,
                    ShowInPanchang = d.TryGetValue("ShowInPanchang", out var sp) && sp.IsBoolean && sp.AsBoolean,
                    ShowInStudio = d.TryGetValue("ShowInStudio", out var ss) && ss.IsBoolean && ss.AsBoolean,
                    ShowInFooter = d.TryGetValue("ShowInFooter", out var sf) && sf.IsBoolean && sf.AsBoolean,
                    IsPrimary = d.TryGetValue("IsPrimary", out var ip) && ip.IsBoolean && ip.AsBoolean,
                    IsEnabled = !d.TryGetValue("IsEnabled", out var ie) || !ie.IsBoolean || ie.AsBoolean,
                    SortOrder = d.TryGetValue("SortOrder", out var so) && so.IsInt32 ? so.AsInt32 : 10,
                }).ToList(),
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Direct AdminDb envelope read failed; HTTP fallback may be used.");
            return null;
        }
    }

    public static EffectiveLocalizationPolicyDto ResolvePolicy(
        EffectiveLocalizationPolicyDto central,
        PanchangLocalizationConfig local)
    {
        if (!local.OverrideCentralLocalization)
            return central with { ControlledBy = "admin", OverrideCentralLocalization = false };

        var def = NormalizeLocale(local.DefaultLocale, central.DefaultLocale);
        var enabled = NormalizeEnabledLocales(local.EnabledLocales, def);
        return new EffectiveLocalizationPolicyDto(
            SiteKey,
            def,
            enabled,
            local.AllowVisitorLanguageSwitch,
            local.ShowLanguagePicker,
            local.ForceLocale,
            "site-override",
            true);
    }

    public void InvalidateCache() => _cacheExpiresAtUtc = DateTime.MinValue;

    private Task<PanchangLocalizationConfig> GetLocalConfigAsync() =>
        _store.GetSingletonAsync(SettingsCollection, "default", () => new PanchangLocalizationConfig());

    private async Task<CentralSnapshot> GetSnapshotAsync(CancellationToken ct)
    {
        if (_cached is not null && DateTime.UtcNow < _cacheExpiresAtUtc)
            return _cached;

        await _gate.WaitAsync(ct);
        try
        {
            if (_cached is not null && DateTime.UtcNow < _cacheExpiresAtUtc)
                return _cached;

            var snap = await LoadFromCentralMongoAsync(ct);
            _cached = snap;
            _cacheExpiresAtUtc = DateTime.UtcNow.AddSeconds(60);
            return snap;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<CentralSnapshot> LoadFromCentralMongoAsync(CancellationToken ct)
    {
        var fallbackPolicy = new EffectiveLocalizationPolicyDto(SiteKey, "hi", ["hi", "en"], true, true, false, "admin", false);
        var fallbackLocales = DefaultLocales();
        if (string.IsNullOrWhiteSpace(_centralConnectionString))
            return new CentralSnapshot(fallbackPolicy, fallbackLocales, new(StringComparer.OrdinalIgnoreCase), false, DateTime.UtcNow);

        try
        {
            var client = Clients.GetOrAdd(_centralConnectionString, cs => new MongoClient(cs));
            var db = client.GetDatabase(_centralDbName);

            var siteDoc = await db.GetCollection<BsonDocument>("websites")
                .Find(Builders<BsonDocument>.Filter.Eq("Key", SiteKey))
                .FirstOrDefaultAsync(ct);

            var policy = fallbackPolicy;
            if (siteDoc is not null)
            {
                var def = NormalizeLocale(siteDoc.TryGetValue("DefaultLocale", out var dl) && dl.IsString ? dl.AsString : "hi", "hi");
                var enList = siteDoc.TryGetValue("EnabledLocales", out var el) && el.IsBsonArray
                    ? el.AsBsonArray.Select(v => v.IsString ? v.AsString : string.Empty)
                    : ["hi", "en"];
                policy = new EffectiveLocalizationPolicyDto(
                    SiteKey,
                    def,
                    NormalizeEnabledLocales(enList, def),
                    !siteDoc.TryGetValue("AllowVisitorLanguageSwitch", out var avs) || !avs.IsBoolean || avs.AsBoolean,
                    !siteDoc.TryGetValue("ShowLanguagePicker", out var slp) || !slp.IsBoolean || slp.AsBoolean,
                    siteDoc.TryGetValue("ForceLocale", out var fl) && fl.IsBoolean && fl.AsBoolean,
                    "admin",
                    false);
            }

            var localeDocs = await db.GetCollection<BsonDocument>("locales")
                .Find(Builders<BsonDocument>.Filter.Eq("IsEnabled", true))
                .ToListAsync(ct);

            var locales = localeDocs.Count > 0
                ? localeDocs.Select(d => new PanchangLocaleDto(
                    NormalizeLocale(d.GetValue("Code", "hi").AsString, "hi"),
                    d.GetValue("NativeName", "English").AsString,
                    d.GetValue("EnglishName", "English").AsString,
                    d.GetValue("Direction", "ltr").AsString,
                    d.GetValue("Icon", string.Empty).AsString,
                    d.TryGetValue("IsDefault", out var idf) && idf.IsBoolean && idf.AsBoolean)).ToList()
                : fallbackLocales;

            var rows = await db.GetCollection<BsonDocument>("translations")
                .Find(Builders<BsonDocument>.Filter.In("Namespace", Namespaces))
                .ToListAsync(ct);

            var byLocale = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                var loc = NormalizeLocale(row.GetValue("Locale", "").AsString, string.Empty);
                var ns = row.GetValue("Namespace", "").AsString;
                var key = row.GetValue("Key", "").AsString;
                var val = row.GetValue("Value", "").AsString;
                if (string.IsNullOrWhiteSpace(loc) || string.IsNullOrWhiteSpace(ns) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(val))
                    continue;
                if (!byLocale.TryGetValue(loc, out var map))
                {
                    map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    byLocale[loc] = map;
                }
                map[$"{ns}.{key}"] = val;
            }

            return new CentralSnapshot(policy, locales, byLocale, true, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to read Hindu Panchang localization directly from AdminDb; using built-in Hindi default fallback.");
            return new CentralSnapshot(fallbackPolicy, fallbackLocales, new(StringComparer.OrdinalIgnoreCase), false, DateTime.UtcNow);
        }
    }

    private static IReadOnlyList<PanchangLocaleDto> FilterLocales(
        IReadOnlyList<PanchangLocaleDto> all, EffectiveLocalizationPolicyDto policy)
    {
        var list = all.Where(l => policy.EnabledLocales.Contains(l.Code, StringComparer.OrdinalIgnoreCase)).ToList();
        if (list.Count == 0)
            list = DefaultLocales().Where(l => policy.EnabledLocales.Contains(l.Code, StringComparer.OrdinalIgnoreCase)).ToList();
        return list.Select(l => l with { IsDefault = string.Equals(l.Code, policy.DefaultLocale, StringComparison.OrdinalIgnoreCase) }).ToList();
    }

    private static void ApplyEntries(IDictionary<string, string> target, IReadOnlyDictionary<string, string> source)
    {
        foreach (var kv in source)
        {
            if (!string.IsNullOrWhiteSpace(kv.Value))
                target[kv.Key] = kv.Value;
        }
    }

    public static string NormalizeLocale(string? locale, string fallback = "hi")
    {
        var raw = (locale ?? string.Empty).Trim().ToLowerInvariant();
        if (raw.StartsWith("hi", StringComparison.Ordinal)) return "hi";
        if (raw.StartsWith("en", StringComparison.Ordinal)) return "en";
        return string.IsNullOrWhiteSpace(raw) ? fallback : raw;
    }

    public static IReadOnlyList<string> NormalizeEnabledLocales(IEnumerable<string>? locales, string defaultLocale)
    {
        var list = new List<string>();
        if (locales is not null)
        {
            foreach (var raw in locales)
            {
                var norm = NormalizeLocale(raw, string.Empty);
                if (!string.IsNullOrWhiteSpace(norm) && !list.Contains(norm, StringComparer.OrdinalIgnoreCase))
                    list.Add(norm);
            }
        }
        if (list.Count == 0)
        {
            list.Add("hi");
            list.Add("en");
        }
        var def = NormalizeLocale(defaultLocale, "hi");
        if (!list.Contains(def, StringComparer.OrdinalIgnoreCase))
            list.Insert(0, def);
        return list;
    }

    private static List<PanchangLocaleDto> DefaultLocales() =>
    [
        new("hi", "हिन्दी", "Hindi", "ltr", "🇮🇳", true),
        new("en", "English", "English", "ltr", "🌐", false),
    ];

    private static Dictionary<string, string> BuiltInFallback(string locale) =>
        string.Equals(locale, "en", StringComparison.OrdinalIgnoreCase)
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["panchang.nav.home"] = "Panchang",
                ["panchang.nav.aarti"] = "Aarti & Chalisa",
                ["panchang.nav.vrat"] = "Vrat & Festivals",
                ["panchang.nav.videoHub"] = "3D Katha Hub",
                ["panchang.nav.itihaas"] = "Aaj Ka Itihaas",
                ["panchang.hero.title"] = "Daily Vedic Panchang & Shubh Muhurat",
                ["panchang.hero.subtitle"] = "Live sunrise-accurate Tithi, Nakshatra, Rahu Kaal, Choghadiya and devotional 3D Kathas.",
                ["panchang.videoHub.title"] = "Daily Spiritual Videos & 3D Katha Hub",
                ["panchang.card.tithi"] = "Tithi",
                ["panchang.card.nakshatra"] = "Nakshatra",
                ["panchang.card.rahuKaal"] = "Rahu Kaal",
                ["panchang.card.abhijit"] = "Abhijit Muhurat",
                ["panchang.footer.tagline"] = "Sanatan Panchang, Shubh Muhurat & Spiritual 3D Katha",
                ["ecosystem.language.label"] = "Language",
            }
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["panchang.nav.home"] = "पंचांग",
                ["panchang.nav.aarti"] = "आरती और चालीसा",
                ["panchang.nav.vrat"] = "व्रत एवं त्यौहार",
                ["panchang.nav.videoHub"] = "३डी कथा हब",
                ["panchang.nav.itihaas"] = "आज का इतिहास",
                ["panchang.hero.title"] = "दैनिक वैदिक पंचांग एवं शुभ मुहूर्त",
                ["panchang.hero.subtitle"] = "सूर्योदय आधारित सटीक तिथि, नक्षत्र, राहुकाल, चौघड़िया एवं भक्तिमय ३डी कथाएँ।",
                ["panchang.videoHub.title"] = "दैनिक आध्यात्मिक वीडियो एवं ३डी कथा हब",
                ["panchang.card.tithi"] = "तिथि",
                ["panchang.card.nakshatra"] = "नक्षत्र",
                ["panchang.card.rahuKaal"] = "राहुकाल",
                ["panchang.card.abhijit"] = "अभिजीत मुहूर्त",
                ["panchang.footer.tagline"] = "सनातन पंचांग, शुभ मुहूर्त एवं आध्यात्मिक ३डी कथा",
                ["ecosystem.language.label"] = "भाषा",
            };

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim();

    private sealed record CentralSnapshot(
        EffectiveLocalizationPolicyDto CentralPolicy,
        IReadOnlyList<PanchangLocaleDto> Locales,
        Dictionary<string, Dictionary<string, string>> TranslationsByLocale,
        bool LoadedFromCentralMongo,
        DateTime LoadedAtUtc);
}
