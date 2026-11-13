using HinduPanchang.Api.Models;

namespace HinduPanchang.Api.Services;

public class VedicPanchangCalculator
{
    private const string CitiesCollection = "cities";
    private const string SutrasCollection = "daily_sutras";
    private const string OverridesCollection = "panchang_overrides";

    private const double Deg2Rad = Math.PI / 180.0;
    private const double Rad2Deg = 180.0 / Math.PI;
    private const double NakshatraSpanDeg = 360.0 / 27.0; // 13°20'
    private const double SynodicMonthDays = 29.530588861;

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

    private static readonly string[] Samvatsara60Cycle =
    [
        "Prabhava", "Vibhava", "Shukla", "Pramoda", "Prajapati",
        "Angirasa", "Shrimukha", "Bhava", "Yuva", "Dhatri",
        "Ishvara", "Bahudhanya", "Pramathi", "Vikrama", "Vrisha",
        "Chitrabhanu", "Svabhanu", "Tarana", "Parthiva", "Vyaya",
        "Sarvajit", "Sarvadharin", "Virodhin", "Vikrita", "Khara",
        "Nandana", "Vijaya", "Jaya", "Manmatha", "Durmukhi",
        "Hevilambi", "Vilambi", "Vikari", "Sharvari", "Plava",
        "Shubhakrit", "Shobhakrit", "Krodhin", "Vishvavasu", "Parabhava",
        "Plavanga", "Kilaka", "Saumya", "Sadharana", "Virodhikrit",
        "Paridhavin", "Pramadin", "Ananda", "Rakshasa", "Nala",
        "Pingala", "Kalayukta", "Siddharthi", "Raudra", "Durmati",
        "Dundubhi", "Rudhirodgarin", "Raktakshi", "Krodhana", "Kshaya"
    ];

    private static readonly string[] VaraNames =
    [
        "Ravivara", "Somavara", "Mangalavara", "Budhavara", "Guruvara", "Shukravara", "Shanivara"
    ];

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

    private static readonly int[] DayStartByWeekday = [0, 3, 6, 2, 5, 1, 4];
    private static readonly int[] NightStartByWeekday = [5, 1, 4, 0, 3, 6, 2];

    private static readonly int[] RahuSlot = [7, 1, 6, 4, 5, 3, 2];
    private static readonly int[] YamagandaSlot = [4, 3, 2, 1, 0, 6, 5];
    private static readonly int[] GulikaSlot = [6, 5, 4, 3, 2, 1, 0];

    private readonly HybridDataStore _store;

    public VedicPanchangCalculator(HybridDataStore store)
    {
        _store = store;
    }

    public readonly record struct AstronomicalAngles(
        double SunTropicalLon,
        double MoonTropicalLon,
        double MoonLatDeg,
        double MoonHorizontalParallaxDeg,
        double ObliquityDeg,
        double AyanamsaDeg,
        double SunNirayana,
        double MoonNirayana);

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

        // 1. Solar Ephemeris (Sunrise / Sunset for exact date & city coordinates)
        var (sunriseMins, sunsetMins) = ComputeSunriseSunsetMinutes(dateOnly, city.Latitude, city.Longitude, city.TimezoneOffsetHours);
        double dayLen = Math.Max(480.0, sunsetMins - sunriseMins);
        double nightLen = 1440.0 - dayLen;
        double solarNoon = (sunriseMins + sunsetMins) / 2.0;

        int weekday = (int)dateOnly.DayOfWeek;
        double slotDay = dayLen / 8.0;
        double slotNight = nightLen / 8.0;

        // 2. Day & Night Choghadiya dynamically timed
        var dayChoghadiya = BuildChoghadiya(sunriseMins, slotDay, DayStartByWeekday[weekday], step: 1);
        var nightChoghadiya = BuildChoghadiya(sunsetMins, slotNight, NightStartByWeekday[weekday], step: 5);

        // 3. Sidereal Lahiri Chitrapaksha Pancha-Anga evaluated at local Sunrise
        double jdDayStart = ToJulianDay(dateOnly);
        double utcSunriseHour = (sunriseMins / 60.0) - city.TimezoneOffsetHours;
        double jdSunrise = jdDayStart + (utcSunriseHour / 24.0);

        var snap = ComputeAstronomicalSnapshot(jdSunrise);
        double elongation = NormalizeDeg(snap.MoonNirayana - snap.SunNirayana);

        int tithiIdx = (int)Math.Floor(elongation / 12.0) % 30;
        string paksha = tithiIdx < 15 ? "Shukla Paksha" : "Krishna Paksha";
        string tithiLabel = tithiIdx == 14 ? "Purnima" : tithiIdx == 29 ? "Amavasya" : TithiNames[tithiIdx % 15];
        string tithi = $"{paksha.Split(' ')[0]} {tithiLabel}";

        int nakshatraIdx = (int)Math.Floor(snap.MoonNirayana / NakshatraSpanDeg) % 27;
        int yogaIdx = (int)Math.Floor(NormalizeDeg(snap.SunNirayana + snap.MoonNirayana) / NakshatraSpanDeg) % 27;
        int halfTithi = (int)Math.Floor(elongation / 6.0) % 60;
        string karana = ComputeKarana(halfTithi);

        // Numerical root refinement for exact transition end times (resolves apogee/perigee speed variation)
        double tithiTarget = NormalizeDeg((tithiIdx + 1) * 12.0);
        double tithiEndMins = FindLimbTransitionMinutes(
            jdSunrise, jdDayStart, city.TimezoneOffsetHours,
            jd => NormalizeDeg(ComputeAstronomicalSnapshot(jd).MoonNirayana - ComputeAstronomicalSnapshot(jd).SunNirayana),
            tithiTarget);

        double nakTarget = NormalizeDeg((nakshatraIdx + 1) * NakshatraSpanDeg);
        double nakEndMins = FindLimbTransitionMinutes(
            jdSunrise, jdDayStart, city.TimezoneOffsetHours,
            jd => ComputeAstronomicalSnapshot(jd).MoonNirayana,
            nakTarget);

        double yogaTarget = NormalizeDeg((yogaIdx + 1) * NakshatraSpanDeg);
        double yogaEndMins = FindLimbTransitionMinutes(
            jdSunrise, jdDayStart, city.TimezoneOffsetHours,
            jd => { var a = ComputeAstronomicalSnapshot(jd); return NormalizeDeg(a.SunNirayana + a.MoonNirayana); },
            yogaTarget);

        double karanaTarget = NormalizeDeg((halfTithi + 1) * 6.0);
        double karanaEndMins = FindLimbTransitionMinutes(
            jdSunrise, jdDayStart, city.TimezoneOffsetHours,
            jd => NormalizeDeg(ComputeAstronomicalSnapshot(jd).MoonNirayana - ComputeAstronomicalSnapshot(jd).SunNirayana),
            karanaTarget);

        int sunRashiIdx = (int)Math.Floor(snap.SunNirayana / 30.0) % 12;
        int moonRashiIdx = (int)Math.Floor(snap.MoonNirayana / 30.0) % 12;

        // 4. Astronomical Amavasya-bracketed Lunar Month (Amanta, Purnimanta, Adhika Maas) & Samvat
        var lunarMonthInfo = DetermineLunarMonthAndYear(jdSunrise, elongation, tithiIdx, dateOnly);

        // 5. Topocentric Lunar Rise / Set across the local calendar day
        var (moonriseStr, moonsetStr) = ComputeMoonRiseSetDisplay(dateOnly, city.Latitude, city.Longitude, city.TimezoneOffsetHours);

        var selectedSutra = sutras.Count > 0
            ? sutras[Math.Abs(dateOnly.DayOfYear + dateOnly.Year) % sutras.Count]
            : new DailySutra
            {
                SanskritText = "कर्मण्येवाधिकारस्ते मा फलेषु कदाचन ।",
                EnglishMeaning = "You have a right to perform your prescribed duty, but you are not entitled to the fruits of actions.",
                Source = "Bhagavad Gita 2.47"
            };

        return new PanchangData
        {
            Date = dateOnly,
            FormattedDate = dateOnly.ToString("dddd, MMMM dd, yyyy"),
            Location = city.Name,
            VikramSamvat = $"{lunarMonthInfo.VikramSamvat} ({NorthSamvatsaraName(lunarMonthInfo.VikramSamvat)})",
            ShakaSamvat = $"{lunarMonthInfo.ShakaSamvat} ({ShakaSamvatsaraName(lunarMonthInfo.ShakaSamvat)})",
            MonthPurnimanta = lunarMonthInfo.PurnimantaName,
            MonthAmanta = lunarMonthInfo.AmantaName,
            IsAdhikaMaas = lunarMonthInfo.IsAdhika,
            Paksha = paksha,
            AyanamsaName = $"Lahiri Chitrapaksha ({snap.AyanamsaDeg:F2}°)",
            CalculationNote = $"Sidereal ephemeris at Sunrise ({FormatTimeOfDay(sunriseMins)}) for {city.Latitude:F2}°N, {city.Longitude:F2}°E.",
            DayOfWeek = dateOnly.DayOfWeek.ToString(),
            Tithi = string.IsNullOrWhiteSpace(ov?.CustomTithi) ? tithi : ov.CustomTithi,
            TithiEndTime = FormatEndTime(tithiEndMins),
            Nakshatra = string.IsNullOrWhiteSpace(ov?.CustomNakshatra) ? Nakshatras[nakshatraIdx] : ov.CustomNakshatra,
            NakshatraEndTime = FormatEndTime(nakEndMins),
            Yoga = Yogas[yogaIdx],
            YogaEndTime = FormatEndTime(yogaEndMins),
            Karana = karana,
            KaranaEndTime = FormatEndTime(karanaEndMins),
            Vaara = $"{VaraNames[weekday]} ({dateOnly.DayOfWeek})",
            Sunrise = FormatTimeOfDay(sunriseMins),
            Sunset = FormatTimeOfDay(sunsetMins),
            Moonrise = moonriseStr,
            Moonset = moonsetStr,
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

    public static AstronomicalAngles ComputeAstronomicalSnapshot(double jd)
    {
        double t = (jd - 2451545.0) / 36525.0;
        double t2 = t * t;
        double t3 = t2 * t;

        // Solar apparent tropical longitude (Jean Meeus Ch. 25)
        double sunMeanLong = 280.46646 + 36000.76983 * t + 0.0003032 * t2;
        double sunAnomaly = NormalizeDeg(357.52911 + 35999.05029 * t - 0.0001537 * t2) * Deg2Rad;
        double sunEqCenter = (1.914602 - 0.004817 * t - 0.000014 * t2) * Math.Sin(sunAnomaly)
            + (0.019993 - 0.000101 * t) * Math.Sin(2.0 * sunAnomaly)
            + 0.000289 * Math.Sin(3.0 * sunAnomaly);
        double omega = (125.04 - 1934.136 * t) * Deg2Rad;
        double nutationLon = -0.00478 * Math.Sin(omega);
        double sunTropical = NormalizeDeg(sunMeanLong + sunEqCenter - 0.00569 + nutationLon);

        // Lunar tropical longitude, latitude & parallax (Jean Meeus Ch. 47)
        double lp = 218.3164477 + 481267.88123421 * t - 0.0015786 * t2 + t3 / 538841.0;
        double d = NormalizeDeg(297.8501921 + 445267.1114034 * t - 0.0018819 * t2) * Deg2Rad;
        double ms = NormalizeDeg(357.5291092 + 35999.0502909 * t) * Deg2Rad;
        double mm = NormalizeDeg(134.9633964 + 477198.8675055 * t + 0.0087414 * t2) * Deg2Rad;
        double f = NormalizeDeg(93.2720950 + 483202.0175233 * t - 0.0036539 * t2) * Deg2Rad;
        double ecc = 1.0 - 0.002516 * t - 0.0000074 * t2;

        double sigmaL =
            6.288774 * Math.Sin(mm)
            + 1.274027 * Math.Sin(2.0 * d - mm)
            + 0.658314 * Math.Sin(2.0 * d)
            + 0.213618 * Math.Sin(2.0 * mm)
            - 0.185116 * ecc * Math.Sin(ms)
            - 0.114332 * Math.Sin(2.0 * f)
            + 0.058793 * Math.Sin(2.0 * d - 2.0 * mm)
            + 0.057066 * ecc * Math.Sin(2.0 * d - ms - mm)
            + 0.053322 * Math.Sin(2.0 * d + mm)
            + 0.045758 * ecc * Math.Sin(2.0 * d - ms)
            - 0.040923 * ecc * Math.Sin(ms - mm)
            - 0.034720 * Math.Sin(d)
            - 0.030383 * ecc * Math.Sin(ms + mm)
            + 0.015327 * Math.Sin(2.0 * d - 2.0 * f)
            - 0.012528 * Math.Sin(mm + 2.0 * f)
            + 0.010980 * Math.Sin(mm - 2.0 * f)
            + 0.010675 * Math.Sin(4.0 * d - mm)
            + 0.010034 * Math.Sin(3.0 * mm)
            + 0.008548 * Math.Sin(4.0 * d - 2.0 * mm)
            - 0.007888 * ecc * Math.Sin(2.0 * d + ms - mm)
            - 0.006766 * ecc * Math.Sin(2.0 * d + ms)
            + 0.005163 * Math.Sin(mm - d)
            + 0.004987 * ecc * Math.Sin(d + ms)
            + 0.004036 * ecc * Math.Sin(2.0 * d - ms + mm);

        double moonTropical = NormalizeDeg(lp + sigmaL + nutationLon);

        double moonLat =
            5.128122 * Math.Sin(f)
            + 0.280602 * Math.Sin(mm + f)
            + 0.277693 * Math.Sin(mm - f)
            + 0.173237 * Math.Sin(2.0 * d - f)
            + 0.055413 * Math.Sin(2.0 * d - mm + f)
            + 0.046271 * Math.Sin(2.0 * d - mm - f)
            + 0.032573 * Math.Sin(2.0 * d + f)
            + 0.017198 * Math.Sin(2.0 * mm + f);

        double distKm = 385000.56
            - 20905.355 * Math.Cos(mm)
            - 3699.111 * Math.Cos(2.0 * d - mm)
            - 2955.968 * Math.Cos(2.0 * d)
            - 569.925 * Math.Cos(2.0 * mm);
        double moonPiDeg = Math.Asin(6378.14 / distKm) * Rad2Deg;

        double obliquity = 23.439291 - 0.0130042 * t + 0.00256 * Math.Cos(omega);
        double ayanamsa = 23.857092 + ((5029.0966 * t + 1.11161 * t2) / 3600.0) + nutationLon;

        double sunNirayana = NormalizeDeg(sunTropical - ayanamsa);
        double moonNirayana = NormalizeDeg(moonTropical - ayanamsa);

        return new AstronomicalAngles(sunTropical, moonTropical, moonLat, moonPiDeg, obliquity, ayanamsa, sunNirayana, moonNirayana);
    }

    public static List<FestivalItem> ComputeDynamicFestivalsForWindow(DateTime startDate, int daysCount = 365)
    {
        var result = new List<FestivalItem>();
        const double istLat = 28.6139;
        const double istLon = 77.2090;
        const double istTz = 5.5;

        var start = startDate.Date;
        int prevSunRashi = -1;

        for (int offset = 0; offset <= daysCount; offset++)
        {
            var d = start.AddDays(offset);
            var (sunriseMins, sunsetMins) = ComputeSunriseSunsetMinutes(d, istLat, istLon, istTz);
            var (nextSunriseMins, _) = ComputeSunriseSunsetMinutes(d.AddDays(1), istLat, istLon, istTz);

            double jdDay = ToJulianDay(d);
            double jdSunrise = jdDay + ((sunriseMins / 60.0 - istTz) / 24.0);
            double jdNextSunrise = ToJulianDay(d.AddDays(1)) + ((nextSunriseMins / 60.0 - istTz) / 24.0);
            double jdNoon = jdDay + (((sunriseMins + sunsetMins) * 0.5 / 60.0 - istTz) / 24.0);
            double jdAparahna = jdDay + (((sunriseMins + (sunsetMins - sunriseMins) * 0.72) / 60.0 - istTz) / 24.0);
            double jdPradosh = jdDay + (((sunsetMins + 36.0) / 60.0 - istTz) / 24.0);
            double jdNishita = jdDay + ((23.75 - istTz) / 24.0);

            var riseSnap = ComputeAstronomicalSnapshot(jdSunrise);
            double riseElong = NormalizeDeg(riseSnap.MoonNirayana - riseSnap.SunNirayana);
            int riseTithi = (int)Math.Floor(riseElong / 12.0) % 30;
            var riseMonth = DetermineLunarMonthAndYear(jdSunrise, riseElong, riseTithi, d);

            var nextRiseSnap = ComputeAstronomicalSnapshot(jdNextSunrise);
            double nextRiseElong = NormalizeDeg(nextRiseSnap.MoonNirayana - nextRiseSnap.SunNirayana);
            int nextRiseTithi = (int)Math.Floor(nextRiseElong / 12.0) % 30;

            // A Kshaya Tithi begins after today's sunrise and ends before tomorrow's sunrise (otherwise missed by pure Udaya loop)
            int tithiAdvance = (nextRiseTithi - riseTithi + 30) % 30;
            int? kshayaTithi = tithiAdvance >= 2 ? (riseTithi + 1) % 30 : null;

            var noonSnap = ComputeAstronomicalSnapshot(jdNoon);
            double noonElong = NormalizeDeg(noonSnap.MoonNirayana - noonSnap.SunNirayana);
            int noonTithi = (int)Math.Floor(noonElong / 12.0) % 30;
            var noonMonth = DetermineLunarMonthAndYear(jdNoon, noonElong, noonTithi, d);

            var aparSnap = ComputeAstronomicalSnapshot(jdAparahna);
            double aparElong = NormalizeDeg(aparSnap.MoonNirayana - aparSnap.SunNirayana);
            int aparTithi = (int)Math.Floor(aparElong / 12.0) % 30;
            var aparMonth = DetermineLunarMonthAndYear(jdAparahna, aparElong, aparTithi, d);

            var pradoshSnap = ComputeAstronomicalSnapshot(jdPradosh);
            double pradoshElong = NormalizeDeg(pradoshSnap.MoonNirayana - pradoshSnap.SunNirayana);
            int pradoshTithi = (int)Math.Floor(pradoshElong / 12.0) % 30;
            var pradoshMonth = DetermineLunarMonthAndYear(jdPradosh, pradoshElong, pradoshTithi, d);

            var nishitaSnap = ComputeAstronomicalSnapshot(jdNishita);
            double nishitaElong = NormalizeDeg(nishitaSnap.MoonNirayana - nishitaSnap.SunNirayana);
            int nishitaTithi = (int)Math.Floor(nishitaElong / 12.0) % 30;
            var nishitaMonth = DetermineLunarMonthAndYear(jdNishita, nishitaElong, nishitaTithi, d);

            bool MatchRiseOrKshaya(int amantaIdxRequired, int targetTithi) =>
                (!riseMonth.IsAdhika && riseMonth.AmantaIdx == amantaIdxRequired && riseTithi == targetTithi) ||
                (kshayaTithi == targetTithi && !pradoshMonth.IsAdhika && pradoshMonth.AmantaIdx == amantaIdxRequired);

            int curSunRashi = (int)Math.Floor(riseSnap.SunNirayana / 30.0) % 12;
            if (prevSunRashi != -1 && curSunRashi != prevSunRashi)
            {
                if (curSunRashi == 9)
                {
                    AddFestivalOnce(result, d, "dyn-makara-sankranti", "मकर संक्रांति / उत्तरायण", "Makara Sankranti", "Solar Sankranti (Makara)", "Makara", "Sankranti", true,
                        "Marks the Sun's sacred entry into Makara (Capricorn) and northward Uttarayana journey.",
                        "Morning holy dip (Snan), Til-Gud offerings, charity (Daan), and Surya Arghya.",
                        "Sattvic sesame and jaggery meal after morning prayers.");
                }
                else if (curSunRashi == 0)
                {
                    AddFestivalOnce(result, d, "dyn-mesha-sankranti", "मेष संक्रांति / बैसाखी", "Mesha Sankranti (Baisakhi / Vishu)", "Solar Sankranti (Mesha)", "Chaitra / Mesha", "Sankranti", true,
                        "Marks the Sun's entry into Mesha (Aries), celebrated as solar New Year and harvest thanksgiving across India.",
                        "Morning river bath, Surya worship, offering grains and seasonal fruits.",
                        "Traditional festive sattvic prasad.");
                }
            }
            prevSunRashi = curSunRashi;

            // Major Muhurat-driven festivals (Pradosh / Nishita / Madhyahna / Aparahna)
            if (!pradoshMonth.IsAdhika && pradoshMonth.PurnimantaIdx == 7 && pradoshTithi == 27) // Dhanteras Pradosh
            {
                AddFestivalOnce(result, d, "dyn-dhanteras", "धनतेरस (धनत्रयोदशी)", "Dhanteras & Dhanvantari Jayanti", "Kartika Krishna Trayodashi", "Kartika", "Major", true,
                    "Honors Lord Dhanvantari (Ayurveda) and heralds the 5-day Deepawali festival.",
                    "Evening Yama Deepam at entrance, Dhanvantari & Kubera worship.",
                    "Prepare home lamps and sattvic sweets.");
            }
            if (!pradoshMonth.IsAdhika && pradoshMonth.PurnimantaIdx == 7 && pradoshTithi == 29) // Deepawali Pradosh Amavasya
            {
                AddFestivalOnce(result, d, "dyn-diwali", "दीपावली (लक्ष्मी पूजन)", "Deepawali (Diwali)", "Kartika Amavasya (Pradosh)", "Kartika", "Major", true,
                    "Festival of Lights honoring Bhagavan Shri Rama's return to Ayodhya and Pradosh Kaal Lakshmi-Ganesha worship.",
                    "Pradosh Kaal Lakshmi-Ganesha Puja, lighting rows of earthen cow-ghee/mustard oil diyas.",
                    "Avoid tamasic foods; share mithai and light homes after sunset.");
            }
            if (!nishitaMonth.IsAdhika && nishitaMonth.PurnimantaIdx == 11 && nishitaTithi == 28) // Phalguna Krishna Chaturdashi Nishita
            {
                AddFestivalOnce(result, d, "dyn-mahashivaratri", "महाशिवरात्रि", "Maha Shivaratri", "Phalguna Krishna Chaturdashi", "Phalguna", "Major", true,
                    "Great night of Lord Shiva symbolizing awareness over darkness and the cosmic Shiva-Shakti union.",
                    "Chatur-Prahara Abhishekam with milk, bilva leaves, honey & Ganga jal plus Om Namah Shivaya jagaran.",
                    "Daylong Phalahar or Nirjala fast concluded next morning.");
            }
            if (!nishitaMonth.IsAdhika && nishitaMonth.AmantaIdx == 4 && nishitaTithi == 22) // Janmashtami Nishita
            {
                AddFestivalOnce(result, d, "dyn-janmashtami", "श्री कृष्ण जन्माष्टमी", "Shri Krishna Janmashtami", "Bhadrapada Krishna Ashtami", "Bhadrapada", "Major", true,
                    "Sacred midnight birth celebration of Bhagavan Shri Krishna in Mathura.",
                    "Fasting through the day, midnight Abhishekam, cradle celebration, panchamrit and makhan-mishri bhog.",
                    "Fast until midnight Janma Darshan (Nishita Kaal).");
            }
            if (!noonMonth.IsAdhika && noonMonth.AmantaIdx == 0 && noonTithi == 8) // Madhyahna Rama Navami
            {
                AddFestivalOnce(result, d, "dyn-ramanavami", "श्री राम नवमी", "Shri Rama Navami", "Chaitra Shukla Navami", "Chaitra", "Major", true,
                    "Birth anniversary of Maryada Purushottam Bhagavan Shri Rama at noon Abhijit lagna in Ayodhya.",
                    "Recite Ramcharitmanas / Sundarakanda, noon Garbhagriha aarti, Panjiri offering.",
                    "Falahar fast until midday birth celebration.");
            }
            if (!noonMonth.IsAdhika && noonMonth.AmantaIdx == 5 && noonTithi == 3) // Madhyahna Ganesh Chaturthi
            {
                AddFestivalOnce(result, d, "dyn-ganesh-chaturthi", "श्री गणेश चतुर्थी", "Ganesh Chaturthi", "Bhadrapada Shukla Chaturthi", "Bhadrapada", "Major", true,
                    "Celebrates the arrival of Vighnaharta Bhagavan Shri Ganesha.",
                    "Prana-Pratishtha of eco-friendly Ganesha murti, Durva grass & 21 Modak offerings.",
                    "Sattvic prasad and daily morning-evening Ganapati Aarti.");
            }
            if (!aparMonth.IsAdhika && aparMonth.AmantaIdx == 6 && aparTithi == 9) // Aparahna Vijayadashami
            {
                AddFestivalOnce(result, d, "dyn-dussehra", "विजयादशमी (दशहरा)", "Vijayadashami (Dussehra)", "Ashvina Shukla Dashami", "Ashvina", "Major", true,
                    "Celebrates victory of Dharma — Lord Rama's triumph over Ravana and Maa Durga's triumph over Mahishasura.",
                    "Shami tree worship, Aparajita Puja, and Shastra/book reverence.",
                    "Festive sattvic feast.");
            }
            if (!pradoshMonth.IsAdhika && pradoshMonth.AmantaIdx == 11 && pradoshTithi == 14) // Pradosh Holika Dahan
            {
                AddFestivalOnce(result, d, "dyn-holi", "होलिका दहन / फाल्गुन पूर्णिमा", "Holika Dahan & Phalguna Purnima", "Phalguna Purnima", "Phalguna", "Major", true,
                    "Symbolizes Prahlada's unwavering devotion and the triumph of truth over arrogance.",
                    "Evening Holika Puja with wheat ears, kumkum, coconut and Pradakshina.",
                    "Next morning celebrates Rangwali Holi in harmony.");
            }

            if (!pradoshMonth.IsAdhika && pradoshTithi is 12 or 27 && !(pradoshMonth.PurnimantaIdx == 7 && pradoshTithi == 27))
            {
                var pPaksha = pradoshTithi == 12 ? "Shukla" : "Krishna";
                var prMonthName = LunarMonths[pradoshMonth.PurnimantaIdx];
                AddFestivalOnce(result, d, $"dyn-pradosh-{d:yyyyMMdd}", $"{prMonthName} {pPaksha} प्रदोष व्रत", $"{prMonthName} {pPaksha} Pradosh Vrat",
                    $"{prMonthName} {pPaksha} Trayodashi", prMonthName, "Pradosh", false,
                    "Twilight observance dedicated to Lord Shiva and Goddess Parvati during auspicious Pradosh Kaal.",
                    "Pradosh twilight Shiva Linga Abhishekam with bilva leaves and Shiva Chalisa recitation.",
                    "Light Sattvic fast broken after twilight Shiva Puja.");
            }

            // Skip generic duplicate lunar tithis during Adhika Maas major names (or prefix clearly)
            if (riseMonth.IsAdhika)
            {
                if (riseTithi is 10 or 25 || kshayaTithi is 10 or 25)
                {
                    int ekTithi = riseTithi is 10 or 25 ? riseTithi : kshayaTithi!.Value;
                    var ePaksha = ekTithi == 10 ? "Shukla" : "Krishna";
                    AddFestivalOnce(result, d, $"dyn-ekadashi-{d:yyyyMMdd}", $"अधिक {riseMonth.PurnimantaName} {ePaksha} एकादशी",
                        $"Adhika {LunarMonths[riseMonth.AmantaIdx]} {ePaksha} Ekadashi",
                        $"{riseMonth.PurnimantaName} {ePaksha} Ekadashi", riseMonth.PurnimantaName, "Ekadashi", false,
                        "Sacred Purushottama Maas Ekadashi fast for spiritual purification and Vishnu bhakti.",
                        "Morning snan, Tulsi archana, Vishnu Sahasranama recitation.",
                        "Abstain from grains, rice, and pulses; break fast during Parana next morning.");
                }
                continue;
            }

            int pIdx = riseMonth.PurnimantaIdx;
            string pMonth = LunarMonths[pIdx];

            // Major annual festivals (Udaya sunrise or Kshaya spanning the day)
            if (MatchRiseOrKshaya(0, 0))
            {
                AddFestivalOnce(result, d, "dyn-ugadi-navratri", "चैत्र नवरात्रि प्रारंभ & हिंदू नववर्ष", "Chaitra Navratri & Hindu New Year", "Chaitra Shukla Pratipada", "Chaitra", "Major", true,
                    "Inaugurates the new Vikram Samvat & Shaka Samvat year along with 9 nights of Devi Shakti worship.",
                    "Kalash Sthapana, Akhand Jyoti, Durga Saptashati paath & Neem-Mishri prasad.",
                    "Nine-day sattvic Navratri vrat rules.");
            }
            else if (MatchRiseOrKshaya(0, 14))
            {
                AddFestivalOnce(result, d, "dyn-hanuman-jayanti", "श्री हनुमान जयंती", "Hanuman Jayanti", "Chaitra Purnima", "Chaitra", "Major", true,
                    "Celebrates the birth of Sankatmochan Shri Hanuman, embodiment of devotion, strength & wisdom.",
                    "Offer vermilion (Sindoor), jasmine oil, boondi laddoo & Hanuman Chalisa / Sundarakanda.",
                    "Sattvic vegetarian meal and charity.");
            }
            else if (MatchRiseOrKshaya(1, 2))
            {
                AddFestivalOnce(result, d, "dyn-akshaya-tritiya", "अक्षय तृतीया", "Akshaya Tritiya", "Vaishakha Shukla Tritiya", "Vaishakha", "Major", true,
                    "Swayamsiddha (ever-auspicious) day symbolizing unending merit, dharma & new beginnings.",
                    "Vishnu-Lakshmi puja, water-pot/grain Daan, Parashurama Jayanti remembrance.",
                    "Charity of cool water, grains, and sattvic foods.");
            }
            else if (MatchRiseOrKshaya(4, 14))
            {
                AddFestivalOnce(result, d, "dyn-raksha-bandhan", "रक्षाबंधन & श्रावण पूर्णिमा", "Raksha Bandhan & Shravana Purnima", "Shravana Purnima", "Shravana", "Major", true,
                    "Sacred celebration of sibling protection vows and Upakarma Vedic learning renewal.",
                    "Morning Gayatri Upakarma, tying sacred Rakhi with tilak and aarti.",
                    "Sattvic festive celebration with family.");
            }
            else if (MatchRiseOrKshaya(6, 0))
            {
                AddFestivalOnce(result, d, "dyn-sharad-navratri", "शारदीय नवरात्रि प्रारंभ", "Sharad Navratri Begins", "Ashvina Shukla Pratipada", "Ashvina", "Major", true,
                    "Nine auspicious nights worshipping Nava Durga culminating in Dussehra.",
                    "Ghatasthapana, Lalita Sahasranama & Durga Saptashati.",
                    "Daily Phalahar or single-meal sattvic discipline.");
            }
            else if (MatchRiseOrKshaya(7, 0))
            {
                AddFestivalOnce(result, d, "dyn-govardhan", "गोवर्धन पूजा & अन्नकूट", "Govardhan Puja & Annakut", "Kartika Shukla Pratipada", "Kartika", "Major", true,
                    "Honors Lord Krishna's protection of Vrindavan beneath Mount Govardhan and gratitude for nature.",
                    "Annakut Mountain Bhog offering, Gau Puja, and Govardhan Parikrama remembrance.",
                    "Traditional Annakut sattvic prasad.");
            }
            else if (MatchRiseOrKshaya(7, 1))
            {
                AddFestivalOnce(result, d, "dyn-bhaidooj", "भाई दूज (यम द्वितीया)", "Bhai Dooj (Yama Dwitiya)", "Kartika Shukla Dwitiya", "Kartika", "Major", true,
                    "Honors sacred sibling bonds inspired by Yamuna and Yamaraja.",
                    "Tilak, aarti, and prayers for sibling longevity and well-being.",
                    "Shared family meal.");
            }
            else if (MatchRiseOrKshaya(7, 10))
            {
                AddFestivalOnce(result, d, "dyn-devutthana", "देवउठनी एकादशी (प्रबोधिनी एकादशी)", "Devutthana Ekadashi", "Kartika Shukla Ekadashi", "Kartika", "Major", true,
                    "Marks the sacred awakening of Lord Vishnu after Chaturmas; auspicious ceremonies resume.",
                    "Evening Tulsi puja, 11 earthen diyas, sugarcane mandap & Vishnu Sahasranama.",
                    "Sattvic Falahar or Nirjala Ekadashi vrat without grains.");
            }
            else if (MatchRiseOrKshaya(7, 11))
            {
                AddFestivalOnce(result, d, "dyn-tulsi-vivah", "तुलसी विवाह", "Tulsi Vivah", "Kartika Shukla Dwadashi", "Kartika", "Major", true,
                    "Ceremonial union of Mata Vrinda (Tulsi) and Bhagavan Shaligram after Devutthana Ekadashi.",
                    "Decorate Tulsi courtyard with sugarcane, offer turmeric, kumkum & mangalashtak.",
                    "Break Ekadashi vrat during Dwadashi Parana.");
            }
            else if (MatchRiseOrKshaya(7, 14))
            {
                AddFestivalOnce(result, d, "dyn-kartik-purnima", "कार्तिक पूर्णिमा & देव दीपावली", "Kartik Purnima & Dev Deepawali", "Kartika Purnima", "Kartika", "Major", true,
                    "Celebrates Lord Shiva's victory over Tripurasura; river ghats glow with countless lamps.",
                    "Brahma Muhurat Snan, Deep Daan by sacred water, Satyanarayan Katha & Shiva Puja.",
                    "Sattvic single meal after Moonrise arghya.");
            }
            else if (MatchRiseOrKshaya(8, 4))
            {
                AddFestivalOnce(result, d, "dyn-vivah-panchami", "विवाह पंचमी", "Vivah Panchami", "Margashirsha Shukla Panchami", "Margashirsha", "Major", true,
                    "Commemorates the sacred celestial wedding of Bhagavan Shri Rama and Mata Sita in Janakpur.",
                    "Ramcharitmanas Balakanda recitation & Rama-Sita kalyanotsava aarti.",
                    "Devotional sattvic observance.");
            }
            else if (MatchRiseOrKshaya(8, 10))
            {
                AddFestivalOnce(result, d, "dyn-gita-jayanti", "गीता जयंती & मोक्षदा एकादशी", "Gita Jayanti & Mokshada Ekadashi", "Margashirsha Shukla Ekadashi", "Margashirsha", "Major", true,
                    "Honors the sacred day Bhagavan Shri Krishna imparted the Bhagavad Gita to Arjuna at Kurukshetra.",
                    "Bhagavad Gita Chapter 12/15 or complete parayana, ghee diya & Gita Daan.",
                    "Grain-free Ekadashi Falahar fast.");
            }
            else if (MatchRiseOrKshaya(10, 4))
            {
                AddFestivalOnce(result, d, "dyn-vasant-panchami", "वसंत पंचमी (सरस्वती पूजा)", "Vasant Panchami & Saraswati Puja", "Magha Shukla Panchami", "Magha", "Major", true,
                    "Celebrates Maa Saraswati (wisdom, music, arts) and welcomes spring (Vasant Ritu).",
                    "Wear yellow attire, Vidyarambham (children's learning initiation), offer yellow flowers.",
                    "Kesari sweet rice and sattvic prasad.");
            }

            // Recurring Ekadashi / Purnima / Amavasya (if not already added as a major named festival above)
            if ((riseTithi is 10 or 25 || kshayaTithi is 10 or 25) && !result.Any(x => x.Date.Date == d))
            {
                int ekTithi = riseTithi is 10 or 25 ? riseTithi : kshayaTithi!.Value;
                var ekPaksha = ekTithi == 10 ? "Shukla" : "Krishna";
                var ekMonth = kshayaTithi == ekTithi && pradoshMonth.PurnimantaIdx != riseMonth.PurnimantaIdx
                    ? LunarMonths[pradoshMonth.PurnimantaIdx]
                    : pMonth;
                var ekTitle = GetEkadashiName(ekMonth, ekPaksha);
                AddFestivalOnce(result, d, $"dyn-ekadashi-{d:yyyyMMdd}", $"{ekTitle} एकादशी", ekTitle,
                    $"{ekMonth} {ekPaksha} Ekadashi", ekMonth, "Ekadashi", false,
                    $"Sacred {ekMonth} {ekPaksha} Ekadashi dedicated to Bhagavan Vishnu and inner clarity.",
                    "Morning snan, Tulsi leaves & panchamrit offering, Vishnu Sahasranama chant.",
                    "No grains, rice or beans; Falahar fast broken during Dwadashi Parana.");
            }
            else if ((riseTithi == 14 || kshayaTithi == 14) && !result.Any(x => x.Date.Date == d))
            {
                var purnimaMonth = kshayaTithi == 14 ? LunarMonths[pradoshMonth.PurnimantaIdx] : pMonth;
                AddFestivalOnce(result, d, $"dyn-purnima-{d:yyyyMMdd}", $"{purnimaMonth} पूर्णिमा", $"{purnimaMonth} Purnima & Satyanarayan Vrat",
                    $"{purnimaMonth} Purnima", purnimaMonth, "Purnima", false,
                    "Full Moon day revered for Sri Satyanarayan Katha, Chandra Arghya, meditation and charity.",
                    "Satyanarayan Puja, Moonrise milk-water Arghya & Anna Daan.",
                    "Daytime light fast concluded after Moon darshan.");
            }
            else if (pradoshTithi == 29 && !result.Any(x => x.Date.Date == d))
            {
                var amMonth = LunarMonths[pradoshMonth.PurnimantaIdx];
                AddFestivalOnce(result, d, $"dyn-amavasya-{d:yyyyMMdd}", $"{amMonth} अमावस्या", $"{amMonth} Amavasya (Pitru Tarpan)",
                    $"{amMonth} Amavasya", amMonth, "Amavasya", false,
                    "New Moon day honoring ancestors (Pitru Tarpan), introspection & charitable feeding.",
                    "Morning Til-Tarpan, Peepal tree circumambulation & cow/needy feeding.",
                    "Simple sattvic meal at midday.");
            }
        }

        return result.OrderBy(f => f.Date).ToList();
    }

    private static void AddFestivalOnce(
        List<FestivalItem> target,
        DateTime date,
        string id,
        string hindi,
        string english,
        string tithi,
        string monthVedic,
        string category,
        bool isMajor,
        string desc,
        string vidhi,
        string fasting)
    {
        if (target.Any(f => f.NameEnglish.Equals(english, StringComparison.OrdinalIgnoreCase)
            && Math.Abs((f.Date.Date - date.Date).TotalDays) < 15))
            return;

        target.Add(new FestivalItem
        {
            Id = $"{id}-{date:yyyy}",
            NameHindi = hindi,
            NameEnglish = english,
            Date = date,
            Tithi = tithi,
            MonthVedic = monthVedic,
            Description = desc,
            RitualsAndPujaVidhi = vidhi,
            FastingRules = fasting,
            IsMajorFestival = isMajor,
            Category = category,
            IsDynamicCalculated = true
        });
    }

    private static string GetEkadashiName(string purnimantaMonth, string paksha) => (purnimantaMonth, paksha) switch
    {
        ("Chaitra", "Krishna") => "Papmochani Ekadashi",
        ("Chaitra", "Shukla") => "Kamada Ekadashi",
        ("Vaishakha", "Krishna") => "Varuthini Ekadashi",
        ("Vaishakha", "Shukla") => "Mohini Ekadashi",
        ("Jyeshtha", "Krishna") => "Apara Ekadashi",
        ("Jyeshtha", "Shukla") => "Nirjala Ekadashi",
        ("Ashadha", "Krishna") => "Yogini Ekadashi",
        ("Ashadha", "Shukla") => "Devshayani Ekadashi",
        ("Shravana", "Krishna") => "Kamika Ekadashi",
        ("Shravana", "Shukla") => "Shravana Putrada Ekadashi",
        ("Bhadrapada", "Krishna") => "Aja Ekadashi",
        ("Bhadrapada", "Shukla") => "Parsva (Parivartini) Ekadashi",
        ("Ashvina", "Krishna") => "Indira Ekadashi",
        ("Ashvina", "Shukla") => "Papankusha Ekadashi",
        ("Kartika", "Krishna") => "Rama Ekadashi",
        ("Kartika", "Shukla") => "Devutthana Ekadashi",
        ("Margashirsha", "Krishna") => "Utpanna Ekadashi",
        ("Margashirsha", "Shukla") => "Mokshada Ekadashi",
        ("Pausha", "Krishna") => "Saphala Ekadashi",
        ("Pausha", "Shukla") => "Pausha Putrada Ekadashi",
        ("Magha", "Krishna") => "Shattila Ekadashi",
        ("Magha", "Shukla") => "Jaya Ekadashi",
        ("Phalguna", "Krishna") => "Vijaya Ekadashi",
        ("Phalguna", "Shukla") => "Amalaki Ekadashi",
        _ => $"{purnimantaMonth} {paksha} Ekadashi"
    };

    private sealed record LunarMonthInfo(
        int AmantaIdx,
        int PurnimantaIdx,
        string AmantaName,
        string PurnimantaName,
        bool IsAdhika,
        int VikramSamvat,
        int ShakaSamvat);

    private static LunarMonthInfo DetermineLunarMonthAndYear(double jdSunrise, double elongation, int tithiIdx, DateTime dateOnly)
    {
        // Locate exact Amavasya instants bracketing jdSunrise via Newtonian refinement on mean synodic rate
        double jdPrevAmavasya = RefineAmavasya(jdSunrise - (elongation / 12.190749));
        if (jdPrevAmavasya > jdSunrise)
            jdPrevAmavasya = RefineAmavasya(jdPrevAmavasya - SynodicMonthDays);

        double jdNextAmavasya = RefineAmavasya(jdPrevAmavasya + SynodicMonthDays);
        if (jdNextAmavasya <= jdSunrise)
        {
            jdPrevAmavasya = jdNextAmavasya;
            jdNextAmavasya = RefineAmavasya(jdPrevAmavasya + SynodicMonthDays);
        }

        var prevSunRashi = (int)Math.Floor(ComputeAstronomicalSnapshot(jdPrevAmavasya).SunNirayana / 30.0) % 12;
        var nextSunRashi = (int)Math.Floor(ComputeAstronomicalSnapshot(jdNextAmavasya).SunNirayana / 30.0) % 12;

        bool isAdhika = prevSunRashi == nextSunRashi;
        int amantaIdx = isAdhika ? (nextSunRashi + 1) % 12 : nextSunRashi % 12;
        int purnimantaIdx = (!isAdhika && tithiIdx >= 15) ? (amantaIdx + 1) % 12 : amantaIdx;

        string amantaName = isAdhika ? $"Adhika {LunarMonths[amantaIdx]}" : LunarMonths[amantaIdx];
        string purnimantaName = isAdhika ? $"Adhika {LunarMonths[purnimantaIdx]}" : LunarMonths[purnimantaIdx];

        // Vikram & Shaka Samvat roll over on Chaitra Shukla Pratipada (Amanta index 0 starting after Phalguna)
        bool beforeChaitraNewYear = dateOnly.Month <= 4 && amantaIdx >= 9;
        int vikramSamvat = dateOnly.Year + (beforeChaitraNewYear ? 56 : 57);
        int shakaSamvat = dateOnly.Year - (beforeChaitraNewYear ? 79 : 78);

        return new LunarMonthInfo(amantaIdx, purnimantaIdx, amantaName, purnimantaName, isAdhika, vikramSamvat, shakaSamvat);
    }

    private static double RefineAmavasya(double jdGuess)
    {
        double jd = jdGuess;
        for (int i = 0; i < 6; i++)
        {
            var snap = ComputeAstronomicalSnapshot(jd);
            double diff = NormalizeDeg(snap.MoonNirayana - snap.SunNirayana);
            if (diff > 180.0) diff -= 360.0;
            if (Math.Abs(diff) < 1e-4) break;
            jd -= diff / 12.190749;
        }
        return jd;
    }

    private static double FindLimbTransitionMinutes(
        double jdStart,
        double jdDayStart,
        double tzOffsetHours,
        Func<double, double> metricDegFn,
        double targetDeg)
    {
        double signedRem = NormalizeDeg(targetDeg - metricDegFn(jdStart));
        double stepDays = 0.02; // ~29 mins
        double prevJd = jdStart;
        double prevRem = signedRem;

        for (int i = 1; i <= 75; i++)
        {
            double curJd = jdStart + i * stepDays;
            double curRem = NormalizeDeg(targetDeg - metricDegFn(curJd));
            // Detect crossing 0 deg smoothly: remainder wrapped from small positive (<40) to near 360 (>320)
            if (prevRem < 40.0 && curRem > 320.0)
            {
                double lo = prevJd;
                double hi = curJd;
                for (int b = 0; b < 16; b++)
                {
                    double mid = 0.5 * (lo + hi);
                    double midRem = NormalizeDeg(targetDeg - metricDegFn(mid));
                    if (midRem < 180.0) lo = mid;
                    else hi = mid;
                }
                double crossJd = 0.5 * (lo + hi);
                return (crossJd - jdDayStart) * 1440.0 + tzOffsetHours * 60.0;
            }
            prevJd = curJd;
            prevRem = curRem;
        }

        return (jdStart - jdDayStart) * 1440.0 + tzOffsetHours * 60.0 + 720.0;
    }

    private static (string Moonrise, string Moonset) ComputeMoonRiseSetDisplay(
        DateTime dateOnly,
        double latDeg,
        double lonDeg,
        double tzOffsetHours)
    {
        double jdMidnightLocal = ToJulianDay(dateOnly) - (tzOffsetHours / 24.0);
        double? riseMins = FindHorizonCrossing(jdMidnightLocal, latDeg, lonDeg, 0, 1440, rising: true);
        double? setMins = FindHorizonCrossing(jdMidnightLocal, latDeg, lonDeg, 0, 1440, rising: false);

        string riseStr;
        if (riseMins.HasValue)
        {
            riseStr = FormatTimeOfDay(riseMins.Value);
        }
        else
        {
            // Lunar day averages 24h50m, so occasionally moonrise slips just past midnight into the early next morning
            double? nextMorningRise = FindHorizonCrossing(jdMidnightLocal, latDeg, lonDeg, 1440, 1620, rising: true);
            riseStr = nextMorningRise.HasValue ? $"{FormatTimeOfDay(nextMorningRise.Value)} (+1d)" : "No Moonrise";
        }

        string setStr;
        if (setMins.HasValue)
        {
            setStr = FormatTimeOfDay(setMins.Value);
        }
        else
        {
            double? nextMorningSet = FindHorizonCrossing(jdMidnightLocal, latDeg, lonDeg, 1440, 1620, rising: false);
            setStr = nextMorningSet.HasValue ? $"{FormatTimeOfDay(nextMorningSet.Value)} (+1d)" : "No Moonset";
        }

        return (riseStr, setStr);
    }

    private static double? FindHorizonCrossing(
        double jdMidnightLocal,
        double latDeg,
        double lonDeg,
        int startMin,
        int endMin,
        bool rising)
    {
        const int step = 10;
        double prevAlt = MoonHorizonResidualDeg(jdMidnightLocal + startMin / 1440.0, latDeg, lonDeg);

        for (int m = startMin + step; m <= endMin; m += step)
        {
            double curAlt = MoonHorizonResidualDeg(jdMidnightLocal + m / 1440.0, latDeg, lonDeg);
            bool crossed = rising ? (prevAlt < 0.0 && curAlt >= 0.0) : (prevAlt >= 0.0 && curAlt < 0.0);
            if (crossed)
            {
                double lo = m - step;
                double hi = m;
                for (int b = 0; b < 14; b++)
                {
                    double mid = 0.5 * (lo + hi);
                    double midAlt = MoonHorizonResidualDeg(jdMidnightLocal + mid / 1440.0, latDeg, lonDeg);
                    if (rising ? (midAlt < 0.0) : (midAlt >= 0.0)) lo = mid;
                    else hi = mid;
                }
                return 0.5 * (lo + hi);
            }
            prevAlt = curAlt;
        }
        return null;
    }

    private static double MoonHorizonResidualDeg(double jd, double latDeg, double lonDeg)
    {
        var snap = ComputeAstronomicalSnapshot(jd);
        double lam = snap.MoonTropicalLon * Deg2Rad;
        double bet = snap.MoonLatDeg * Deg2Rad;
        double eps = snap.ObliquityDeg * Deg2Rad;

        double sinLam = Math.Sin(lam);
        double cosLam = Math.Cos(lam);
        double raRad = Math.Atan2(sinLam * Math.Cos(eps) - Math.Tan(bet) * Math.Sin(eps), cosLam);
        double declRad = Math.Asin(Math.Clamp(Math.Sin(bet) * Math.Cos(eps) + Math.Cos(bet) * Math.Sin(eps) * sinLam, -1.0, 1.0));

        double d = jd - 2451545.0;
        double gmstDeg = NormalizeDeg(280.46061837 + 360.98564736629 * d);
        double raDeg = NormalizeDeg(raRad * Rad2Deg);
        double hourAngle = (gmstDeg + lonDeg - raDeg) * Deg2Rad;

        double latRad = latDeg * Deg2Rad;
        double sinAlt = Math.Sin(latRad) * Math.Sin(declRad) + Math.Cos(latRad) * Math.Cos(declRad) * Math.Cos(hourAngle);
        double altDeg = Math.Asin(Math.Clamp(sinAlt, -1.0, 1.0)) * Rad2Deg;

        // Upper-limb threshold with atmospheric refraction (-34') and horizontal parallax (~+0.95 deg)
        double h0 = (0.7275 * snap.MoonHorizontalParallaxDeg) - 0.5667;
        return altDeg - h0;
    }

    private static List<ChoghadiyaPeriod> BuildChoghadiya(double baseMins, double slotLen, int startIdx, int step)
    {
        var list = new List<ChoghadiyaPeriod>(8);
        for (int i = 0; i < 8; i++)
        {
            var entry = DayChoghadiyaCycle[(startIdx + i * step) % 7];
            list.Add(new ChoghadiyaPeriod
            {
                Name = entry.Name,
                Type = entry.Type,
                TimeRange = FormatRange(baseMins + i * slotLen, baseMins + (i + 1) * slotLen)
            });
        }
        return list;
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

        double latRad = latitudeDeg * Deg2Rad;
        double zenithRad = 90.833 * Deg2Rad;
        double cosHa = (Math.Cos(zenithRad) / (Math.Cos(latRad) * Math.Cos(decl))) - (Math.Tan(latRad) * Math.Tan(decl));
        cosHa = Math.Clamp(cosHa, -0.98, 0.98);
        double haDeg = Math.Acos(cosHa) * Rad2Deg;

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

    public static string NorthSamvatsaraName(int vikramSamvat) =>
        Samvatsara60Cycle[((vikramSamvat + 10) % 60 + 60) % 60];

    public static string ShakaSamvatsaraName(int shakaSamvat) =>
        Samvatsara60Cycle[((shakaSamvat + 11) % 60 + 60) % 60];

    private static string FormatEndTime(double localMinutes) =>
        localMinutes >= 1440.0
            ? $"Until {FormatTimeOfDay(localMinutes)} (+1d)"
            : $"Until {FormatTimeOfDay(localMinutes)}";

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
