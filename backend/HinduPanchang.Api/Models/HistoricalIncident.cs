namespace HinduPanchang.Api.Models;

public class HistoricalIncident
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DateKey { get; set; } = string.Empty; // e.g. "11-12" (Month-Day)
    public int Year { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "Indian History"; // "Vedic Era", "Freedom Struggle", "Spiritual Saint", "Science & Astronomy"
    public string Description { get; set; } = string.Empty;
    public string CulturalSignificance { get; set; } = string.Empty;
    public string? RelatedVideoUrl { get; set; }
    public bool IsExactDateMatch { get; set; } = true;
}

public class DailyKatha
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string DeityOrSage { get; set; } = string.Empty;
    public string StorySummary { get; set; } = string.Empty;
    public string MoralTeaching { get; set; } = string.Empty;
    public string SanskritMantra { get; set; } = string.Empty;
}
