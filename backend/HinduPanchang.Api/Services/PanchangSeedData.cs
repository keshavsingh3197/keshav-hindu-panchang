using HinduPanchang.Api.Models;

namespace HinduPanchang.Api.Services;

public static class PanchangSeedData
{
    public static List<PanchangCity> DefaultCities() =>
    [
        new() { Id = "city-del", Name = "New Delhi, India", LabelHindi = "नई दिल्ली (New Delhi)", Country = "India", Latitude = 28.6139, Longitude = 77.2090, TimezoneOffsetHours = 5.5, IsActive = true },
        new() { Id = "city-vns", Name = "Varanasi, India", LabelHindi = "काशी / वाराणसी (Varanasi)", Country = "India", Latitude = 25.3176, Longitude = 82.9739, TimezoneOffsetHours = 5.5, IsActive = true },
        new() { Id = "city-ayo", Name = "Ayodhya, India", LabelHindi = "अयोध्या (Ayodhya)", Country = "India", Latitude = 26.7922, Longitude = 82.1998, TimezoneOffsetHours = 5.5, IsActive = true },
        new() { Id = "city-bom", Name = "Mumbai, India", LabelHindi = "मुंबई (Mumbai)", Country = "India", Latitude = 19.0760, Longitude = 72.8777, TimezoneOffsetHours = 5.5, IsActive = true },
        new() { Id = "city-blr", Name = "Bengaluru, India", LabelHindi = "बेंगलुरु (Bengaluru)", Country = "India", Latitude = 12.9716, Longitude = 77.5946, TimezoneOffsetHours = 5.5, IsActive = true },
        new() { Id = "city-jai", Name = "Jaipur, India", LabelHindi = "जयपुर (Jaipur)", Country = "India", Latitude = 26.9124, Longitude = 75.7873, TimezoneOffsetHours = 5.5, IsActive = true },
        new() { Id = "city-lon", Name = "London, UK", LabelHindi = "लंदन (London, UK)", Country = "UK", Latitude = 51.5072, Longitude = -0.1276, TimezoneOffsetHours = 0.0, IsActive = true },
        new() { Id = "city-nyc", Name = "New York, USA", LabelHindi = "न्यूयॉर्क (New York, USA)", Country = "USA", Latitude = 40.7128, Longitude = -74.0060, TimezoneOffsetHours = -5.0, IsActive = true }
    ];

    public static List<DailySutra> DefaultSutras() =>
    [
        new() { Id = "sut-1", SanskritText = "कर्मण्येवाधिकारस्ते मा फलेषु कदाचन । मा कर्मफलहेतुर्भूर्मा ते सङ्गोऽस्त्वकर्मणि ॥", EnglishMeaning = "You have a right to perform your prescribed duty, but you are not entitled to the fruits of actions.", Source = "Bhagavad Gita 2.47" },
        new() { Id = "sut-2", SanskritText = "सर्वे भवन्तु सुखिनः सर्वे सन्तु निरामयाः । सर्वे भद्राणि पश्यन्तु मा कश्चिद्दुःखभाग्भवेत् ॥", EnglishMeaning = "May all sentient beings be at peace, free from illness, witness auspiciousness, and suffer no sorrow.", Source = "Brihadaranyaka Upanishad" },
        new() { Id = "sut-3", SanskritText = "सत्यमेव जयते नानृतं सत्येन पन्था विततो देवयानः ।", EnglishMeaning = "Truth alone triumphs, not falsehood; through truth lies the divine path of light.", Source = "Mundaka Upanishad 3.1.6" },
        new() { Id = "sut-4", SanskritText = "योगः कर्मसु कौशलम् ।", EnglishMeaning = "Yoga is excellence and mindfulness in every action you undertake.", Source = "Bhagavad Gita 2.50" },
        new() { Id = "sut-5", SanskritText = "असतो मा सद्गमय । तमसो मा ज्योतिर्गमय । मृत्योर्मा अमृतं गमय ॥", EnglishMeaning = "Lead us from the unreal to the real, from darkness to light, from mortality to immortality.", Source = "Brihadaranyaka Upanishad 1.3.28" },
        new() { Id = "sut-6", SanskritText = "विद्या ददाति विनयं विनयाद् याति पात्रताम् ।", EnglishMeaning = "True knowledge bestows humility, and from humility comes worthiness and lasting prosperity.", Source = "Hitopadesha" },
        new() { Id = "sut-7", SanskritText = "उद्यमेन हि सिध्यन्ति कार्याणि न मनोरथैः ।", EnglishMeaning = "Works are accomplished through dedicated effort, never by mere wishes alone.", Source = "Panchatantra" }
    ];

    public static List<HistoricalIncident> DefaultIncidents() =>
    [
        new() { Id = "inc-1", DateKey = "11-12", Year = 1930, Title = "First Round Table Conference Inaugurated", Category = "Indian History", Description = "The first of three Round Table Conferences convened in London to deliberate constitutional reforms for India.", CulturalSignificance = "Milestone on India's journey toward self-governance and constitutional sovereignty." },
        new() { Id = "inc-2", DateKey = "11-12", Year = 1896, Title = "Birth of Dr. Salim Ali (Birdman of India)", Category = "Science & Astronomy", Description = "Birth anniversary of pioneering Indian ornithologist and naturalist Dr. Salim Ali.", CulturalSignificance = "Celebrated symbol of reverence for nature and ecological conservation." },
        new() { Id = "inc-3", DateKey = "11-13", Year = 1780, Title = "Maharaja Ranjit Singh Birth Anniversary", Category = "Indian History", Description = "Birth of Maharaja Ranjit Singh, founder of the sovereign Sikh Empire ('Sher-e-Punjab').", CulturalSignificance = "Renowned for protecting sacred shrines, temple patronage, and cultural harmony." },
        new() { Id = "inc-4", DateKey = "11-14", Year = 1889, Title = "Bal Diwas & Pandit Jawaharlal Nehru Jayanti", Category = "Indian History", Description = "Observed across India as Children's Day celebrating education, curiosity, and youth empowerment.", CulturalSignificance = "Highlights the nurturing of future scholars and builders of the nation." },
        new() { Id = "inc-5", DateKey = "11-15", Year = 1875, Title = "Janjatiya Gaurav Divas — Birsa Munda Jayanti", Category = "Freedom Struggle", Description = "Birth anniversary of tribal freedom fighter and folk hero Bhagwan Birsa Munda.", CulturalSignificance = "Honors indigenous stewardship, courage, and India's grassroots resistance." },
        new() { Id = "inc-6", DateKey = "11-19", Year = 1828, Title = "Rani Lakshmibai of Jhansi Birth Anniversary", Category = "Freedom Struggle", Description = "Birth of Veerangana Rani Lakshmibai in Varanasi, one of the foremost leaders of India's 1857 war of independence.", CulturalSignificance = "Eternal emblem of valor, self-respect, and Matrubhoomi devotion." },
        new() { Id = "inc-7", DateKey = "12-22", Year = 1887, Title = "National Mathematics Day — Srinivasa Ramanujan Jayanti", Category = "Science & Astronomy", Description = "Birth anniversary of mathematical genius Srinivasa Ramanujan in Erode, Tamil Nadu.", CulturalSignificance = "Honors India's deep mathematical and astronomical tradition." },
        new() { Id = "inc-8", DateKey = "01-12", Year = 1863, Title = "National Youth Day — Swami Vivekananda Jayanti", Category = "Spiritual Saint", Description = "Birth anniversary of Swami Vivekananda, who carried Vedanta and Yoga to the Parliament of Religions.", CulturalSignificance = "Awakens spiritual strength, selfless service, and confident youth leadership." },
        new() { Id = "inc-9", DateKey = "08-15", Year = 1947, Title = "India's Independence Day (Swatantrata Diwas)", Category = "Freedom Struggle", Description = "India attained sovereignty and began its modern democratic journey.", CulturalSignificance = "Tribute to millions of freedom fighters and cultural renaissance." },
        new() { Id = "inc-10", DateKey = "12-31", Year = 1600, Title = "Royal Charter of 1600 Signed — Turning Point in Indian Trade History", Category = "Indian History", Description = "Queen Elizabeth I granted the charter that reshaped maritime trade corridors across the Indian Ocean.", CulturalSignificance = "Historical reminder of India's vast pre-colonial global trade and maritime heritage." },
        new() { Id = "inc-11", DateKey = "01-23", Year = 1897, Title = "Parakram Diwas — Netaji Subhas Chandra Bose Jayanti", Category = "Freedom Struggle", Description = "Birth anniversary of Netaji Subhas Chandra Bose in Cuttack, founder of the Azad Hind Fauj.", CulturalSignificance = "Celebrates fearless patriotism, disciplined organization, and devotion to freedom." },
        new() { Id = "inc-12", DateKey = "01-26", Year = 1950, Title = "Republic Day — Constitution of India Adopted", Category = "Indian History", Description = "The Constitution of India came into effect establishing a sovereign, democratic republic.", CulturalSignificance = "Inspired by ancient republic traditions of Vaishali and modern constitutional justice." },
        new() { Id = "inc-13", DateKey = "02-28", Year = 1928, Title = "National Science Day — Discovery of the Raman Effect", Category = "Science & Astronomy", Description = "Sir C.V. Raman announced the discovery of light scattering (Raman Effect), earning the 1930 Nobel Prize in Physics.", CulturalSignificance = "Inspires scientific temper grounded in India's millennia-old inquiry tradition." },
        new() { Id = "inc-14", DateKey = "04-14", Year = 1891, Title = "Dr. B.R. Ambedkar Jayanti & Solar Mesha Sankranti Tradition", Category = "Indian History", Description = "Birth anniversary of chief architect of the Indian Constitution, Dr. Bhimrao Ramji Ambedkar.", CulturalSignificance = "Advocates equality, education, and constitutional fraternity for every citizen." },
        new() { Id = "inc-15", DateKey = "05-11", Year = 1998, Title = "National Technology Day — Pokhran-II Scientific Milestone", Category = "Science & Astronomy", Description = "India demonstrated self-reliant aerospace and strategic engineering capabilities.", CulturalSignificance = "Recognizes indigenous Indian R&D across space, defence, and clean technology." },
        new() { Id = "inc-16", DateKey = "09-05", Year = 1888, Title = "Teachers' Day — Dr. Sarvepalli Radhakrishnan Jayanti", Category = "Spiritual Saint", Description = "Birth anniversary of philosopher-president Dr. Radhakrishnan, renowned interpreter of the Upanishads & Gita.", CulturalSignificance = "Honors the Guru-Shishya tradition and lifelong illumination through learning." },
        new() { Id = "inc-17", DateKey = "10-02", Year = 1869, Title = "Gandhi Jayanti & Lal Bahadur Shastri Jayanti", Category = "Freedom Struggle", Description = "Birth anniversaries of Mahatma Gandhi (Ahimsa & Satyagraha) and PM Lal Bahadur Shastri (Jai Jawan Jai Kisan).", CulturalSignificance = "Embodies ethical leadership, simplicity, and service to rural & national strength." },
        new() { Id = "inc-18", DateKey = "10-15", Year = 1931, Title = "World Students' Day — Dr. A.P.J. Abdul Kalam Jayanti", Category = "Science & Astronomy", Description = "Birth anniversary of the Missile Man and People's President Dr. A.P.J. Abdul Kalam.", CulturalSignificance = "Harmonizes Gita/Tirukkural wisdom with aerospace engineering and youthful dreaming." }
    ];

    public static List<DailyKatha> DefaultKathas() =>
    [
        new() { Id = "katha-1", Title = "The Power of Truth & Perseverance: Raja Harishchandra", DeityOrSage = "Maharishi Vishwamitra & Raja Harishchandra", StorySummary = "King Harishchandra surrendered his kingdom and endured extreme hardship to uphold a spoken vow of Truth (Satya).", MoralTeaching = "Satya and Dharma outlast all transient worldly wealth.", SanskritMantra = "सत्यमेव जयते नानृतम् ॥" },
        new() { Id = "katha-2", Title = "Faith and Focus: Dhruva's Tapasya", DeityOrSage = "Bhakta Dhruva & Narada Muni", StorySummary = "Young Dhruva performed unwavering meditation guided by Guru Narada Muni's 'Om Namo Bhagavate Vasudevaya' mantra, earning eternal placement as the Pole Star.", MoralTeaching = "Pure devotion and resolute determination overcome any obstacle.", SanskritMantra = "ॐ नमो भगवते वासुदेवाय ॥" },
        new() { Id = "katha-3", Title = "Gratitude in Friendship: Krishna & Sudama", DeityOrSage = "Bhagavan Shri Krishna & Sudama", StorySummary = "Krishna welcomed humble childhood friend Sudama with royal honor, treasuring a simple handful of poha offered with pure love.", MoralTeaching = "Sincere devotion and humility surpass lavish material offerings.", SanskritMantra = "कृष्णाय वासुदेवाय हरये परमात्मने ॥" },
        new() { Id = "katha-4", Title = "Duty Beyond Doubt: Arjuna on Kurukshetra", DeityOrSage = "Shri Krishna & Arjuna", StorySummary = "On the battlefield of Kurukshetra, Krishna imparted the eternal Bhagavad Gita, clarifying Nishkama Karma and steadfast duty.", MoralTeaching = "Act with courage and clarity, offering results to the Divine.", SanskritMantra = "योगस्थः कुरु कर्माणि ॥" }
    ];

    /// <summary>
    /// Upcoming festivals are dynamically calculated via astronomical Tithi &amp; Masa in
    /// <see cref="VedicPanchangCalculator.ComputeDynamicFestivalsForWindow"/>; the persisted
    /// collection holds only admin-created custom festivals or overrides.
    /// </summary>
    public static List<FestivalItem> DefaultFestivals() => [];

    public static List<DevotionalKitItem> DefaultDevotionalKits() =>
    [
        new() { Id = "kit-1", Icon = "🪔", Title = "Puja Samagri & Brass Diyas", Description = "Organic cow-ghee wicks, brass pancha-aarti lamp, camphor, and sacred dhoop for home worship.", ActionLabel = "View Puja Videos", ExternalUrl = "/videos", SortOrder = 10, IsActive = true },
        new() { Id = "kit-2", Icon = "📿", Title = "Rudraksha & Tulsi Japa Mala", Description = "108-bead certified Tulsi and 5-Mukhi Rudraksha malas for daily mantra sadhana and meditation.", ActionLabel = "Explore Mantra Videos", ExternalUrl = "/videos", SortOrder = 20, IsActive = true },
        new() { Id = "kit-4", Icon = "📜", Title = "Daily Pauranik Katha & Itihaas", Description = "Read today's inspiring Vedic story, moral sloka, and Indian cultural milestones.", ActionLabel = "Read Aaj Ka Itihaas →", ExternalUrl = "/itihaas", SortOrder = 40, IsActive = true }
    ];

    public static List<SpiritualVideo> DefaultVideos() => [];

    public static PanchangSiteConfig DefaultSiteConfig() => new()
    {
        Id = "default",
        SiteNameHindi = "पंचांगम् & संस्कृति",
        SiteSubtitle = "Vedic Panchang & Aaj Ka Itihaas",
        DefaultCityName = "New Delhi, India",
        BroadcastChannelName = string.Empty,
        BroadcastChannelUrl = string.Empty,
        DailyBroadcastBanner = "Daily Panchang briefs, Vrat Katha, Mantra meditations & cultural documentaries synced from connected creator channels."
    };
}
