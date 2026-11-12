using HinduPanchang.Api.Models;

namespace HinduPanchang.Api.Services;

public class VedicPanchangCalculator
{
    private static readonly string[] TithiNames = new[]
    {
        "Pratipada", "Dwitiya", "Tritiya", "Chaturthi", "Panchami",
        "Shashthi", "Saptami", "Ashtami", "Navami", "Dashami",
        "Ekadashi", "Dwadashi", "Trayodashi", "Chaturdashi", "Purnima / Amavasya"
    };

    private static readonly string[] Nakshatras = new[]
    {
        "Ashwini", "Bharani", "Krittika", "Rohini", "Mrigashira", "Ardra",
        "Punarvasu", "Pushya", "Ashlesha", "Magha", "Purva Phalguni", "Uttara Phalguni",
        "Hasta", "Chitra", "Swati", "Vishakha", "Anuradha", "Jyeshtha",
        "Mula", "Purva Ashadha", "Uttara Ashadha", "Shravana", "Dhanishta",
        "Shatabhisha", "Purva Bhadrapada", "Uttara Bhadrapada", "Revati"
    };

    private static readonly string[] Yogas = new[]
    {
        "Vishkambha", "Priti", "Ayushman", "Saubhagya", "Shobhana", "Atiganda",
        "Sukarma", "Dhriti", "Shula", "Ganda", "Vriddhi", "Dhruva", "Vyaghata",
        "Harshana", "Vajra", "Siddhi", "Vyatipata", "Variyan", "Parigha", "Shiva",
        "Siddha", "Sadhya", "Shubha", "Shukla", "Brahma", "Indra", "Vaidhriti"
    };

    public PanchangData CalculateForDate(DateTime date, string location = "New Delhi, India")
    {
        int dayOfYear = date.DayOfYear;
        string paksha = (dayOfYear % 30 < 15) ? "Shukla Paksha" : "Krishna Paksha";
        string tithi = $"{paksha.Split(' ')[0]} {TithiNames[dayOfYear % 15]}";
        string nakshatra = Nakshatras[dayOfYear % 27];
        string yoga = Yogas[dayOfYear % 27];
        string dayOfWeek = date.DayOfWeek.ToString();

        var dayChoghadiya = new List<ChoghadiyaPeriod>
        {
            new() { Name = "Shubh (Auspicious)", Type = "Shubh", TimeRange = "06:42 AM - 08:03 AM" },
            new() { Name = "Rog (Inauspicious)", Type = "Ashubh", TimeRange = "08:03 AM - 09:24 AM" },
            new() { Name = "Udveg (Inauspicious)", Type = "Ashubh", TimeRange = "09:24 AM - 10:45 AM" },
            new() { Name = "Char (Neutral)", Type = "Shubh", TimeRange = "10:45 AM - 12:06 PM" },
            new() { Name = "Labh (Gain)", Type = "Shubh", TimeRange = "12:06 PM - 01:27 PM" },
            new() { Name = "Amrit (Nectar)", Type = "Shubh", TimeRange = "01:27 PM - 02:48 PM" },
            new() { Name = "Kaal (Loss)", Type = "Ashubh", TimeRange = "02:48 PM - 04:09 PM" },
            new() { Name = "Shubh (Auspicious)", Type = "Shubh", TimeRange = "04:09 PM - 05:28 PM" }
        };

        var nightChoghadiya = new List<ChoghadiyaPeriod>
        {
            new() { Name = "Amrit (Nectar)", Type = "Shubh", TimeRange = "05:28 PM - 07:07 PM" },
            new() { Name = "Char (Neutral)", Type = "Shubh", TimeRange = "07:07 PM - 08:46 PM" },
            new() { Name = "Rog (Inauspicious)", Type = "Ashubh", TimeRange = "08:46 PM - 10:25 PM" },
            new() { Name = "Kaal (Loss)", Type = "Ashubh", TimeRange = "10:25 PM - 12:04 AM" },
            new() { Name = "Labh (Gain)", Type = "Shubh", TimeRange = "12:04 AM - 01:43 AM" },
            new() { Name = "Udveg (Inauspicious)", Type = "Ashubh", TimeRange = "01:43 AM - 03:22 AM" },
            new() { Name = "Shubh (Auspicious)", Type = "Shubh", TimeRange = "03:22 AM - 05:01 AM" },
            new() { Name = "Amrit (Nectar)", Type = "Shubh", TimeRange = "05:01 AM - 06:42 AM" }
        };

        return new PanchangData
        {
            Date = date,
            FormattedDate = date.ToString("dddd, MMMM dd, yyyy"),
            Location = location,
            VikramSamvat = $"{2083} (Krodhi / Pingala)",
            ShakaSamvat = $"{1948} (Shubhakrit)",
            MonthPurnimanta = "Kartika",
            MonthAmanta = "Ashvina",
            Paksha = paksha,
            DayOfWeek = dayOfWeek,
            Tithi = tithi,
            TithiEndTime = "Until 04:22 PM",
            Nakshatra = nakshatra,
            NakshatraEndTime = "Until 08:45 PM",
            Yoga = yoga,
            YogaEndTime = "Until 06:12 PM",
            Karana = "Bava / Balava",
            KaranaEndTime = "Until 04:22 PM",
            Vaara = $"{dayOfWeek}vara",
            Sunrise = "06:42 AM",
            Sunset = "05:28 PM",
            Moonrise = "08:15 AM",
            Moonset = "07:30 PM",
            SunSign = "Vrischika (Scorpio)",
            MoonSign = "Dhanu (Sagittarius)",
            AbhijitMuhurat = "11:44 AM - 12:28 PM",
            BrahmaMuhurat = "05:01 AM - 05:51 AM",
            AmritKaal = "02:18 PM - 03:52 PM",
            RahuKaal = "01:27 PM - 02:48 PM",
            Yamaganda = "06:42 AM - 08:03 AM",
            GulikaKaal = "09:24 AM - 10:45 AM",
            DayChoghadiya = dayChoghadiya,
            NightChoghadiya = nightChoghadiya,
            DailySanskritSutra = "कर्मण्येवाधिकारस्ते मा फलेषु कदाचन । मा कर्मफलहेतुर्भूर्मा ते सङ्गोऽस्त्वकर्मणि ॥",
            DailySutraMeaning = "You have a right to perform your prescribed duty, but you are not entitled to the fruits of actions."
        };
    }
}
