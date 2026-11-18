using HinduPanchang.Api.Models;

namespace HinduPanchang.Api.Services;

/// <summary>
/// Classical Drigganita astronomical helper computing exact Sunrise/Sunset twilight,
/// Nakshatra-span Varjyam &amp; Amrit Kalam, Sunrise-to-next-Sunrise multi-transitions,
/// planetary Horas, 12 rising Udaya Lagnas, Tarabalam/Chandrabalam, Nivas/Shool,
/// Anandadi Yoga, Panchaka, and 24-hour chart segments for any global coordinate.
/// </summary>
internal static class VedicExtendedEngine
{
    private const double Deg2Rad = Math.PI / 180.0;
    private const double Rad2Deg = 180.0 / Math.PI;
    private const double NakSpan = 360.0 / 27.0;

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
        ["Bava", "Balava", "Kaulava", "Taitila", "Gara", "Vanija", "Vishti (Bhadra)"];

    private static readonly string[] Rashis =
    [
        "Mesha (Aries)", "Vrishabha (Taurus)", "Mithuna (Gemini)", "Karka (Cancer)",
        "Simha (Leo)", "Kanya (Virgo)", "Tula (Libra)", "Vrischika (Scorpio)",
        "Dhanu (Sagittarius)", "Makara (Capricorn)", "Kumbha (Aquarius)", "Meena (Pisces)"
    ];

    // Standard Varjyam start offsets in Ghatis (out of 60 per Nakshatra span; duration = 4 Ghatis = span / 15).
    // Amrit Kalam follows at (VarjyamStart + 24) % 60 for 4 Ghatis.
    private static readonly double[] VarjyamStartGhati =
    [
        50, 24, 30, 40, 14, 21, 30, 20, 32,
        30, 20, 18, 21, 20, 14, 14, 10, 14,
        20, 24, 20, 10, 10, 18, 16, 24, 30
    ];

    private static readonly string[] TithiLords =
    [
        "Agni", "Brahma", "Gauri", "Ganesha", "Naga", "Kartikeya", "Surya", "Shiva",
        "Durga", "Yama", "Vishwadeva", "Vishnu", "Kamadeva", "Shiva", "Chandra"
    ];

    private static readonly string[] NakshatraLords =
    [
        "Ashwini Kumarau", "Yama", "Agni", "Brahma", "Chandra", "Rudra", "Aditi", "Brihaspati", "Sarpa",
        "Pitras", "Bhaga", "Aryama", "Savitar", "Tvashtar", "Vayu", "Indragni", "Mitra", "Indra",
        "Nirriti", "Apah", "Vishwadeva", "Vishnu", "Vasu", "Varuna", "Aja Ekapada", "Ahir Budhnya", "Pushan"
    ];

    // Chaldean order descending orbital speed: Saturn, Jupiter, Mars, Sun, Venus, Mercury, Moon
    private static readonly (string En, string Sa, string Quality, string BestFor)[] HoraPlanets =
    [
        ("Saturn (Shani)", "शनि", "Ashubh", "Iron, discipline, tough labor, justice"),
        ("Jupiter (Guru)", "गुरु", "Shubh", "Learning, mantras, finance, marriages"),
        ("Mars (Mangala)", "मंगल", "Ashubh", "Debate, competition, engineering, courage"),
        ("Sun (Surya)", "सूर्य", "Moderate", "Authority, government, health, leadership"),
        ("Venus (Shukra)", "शुक्र", "Shubh", "Arts, music, travel, auspicious ceremonies"),
        ("Mercury (Budha)", "बुध", "Shubh", "Trade, writing, accounts, study"),
        ("Moon (Chandra)", "चन्द्र", "Shubh", "Devotion, liquids, calm journeys, home")
    ];

    // Index into HoraPlanets for Day Hora #1 at Sunrise by DayOfWeek (Sunday..Saturday)
    private static readonly int[] SunriseHoraPlanetIdx = [3, 6, 2, 5, 1, 4, 0];

    private static readonly (string Name, string Type)[] DayChogCycle =
    [
        ("Udveg", "Ashubh"), ("Char", "Shubh"), ("Labh", "Shubh"), ("Amrit", "Shubh"),
        ("Kaal", "Ashubh"), ("Shubh", "Shubh"), ("Rog", "Ashubh")
    ];

    private static readonly int[] DayChogStartByWeekday = [0, 3, 6, 2, 5, 1, 4];

    private static readonly string[] DishaShoolByWeekday =
    [
        "West (Avoid westward journey)",
        "East (Avoid eastward journey)",
        "North (Avoid northward journey)",
        "North (Avoid northward journey)",
        "South (Avoid southward journey)",
        "West (Avoid westward journey)",
        "East (Avoid eastward journey)"
    ];

    private static readonly string[] AnandadiCycle =
    [
        "Ananda (Shubh)", "Kaladanda (Ashubh)", "Dhumra (Ashubh)", "Prajapati (Shubh)",
        "Saumya (Shubh)", "Dhwanksha (Ashubh)", "Dhwaja (Shubh)", "Srivatsa (Shubh)",
        "Vajra (Ashubh)", "Mudgara (Ashubh)", "Chhatra (Shubh)", "Mitra (Shubh)",
        "Manasa (Shubh)", "Padma (Shubh)", "Lumbaka (Ashubh)", "Utpata (Ashubh)",
        "Mrityu (Ashubh)", "Kana (Ashubh)", "Siddhi (Shubh)", "Shubha (Shubh)",
        "Amrita (Shubh)", "Musala (Ashubh)", "Gada (Ashubh)", "Matanga (Shubh)",
        "Rakshasa (Ashubh)", "Chara (Shubh)", "Sthira (Shubh)", "Vardhamana (Shubh)"
    ];

    private static readonly int[] AnandadiStartOffset = [0, 24, 17, 13, 10, 6, 3];

    private static readonly string[] DishaByNakShool =
    [
        "East", "East", "East", "South", "South", "South", "North", "East", "West",
        "North", "South", "North", "West", "South", "North", "North", "South", "East",
        "West", "West", "South", "South", "South", "North", "West", "East", "West"
    ];

    private static readonly int[][] DurMuhurtaSlots15 =
    [
        [13],       // Sun: 14th muhurta (0-based 13)
        [8, 11],    // Mon: 9th & 12th
        [3],        // Tue: 4th (+ night 7th handled in text)
        [7],        // Wed: 8th (midday)
        [5, 11],    // Thu: 6th & 12th
        [3, 8],     // Fri: 4th & 9th
        [0, 1]      // Sat: 1st & 2nd
    ];

    /// <summary>
    /// Accurate solar horizon rise/set + Civil (-6°), Nautical (-12°), Astronomical (-18°) dawn
    /// via Meeus solar declination &amp; equation of time.
    /// </summary>
    public static (double SunriseMins, double SunsetMins, double CivilDawn, double NauticalDawn, double AstrDawn) ComputeSunHorizonEvents(
        DateTime dateOnly,
        double latitudeDeg,
        double longitudeDeg,
        double tzOffsetHours)
    {
        double jdNoonLocal = ToJulianDay(dateOnly) + (12.0 - tzOffsetHours) / 24.0;
        double SolveAngle(double zenithDeg, bool rising)
        {
            // Two Newton-style refinement passes for exact solar RA/Decl at the actual crossing instant
            double guessMins = rising ? 360.0 : 1080.0;
            for (int pass = 0; pass < 3; pass++)
            {
                double jd = ToJulianDay(dateOnly) + (guessMins / 60.0 - tzOffsetHours) / 24.0;
                double t = (jd - 2451545.0) / 36525.0;
                var snap = VedicPanchangCalculator.ComputeAstronomicalSnapshot(jd);
                double lam = snap.SunTropicalLon * Deg2Rad;
                double eps = snap.ObliquityDeg * Deg2Rad;

                double raRad = Math.Atan2(Math.Cos(eps) * Math.Sin(lam), Math.Cos(lam));
                double declRad = Math.Asin(Math.Sin(eps) * Math.Sin(lam));

                double l0 = NormalizeDeg(280.46646 + 36000.76983 * t + 0.0003032 * t * t);
                double raDeg = NormalizeDeg(raRad * Rad2Deg);
                double eqTimeMins = 4.0 * Wrap180(l0 - 0.00569 - raDeg);

                double latRad = latitudeDeg * Deg2Rad;
                double cosHa = (Math.Cos(zenithDeg * Deg2Rad) - Math.Sin(latRad) * Math.Sin(declRad))
                    / (Math.Cos(latRad) * Math.Cos(declRad));
                cosHa = Math.Clamp(cosHa, -0.995, 0.995);
                double haDeg = Math.Acos(cosHa) * Rad2Deg;

                double noonMins = 720.0 - 4.0 * longitudeDeg - eqTimeMins + tzOffsetHours * 60.0;
                guessMins = rising ? noonMins - 4.0 * haDeg : noonMins + 4.0 * haDeg;
            }
            return guessMins;
        }

        double sunrise = SolveAngle(90.833, rising: true);
        double sunset = SolveAngle(90.833, rising: false);
        double civil = SolveAngle(96.0, rising: true);
        double nautical = SolveAngle(102.0, rising: true);
        double astr = SolveAngle(108.0, rising: true);
        _ = jdNoonLocal;
        return (sunrise, sunset, civil, nautical, astr);
    }

    public static void PopulateExtendedAndChart(
        PanchangData data,
        DateTime dateOnly,
        PanchangCity city,
        double tzOffsetHours,
        bool isDst,
        double sunriseMins,
        double sunsetMins,
        double nextSunriseMins,
        double civilDawn,
        double nauticalDawn,
        double astrDawn,
        VedicPanchangCalculator.AstronomicalAngles sunriseSnap,
        int tithiIdx,
        int nakshatraIdx,
        int yogaIdx,
        int halfTithi,
        double jdSunrise,
        double jdNextSunrise,
        double jdDayStart,
        double? moonriseMins,
        double? moonsetMins)
    {
        double dayLen = Math.Max(360.0, sunsetMins - sunriseMins);
        double nightLen = Math.Max(360.0, (nextSunriseMins + 1440.0) - sunsetMins);
        double dayMuhurta = dayLen / 15.0;
        double nightMuhurta = nightLen / 15.0;
        double solarNoon = (sunriseMins + sunsetMins) * 0.5;
        double nightMidnight = sunsetMins + nightLen * 0.5;
        double panchangEndMins = nextSunriseMins + 1440.0;
        int weekday = (int)dateOnly.DayOfWeek;

        // 1. Dynamic Muhurtas (ratrimana / 15 & dinamana / 15) + weekday Dur Muhurtam
        data.AbhijitMuhurat = weekday == 3
            ? "None (Avoided on Wednesday)"
            : FormatRange(solarNoon - 0.5 * dayMuhurta, solarNoon + 0.5 * dayMuhurta);
        data.BrahmaMuhurat = FormatRange(sunriseMins - 2.0 * nightMuhurta, sunriseMins - nightMuhurta);

        // 2. Nakshatra-span Varjyam & Amrit Kalam (for active Nakshatra & any next Nakshatra active today)
        var (amritStr, varjyamStr, vishaStart, vishaEnd) = ComputeNakshatraVarjyamAndAmrit(
            jdSunrise, jdDayStart, tzOffsetHours, sunriseMins, panchangEndMins, nakshatraIdx);
        data.AmritKaal = amritStr;

        // 3. Sunrise-to-next-Sunrise Transition Traces
        var tithiTransitions = TraceLimbTransitions(
            jdSunrise, jdDayStart, tzOffsetHours, sunriseMins, panchangEndMins,
            jd => NormalizeDeg(VedicPanchangCalculator.ComputeAstronomicalSnapshot(jd).MoonNirayana - VedicPanchangCalculator.ComputeAstronomicalSnapshot(jd).SunNirayana),
            12.0, 30, idx => FormatTithiName(idx));

        var nakTransitions = TraceLimbTransitions(
            jdSunrise, jdDayStart, tzOffsetHours, sunriseMins, panchangEndMins,
            jd => VedicPanchangCalculator.ComputeAstronomicalSnapshot(jd).MoonNirayana,
            NakSpan, 27, idx => Nakshatras[idx]);

        var yogaTransitions = TraceLimbTransitions(
            jdSunrise, jdDayStart, tzOffsetHours, sunriseMins, panchangEndMins,
            jd =>
            {
                var a = VedicPanchangCalculator.ComputeAstronomicalSnapshot(jd);
                return NormalizeDeg(a.SunNirayana + a.MoonNirayana);
            },
            NakSpan, 27, idx => Yogas[idx]);

        var karanaTransitions = TraceLimbTransitions(
            jdSunrise, jdDayStart, tzOffsetHours, sunriseMins, panchangEndMins,
            jd => NormalizeDeg(VedicPanchangCalculator.ComputeAstronomicalSnapshot(jd).MoonNirayana - VedicPanchangCalculator.ComputeAstronomicalSnapshot(jd).SunNirayana),
            6.0, 60, ComputeKaranaName);

        var moonSignTransitions = TraceLimbTransitions(
            jdSunrise, jdDayStart, tzOffsetHours, sunriseMins, panchangEndMins,
            jd => VedicPanchangCalculator.ComputeAstronomicalSnapshot(jd).MoonNirayana,
            30.0, 12, idx => Rashis[idx]);

        var padaTransitions = TraceLimbTransitions(
            jdSunrise, jdDayStart, tzOffsetHours, sunriseMins, panchangEndMins,
            jd => VedicPanchangCalculator.ComputeAstronomicalSnapshot(jd).MoonNirayana,
            NakSpan / 4.0, 108, idx => $"{Nakshatras[idx / 4]} Pada {(idx % 4) + 1}");

        // 4. Planetary Hora (12 Day + 12 Night)
        var (dayHora, nightHora) = BuildHoras(weekday, sunriseMins, sunsetMins, dayLen, nightLen);

        // 5. 12 Rising Udaya Lagnas & Panchaka Rahita Muhurta
        var lagnas = BuildUdayaLagnas(jdSunrise, jdNextSunrise, jdDayStart, tzOffsetHours, city.Latitude, city.Longitude, panchangEndMins, dateOnly);
        var panchakaRahita = BuildPanchakaRahita(lagnas, tithiIdx, weekday, nakshatraIdx);

        // 6. Extended Metrics, Ritu/Ayana, Nivas/Shool, Tarabalam/Chandrabalam
        double sunElong = NormalizeDeg(sunriseSnap.MoonNirayana - sunriseSnap.SunNirayana);
        int durSlot = DurMuhurtaSlots15[weekday][0];
        var durList = DurMuhurtaSlots15[weekday]
            .Select(s => FormatRange(sunriseMins + s * dayMuhurta, sunriseMins + (s + 1) * dayMuhurta))
            .ToList();
        if (weekday == 2) // Tuesday includes 7th night muhurta (approx night + 6th)
            durList.Add(FormatRange(sunsetMins + 6.0 * nightMuhurta, sunsetMins + 7.0 * nightMuhurta));

        var suryaNakIdx = (int)Math.Floor(sunriseSnap.SunNirayana / NakSpan) % 27;
        var suryaPada = (int)Math.Floor((sunriseSnap.SunNirayana % NakSpan) / (NakSpan / 4.0)) % 4 + 1;

        var moonRashiIdx = (int)Math.Floor(sunriseSnap.MoonNirayana / 30.0) % 12;
        var sunRashiIdx = (int)Math.Floor(sunriseSnap.SunNirayana / 30.0) % 12;
        int anandIdx = (AnandadiStartOffset[weekday] + nakshatraIdx) % 28;

        var kaliyuga = dateOnly.Year + 3101 + (dateOnly.Month >= 4 ? 1 : 0);
        long jdn = (long)Math.Floor(jdSunrise + 0.5);
        long ahargana = jdn - 588465;

        var goodRashis = new List<string>();
        foreach (int offset in new[] { 0, 2, 5, 6, 9, 10 })
            goodRashis.Add(Rashis[(moonRashiIdx + offset) % 12].Split(' ')[0]);

        var goodNakshatras = new List<string>();
        var shubhTaras = new HashSet<int> { 0, 1, 3, 5, 7, 8 }; // Janma/Sampat/Kshema/Sadhana/Mitra/ParamaMitra (0-based offsets mod 9 -> 1,2,4,6,8,9 in 1-based Tarabalam)
        for (int n = 0; n < 27; n++)
        {
            int rel = ((nakshatraIdx - n) % 9 + 9) % 9;
            if (shubhTaras.Contains(rel)) goodNakshatras.Add(Nakshatras[n]);
        }

        // Panchaka status check (Moon in last half of Dhanishta through Revati: Nirayana >= 293.33 deg -> Shatabhisha/PurvaBhadra/UttaraBhadra/Revati/Dhanishta half)
        bool isPanchak = sunriseSnap.MoonNirayana >= 293.333333;
        string panchakLabel = isPanchak ? (weekday switch
        {
            0 => "Roga Panchak (Sun)",
            1 => "Raj Panchak (Mon)",
            2 => "Agni Panchak (Tue)",
            4 => "Mrityu/Nrip Panchak",
            5 => "Chora Panchak (Fri)",
            6 => "Mrityu Panchak (Sat)",
            _ => "Panchaka Active"
        }) : "None (Panchaka Free)";

        string agnivasa = (((tithiIdx + 1) + (weekday + 1) + 1) % 4) switch
        {
            0 or 3 => "Prithvi (Auspicious for Homa)",
            1 => "Akasha (Avoid Homa)",
            _ => "Patala (Avoid Homa)"
        };

        string shivavasa = (tithiIdx + 1) switch
        {
            1 or 8 or 15 or 22 or 29 => "Smashana",
            2 or 9 or 16 or 23 or 30 => "Gauri Sannidhi (Auspicious)",
            3 or 10 or 17 or 24 => "Sabha",
            4 or 11 or 18 or 25 => "Krida",
            5 or 12 or 19 or 26 => "Kailash (Best)",
            6 or 13 or 20 or 27 => "Vrisharudha (Good)",
            _ => "Bhojana"
        };

        string chandravasa = (moonRashiIdx % 4) switch
        {
            0 => "East",
            1 => "South",
            2 => "West",
            _ => "North"
        };

        string rahuVasa = weekday switch
        {
            0 => "North",
            1 => "North-West",
            2 => "West",
            3 => "South-West",
            4 => "South",
            5 => "South-East",
            _ => "East"
        };

        string rituDrik = sunRashiIdx switch
        {
            0 or 11 => "Vasanta (Spring)",
            1 or 2 => "Grishma (Summer)",
            3 or 4 => "Varsha (Monsoon)",
            5 or 6 => "Sharad (Autumn)",
            7 or 8 => "Hemanta (Pre-Winter)",
            _ => "Shishira (Winter)"
        };
        string ayanaDrik = sunriseSnap.SunTropicalLon is >= 90.0 and < 270.0
            ? "Dakshinayana"
            : "Uttarayana";

        data.LocationMeta = new PanchangLocationMeta
        {
            CityId = city.Id,
            DisplayName = city.Name,
            State = city.State,
            Country = city.Country,
            Latitude = city.Latitude,
            Longitude = city.Longitude,
            ElevationMeters = city.ElevationMeters,
            TimeZoneId = city.TimeZoneId,
            EffectiveUtcOffsetHours = tzOffsetHours,
            IsDaylightSavingActive = isDst,
            SunriseMethod = "Sidereal Lahiri / Chitrapaksha at Local Upper-Limb Sunrise (-50' refraction, 0 m elevation)"
        };

        data.DayHora = dayHora;
        data.NightHora = nightHora;
        data.UdayaLagnas = lagnas;
        data.PanchakaRahitaPeriods = panchakaRahita;

        data.Extended = new PanchangExtendedInfo
        {
            LahiriAyanamsaDeg = Math.Round(sunriseSnap.AyanamsaDeg, 6),
            PurnimantaDayBanner = $"{(tithiIdx >= 15 ? tithiIdx - 14 : tithiIdx + 1):D2}, {data.MonthPurnimanta}",
            AmantaDayBanner = $"{((tithiIdx % 15) + 1):D2}, {data.MonthAmanta}",
            Dinamana = FormatDuration(dayLen),
            Ratrimana = FormatDuration(nightLen),
            Madhyahna = FormatTime12(solarNoon),
            CivilDawn = FormatTime12(civilDawn),
            NauticalDawn = FormatTime12(nauticalDawn),
            AstronomicalDawn = FormatTime12(astrDawn),
            Ritu = rituDrik,
            VedicRitu = rituDrik,
            Ayana = ayanaDrik,
            KaliyugaYear = $"{kaliyuga}",
            KaliAhargana = $"{ahargana:N0}",
            JulianDay = $"{jdDayStart:F1}",
            ModifiedJulianDay = $"{(int)Math.Floor(jdDayStart - 2400000.5)}",
            NationalCivilDate = ComputeIndianNationalDate(dateOnly),
            TithiPercentElapsedAtSunrise = Math.Round((sunElong % 12.0) / 12.0 * 100.0, 1),
            NakshatraPercentElapsedAtSunrise = Math.Round((sunriseSnap.MoonNirayana % NakSpan) / NakSpan * 100.0, 1),
            YogaPercentElapsedAtSunrise = Math.Round((NormalizeDeg(sunriseSnap.SunNirayana + sunriseSnap.MoonNirayana) % NakSpan) / NakSpan * 100.0, 1),
            TithiSwami = TithiLords[tithiIdx % 15],
            NakshatraSwami = NakshatraLords[nakshatraIdx],
            YogaSwami = Yogas[yogaIdx],
            SuryaNakshatra = $"{Nakshatras[suryaNakIdx]} (Pada {suryaPada})",
            NextTithi = FormatTithiName((tithiIdx + 1) % 30),
            NextNakshatra = Nakshatras[(nakshatraIdx + 1) % 27],
            NextYoga = Yogas[(yogaIdx + 1) % 27],
            NextKarana = ComputeKaranaName((halfTithi + 1) % 60),
            TithiTransitions = tithiTransitions,
            NakshatraTransitions = nakTransitions,
            YogaTransitions = yogaTransitions,
            KaranaTransitions = karanaTransitions,
            NakshatraPadas = padaTransitions,
            MoonSignTransitions = moonSignTransitions,
            PratahSandhya = FormatRange(sunriseMins - 1.5 * nightMuhurta, sunriseMins),
            VijayaMuhurat = FormatRange(sunriseMins + 10.0 * dayMuhurta, sunriseMins + 11.0 * dayMuhurta),
            GodhuliMuhurat = FormatRange(sunsetMins, sunsetMins + 0.5 * nightMuhurta),
            SayahnaSandhya = FormatRange(sunsetMins, sunsetMins + 1.5 * nightMuhurta),
            NishitaMuhurat = FormatRange(nightMidnight - 0.5 * nightMuhurta, nightMidnight + 0.5 * nightMuhurta),
            RaviYoga = isPanchak ? "Check Sun/Moon Yoga" : "Auspicous Sun-Moon Alignment",
            Varjyam = varjyamStr,
            DurMuhurtam = string.Join(" | ", durList),
            PanchakStatus = panchakLabel,
            Baana = $"{(sunriseSnap.SunNirayana % 5 < 1 ? "Roga" : "Nripa / Agni checks")} (Slot {durSlot + 1})",
            AnandadiYoga = AnandadiCycle[anandIdx],
            TamilYoga = AnandadiCycle[anandIdx].Contains("Shubh") ? "Siddha Yoga (Shubh)" : "Marana / Prabhalarishta Caution",
            DishaShool = DishaShoolByWeekday[weekday],
            NakshatraShool = $"Avoid {DishaByNakShool[nakshatraIdx]} travel during {Nakshatras[nakshatraIdx]}",
            Agnivasa = agnivasa,
            Chandravasa = chandravasa,
            Shivavasa = shivavasa,
            RahuVasa = rahuVasa,
            Homahuti = $"{Nakshatras[nakshatraIdx]} Lord ({NakshatraLords[nakshatraIdx]})",
            ChandrabalamGoodRashis = goodRashis,
            TarabalamGoodNakshatras = goodNakshatras,
            SunriseMinutes = sunriseMins,
            SunsetMinutes = sunsetMins,
            NextSunriseMinutes = panchangEndMins,
            MoonriseMinutes = moonriseMins,
            MoonsetMinutes = moonsetMins,
            Credits =
            [
                new PanchangCalculationCredit
                {
                    Component = "Planetary Ephemeris",
                    Standard = "Jean Meeus Ch. 25 & Ch. 47",
                    Details = "Solar & Lunar apparent tropical longitudes, lunar latitude, horizontal parallax & nutation computed deterministically."
                },
                new PanchangCalculationCredit
                {
                    Component = "Sidereal Ayanamsa",
                    Standard = $"Lahiri / Chitrapaksha ({sunriseSnap.AyanamsaDeg:F6}°)",
                    Details = "True Chitra Spica (180° opposite) sidereal frame evaluated dynamically at each location's Sunrise."
                },
                new PanchangCalculationCredit
                {
                    Component = "Global Timezone & DST",
                    Standard = city.TimeZoneId,
                    Details = $"Resolved via IANA TimeZoneInfo ({FormatUtcOffset(tzOffsetHours)}{(isDst ? ", DST active" : "")}) at {city.Latitude:F4}°, {city.Longitude:F4}°."
                },
                new PanchangCalculationCredit
                {
                    Component = "Horizon & Muhurta Rules",
                    Standard = "Upper-Limb Sunrise & Dinamana/Ratrimana ÷ 15",
                    Details = "Atmospheric refraction -34' + solar semidiameter 16' (-50' zenith 90.833°), Panchang day from Sunrise to next Sunrise."
                }
            ]
        };

        // 7. Build Sunrise-to-next-Sunrise Visual Timeline Graph Payload
        var chart = new PanchangTimelineData
        {
            DayStartMinutes = sunriseMins,
            DayEndMinutes = panchangEndMins,
            SunsetMinutes = sunsetMins,
            TithiBars = ToSegments(tithiTransitions, "Tithi", "neutral", sunriseMins, panchangEndMins),
            NakshatraBars = ToSegments(nakTransitions, "Nakshatra", "neutral", sunriseMins, panchangEndMins),
            YogaBars = ToSegments(yogaTransitions, "Yoga", "neutral", sunriseMins, panchangEndMins),
            KaranaBars = ToSegments(karanaTransitions, "Karana", "neutral", sunriseMins, panchangEndMins)
        };

        double daySlot = dayLen / 8.0;
        AddBand(chart.DoshaBands, "Rahu Kalam", "Inauspicious", "ashubh",
            sunriseMins + VedicSlotRahu(weekday) * daySlot, sunriseMins + (VedicSlotRahu(weekday) + 1) * daySlot, sunriseMins, panchangEndMins);
        AddBand(chart.DoshaBands, "Yamaganda", "Inauspicious", "ashubh",
            sunriseMins + VedicSlotYamaganda(weekday) * daySlot, sunriseMins + (VedicSlotYamaganda(weekday) + 1) * daySlot, sunriseMins, panchangEndMins);
        AddBand(chart.DoshaBands, "Gulika", "Inauspicious", "ashubh",
            sunriseMins + VedicSlotGulika(weekday) * daySlot, sunriseMins + (VedicSlotGulika(weekday) + 1) * daySlot, sunriseMins, panchangEndMins);
        if (vishaStart.HasValue && vishaEnd.HasValue)
            AddBand(chart.DoshaBands, "Varjyam", "Dosha", "ashubh", vishaStart.Value, vishaEnd.Value, sunriseMins, panchangEndMins);

        if (weekday != 3)
            AddBand(chart.AuspiciousBands, "Abhijit", "Auspicious", "shubh", solarNoon - 0.5 * dayMuhurta, solarNoon + 0.5 * dayMuhurta, sunriseMins, panchangEndMins);
        AddBand(chart.AuspiciousBands, "Vijaya", "Auspicious", "shubh", sunriseMins + 10.0 * dayMuhurta, sunriseMins + 11.0 * dayMuhurta, sunriseMins, panchangEndMins);
        AddBand(chart.AuspiciousBands, "Nishita", "Auspicious", "shubh", nightMidnight - 0.5 * nightMuhurta, nightMidnight + 0.5 * nightMuhurta, sunriseMins, panchangEndMins);

        data.Timeline = chart;
    }

    private static (string AmritStr, string VarjyamStr, double? VishaStart, double? VishaEnd) ComputeNakshatraVarjyamAndAmrit(
        double jdSunrise,
        double jdDayStart,
        double tzOffsetHours,
        double sunriseMins,
        double panchangEndMins,
        int nakshatraIdx)
    {
        double targetStartDeg = nakshatraIdx * NakSpan;
        double targetEndDeg = NormalizeDeg((nakshatraIdx + 1) * NakSpan);

        // Find backward start & forward end of current Nakshatra
        double jdStart = FindCrossingBackward(jdSunrise, targetStartDeg,
            jd => VedicPanchangCalculator.ComputeAstronomicalSnapshot(jd).MoonNirayana);
        double jdEnd = FindCrossingForward(jdSunrise, targetEndDeg,
            jd => VedicPanchangCalculator.ComputeAstronomicalSnapshot(jd).MoonNirayana);

        double startMins = (jdStart - jdDayStart) * 1440.0 + tzOffsetHours * 60.0;
        double endMins = (jdEnd - jdDayStart) * 1440.0 + tzOffsetHours * 60.0;
        double dur = Math.Max(600.0, endMins - startMins);

        double vGhati = VarjyamStartGhati[nakshatraIdx];
        double aGhati = (vGhati + 24.0) % 60.0;

        double vStart = startMins + (vGhati / 60.0) * dur;
        double vEnd = vStart + (4.0 / 60.0) * dur;
        double aStart = startMins + (aGhati / 60.0) * dur;
        double aEnd = aStart + (4.0 / 60.0) * dur;

        // Also check next Nakshatra's Amrit/Varjyam if current ended well before next sunrise
        string FormatClamped(double s, double e, string fallbackLabel)
        {
            if (e >= sunriseMins && s <= panchangEndMins)
                return FormatRange(s, e);
            // Try next Nakshatra
            int nextNak = (nakshatraIdx + 1) % 27;
            double nextStart = endMins;
            double nextEnd = nextStart + 1440.0;
            double g = fallbackLabel == "Varjyam" ? VarjyamStartGhati[nextNak] : (VarjyamStartGhati[nextNak] + 24.0) % 60.0;
            double ns = nextStart + (g / 60.0) * (nextEnd - nextStart);
            double ne = ns + (4.0 / 60.0) * (nextEnd - nextStart);
            return ne >= sunriseMins && ns <= panchangEndMins ? FormatRange(ns, ne) : FormatRange(s, e);
        }

        var amritDisplay = FormatClamped(aStart, aEnd, "Amrit");
        var varjyamDisplay = FormatClamped(vStart, vEnd, "Varjyam");
        return (amritDisplay, varjyamDisplay, vStart, vEnd);
    }

    private static List<PanchangTransitionItem> TraceLimbTransitions(
        double jdSunrise,
        double jdDayStart,
        double tzOffsetHours,
        double sunriseMins,
        double panchangEndMins,
        Func<double, double> metricFn,
        double stepDeg,
        int modCount,
        Func<int, string> labelFn)
    {
        var items = new List<PanchangTransitionItem>();
        double curStartMins = sunriseMins;
        double curJd = jdSunrise;

        for (int guard = 0; guard < 8 && curStartMins < panchangEndMins - 1.0; guard++)
        {
            double angle = NormalizeDeg(metricFn(curJd));
            int idx = ((int)Math.Floor(angle / stepDeg) % modCount + modCount) % modCount;
            double nextTarget = NormalizeDeg((idx + 1) * stepDeg);
            double crossJd = FindCrossingForward(curJd, nextTarget, metricFn);
            double endMins = (crossJd - jdDayStart) * 1440.0 + tzOffsetHours * 60.0;

            bool fullNight = endMins >= panchangEndMins - 1.0;
            double clampedEnd = fullNight ? panchangEndMins : endMins;

            items.Add(new PanchangTransitionItem
            {
                Name = labelFn(idx),
                StartMinutesFromMidnight = curStartMins,
                EndMinutesFromMidnight = clampedEnd,
                IsFullNight = fullNight,
                TimeRangeDisplay = FormatRange(curStartMins, clampedEnd),
                EndDisplay = fullNight ? "Full Night" : FormatEndLabel(endMins)
            });

            if (fullNight) break;
            curStartMins = endMins;
            curJd = crossJd + (2.0 / 1440.0); // nudge +2 mins cleanly past exact cusp
        }

        return items;
    }

    private static (List<PanchangHoraPeriod> Day, List<PanchangHoraPeriod> Night) BuildHoras(
        int weekday,
        double sunriseMins,
        double sunsetMins,
        double dayLen,
        double nightLen)
    {
        int startPlanet = SunriseHoraPlanetIdx[weekday];
        double daySlot = dayLen / 12.0;
        double nightSlot = nightLen / 12.0;

        var day = new List<PanchangHoraPeriod>(12);
        var night = new List<PanchangHoraPeriod>(12);

        for (int i = 0; i < 12; i++)
        {
            var pDay = HoraPlanets[(startPlanet + i) % 7];
            double ds = sunriseMins + i * daySlot;
            double de = ds + daySlot;
            day.Add(new PanchangHoraPeriod
            {
                Planet = pDay.En,
                PlanetSanskrit = pDay.Sa,
                Quality = pDay.Quality,
                BestFor = pDay.BestFor,
                StartMinutes = ds,
                EndMinutes = de,
                TimeRange = FormatRange(ds, de)
            });

            var pNight = HoraPlanets[(startPlanet + 12 + i) % 7];
            double ns = sunsetMins + i * nightSlot;
            double ne = ns + nightSlot;
            night.Add(new PanchangHoraPeriod
            {
                Planet = pNight.En,
                PlanetSanskrit = pNight.Sa,
                Quality = pNight.Quality,
                BestFor = pNight.BestFor,
                StartMinutes = ns,
                EndMinutes = ne,
                TimeRange = FormatRange(ns, ne)
            });
        }

        return (day, night);
    }

    private static List<PanchangLagnaPeriod> BuildUdayaLagnas(
        double jdSunrise,
        double jdNextSunrise,
        double jdDayStart,
        double tzOffsetHours,
        double latDeg,
        double lonDeg,
        double panchangEndMins,
        DateTime dateOnly)
    {
        var lagnas = new List<PanchangLagnaPeriod>(14);
        double curStartMins = (jdSunrise - jdDayStart) * 1440.0 + tzOffsetHours * 60.0;
        double curJd = jdSunrise;

        double AscDeg(double jd)
        {
            double d = jd - 2451545.0;
            double gmst = NormalizeDeg(280.46061837 + 360.98564736629 * d);
            double lstRad = NormalizeDeg(gmst + lonDeg) * Deg2Rad;
            var snap = VedicPanchangCalculator.ComputeAstronomicalSnapshot(jd);
            double epsRad = snap.ObliquityDeg * Deg2Rad;
            double latRad = Math.Clamp(latDeg, -66.0, 66.0) * Deg2Rad;

            double trop = Math.Atan2(
                Math.Cos(lstRad),
                -(Math.Sin(lstRad) * Math.Cos(epsRad) + Math.Tan(latRad) * Math.Sin(epsRad))) * Rad2Deg;
            return NormalizeDeg(trop - snap.AyanamsaDeg);
        }

        for (int guard = 0; guard < 14 && curStartMins < panchangEndMins - 1.0 && curJd < jdNextSunrise; guard++)
        {
            double asc = AscDeg(curJd);
            int signIdx = (int)Math.Floor(asc / 30.0) % 12;
            double targetDeg = NormalizeDeg((signIdx + 1) * 30.0);

            // Bisection forward over ~3h max (every zodiac sign rises inside < 160 mins away from poles)
            double stepJd = 10.0 / 1440.0;
            double prevRem = NormalizeDeg(targetDeg - asc);
            double lo = curJd;
            double hi = curJd;
            bool found = false;
            for (int k = 1; k <= 24; k++)
            {
                double test = curJd + k * stepJd;
                double curRem = NormalizeDeg(targetDeg - AscDeg(test));
                if (prevRem < 55.0 && curRem > 305.0)
                {
                    lo = curJd + (k - 1) * stepJd;
                    hi = test;
                    found = true;
                    break;
                }
                prevRem = curRem;
            }

            double endMins = panchangEndMins;
            double nextJd = jdNextSunrise;
            if (found)
            {
                for (int b = 0; b < 14; b++)
                {
                    double mid = 0.5 * (lo + hi);
                    if (NormalizeDeg(targetDeg - AscDeg(mid)) < 180.0) lo = mid;
                    else hi = mid;
                }
                nextJd = 0.5 * (lo + hi);
                endMins = Math.Min(panchangEndMins, (nextJd - jdDayStart) * 1440.0 + tzOffsetHours * 60.0);
            }

            lagnas.Add(new PanchangLagnaPeriod
            {
                Name = Rashis[signIdx],
                Quality = signIdx is 1 or 2 or 3 or 5 or 6 or 8 or 11 ? "Shubh / Benefic" : "Standard",
                StartMinutes = curStartMins,
                EndMinutes = endMins,
                TimeRange = FormatRange(curStartMins, endMins)
            });

            if (!found || endMins >= panchangEndMins - 1.0) break;
            curStartMins = endMins;
            curJd = nextJd + (1.0 / 1440.0);
        }

        _ = dateOnly;
        return lagnas;
    }

    private static List<PanchangLagnaPeriod> BuildPanchakaRahita(
        List<PanchangLagnaPeriod> lagnas,
        int tithiIdx,
        int weekday,
        int nakshatraIdx)
    {
        var result = new List<PanchangLagnaPeriod>(lagnas.Count);
        int t = tithiIdx + 1;
        int w = weekday + 1;
        int n = nakshatraIdx + 1;

        for (int i = 0; i < lagnas.Count; i++)
        {
            int l = i + 1;
            int sum = (t + w + n + l) % 9;
            string q = sum switch
            {
                1 => "Mrityu Panchaka (Avoid)",
                2 => "Agni Panchaka (Avoid fire/construction)",
                4 => "Raja Panchaka (Mixed)",
                6 => "Chora Panchaka (Caution travel)",
                8 => "Roga Panchaka (Avoid)",
                _ => "Good Muhurta (Panchaka Free)"
            };
            result.Add(new PanchangLagnaPeriod
            {
                Name = lagnas[i].Name.Split(' ')[0],
                Quality = q,
                StartMinutes = lagnas[i].StartMinutes,
                EndMinutes = lagnas[i].EndMinutes,
                TimeRange = lagnas[i].TimeRange
            });
        }
        return result;
    }

    private static double FindCrossingForward(double jdStart, double targetDeg, Func<double, double> metricFn)
    {
        const double step = 0.02;
        double prevJd = jdStart;
        double prevRem = NormalizeDeg(targetDeg - NormalizeDeg(metricFn(jdStart)));

        for (int i = 1; i <= 90; i++)
        {
            double curJd = jdStart + i * step;
            double curRem = NormalizeDeg(targetDeg - NormalizeDeg(metricFn(curJd)));
            if (prevRem < 45.0 && curRem > 315.0)
            {
                double lo = prevJd;
                double hi = curJd;
                for (int b = 0; b < 16; b++)
                {
                    double mid = 0.5 * (lo + hi);
                    if (NormalizeDeg(targetDeg - NormalizeDeg(metricFn(mid))) < 180.0) lo = mid;
                    else hi = mid;
                }
                return 0.5 * (lo + hi);
            }
            prevJd = curJd;
            prevRem = curRem;
        }
        return jdStart + 1.5;
    }

    private static double FindCrossingBackward(double jdEnd, double targetDeg, Func<double, double> metricFn)
    {
        const double step = 0.02;
        double curJd = jdEnd;
        double nextDist = NormalizeDeg(NormalizeDeg(metricFn(jdEnd)) - targetDeg);

        for (int i = 1; i <= 90; i++)
        {
            double prevJd = jdEnd - i * step;
            double prevDist = NormalizeDeg(NormalizeDeg(metricFn(prevJd)) - targetDeg);
            if (nextDist < 45.0 && prevDist > 315.0)
            {
                double lo = prevJd;
                double hi = curJd;
                for (int b = 0; b < 16; b++)
                {
                    double mid = 0.5 * (lo + hi);
                    if (NormalizeDeg(NormalizeDeg(metricFn(mid)) - targetDeg) > 180.0) lo = mid;
                    else hi = mid;
                }
                return 0.5 * (lo + hi);
            }
            curJd = prevJd;
            nextDist = prevDist;
        }
        return jdEnd - 1.0;
    }

    private static List<PanchangTimelineSegment> ToSegments(
        List<PanchangTransitionItem> items,
        string category,
        string tone,
        double dayStart,
        double dayEnd)
    {
        var list = new List<PanchangTimelineSegment>(items.Count);
        double total = Math.Max(60.0, dayEnd - dayStart);
        foreach (var item in items)
        {
            double s = Math.Clamp(item.StartMinutesFromMidnight, dayStart, dayEnd);
            double e = Math.Clamp(item.EndMinutesFromMidnight, s, dayEnd);
            list.Add(new PanchangTimelineSegment
            {
                Label = item.Name,
                Category = category,
                Tone = tone,
                StartMinutes = s,
                EndMinutes = e,
                StartPercent = Math.Round((s - dayStart) / total * 100.0, 2),
                WidthPercent = Math.Round(Math.Max(1.0, (e - s) / total * 100.0), 2),
                TimeDisplay = item.IsFullNight ? "Full Night" : item.EndDisplay
            });
        }
        return list;
    }

    private static void AddBand(
        List<PanchangTimelineSegment> target,
        string label,
        string category,
        string tone,
        double startMins,
        double endMins,
        double dayStart,
        double dayEnd)
    {
        if (endMins <= dayStart || startMins >= dayEnd) return;
        double s = Math.Clamp(startMins, dayStart, dayEnd);
        double e = Math.Clamp(endMins, s, dayEnd);
        double total = Math.Max(60.0, dayEnd - dayStart);
        target.Add(new PanchangTimelineSegment
        {
            Label = label,
            Category = category,
            Tone = tone,
            StartMinutes = s,
            EndMinutes = e,
            StartPercent = Math.Round((s - dayStart) / total * 100.0, 2),
            WidthPercent = Math.Round(Math.Max(1.0, (e - s) / total * 100.0), 2),
            TimeDisplay = FormatRange(s, e)
        });
    }

    private static string ComputeIndianNationalDate(DateTime d)
    {
        // Saka National Civil Calendar (Saka Samvat + 1-indexed Chaitra=22 Mar / 21 Mar leap)
        bool leap = DateTime.IsLeapYear(d.Year);
        int dayOfYear = d.DayOfYear;
        int chaitraStartDoy = leap ? 81 : 82;
        int sakaYear = dayOfYear < chaitraStartDoy ? d.Year - 79 : d.Year - 78;
        int diff = dayOfYear >= chaitraStartDoy
            ? dayOfYear - chaitraStartDoy
            : dayOfYear + (DateTime.IsLeapYear(d.Year - 1) ? 366 : 365) - (DateTime.IsLeapYear(d.Year - 1) ? 81 : 82);

        (string Name, int Days)[] months =
        [
            ("Chaitra", leap ? 31 : 30), ("Vaisakha", 31), ("Jyaishtha", 31),
            ("Ashadha", 31), ("Shravana", 31), ("Bhadra", 31),
            ("Ashvina", 30), ("Kartika", 30), ("Agrahayana", 30),
            ("Pausha", 30), ("Magha", 30), ("Phalguna", 30)
        ];

        int rem = diff;
        foreach (var (name, len) in months)
        {
            if (rem < len)
                return $"{name} {(rem + 1):D2}, {sakaYear}";
            rem -= len;
        }
        return $"Phalguna 30, {sakaYear}";
    }

    private static int VedicSlotRahu(int weekday) => weekday switch { 0 => 7, 1 => 1, 2 => 6, 3 => 4, 4 => 5, 5 => 3, _ => 2 };
    private static int VedicSlotYamaganda(int weekday) => weekday switch { 0 => 4, 1 => 3, 2 => 2, 3 => 1, 4 => 0, 5 => 6, _ => 5 };
    private static int VedicSlotGulika(int weekday) => weekday switch { 0 => 6, 1 => 5, 2 => 4, 3 => 3, 4 => 2, 5 => 1, _ => 0 };

    private static string FormatTithiName(int tithiIdx)
    {
        string prefix = tithiIdx < 15 ? "Shukla" : "Krishna";
        string baseName = tithiIdx == 14 ? "Purnima" : tithiIdx == 29 ? "Amavasya" : TithiNames[tithiIdx % 15];
        return $"{prefix} {baseName}";
    }

    private static string ComputeKaranaName(int halfTithi) => halfTithi switch
    {
        0 => "Kimstughna",
        57 => "Shakuni",
        58 => "Chatushpada",
        59 => "Nagava",
        _ => MovableKaranas[(halfTithi - 1) % 7]
    };

    private static string FormatDuration(double mins)
    {
        int totalSecs = (int)Math.Round(mins * 60.0);
        int h = totalSecs / 3600;
        int m = (totalSecs % 3600) / 60;
        int s = totalSecs % 60;
        return $"{h:D2}h {m:D2}m {s:D2}s";
    }

    private static string FormatRange(double s, double e) => $"{FormatTime12(s)} - {FormatTime12(e)}";

    private static string FormatEndLabel(double mins) =>
        mins >= 1440.0 ? $"{FormatTime12(mins)} (+1d)" : FormatTime12(mins);

    private static string FormatTime12(double totalMinutes)
    {
        int rounded = ((int)Math.Round(totalMinutes) % 1440 + 1440) % 1440;
        int h24 = rounded / 60;
        int m = rounded % 60;
        string suffix = h24 >= 12 ? "PM" : "AM";
        int h12 = h24 % 12;
        if (h12 == 0) h12 = 12;
        return $"{h12:D2}:{m:D2} {suffix}";
    }

    private static string FormatUtcOffset(double hours)
    {
        int totalMins = (int)Math.Round(hours * 60.0);
        char sign = totalMins >= 0 ? '+' : '-';
        int abs = Math.Abs(totalMins);
        return $"UTC{sign}{abs / 60:D2}:{abs % 60:D2}";
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

    private static double Wrap180(double deg)
    {
        double res = NormalizeDeg(deg);
        return res > 180.0 ? res - 360.0 : res;
    }
}
