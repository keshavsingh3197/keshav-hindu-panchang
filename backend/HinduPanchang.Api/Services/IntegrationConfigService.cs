using HinduPanchang.Api.Models;

namespace HinduPanchang.Api.Services;

/// <summary>
/// Manages persistent Admin-only integration settings and masked secrets.
/// </summary>
public class IntegrationConfigService
{
    private const string CollectionName = "integration_secrets";
    private readonly HybridDataStore _store;
    private readonly IConfiguration _config;

    public IntegrationConfigService(HybridDataStore store, IConfiguration config)
    {
        _store = store;
        _config = config;
    }

    public async Task<PanchangIntegrationSecrets> GetSecretsAsync()
    {
        var fallbackUrl = _config["Ecosystem:AdminConfigUrl"] ?? "https://id.keshavsingh.in/api/config";
        return await _store.GetSingletonAsync(CollectionName, "default", () => new PanchangIntegrationSecrets
        {
            Id = "default",
            CentralConfigUrl = fallbackUrl,
            AutoSyncOnStartup = true
        });
    }

    public async Task<PanchangIntegrationSettingsView> GetMaskedViewAsync()
    {
        var sec = await GetSecretsAsync();
        var envKey = Environment.GetEnvironmentVariable("YOUTUBE_API_KEY");
        var effectiveKey = !string.IsNullOrWhiteSpace(sec.YouTubeApiKey) ? sec.YouTubeApiKey : envKey;

        return new PanchangIntegrationSettingsView
        {
            HasYouTubeApiKey = !string.IsNullOrWhiteSpace(effectiveKey),
            MaskedYouTubeApiKey = MaskSecret(effectiveKey),
            CentralConfigUrl = string.IsNullOrWhiteSpace(sec.CentralConfigUrl)
                ? (_config["Ecosystem:AdminConfigUrl"] ?? "https://id.keshavsingh.in/api/config")
                : sec.CentralConfigUrl,
            AutoSyncOnStartup = sec.AutoSyncOnStartup,
            LastEcosystemSyncUtc = sec.LastEcosystemSyncUtc,
            LastVideoSyncUtc = sec.LastVideoSyncUtc
        };
    }

    public async Task<PanchangIntegrationSettingsView> UpdateAsync(UpdatePanchangIntegrationsRequest req)
    {
        var sec = await GetSecretsAsync();

        if (req.ClearYouTubeApiKey)
            sec.YouTubeApiKey = string.Empty;
        else if (!string.IsNullOrWhiteSpace(req.YouTubeApiKey) && !req.YouTubeApiKey.Contains("••••"))
            sec.YouTubeApiKey = req.YouTubeApiKey.Trim();

        if (!string.IsNullOrWhiteSpace(req.CentralConfigUrl))
            sec.CentralConfigUrl = req.CentralConfigUrl.Trim();

        if (req.AutoSyncOnStartup.HasValue)
            sec.AutoSyncOnStartup = req.AutoSyncOnStartup.Value;

        await _store.SaveSingletonAsync(CollectionName, "default", sec);
        return await GetMaskedViewAsync();
    }

    public async Task MarkSyncCompletedAsync(bool syncedVideos)
    {
        var sec = await GetSecretsAsync();
        var now = DateTime.UtcNow;
        sec.LastEcosystemSyncUtc = now;
        if (syncedVideos) sec.LastVideoSyncUtc = now;
        await _store.SaveSingletonAsync(CollectionName, "default", sec);
    }

    public async Task<string> GetEffectiveYouTubeApiKeyAsync()
    {
        var sec = await GetSecretsAsync();
        if (!string.IsNullOrWhiteSpace(sec.YouTubeApiKey)) return sec.YouTubeApiKey;
        return Environment.GetEnvironmentVariable("YOUTUBE_API_KEY") ?? _config["YouTube:ApiKey"] ?? string.Empty;
    }

    public async Task<string> GetEffectiveCentralConfigUrlAsync()
    {
        var sec = await GetSecretsAsync();
        if (!string.IsNullOrWhiteSpace(sec.CentralConfigUrl)) return sec.CentralConfigUrl;
        return _config["Ecosystem:AdminConfigUrl"] ?? "https://id.keshavsingh.in/api/config";
    }

    public static string MaskSecret(string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret)) return string.Empty;
        var s = secret.Trim();
        if (s.Length <= 8) return new string('•', s.Length);
        return $"{s[..4]}••••{s[^4..]}";
    }
}
