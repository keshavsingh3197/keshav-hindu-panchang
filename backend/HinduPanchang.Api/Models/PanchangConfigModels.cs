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
    public string BroadcastChannelName { get; set; } = "Keshav Singh Official";
    public string BroadcastChannelUrl { get; set; } = "https://www.youtube.com/@KeshavSingh";
    public string DailyBroadcastBanner { get; set; } = "Watch Daily Rashifal, Ekadashi Katha & Cultural Documentaries on our YouTube Channel.";
    public List<string> VideoCategories { get; set; } =
    [
        "Daily News & Rashifal",
        "Pauranik Katha",
        "Mantra & Meditation",
        "Temple Darshan"
    ];
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
