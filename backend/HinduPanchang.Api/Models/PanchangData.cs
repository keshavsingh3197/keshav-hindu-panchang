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
    public bool IsAdhikaMaas { get; set; }
    public string Paksha { get; set; } = "Shukla Paksha";
    public string AyanamsaName { get; set; } = "Lahiri (Chitrapaksha)";
    public string CalculationNote { get; set; } = string.Empty;
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

    // Comprehensive Location, Methodology, Extended Timings, Hora, Lagna & Chart Data
    public PanchangLocationMeta LocationMeta { get; set; } = new();
    public PanchangExtendedInfo Extended { get; set; } = new();
    public List<PanchangHoraPeriod> DayHora { get; set; } = new();
    public List<PanchangHoraPeriod> NightHora { get; set; } = new();
    public List<PanchangLagnaPeriod> UdayaLagnas { get; set; } = new();
    public List<PanchangLagnaPeriod> PanchakaRahitaPeriods { get; set; } = new();
    public PanchangTimelineData Timeline { get; set; } = new();
}

public class PanchangLocationMeta
{
    public string CityId { get; set; } = "city-del";
    public string DisplayName { get; set; } = "New Delhi, India";
    public string State { get; set; } = "Delhi";
    public string Country { get; set; } = "India";
    public double Latitude { get; set; } = 28.6139;
    public double Longitude { get; set; } = 77.2090;
    public double ElevationMeters { get; set; } = 0;
    public string TimeZoneId { get; set; } = "Asia/Kolkata";
    public double EffectiveUtcOffsetHours { get; set; } = 5.5;
    public bool IsDaylightSavingActive { get; set; }
    public string SunriseMethod { get; set; } = "Upper Limb (Edges) with -50' refraction, 0 m elevation";
}

public class PanchangExtendedInfo
{
    public double LahiriAyanamsaDeg { get; set; }
    public string PurnimantaDayBanner { get; set; } = string.Empty;
    public string AmantaDayBanner { get; set; } = string.Empty;
    public string Dinamana { get; set; } = string.Empty;
    public string Ratrimana { get; set; } = string.Empty;
    public string Madhyahna { get; set; } = string.Empty;
    public string CivilDawn { get; set; } = string.Empty;
    public string NauticalDawn { get; set; } = string.Empty;
    public string AstronomicalDawn { get; set; } = string.Empty;
    public string Ritu { get; set; } = string.Empty;
    public string VedicRitu { get; set; } = string.Empty;
    public string Ayana { get; set; } = string.Empty;
    public string KaliyugaYear { get; set; } = string.Empty;
    public string KaliAhargana { get; set; } = string.Empty;
    public string JulianDay { get; set; } = string.Empty;
    public string ModifiedJulianDay { get; set; } = string.Empty;
    public string NationalCivilDate { get; set; } = string.Empty;

    // Transitions & Percentages
    public double TithiPercentElapsedAtSunrise { get; set; }
    public double NakshatraPercentElapsedAtSunrise { get; set; }
    public double YogaPercentElapsedAtSunrise { get; set; }
    public string TithiSwami { get; set; } = string.Empty;
    public string NakshatraSwami { get; set; } = string.Empty;
    public string YogaSwami { get; set; } = string.Empty;
    public string SuryaNakshatra { get; set; } = string.Empty;
    public string NextTithi { get; set; } = string.Empty;
    public string NextNakshatra { get; set; } = string.Empty;
    public string NextYoga { get; set; } = string.Empty;
    public string NextKarana { get; set; } = string.Empty;
    public List<PanchangTransitionItem> TithiTransitions { get; set; } = new();
    public List<PanchangTransitionItem> NakshatraTransitions { get; set; } = new();
    public List<PanchangTransitionItem> YogaTransitions { get; set; } = new();
    public List<PanchangTransitionItem> KaranaTransitions { get; set; } = new();
    public List<PanchangTransitionItem> NakshatraPadas { get; set; } = new();
    public List<PanchangTransitionItem> MoonSignTransitions { get; set; } = new();

    // Extended Shubh / Ashubh Muhurats
    public string PratahSandhya { get; set; } = string.Empty;
    public string VijayaMuhurat { get; set; } = string.Empty;
    public string GodhuliMuhurat { get; set; } = string.Empty;
    public string SayahnaSandhya { get; set; } = string.Empty;
    public string NishitaMuhurat { get; set; } = string.Empty;
    public string RaviYoga { get; set; } = string.Empty;
    public string Varjyam { get; set; } = string.Empty;
    public string DurMuhurtam { get; set; } = string.Empty;
    public string PanchakStatus { get; set; } = string.Empty;
    public string Baana { get; set; } = string.Empty;

    // Anandadi Yoga, Nivas & Shool, Tarabalam / Chandrabalam
    public string AnandadiYoga { get; set; } = string.Empty;
    public string TamilYoga { get; set; } = string.Empty;
    public string DishaShool { get; set; } = string.Empty;
    public string NakshatraShool { get; set; } = string.Empty;
    public string Agnivasa { get; set; } = string.Empty;
    public string Chandravasa { get; set; } = string.Empty;
    public string Shivavasa { get; set; } = string.Empty;
    public string RahuVasa { get; set; } = string.Empty;
    public string Homahuti { get; set; } = string.Empty;
    public List<string> ChandrabalamGoodRashis { get; set; } = new();
    public List<string> TarabalamGoodNakshatras { get; set; } = new();

    // Raw timing offsets (minutes from midnight 00:00 of Panchang Date; >= 1440 means next day)
    // Used by the Angular UI to convert seamlessly between 12h, 24h, 24h+, and Vedic Ghati clocks with zero reload.
    public double SunriseMinutes { get; set; }
    public double SunsetMinutes { get; set; }
    public double NextSunriseMinutes { get; set; }
    public double? MoonriseMinutes { get; set; }
    public double? MoonsetMinutes { get; set; }
    public List<PanchangCalculationCredit> Credits { get; set; } = new();
}

public class PanchangTransitionItem
{
    public string Name { get; set; } = string.Empty;
    public double StartMinutesFromMidnight { get; set; }
    public double EndMinutesFromMidnight { get; set; }
    public string TimeRangeDisplay { get; set; } = string.Empty;
    public string EndDisplay { get; set; } = string.Empty;
    public bool IsFullNight { get; set; }
    public string Note { get; set; } = string.Empty;
}

public class PanchangHoraPeriod
{
    public string Planet { get; set; } = string.Empty;
    public string PlanetSanskrit { get; set; } = string.Empty;
    public string Quality { get; set; } = "Shubh";
    public string BestFor { get; set; } = string.Empty;
    public string TimeRange { get; set; } = string.Empty;
    public double StartMinutes { get; set; }
    public double EndMinutes { get; set; }
}

public class PanchangLagnaPeriod
{
    public string Name { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;
    public string TimeRange { get; set; } = string.Empty;
    public double StartMinutes { get; set; }
    public double EndMinutes { get; set; }
}

public class PanchangTimelineData
{
    public double DayStartMinutes { get; set; }
    public double DayEndMinutes { get; set; }
    public double SunsetMinutes { get; set; }
    public List<PanchangTimelineSegment> TithiBars { get; set; } = new();
    public List<PanchangTimelineSegment> NakshatraBars { get; set; } = new();
    public List<PanchangTimelineSegment> YogaBars { get; set; } = new();
    public List<PanchangTimelineSegment> KaranaBars { get; set; } = new();
    public List<PanchangTimelineSegment> DoshaBands { get; set; } = new();
    public List<PanchangTimelineSegment> AuspiciousBands { get; set; } = new();
}

public class PanchangTimelineSegment
{
    public string Label { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Tone { get; set; } = "neutral"; // shubh | ashubh | neutral
    public double StartMinutes { get; set; }
    public double EndMinutes { get; set; }
    public double StartPercent { get; set; }
    public double WidthPercent { get; set; }
    public string TimeDisplay { get; set; } = string.Empty;
}

public class PanchangCalculationCredit
{
    public string Component { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}

public class ChoghadiyaPeriod
{
    public string Name { get; set; } = string.Empty; // Shubh, Labh, Amrit, Char, Rog, Kaal, Udveg
    public string Type { get; set; } = "Shubh"; // Shubh or Ashubh
    public string TimeRange { get; set; } = string.Empty;
}
