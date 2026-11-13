using System.Net;
using System.Reflection;
using System.Text;
using HinduPanchang.Api.Controllers;
using HinduPanchang.Api.Models;
using HinduPanchang.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HinduPanchang.Api.Tests;

public class HinduPanchangTests
{
    private sealed class StubHostEnv(string root) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "HinduPanchang.Api.Tests";
        public string ContentRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = root;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(responder(request));
    }

    private sealed class StubHttpClientFactory(Func<HttpRequestMessage, HttpResponseMessage> responder) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(new StubHandler(responder));
    }

    private static string CreateTempRoot()
    {
        var dir = Path.Combine(Path.GetTempPath(), "panchang-tests-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static HybridDataStore CreateStore(string root)
    {
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        return new HybridDataStore(cfg, new StubHostEnv(root), NullLogger<HybridDataStore>.Instance);
    }

    [Fact]
    public void SeedData_HasZeroFakeYouTubeVideos_AndNoLegacyHandles()
    {
        var videos = PanchangSeedData.DefaultVideos();
        Assert.Empty(videos);

        var site = PanchangSeedData.DefaultSiteConfig();
        Assert.Equal(string.Empty, site.BroadcastChannelName);
        Assert.Equal(string.Empty, site.BroadcastChannelUrl);

        var kits = PanchangSeedData.DefaultDevotionalKits();
        Assert.DoesNotContain(kits, k => k.ExternalUrl.Contains("keshavsingh.in", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ParseAtomXml_ExtractsRealVideosViewsAndCategories()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns:yt="http://www.youtube.com/xml/schemas/2015"
                  xmlns:media="http://search.yahoo.com/mrss/"
                  xmlns="http://www.w3.org/2005/Atom">
              <entry>
                <yt:videoId>OmNamah12345</yt:videoId>
                <title>Morning Shiva Mantra Meditation &amp; Daily Puja</title>
                <published>2026-11-10T05:30:00+00:00</published>
                <media:group>
                  <media:description>Daily peaceful chant from Omkar Shiv Bhakti.</media:description>
                  <media:thumbnail url="https://i.ytimg.com/vi/OmNamah12345/hqdefault.jpg" />
                  <media:community>
                    <media:statistics views="12450" />
                  </media:community>
                </media:group>
              </entry>
            </feed>
            """;

        var channel = new CentralChannelConfig
        {
            Name = "Omkar Shiv Bhakti",
            Handle = "@OmkarShivBhakti",
            ChannelId = "UCRealBhaktiChannelId12345",
            Url = "https://www.youtube.com/@OmkarShivBhakti",
            Niche = "Spiritual & Devotional"
        };

        var videos = YouTubeFeedSyncService.ParseAtomXml(xml, channel, 5);
        Assert.Single(videos);
        Assert.Equal("OmNamah12345", videos[0].YouTubeVideoId);
        Assert.Equal(12450, videos[0].ViewCount);
        Assert.Equal("Mantra & Meditation", videos[0].Category);
        Assert.Equal("Omkar Shiv Bhakti", videos[0].ChannelName);
        Assert.True(videos[0].IsAutoSynced);
    }

    [Fact]
    public async Task CentralSync_ImportsRealChannelsAndPurgesLegacyDemoVideos()
    {
        var root = CreateTempRoot();
        try
        {
            var store = CreateStore(root);
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Ecosystem:AdminConfigUrl"] = "https://central.test/api/config"
            }).Build();

            var videoService = new VideoCurationService(store);
            // Seed a legacy demo video to verify purge
            await videoService.AddOrUpdateVideoAsync(new SpiritualVideo
            {
                Id = "legacy-demo",
                Title = "Me at the zoo",
                YouTubeVideoId = "jNQXAC9IVRw",
                ChannelName = "Keshav Singh Official",
                ChannelUrl = "https://www.youtube.com/@KeshavSingh"
            });

            var httpFactory = new StubHttpClientFactory(req =>
            {
                var uri = req.RequestUri!.ToString();
                if (uri.StartsWith("https://central.test/api/config"))
                {
                    const string payload = """
                    {
                      "values": { "panchang.title": "Live Vedic Panchang & Dharma Hub" },
                      "websites": [
                        { "key": "studio", "name": "Studio AI", "shortLabel": "Studio", "tagline": "Create Pauranik shorts", "url": "https://studio.example.in", "icon": "🎬", "showInEcosystemBar": true }
                      ],
                      "channels": [
                        { "key": "omkar", "name": "Omkar Shiv Bhakti", "handle": "@OmkarShivBhakti", "platform": "youtube", "channelId": "UCOmkarTestId1234567890", "url": "https://www.youtube.com/@OmkarShivBhakti", "niche": "Devotional", "showInPanchang": true, "isPrimary": true, "isEnabled": true, "sortOrder": 1 }
                      ]
                    }
                    """;
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(payload, Encoding.UTF8, "application/json")
                    };
                }

                if (uri.Contains("feeds/videos.xml"))
                {
                    const string rss = """
                    <feed xmlns:yt="http://www.youtube.com/xml/schemas/2015" xmlns:media="http://search.yahoo.com/mrss/" xmlns="http://www.w3.org/2005/Atom">
                      <entry>
                        <yt:videoId>ShivaKatha99</yt:videoId>
                        <title>Pradosh Vrat Katha Special</title>
                        <published>2026-11-11T08:00:00Z</published>
                        <media:group>
                          <media:description>Sacred Katha reading</media:description>
                          <media:community><media:statistics views="9870" /></media:community>
                        </media:group>
                      </entry>
                    </feed>
                    """;
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(rss, Encoding.UTF8, "application/xml")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var cultural = new CulturalContentService(store);
            var integrations = new IntegrationConfigService(store, cfg);
            var ytFeed = new YouTubeFeedSyncService(httpFactory, NullLogger<YouTubeFeedSyncService>.Instance);
            var sync = new CentralEcosystemSyncService(integrations, cultural, videoService, ytFeed, httpFactory, NullLogger<CentralEcosystemSyncService>.Instance);

            var result = await sync.SyncAllAsync(syncVideos: true);

            Assert.Equal(1, result.LegacyVideosPurged);
            Assert.Equal(1, result.ChannelsSynced);
            Assert.Equal(1, result.VideosSynced);
            Assert.True(result.SiteConfigUpdated);

            var liveVideos = await videoService.GetVideosAsync();
            Assert.Single(liveVideos);
            Assert.Equal("ShivaKatha99", liveVideos[0].YouTubeVideoId);
            Assert.Equal("Omkar Shiv Bhakti", liveVideos[0].ChannelName);
            Assert.True(liveVideos[0].IsFeaturedDailyVideo);

            var updatedSite = await cultural.GetSiteConfigAsync();
            Assert.Equal("Omkar Shiv Bhakti", updatedSite.BroadcastChannelName);
            Assert.Equal("https://www.youtube.com/@OmkarShivBhakti", updatedSite.BroadcastChannelUrl);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task IntegrationConfig_MasksApiKeyOnGet_AndNeverReturnsPlaintext()
    {
        var root = CreateTempRoot();
        try
        {
            var store = CreateStore(root);
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
            var svc = new IntegrationConfigService(store, cfg);

            await svc.UpdateAsync(new UpdatePanchangIntegrationsRequest
            {
                YouTubeApiKey = "AIzaSySecretKey99887766",
                AutoSyncOnStartup = false
            });

            var view = await svc.GetMaskedViewAsync();
            Assert.True(view.HasYouTubeApiKey);
            Assert.False(view.AutoSyncOnStartup);
            Assert.DoesNotContain("SecretKey", view.MaskedYouTubeApiKey);
            Assert.DoesNotContain("AIza", view.MaskedYouTubeApiKey);
            Assert.Equal("••••7766", view.MaskedYouTubeApiKey);

            // Submitting masked string should preserve existing secret without overwriting it
            await svc.UpdateAsync(new UpdatePanchangIntegrationsRequest
            {
                YouTubeApiKey = view.MaskedYouTubeApiKey
            });
            Assert.Equal("AIzaSySecretKey99887766", await svc.GetEffectiveYouTubeApiKeyAsync());
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void PanchangAdminController_EnforcesAdminRoleAuthorization()
    {
        var attr = typeof(PanchangAdminController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("Admin", attr!.Roles);
    }
}
