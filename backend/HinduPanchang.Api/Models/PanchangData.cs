namespace HinduPanchang.Api.Models;

public class PanchangData
{
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string FormattedDate { get; set; } = string.Empty;
    public string Location { get; set; } = "New Delhi, India";
    public string VikramSamvat { get; set; } = "2083 (Kaal-Yukt)";
    public string ShakaSamvat { get; set; } = "1948 (Krodhi)";
    public string MonthPurnimanta { get; set; } = "Kartika";
    public string MonthAmanta { get; set; } = "Kartika";
    public string Paksha { get; set; } = "Shukla Paksha";
    public string DayOfWeek { get; set; } = "Thursday";

    // 5 Limbs of Panchang (Pancha-Anga)
    public string Tithi { get; set; } = "Shukla Tritiya";
    public string TithiEndTime { get; set; } = "Until 04:12 PM";
    public string Nakshatra { get; set; } = "Mula";
    public string NakshatraEndTime { get; set; } = "Until 08:35 PM";
    public string Yoga { get; set; } = "Atiganda";
    public string YogaEndTime { get; set; } = "Until 06:14 PM";
    public string Karana { get; set; } = "Taitila";
    public string KaranaEndTime { get; set; } = "Until 04:12 PM";
    public string Vaara { get; set; } = "Guruvara (Thursday)";

    // Planetary / Celestial Times
    public string Sunrise { get; set; } = "06:42 AM";
    public string Sunset { get; set; } = "05:28 PM";
    public string Moonrise { get; set; } = "08:15 AM";
    public string Moonset { get; set; } = "07:22 PM";
    public string SunSign { get; set; } = "Tula (Libra)";
    public string MoonSign { get; set; } = "Dhanu (Sagittarius)";

    // Auspicious & Inauspicious Muhurats
    public string AbhijitMuhurat { get; set; } = "11:44 AM - 12:26 PM";
    public string BrahmaMuhurat { get; set; } = "05:01 AM - 05:51 AM";
    public string AmritKaal { get; set; } = "02:15 PM - 03:48 PM";
    public string RahuKaal { get; set; } = "01:26 PM - 02:47 PM";
    public string Yamaganda { get; set; } = "06:42 AM - 08:03 AM";
    public string GulikaKaal { get; set; } = "09:24 AM - 10:45 AM";

    // Choghadiya
    public List<ChoghadiyaPeriod> DayChoghadiya { get; set; } = new();
    public List<ChoghadiyaPeriod> NightChoghadiya { get; set; } = new();

    public string DailySanskritSutra { get; set; } = "सर्वे भवन्तु सुखिनः सर्वे सन्तु निरामयाः ।";
    public string DailySutraMeaning { get; set; } = "May all sentient beings be at peace, may all be free from illness.";
}

public class ChoghadiyaPeriod
{
    public string Name { get; set; } = string.Empty; // Shubh, Labh, Amrit, Char, Rog, Kaal, Udveg
    public string Type { get; set; } = "Shubh"; // Shubh or Ashubh
    public string TimeRange { get; set; } = string.Empty;
}
