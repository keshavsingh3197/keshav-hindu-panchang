using HinduPanchang.Api.Models;

namespace HinduPanchang.Api.Services;

public class VideoCurationService
{
    private static readonly List<SpiritualVideo> Videos = new()
    {
        new SpiritualVideo
        {
            Id = "vid-1",
            Title = "आज का दैनिक राशिफल और पंचांग विचार | Daily Horoscope & Vedic Insights",
            Description = "Comprehensive astrological forecast for all 12 rashis (Mesh to Meen), auspicious tithi muhurats, Rahu Kaal warnings, and remedy mantras for today.",
            YouTubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
            YouTubeVideoId = "dQw4w9WgXcQ",
            EmbedUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
            ThumbnailUrl = "https://images.unsplash.com/photo-1545231027-637d2f6210f8?auto=format&fit=crop&w=600&q=80",
            Category = "Daily News & Rashifal",
            PublishedDate = DateTime.UtcNow,
            DurationMinutes = "14:20",
            ViewCount = 42800,
            IsFeaturedDailyVideo = true
        },
        new SpiritualVideo
        {
            Id = "vid-2",
            Title = "देवउठनी एकादशी व्रत कथा एवं तुलसी विवाह विधि | Devutthana Ekadashi Katha",
            Description = "Full sacred story of Prabodhini Ekadashi, how to perform Tulsi Vivah at home, sugarcane mandap rituals, and auspicious timings for marriage commencement.",
            YouTubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
            YouTubeVideoId = "dQw4w9WgXcQ",
            EmbedUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
            ThumbnailUrl = "https://images.unsplash.com/photo-1514533450685-4493e01d1fdc?auto=format&fit=crop&w=600&q=80",
            Category = "Pauranik Katha",
            PublishedDate = DateTime.UtcNow.AddDays(-1),
            DurationMinutes = "18:45",
            ViewCount = 68400,
            IsFeaturedDailyVideo = false
        },
        new SpiritualVideo
        {
            Id = "vid-3",
            Title = "श्री महामृत्युंजय मंत्र 108 बार जाप | Maha Mrityunjaya Mantra 108 Times",
            Description = "Powerful Vedic chanting of Lord Shiva's Maha Mrityunjaya Mantra for health, longevity, inner peace, and divine protection from negativity.",
            YouTubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
            YouTubeVideoId = "dQw4w9WgXcQ",
            EmbedUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
            ThumbnailUrl = "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?auto=format&fit=crop&w=600&q=80",
            Category = "Mantra & Meditation",
            PublishedDate = DateTime.UtcNow.AddDays(-2),
            DurationMinutes = "45:10",
            ViewCount = 125000,
            IsFeaturedDailyVideo = false
        },
        new SpiritualVideo
        {
            Id = "vid-4",
            Title = "काशी विश्वनाथ मंदिर एवं देव दीपावली महाआरती लाइव दर्शन | Kashi Vishwanath Darshan",
            Description = "Breathtaking visual darshan of Kashi Vishwanath temple, Ganga Aarti at Dashashwamedh Ghat, and festive preparations for Dev Deepawali.",
            YouTubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
            YouTubeVideoId = "dQw4w9WgXcQ",
            EmbedUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
            ThumbnailUrl = "https://images.unsplash.com/photo-1609766857041-ed402ea8069a?auto=format&fit=crop&w=600&q=80",
            Category = "Temple Darshan",
            PublishedDate = DateTime.UtcNow.AddDays(-3),
            DurationMinutes = "22:15",
            ViewCount = 89200,
            IsFeaturedDailyVideo = false
        }
    };

    public Task<List<SpiritualVideo>> GetVideosAsync(string? category = null)
    {
        if (string.IsNullOrWhiteSpace(category) || category.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(Videos);
        }

        return Task.FromResult(Videos.Where(v => v.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList());
    }

    public Task<SpiritualVideo?> GetFeaturedVideoAsync()
    {
        var featured = Videos.FirstOrDefault(v => v.IsFeaturedDailyVideo) ?? Videos.FirstOrDefault();
        return Task.FromResult(featured);
    }

    public Task<SpiritualVideo> AddOrUpdateVideoAsync(SpiritualVideo video)
    {
        if (string.IsNullOrWhiteSpace(video.Id))
        {
            video.Id = "vid-" + Guid.NewGuid().ToString()[..8];
        }

        if (string.IsNullOrWhiteSpace(video.EmbedUrl) && !string.IsNullOrWhiteSpace(video.YouTubeVideoId))
        {
            video.EmbedUrl = $"https://www.youtube.com/embed/{video.YouTubeVideoId}";
        }

        if (video.IsFeaturedDailyVideo)
        {
            foreach (var v in Videos)
            {
                v.IsFeaturedDailyVideo = false;
            }
        }

        var existing = Videos.FirstOrDefault(v => v.Id == video.Id);
        if (existing != null)
        {
            Videos.Remove(existing);
        }

        Videos.Insert(0, video);
        return Task.FromResult(video);
    }
}
