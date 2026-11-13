namespace HinduPanchang.Api.Models;

public class PanchangCity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LabelHindi { get; set; } = string.Empty;
    public string Country { get; set; } = "India";
    public double Latitude { get; set; } = 28.6139;
    public double Longitude { get; set; } = 77.2090;
    public double TimezoneOffsetHours { get; set; } = 5.5;
    public bool IsActive { get; set; } = true;
}

public class DailySutra
{
    public string Id { get; set; } = string.Empty;
    public string SanskritText { get; set; } = string.Empty;
    public string EnglishMeaning { get; set; } = string.Empty;
    public string Source { get; set; } = "Bhagavad Gita";
}

public class PanchangOverride
{
    public string Id { get; set; } = string.Empty; // yyyy-MM-dd
    public string DateKey { get; set; } = string.Empty;
    public string? CustomTithi { get; set; }
    public string? CustomNakshatra { get; set; }
    public string? SpecialVratOrEvent { get; set; }
    public string? CustomSutra { get; set; }
    public string? CustomSutraMeaning { get; set; }
}

public class DevotionalKitItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = "🪔";
    public string Description { get; set; } = string.Empty;
    public string ActionLabel { get; set; } = "Explore Samagri";
    public string ExternalUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 10;
    public bool IsActive { get; set; } = true;
}

public class PanchangSiteConfig
{
    public string Id { get; set; } = "default";
    public string SiteNameHindi { get; set; } = "पंचांगम् & संस्कृति";
    public string SiteSubtitle { get; set; } = "Vedic Panchang & Aaj Ka Itihaas";
    public string DefaultCityName { get; set; } = "New Delhi, India";
    public string BroadcastChannelName { get; set; } = string.Empty;
    public string BroadcastChannelUrl { get; set; } = string.Empty;
    public string DailyBroadcastBanner { get; set; } = "Daily Panchang briefings, Vrat Katha, Mantra meditations & cultural documentaries synced from connected creator channels.";
    public List<string> VideoCategories { get; set; } =
    [
        "Daily News & Rashifal",
        "Pauranik Katha",
        "Mantra & Meditation",
        "Temple Darshan"
    ];
    public AdMonetizationConfig Ads { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class AdMonetizationConfig
{
    public bool AdsEnabled { get; set; }
    public string AdSenseClientId { get; set; } = string.Empty;
    public string HeaderAdSlotId { get; set; } = string.Empty;
    public string InFeedAdSlotId { get; set; } = string.Empty;
    public string SidebarAdSlotId { get; set; } = string.Empty;
    public string FooterAdSlotId { get; set; } = string.Empty;
    public string AdLabel { get; set; } = "Advertisement";
    public bool SponsorBannerEnabled { get; set; }
    public string SponsorBadge { get; set; } = "Featured";
    public string SponsorTitle { get; set; } = string.Empty;
    public string SponsorDescription { get; set; } = string.Empty;
    public string SponsorCtaText { get; set; } = "Learn More";
    public string SponsorUrl { get; set; } = string.Empty;
}

public class PanchangIntegrationSecrets
{
    public string Id { get; set; } = "default";
    public string YouTubeApiKey { get; set; } = string.Empty;
    public string CentralConfigUrl { get; set; } = "https://id.keshavsingh.in/api/config";
    public bool AutoSyncOnStartup { get; set; } = true;
    public DateTime? LastEcosystemSyncUtc { get; set; }
    public DateTime? LastVideoSyncUtc { get; set; }
}

public class PanchangIntegrationSettingsView
{
    public bool HasYouTubeApiKey { get; set; }
    public string MaskedYouTubeApiKey { get; set; } = string.Empty;
    public bool YouTubeKeyManagedByCentral { get; set; }
    public string IntegrationSource { get; set; } = "none";
    public bool CentralAdSenseConfigured { get; set; }
    public string CentralAdSenseClientId { get; set; } = string.Empty;
    public string CentralConfigUrl { get; set; } = "https://id.keshavsingh.in/api/config";
    public bool AutoSyncOnStartup { get; set; } = true;
    public DateTime? LastEcosystemSyncUtc { get; set; }
    public DateTime? LastVideoSyncUtc { get; set; }
}

public class UpdatePanchangIntegrationsRequest
{
    public string? YouTubeApiKey { get; set; }
    public bool ClearYouTubeApiKey { get; set; }
    public string? CentralConfigUrl { get; set; }
    public bool? AutoSyncOnStartup { get; set; }
}

public class CentralConfigEnvelope
{
    public string? SiteTitle { get; set; }
    public Dictionary<string, string>? Values { get; set; }
    public List<CentralWebsiteConfig>? Websites { get; set; }
    public List<CentralChannelConfig>? Channels { get; set; }
}

public class CentralWebsiteConfig
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ShortLabel { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool ShowInEcosystemBar { get; set; }
}

public class CentralChannelConfig
{
    public string Id { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Handle { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Niche { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public bool ShowInAnalytics { get; set; }
    public bool ShowInPanchang { get; set; }
    public bool ShowInStudio { get; set; }
    public bool ShowInFooter { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsEnabled { get; set; }
    public int SortOrder { get; set; }
}

public class PanchangSyncSummary
{
    public int ChannelsSynced { get; set; }
    public int VideosSynced { get; set; }
    public int LegacyVideosPurged { get; set; }
    public int KitsUpdated { get; set; }
    public bool SiteConfigUpdated { get; set; }
    public List<string> Warnings { get; set; } = [];
    public DateTime SyncedAtUtc { get; set; } = DateTime.UtcNow;
}

public class PanchangExportBundle
{
    public DateTime ExportedAtUtc { get; set; } = DateTime.UtcNow;
    public PanchangSiteConfig SiteConfig { get; set; } = new();
    public List<SpiritualVideo> Videos { get; set; } = [];
    public List<FestivalItem> Festivals { get; set; } = [];
    public List<DevotionalKitItem> Kits { get; set; } = [];
    public List<HistoricalIncident> Incidents { get; set; } = [];
    public List<DailyKatha> Kathas { get; set; } = [];
    public List<DailySutra> Sutras { get; set; } = [];
}
