using System.Text.Json;
using HinduPanchang.Api.Models;

namespace HinduPanchang.Api.Services;

/// <summary>
/// Synchronizes creator channels, ecosystem websites, devotional kits, and live YouTube feeds
/// from the Central Admin configuration API (https://id.keshavsingh.in/api/config).
/// </summary>
public class CentralEcosystemSyncService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly IntegrationConfigService _integrations;
    private readonly CulturalContentService _cultural;
    private readonly VideoCurationService _videos;
    private readonly YouTubeFeedSyncService _ytFeed;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<CentralEcosystemSyncService> _logger;

    public CentralEcosystemSyncService(
        IntegrationConfigService integrations,
        CulturalContentService cultural,
        VideoCurationService videos,
        YouTubeFeedSyncService ytFeed,
        IHttpClientFactory httpFactory,
        ILogger<CentralEcosystemSyncService> logger)
    {
        _integrations = integrations;
        _cultural = cultural;
        _videos = videos;
        _ytFeed = ytFeed;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task<PanchangSyncSummary> SyncAllAsync(bool syncVideos = true, CancellationToken ct = default)
    {
        var summary = new PanchangSyncSummary { SyncedAtUtc = DateTime.UtcNow };

        // 1. Always purge stale demo/placeholder videos first
        summary.LegacyVideosPurged = await _videos.PurgeLegacyDemoVideosAsync();

        // 2. Fetch Central Config envelope
        var configUrl = await _integrations.GetEffectiveCentralConfigUrlAsync();
        CentralConfigEnvelope? envelope = null;
        try
        {
            var client = _httpFactory.CreateClient("CentralConfigClient");
            using var res = await client.GetAsync(configUrl, ct);
            if (!res.IsSuccessStatusCode)
            {
                summary.Warnings.Add($"Central config HTTP {(int)res.StatusCode} from {configUrl}");
                return summary;
            }

            var json = await res.Content.ReadAsStringAsync(ct);
            envelope = JsonSerializer.Deserialize<CentralConfigEnvelope>(json, JsonOpts);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Central config sync failed from {Url}", configUrl);
            summary.Warnings.Add($"Central config unreachable ({ex.Message})");
            return summary;
        }

        if (envelope == null)
        {
            summary.Warnings.Add("Central config returned empty payload.");
            return summary;
        }

        // 3. Identify Panchang-relevant YouTube channels
        var allYtChannels = (envelope.Channels ?? [])
            .Where(c => c.IsEnabled && string.Equals(c.Platform, "youtube", StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.SortOrder)
            .ToList();

        var panchangChannels = allYtChannels.Where(c => c.ShowInPanchang).ToList();
        if (panchangChannels.Count == 0)
            panchangChannels = allYtChannels;

        summary.ChannelsSynced = panchangChannels.Count;

        // 4. Sync PanchangSiteConfig primary broadcast channel cleanly
        var primaryChannel = panchangChannels.FirstOrDefault(c => c.ShowInPanchang && c.Name.Contains("Bhakti", StringComparison.OrdinalIgnoreCase))
            ?? panchangChannels.FirstOrDefault(c => c.IsPrimary)
            ?? panchangChannels.FirstOrDefault();

        var siteConfig = await _cultural.GetSiteConfigAsync();
        bool siteDirty = false;
        if (primaryChannel != null)
        {
            bool hasLegacyChannel = string.IsNullOrWhiteSpace(siteConfig.BroadcastChannelName)
                || string.Equals(siteConfig.BroadcastChannelName, "Keshav Singh Official", StringComparison.OrdinalIgnoreCase)
                || siteConfig.BroadcastChannelUrl.Contains("@KeshavSingh", StringComparison.OrdinalIgnoreCase);

            if (hasLegacyChannel)
            {
                siteConfig.BroadcastChannelName = primaryChannel.Name;
                siteConfig.BroadcastChannelUrl = !string.IsNullOrWhiteSpace(primaryChannel.Url)
                    ? primaryChannel.Url
                    : $"https://www.youtube.com/{primaryChannel.Handle}";
                siteDirty = true;
            }
        }

        if (envelope.Values != null)
        {
            if (envelope.Values.TryGetValue("panchang.title", out var centralTitle)
                && !string.IsNullOrWhiteSpace(centralTitle)
                && siteConfig.SiteNameHindi == "पंचांगम् & संस्कृति")
            {
                siteConfig.SiteSubtitle = centralTitle;
                siteDirty = true;
            }
        }

        if (siteDirty)
        {
            await _cultural.SaveSiteConfigAsync(siteConfig);
            summary.SiteConfigUpdated = true;
        }

        // 5. Upsert dynamic Studio AI devotional kit card if Central websites includes "studio"
        var studioSite = (envelope.Websites ?? []).FirstOrDefault(w =>
            string.Equals(w.Key, "studio", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(w.Url));
        if (studioSite != null)
        {
            var existingKits = await _cultural.GetDevotionalKitsAsync(includeInactive: true);
            var legacyKit = existingKits.FirstOrDefault(k => k.Id == "kit-3" || k.Id == "kit-studio");
            if (legacyKit == null || !string.Equals(legacyKit.ExternalUrl, studioSite.Url, StringComparison.OrdinalIgnoreCase))
            {
                await _cultural.UpsertDevotionalKitAsync(new DevotionalKitItem
                {
                    Id = legacyKit?.Id ?? "kit-studio",
                    Icon = string.IsNullOrWhiteSpace(studioSite.Icon) ? "🎬" : studioSite.Icon,
                    Title = $"Animated Vedic Stories ({studioSite.Name})",
                    Description = string.IsNullOrWhiteSpace(studioSite.Tagline)
                        ? "Explore AI-crafted Pauranik storytelling and devotional series."
                        : studioSite.Tagline,
                    ActionLabel = $"Open {studioSite.ShortLabel} →",
                    ExternalUrl = studioSite.Url,
                    SortOrder = 30,
                    IsActive = true
                });
                summary.KitsUpdated++;
            }
        }

        // 6. Sync live YouTube videos from each connected Panchang channel
        if (syncVideos && panchangChannels.Count > 0)
        {
            var apiKey = await _integrations.GetEffectiveYouTubeApiKeyAsync();
            var fetchedAll = new List<SpiritualVideo>();
            foreach (var ch in panchangChannels)
            {
                var vids = await _ytFeed.FetchLatestVideosAsync(ch, apiKey, maxResults: 8, ct);
                if (vids.Count == 0)
                    summary.Warnings.Add($"No public videos returned for {ch.Name} ({ch.Handle}).");
                fetchedAll.AddRange(vids);
            }

            if (fetchedAll.Count > 0)
                summary.VideosSynced = await _videos.UpsertSyncedVideosAsync(fetchedAll);
        }

        await _integrations.MarkSyncCompletedAsync(syncVideos);
        return summary;
    }
}
