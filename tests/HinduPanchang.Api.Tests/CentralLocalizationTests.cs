using HinduPanchang.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HinduPanchang.Api.Tests;

public sealed class CentralLocalizationTests
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

    [Fact]
    public async Task PanchangLocalization_DefaultsToHindi_WhenNoCentralMongoConfigured()
    {
        var root = Path.Combine(Path.GetTempPath(), "panchang-i18n-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(root);
        try
        {
            var cfg = new ConfigurationBuilder().Build();
            var store = new HybridDataStore(cfg, new StubHostEnv(root), NullLogger<HybridDataStore>.Instance);
            var service = new CentralLocalizationService(store, cfg, NullLogger<CentralLocalizationService>.Instance);

            var manifest = await service.GetManifestAsync();
            Assert.Equal("hi", manifest.Policy.DefaultLocale);
            Assert.Equal("admin", manifest.Policy.ControlledBy);
            Assert.Contains("hi", manifest.Policy.EnabledLocales);
            Assert.Contains("en", manifest.Policy.EnabledLocales);

            var hiBundle = await service.GetBundleAsync(null);
            Assert.Equal("hi", hiBundle.Locale);
            Assert.Equal("दैनिक वैदिक पंचांग एवं शुभ मुहूर्त", hiBundle.Entries["panchang.hero.title"]);

            var enBundle = await service.GetBundleAsync("en");
            Assert.Equal("en", enBundle.Locale);
            Assert.Equal("Daily Vedic Panchang & Shubh Muhurat", enBundle.Entries["panchang.hero.title"]);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task PanchangLocalization_PerSiteOverride_ForcesChosenLocale()
    {
        var root = Path.Combine(Path.GetTempPath(), "panchang-i18n-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(root);
        try
        {
            var cfg = new ConfigurationBuilder().Build();
            var store = new HybridDataStore(cfg, new StubHostEnv(root), NullLogger<HybridDataStore>.Instance);
            var service = new CentralLocalizationService(store, cfg, NullLogger<CentralLocalizationService>.Instance);

            var updated = await service.UpdateLocalSettingsAsync(
                new UpdateSiteLocalizationRequest(
                    OverrideCentralLocalization: true,
                    DefaultLocale: "en",
                    EnabledLocales: ["en"],
                    AllowVisitorLanguageSwitch: false,
                    ShowLanguagePicker: false,
                    ForceLocale: true),
                "admin-user");

            Assert.Equal("site-override", updated.EffectivePolicy.ControlledBy);
            Assert.Equal("en", updated.EffectivePolicy.DefaultLocale);
            Assert.True(updated.EffectivePolicy.ForceLocale);

            var requestedHi = await service.GetBundleAsync("hi");
            Assert.Equal("en", requestedHi.Locale);
            Assert.Equal("Daily Vedic Panchang & Shubh Muhurat", requestedHi.Entries["panchang.hero.title"]);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task TryLoadCentralEnvelopeFromMongoAsync_ReturnsNull_WhenUnconfigured()
    {
        var root = Path.Combine(Path.GetTempPath(), "panchang-i18n-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(root);
        try
        {
            var cfg = new ConfigurationBuilder().Build();
            var store = new HybridDataStore(cfg, new StubHostEnv(root), NullLogger<HybridDataStore>.Instance);
            var service = new CentralLocalizationService(store, cfg, NullLogger<CentralLocalizationService>.Instance);

            var envelope = await service.TryLoadCentralEnvelopeFromMongoAsync();
            Assert.Null(envelope);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
