using System.Security.Cryptography;
using System.Text;
using HinduPanchang.Api.Models;
using HinduPanchang.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HinduPanchang.Api.Tests;

public sealed class CentralAdminIntegrationTests
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

    private static string EncryptVersioned(string plainText, byte[] key, byte keyId = 1)
    {
        var nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipher = new byte[plainBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(key, 16);
        aes.Encrypt(nonce, plainBytes, cipher, tag);

        var packed = new byte[2 + 12 + 16 + cipher.Length];
        packed[0] = 1;
        packed[1] = keyId;
        Buffer.BlockCopy(nonce, 0, packed, 2, 12);
        Buffer.BlockCopy(tag, 0, packed, 14, 16);
        Buffer.BlockCopy(cipher, 0, packed, 30, cipher.Length);
        return Convert.ToBase64String(packed);
    }

    [Fact]
    public void DecryptProtectedValue_DecryptsVersionedCiphertext()
    {
        var rawKey = RandomNumberGenerator.GetBytes(32);
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Encryption:DataKey"] = Convert.ToBase64String(rawKey),
                ["Encryption:DataKeyId"] = "1"
            })
            .Build();

        var keys = CentralAdminConfigReader.LoadEncryptionKeys(cfg, out var keyId);
        var cipher = EncryptVersioned("AIzaCentralSecret4321", rawKey, keyId);

        var plain = CentralAdminConfigReader.DecryptProtectedValue(cipher, keys, keyId);
        Assert.Equal("AIzaCentralSecret4321", plain);
    }

    [Fact]
    public async Task IntegrationConfig_And_SiteConfig_ConsumeCentralReaderWithoutDuplicatingLocalSecrets()
    {
        var root = Path.Combine(Path.GetTempPath(), "panchang-central-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(root);
        try
        {
            var cfg = new ConfigurationBuilder().Build();
            var store = new HybridDataStore(cfg, new StubHostEnv(root), NullLogger<HybridDataStore>.Instance);
            var central = new CentralAdminConfigReader(
                _ => Task.FromResult<CentralPanchangIntegrationsSnapshot?>(
                    new CentralPanchangIntegrationsSnapshot("AIzaCentralYouTube9876", "ca-pub-9876543210987654")),
                NullLogger<CentralAdminConfigReader>.Instance);

            var integrations = new IntegrationConfigService(store, cfg, central);
            Assert.Equal("AIzaCentralYouTube9876", await integrations.GetEffectiveYouTubeApiKeyAsync());

            var view = await integrations.GetMaskedViewAsync();
            Assert.True(view.HasYouTubeApiKey);
            Assert.True(view.YouTubeKeyManagedByCentral);
            Assert.Equal("central", view.IntegrationSource);
            Assert.Equal("••••9876", view.MaskedYouTubeApiKey);
            Assert.True(view.CentralAdSenseConfigured);
            Assert.Equal("ca-pub-9876543210987654", view.CentralAdSenseClientId);

            var cultural = new CulturalContentService(store, central);
            var saved = await cultural.SaveSiteConfigAsync(new PanchangSiteConfig
            {
                SiteNameHindi = "पंचांगम् & संस्कृति",
                Ads = new AdMonetizationConfig
                {
                    AdsEnabled = true,
                    AdSenseClientId = "ca-pub-9876543210987654",
                    HeaderAdSlotId = "1234567890"
                }
            });

            // Save keeps AdsEnabled=true and returns hydrated central AdSenseClientId
            Assert.True(saved.Ads.AdsEnabled);
            Assert.Equal("ca-pub-9876543210987654", saved.Ads.AdSenseClientId);
            Assert.Equal("1234567890", saved.Ads.HeaderAdSlotId);

            // Verify raw local singleton did not duplicate Central's AdSenseClientId
            var raw = await store.GetSingletonAsync<PanchangSiteConfig>("site_config", "default", () => new PanchangSiteConfig());
            Assert.Equal(string.Empty, raw.Ads.AdSenseClientId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
}
