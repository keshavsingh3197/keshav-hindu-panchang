using HinduPanchang.Api.Models;

namespace HinduPanchang.Api.Services;

public class CulturalContentService
{
    private static readonly List<HistoricalIncident> MockIncidents = new()
    {
        new HistoricalIncident
        {
            Id = "inc-1",
            DateKey = "11-12",
            Year = 1930,
            Title = "First Round Table Conference Inaugurated",
            Category = "Indian History",
            Description = "The first of three Round Table Conferences convened in London to discuss constitutional reforms in India.",
            CulturalSignificance = "Marked an epochal milestone in India's struggle for self-governance and constitutional sovereignty."
        },
        new HistoricalIncident
        {
            Id = "inc-2",
            DateKey = "11-12",
            Year = 1896,
            Title = "Birth of Dr. Salim Ali (Birdman of India)",
            Category = "Science & Ecology",
            Description = "Birth anniversary of natural historian Dr. Salim Ali, who conducted systematic bird surveys across India and pioneered wildlife conservation.",
            CulturalSignificance = "Revered across India as a symbol of compassionate stewardship of nature and living beings."
        },
        new HistoricalIncident
        {
            Id = "inc-3",
            DateKey = "11-12",
            Year = 1469,
            Title = "Sacred Prakash Parv & Guru Nanak Dev Ji Teachings",
            Category = "Spiritual Saint",
            Description = "Observance of the universal message of 'Ik Onkar' (One Divine Creator), equality, and selfless service (Sewa).",
            CulturalSignificance = "Deep spiritual reverence uniting devotees across India in langar and naam-simran."
        },
        new HistoricalIncident
        {
            Id = "inc-4",
            DateKey = "11-13",
            Year = 1780,
            Title = "Maharaja Ranjit Singh Birth Anniversary",
            Category = "Indian History",
            Description = "Birth of the legendary Maharaja Ranjit Singh, founder of the Sikh Empire known as the 'Sher-e-Punjab' (Lion of Punjab).",
            CulturalSignificance = "Protected sacred temples, donated gold for the Golden Temple in Amritsar, and fostered communal harmony."
        }
    };

    private static readonly List<FestivalItem> UpcomingFestivals = new()
    {
        new FestivalItem
        {
            Id = "f-1",
            NameHindi = "देवउठनी एकादशी (प्रबोधिनी एकादशी)",
            NameEnglish = "Devutthana Ekadashi",
            Date = DateTime.UtcNow.AddDays(2),
            Tithi = "Shukla Paksha Ekadashi",
            MonthVedic = "Kartika",
            Description = "Marks the awakening of Lord Vishnu after four months of divine slumber (Chaturmas). Auspicious ceremonies (Vivah Muhurats) commence.",
            RitualsAndPujaVidhi = "Devotees observe strict fasting, perform Tulsi Vivah with Shaligram, and light sugarcane mandaps with diyas.",
            FastingRules = "Falahar fast, grains and legumes abstained.",
            IsMajorFestival = true
        },
        new FestivalItem
        {
            Id = "f-2",
            NameHindi = "तुलसी विवाह",
            NameEnglish = "Tulsi Vivah",
            Date = DateTime.UtcNow.AddDays(3),
            Tithi = "Shukla Paksha Dwadashi",
            MonthVedic = "Kartika",
            Description = "Ceremonial marriage of the sacred Tulsi plant (incarnation of Vrinda) with Lord Shaligram (Vishnu).",
            RitualsAndPujaVidhi = "Decorating Tulsi pot with turmeric, vermillion, saree, and singing Vedic mangalashtak hymns.",
            FastingRules = "Optional fast till twilight puja.",
            IsMajorFestival = true
        },
        new FestivalItem
        {
            Id = "f-3",
            NameHindi = "कार्तिक पूर्णिमा (देव दीपावली)",
            NameEnglish = "Kartik Purnima & Dev Deepawali",
            Date = DateTime.UtcNow.AddDays(5),
            Tithi = "Purnima",
            MonthVedic = "Kartika",
            Description = "Celebration of Lord Shiva's victory over the demon Tripurasura. The ghats of Varanasi are illuminated with millions of earthen lamps.",
            RitualsAndPujaVidhi = "Holy dip (Snan) in sacred rivers at sunrise and lighting lamps at temples and water bodies.",
            FastingRules = "Full day fast until Chandra darshan.",
            IsMajorFestival = true
        }
    };

    public Task<List<HistoricalIncident>> GetIncidentsForDateAsync(DateTime date)
    {
        string key = date.ToString("MM-dd");
        var matches = MockIncidents.Where(i => i.DateKey == key).ToList();
        if (!matches.Any())
        {
            matches = MockIncidents.Take(3).ToList();
        }
        return Task.FromResult(matches);
    }

    public Task<DailyKatha> GetDailyKathaAsync(DateTime date)
    {
        var katha = new DailyKatha
        {
            Id = "katha-" + date.ToString("yyyyMMdd"),
            Title = "The Power of Truth & Perseverance: Raja Harishchandra",
            DeityOrSage = "Maharishi Vishwamitra & Raja Harishchandra",
            StorySummary = "King Harishchandra gave away his entire kingdom, wealth, and worldly possessions to fulfill a promise and honor the sanctity of truth (Satya). Despite severe personal loss and testing by sage Vishwamitra, he never wavered from dharma.",
            MoralTeaching = "Truth (Satya) and Integrity are the ultimate foundation of righteous life. Material riches fade, but the honor of dharma endures through eternity.",
            SanskritMantra = "सत्यमेव जयते नानृतम् (Truth alone triumphs, not untruth)."
        };
        return Task.FromResult(katha);
    }

    public Task<List<FestivalItem>> GetUpcomingFestivalsAsync()
    {
        return Task.FromResult(UpcomingFestivals);
    }
}
