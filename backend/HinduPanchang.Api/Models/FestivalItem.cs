namespace HinduPanchang.Api.Models;

public class FestivalItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string NameHindi { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Tithi { get; set; } = string.Empty;
    public string MonthVedic { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RitualsAndPujaVidhi { get; set; } = string.Empty;
    public string FastingRules { get; set; } = string.Empty;
    public bool IsMajorFestival { get; set; }
}
