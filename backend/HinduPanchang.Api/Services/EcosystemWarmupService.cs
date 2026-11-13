using HinduPanchang.Api.Services;

namespace HinduPanchang.Api.Services;

/// <summary>
/// Performs non-blocking initial ecosystem cleanup & YouTube feed warmup shortly after startup.
/// </summary>
public class EcosystemWarmupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<EcosystemWarmupService> _logger;

    public EcosystemWarmupService(IServiceProvider services, ILogger<EcosystemWarmupService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            using var scope = _services.CreateScope();
            var integrations = scope.ServiceProvider.GetRequiredService<IntegrationConfigService>();
            var videoService = scope.ServiceProvider.GetRequiredService<VideoCurationService>();
            var syncService = scope.ServiceProvider.GetRequiredService<CentralEcosystemSyncService>();

            await videoService.PurgeLegacyDemoVideosAsync();

            var settings = await integrations.GetSecretsAsync();
            if (!settings.AutoSyncOnStartup) return;

            var result = await syncService.SyncAllAsync(syncVideos: true, stoppingToken);
            _logger.LogInformation(
                "Startup ecosystem warmup completed: {Channels} channels, {Videos} videos, {Purged} legacy purged.",
                result.ChannelsSynced, result.VideosSynced, result.LegacyVideosPurged);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Non-fatal exception during Panchang startup ecosystem warmup.");
        }
    }
}
