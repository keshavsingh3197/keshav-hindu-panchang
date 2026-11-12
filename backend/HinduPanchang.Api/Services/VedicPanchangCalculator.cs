using HinduPanchang.Api.Models;

namespace HinduPanchang.Api.Services;

public class VedicPanchangCalculator
{
    private const string CitiesCollection = "cities";
    private const string SutrasCollection = "daily_sutras";
    private const string OverridesCollection = "panchang_overrides";

    private static readonly string[] TithiNames =
    [
        "Pratipada", "Dwitiya", "Tritiya", "Chaturthi", "Panchami",
        "Shashthi", "Saptami", "Ashtami", "Navami", "Dashami",
        "Ekadashi", "Dwadashi", "Trayodashi", "Chaturdashi", "Purnima"
    ];

    private static readonly string[] Nakshatras =
    [
        "Ashwini", "Bharani", "Krittika", "Rohini", "Mrigashira", "Ardra",
        "Punarvasu", "Pushya", "Ashlesha", "Magha", "Purva Phalguni", "Uttara Phalguni",
        "Hasta", "Chitra", "Swati", "Vishakha", "Anuradha", "Jyeshtha",
        "Mula", "Purva Ashadha", "Uttara Ashadha", "Shravana", "Dhanishta",
        "Shatabhisha", "Purva Bhadrapada", "Uttara Bhadrapada", "Revati"
    ];

    private static readonly string[] Yogas =
    [
        "Vishkambha", "Priti", "Ayushman", "Saubhagya", "Shobhana", "Atiganda",
        "Sukarma", "Dhriti", "Shula", "Ganda", "Vriddhi", "Dhruva", "Vyaghata",
        "Harshana", "Vajra", "Siddhi", "Vyatipata", "Variyan", "Parigha", "Shiva",
        "Siddha", "Sadhya", "Shubha", "Shukla", "Brahma", "Indra", "Vaidhriti"
    ];

    private static readonly string[] MovableKaranas =
    [
        "Bava", "Balava", "Kaulava", "Taitila", "Gara", "Vanija", "Vishti (Bhadra)"
    ];

    private static readonly string[] Rashis =
    [
        "Mesha (Aries)", "Vrishabha (Taurus)", "Mithuna (Gemini)", "Karka (Cancer)",
        "Simha (Leo)", "Kanya (Virgo)", "Tula (Libra)", "Vrischika (Scorpio)",
        "Dhanu (Sagittarius)", "Makara (Capricorn)", "Kumbha (Aquarius)", "Meena (Pisces)"
    ];

    private static readonly string[] LunarMonths =
    [
        "Chaitra", "Vaishakha", "Jyeshtha", "Ashadha", "Shravana", "Bhadrapada",
        "Ashvina", "Kartika", "Margashirsha", "Pausha", "Magha", "Phalguna"
    ];

    private static readonly string[] VaraNames =
    [
        "Ravivara", "Somavara", "Mangalavara", "Budhavara", "Guruvara", "Shukravara", "Shanivara"
    ];

    // Choghadiya standard cyclic sequences (7 types):
    // Day cycle index: 0=Udveg, 1=Char, 2=Labh, 3=Amrit, 4=Kaal, 5=Shubh, 6=Rog (advance +1 each slot)
    private static readonly (string Name, string Type)[] DayChoghadiyaCycle =
    [
        ("Udveg (Travel / Caution)", "Ashubh"),
        ("Char (Dynamic / Travel)", "Shubh"),
        ("Labh (Profit / Start)", "Shubh"),
        ("Amrit (Nectar / Best)", "Shubh"),
        ("Kaal (Loss / Avoid)", "Ashubh"),
        ("Shubh (Auspicious)", "Shubh"),
        ("Rog (Health Caution)", "Ashubh")
    ];

    // Start index in DayChoghadiyaCycle for Day (Sun..Sat) and Night (Sun..Sat)
    private static readonly int[] DayStartByWeekday = [0, 3, 6, 2, 5, 1, 4];
    private static readonly int[] NightStartByWeekday = [5, 1, 4, 0, 3, 6, 2];

    // 1/8th daytime slot index (0..7) per DayOfWeek (Sun=0..Sat=6)
    private static readonly int[] RahuSlot = [7, 1, 6, 4, 5, 3, 2];
    private static readonly int[] YamagandaSlot = [4, 3, 2, 1, 0, 6, 5];
    private static readonly int[] GulikaSlot = [6, 5, 4, 3, 2, 1, 0];

    private readonly HybridDataStore _store;

    public VedicPanchangCalculator(HybridDataStore store)
    {
        _store = store;
    }

    public async Task<List<PanchangCity>> GetCitiesAsync(bool includeInactive = false)
    {
        var list = await _store.GetListAsync(CitiesCollection, PanchangSeedData.DefaultCities);
        return (includeInactive ? list : list.Where(c => c.IsActive)).ToList();
    }

    public async Task<PanchangCity> UpsertCityAsync(PanchangCity city)
    {
        if (string.IsNullOrWhiteSpace(city.Id))
            city.Id = $"city-{Guid.NewGuid():N}"[..10];
        return await _store.UpsertItemAsync(CitiesCollection, city.Id, c => c.Id, city, PanchangSeedData.DefaultCities);
    }

    public Task<bool> DeleteCityAsync(string id) =>
        _store.DeleteItemAsync(CitiesCollection, id, c => c.Id, PanchangSeedData.DefaultCities);

    public Task<List<DailySutra>> GetSutrasAsync() =>
        _store.GetListAsync(SutrasCollection, PanchangSeedData.DefaultSutras);

    public async Task<DailySutra> UpsertSutraAsync(DailySutra sutra)
    {
        if (string.IsNullOrWhiteSpace(sutra.Id))
            sutra.Id = $"sut-{Guid.NewGuid():N}"[..10];
        return await _store.UpsertItemAsync(SutrasCollection, sutra.Id, s => s.Id, sutra, PanchangSeedData.DefaultSutras);
    }

    public Task<bool> DeleteSutraAsync(string id) =>
        _store.DeleteItemAsync(SutrasCollection, id, s => s.Id, PanchangSeedData.DefaultSutras);

    public Task<List<PanchangOverride>> GetOverridesAsync() =>
        _store.GetListAsync<PanchangOverride>(OverridesCollection, () => []);

    public Task<PanchangOverride> UpsertOverrideAsync(PanchangOverride ov)
    {
        ov.Id = ov.DateKey;
        return _store.UpsertItemAsync<PanchangOverride>(OverridesCollection, ov.DateKey, x => x.DateKey, ov, () => []);
    }

    public Task<bool> DeleteOverrideAsync(string dateKey) =>
        _store.DeleteItemAsync<PanchangOverride>(OverridesCollection, dateKey, x => x.DateKey, () => []);

    public async Task<PanchangData> CalculateForDateAsync(DateTime date, string location = "New Delhi, India")
    {
        var dateOnly = date.Date;
        var cities = await GetCitiesAsync(includeInactive: true);
        var city = cities.FirstOrDefault(c => c.Name.Equals(location, StringComparison.OrdinalIgnoreCase))
            ?? cities.FirstOrDefault(c => location.Contains(c.Name.Split(',')[0], StringComparison.OrdinalIgnoreCase))
            ?? cities.FirstOrDefault()
            ?? new PanchangCity { Name = location, Latitude = 28.6139, Longitude = 77.2090, TimezoneOffsetHours = 5.5 };

        var sutras = await GetSutrasAsync();
        var overrides = await GetOverridesAsync();
        var dateKey = dateOnly.ToString("yyyy-MM-dd");
        var ov = overrides.FirstOrDefault(o => o.DateKey == dateKey);

        // ---- 1. Solar Ephemeris (Sunrise / Sunset for exact date & city coordinates) ----
        var (sunriseMins, sunsetMins) = ComputeSunriseSunsetMinutes(dateOnly, city.Latitude, city.Longitude, city.TimezoneOffsetHours);
        double dayLen = Math.Max(480.0, sunsetMins - sunriseMins);
        double nightLen = 1440.0 - dayLen;
        double solarNoon = (sunriseMins + sunsetMins) / 2.0;

        int weekday = (int)dateOnly.DayOfWeek;
        double slotDay = dayLen / 8.0;
        double slotNight = nightLen / 8.0;

        // ---- 2. Day & Night Choghadiya dynamically timed ----
        var dayChoghadiya = new List<ChoghadiyaPeriod>(8);
        int dayStart = DayStartByWeekday[weekday];
        for (int i = 0; i < 8; i++)
        {
            var entry = DayChoghadiyaCycle[(dayStart + i) % 7];
            dayChoghadiya.Add(new ChoghadiyaPeriod
            {
                Name = entry.Name,
                Type = entry.Type,
                TimeRange = FormatRange(sunriseMins + i * slotDay, sunriseMins + (i + 1) * slotDay)
            });
        }

        var nightChoghadiya = new List<ChoghadiyaPeriod>(8);
        int nightStart = NightStartByWeekday[weekday];
        for (int i = 0; i < 8; i++)
        {
            var entry = DayChoghadiyaCycle[(nightStart + i * 5) % 7]; // Vedic night sequence steps by 5 mod 7
            nightChoghadiya.Add(new ChoghadiyaPeriod
            {
                Name = entry.Name,
                Type = entry.Type,
                TimeRange = FormatRange(sunsetMins + i * slotNight, sunsetMins + (i + 1) * slotNight)
            });
        }

        // ---- 3. Astronomical Vedic Pancha-Anga from J2000.0 (at local sunrise) ----
        double utcSunriseHour = (sunriseMins / 60.0) - city.TimezoneOffsetHours;
        double jd = ToJulianDay(dateOnly) + (utcSunriseHour / 24.0);
        double d = jd - 2451545.0; // Days since J2000.0

        double sunMeanLong = NormalizeDeg(280.460 + 0.9856474 * d);
        double sunAnomaly = NormalizeDeg(357.528 + 0.9856003 * d) * (Math.PI / 180.0);
        double sunEcliptic = NormalizeDeg(sunMeanLong + 1.915 * Math.Sin(sunAnomaly) + 0.020 * Math.Sin(2 * sunAnomaly));

        double moonMeanLong = NormalizeDeg(218.316 + 13.176396 * d);
        double moonAnomaly = NormalizeDeg(134.963 + 13.064993 * d) * (Math.PI / 180.0);
        double moonDist = NormalizeDeg(93.272 + 13.229350 * d) * (Math.PI / 180.0);
        double moonEcliptic = NormalizeDeg(
            moonMeanLong
            + 6.289 * Math.Sin(moonAnomaly)
            + 1.274 * Math.Sin(2 * moonDist - moonAnomaly)
            + 0.658 * Math.Sin(2 * moonDist));

        // Lahiri Ayanamsa (~24.22 deg in 2026, precessing 50.29"/year)
        double ayanamsa = 23.85 + 0.01397 * (d / 365.25);
        double sunNirayana = NormalizeDeg(sunEcliptic - ayanamsa);
        double moonNirayana = NormalizeDeg(moonEcliptic - ayanamsa);

        double elongation = NormalizeDeg(moonNirayana - sunNirayana);
        int tithiIdx = (int)Math.Floor(elongation / 12.0); // 0..29
        string paksha = tithiIdx < 15 ? "Shukla Paksha" : "Krishna Paksha";
        string tithiLabel = (tithiIdx == 14) ? "Purnima" : (tithiIdx == 29 ? "Amavasya" : TithiNames[tithiIdx % 15]);
        string tithi = $"{paksha.Split(' ')[0]} {tithiLabel}";
        double tithiFractionLeft = 1.0 - ((elongation % 12.0) / 12.0);
        double tithiEndMins = sunriseMins + (tithiFractionLeft * 1416.0);

        int nakshatraIdx = (int)Math.Floor(moonNirayana / (360.0 / 27.0)) % 27;
        double nakFractionLeft = 1.0 - ((moonNirayana % (360.0 / 27.0)) / (360.0 / 27.0));
        double nakEndMins = sunriseMins + (nakFractionLeft * 1440.0);

        int yogaIdx = (int)Math.Floor(NormalizeDeg(sunNirayana + moonNirayana) / (360.0 / 27.0)) % 27;
        int halfTithi = (int)Math.Floor(elongation / 6.0); // 0..59
        string karana = ComputeKarana(halfTithi);

        int sunRashiIdx = (int)Math.Floor(sunNirayana / 30.0) % 12;
        int moonRashiIdx = (int)Math.Floor(moonNirayana / 30.0) % 12;

        // Approximate Hindu Lunar Month & Samvat years
        int solarMonthOffset = sunRashiIdx; // 0=Mesha (April -> Vaishakha)
        int purnimantaIdx = (solarMonthOffset + 1 + (tithiIdx >= 15 ? 1 : 0)) % 12;
        int amantaIdx = (solarMonthOffset + 1) % 12;
        int samvatAdjustment = (dateOnly.Month < 4 || (dateOnly.Month == 4 && dateOnly.Day < 10)) ? 56 : 57;
        int vikramSamvat = dateOnly.Year + samvatAdjustment;
        int shakaSamvat = dateOnly.Year - (samvatAdjustment == 56 ? 79 : 78);

        // Approximate Moonrise/Moonset dynamically shifted by lunar phase (~49m per tithi)
        double moonriseMins = (sunriseMins + (tithiIdx * 48.8)) % 1440.0;
        double moonsetMins = (moonriseMins + 710.0) % 1440.0;

        var selectedSutra = sutras.Count > 0
            ? sutras[Math.Abs(dateOnly.DayOfYear + dateOnly.Year) % sutras.Count]
            : new DailySutra
            {
                SanskritText = "कर्मण्येवाधिकारस्ते मा फलेषु कदाचन ।",
                EnglishMeaning = "You have a right to perform your prescribed duty, but you are not entitled to the fruits of actions."
            };

        return new PanchangData
        {
            Date = dateOnly,
            FormattedDate = dateOnly.ToString("dddd, MMMM dd, yyyy"),
            Location = city.Name,
            VikramSamvat = $"{vikramSamvat} ({SolarSamvatsaraName(vikramSamvat)})",
            ShakaSamvat = $"{shakaSamvat}",
            MonthPurnimanta = LunarMonths[purnimantaIdx],
            MonthAmanta = LunarMonths[amantaIdx],
            Paksha = paksha,
            DayOfWeek = dateOnly.DayOfWeek.ToString(),
            Tithi = string.IsNullOrWhiteSpace(ov?.CustomTithi) ? tithi : ov.CustomTithi,
            TithiEndTime = $"Until {FormatTimeOfDay(tithiEndMins)}",
            Nakshatra = string.IsNullOrWhiteSpace(ov?.CustomNakshatra) ? Nakshatras[nakshatraIdx] : ov.CustomNakshatra,
            NakshatraEndTime = $"Until {FormatTimeOfDay(nakEndMins)}",
            Yoga = Yogas[yogaIdx],
            YogaEndTime = $"Active on {dateOnly:MMM dd}",
            Karana = karana,
            KaranaEndTime = $"Until {FormatTimeOfDay(tithiEndMins)}",
            Vaara = $"{VaraNames[weekday]} ({dateOnly.DayOfWeek})",
            Sunrise = FormatTimeOfDay(sunriseMins),
            Sunset = FormatTimeOfDay(sunsetMins),
            Moonrise = FormatTimeOfDay(moonriseMins),
            Moonset = FormatTimeOfDay(moonsetMins),
            SunSign = Rashis[sunRashiIdx],
            MoonSign = Rashis[moonRashiIdx],
            AbhijitMuhurat = weekday == 3 ? "Avoided (Wednesday)" : FormatRange(solarNoon - 24.0, solarNoon + 24.0),
            BrahmaMuhurat = FormatRange(sunriseMins - 96.0, sunriseMins - 48.0),
            AmritKaal = FormatRange(sunriseMins + dayLen * 0.56, sunriseMins + dayLen * 0.68),
            RahuKaal = FormatSlot(sunriseMins, slotDay, RahuSlot[weekday]),
            Yamaganda = FormatSlot(sunriseMins, slotDay, YamagandaSlot[weekday]),
            GulikaKaal = FormatSlot(sunriseMins, slotDay, GulikaSlot[weekday]),
            DayChoghadiya = dayChoghadiya,
            NightChoghadiya = nightChoghadiya,
            DailySanskritSutra = string.IsNullOrWhiteSpace(ov?.CustomSutra) ? selectedSutra.SanskritText : ov.CustomSutra,
            DailySutraMeaning = !string.IsNullOrWhiteSpace(ov?.SpecialVratOrEvent)
                ? $"Special Observance: {ov.SpecialVratOrEvent}"
                : string.IsNullOrWhiteSpace(ov?.CustomSutraMeaning)
                    ? $"{selectedSutra.EnglishMeaning} ({selectedSutra.Source})"
                    : ov.CustomSutraMeaning
        };
    }

    private static (double SunriseMins, double SunsetMins) ComputeSunriseSunsetMinutes(
        DateTime date,
        double latitudeDeg,
        double longitudeDeg,
        double tzOffsetHours)
    {
        int n = date.DayOfYear;
        double gamma = (2.0 * Math.PI / 365.0) * (n - 1);

        double eqTime = 229.18 * (
            0.000075
            + 0.001868 * Math.Cos(gamma)
            - 0.032077 * Math.Sin(gamma)
            - 0.014615 * Math.Cos(2 * gamma)
            - 0.040849 * Math.Sin(2 * gamma));

        double decl = 0.006918
            - 0.399912 * Math.Cos(gamma)
            + 0.070257 * Math.Sin(gamma)
            - 0.006758 * Math.Cos(2 * gamma)
            + 0.000907 * Math.Sin(2 * gamma)
            - 0.002697 * Math.Cos(3 * gamma)
            + 0.001480 * Math.Sin(3 * gamma);

        double latRad = latitudeDeg * (Math.PI / 180.0);
        double zenithRad = 90.833 * (Math.PI / 180.0);
        double cosHa = (Math.Cos(zenithRad) / (Math.Cos(latRad) * Math.Cos(decl))) - (Math.Tan(latRad) * Math.Tan(decl));
        cosHa = Math.Clamp(cosHa, -0.98, 0.98);
        double haDeg = Math.Acos(cosHa) * (180.0 / Math.PI);

        double solarNoonUtcMins = 720.0 - (4.0 * longitudeDeg) - eqTime;
        double tzMins = tzOffsetHours * 60.0;
        double sunrise = solarNoonUtcMins - (4.0 * haDeg) + tzMins;
        double sunset = solarNoonUtcMins + (4.0 * haDeg) + tzMins;

        return (sunrise, sunset);
    }

    private static double ToJulianDay(DateTime date)
    {
        int y = date.Year;
        int m = date.Month;
        if (m <= 2) { y--; m += 12; }
        int a = y / 100;
        int b = 2 - a + (a / 4);
        return Math.Floor(365.25 * (y + 4716)) + Math.Floor(30.6001 * (m + 1)) + date.Day + b - 1524.5;
    }

    private static double NormalizeDeg(double deg)
    {
        double res = deg % 360.0;
        return res < 0 ? res + 360.0 : res;
    }

    private static string ComputeKarana(int halfTithi) => halfTithi switch
    {
        0 => "Kimstughna",
        57 => "Shakuni",
        58 => "Chatushpada",
        59 => "Nagava",
        _ => MovableKaranas[(halfTithi - 1) % 7]
    };

    private static string SolarSamvatsaraName(int samvat)
    {
        string[] sample = ["Pingala", "Kalayukta", "Siddharthi", "Raudra", "Durmati", "Dundubhi"];
        return sample[Math.Abs(samvat) % sample.Length];
    }

    private static string FormatSlot(double sunriseMins, double slotDuration, int slotIndex) =>
        FormatRange(sunriseMins + slotIndex * slotDuration, sunriseMins + (slotIndex + 1) * slotDuration);

    private static string FormatRange(double startMins, double endMins) =>
        $"{FormatTimeOfDay(startMins)} - {FormatTimeOfDay(endMins)}";

    private static string FormatTimeOfDay(double totalMinutes)
    {
        int rounded = ((int)Math.Round(totalMinutes) % 1440 + 1440) % 1440;
        int hours24 = rounded / 60;
        int mins = rounded % 60;
        string suffix = hours24 >= 12 ? "PM" : "AM";
        int hours12 = hours24 % 12;
        if (hours12 == 0) hours12 = 12;
        return $"{hours12:D2}:{mins:D2} {suffix}";
    }
}
