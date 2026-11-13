using System.Security.Claims;
using System.Text;
using HinduPanchang.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Port injection from environment (e.g. Render $PORT)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Persistence + Dynamic Domain Services
builder.Services.AddSingleton<HybridDataStore>();
builder.Services.AddSingleton<CentralAdminConfigReader>();
builder.Services.AddSingleton<VedicPanchangCalculator>();
builder.Services.AddSingleton<CulturalContentService>();
builder.Services.AddSingleton<VideoCurationService>();
builder.Services.AddSingleton<IntegrationConfigService>();
builder.Services.AddHttpClient("CentralConfigClient", c => c.Timeout = TimeSpan.FromSeconds(15));
builder.Services.AddHttpClient("YouTubeClient", c => c.Timeout = TimeSpan.FromSeconds(20));
builder.Services.AddSingleton<YouTubeFeedSyncService>();
builder.Services.AddSingleton<CentralEcosystemSyncService>();
builder.Services.AddHostedService<EcosystemWarmupService>();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));

// Native .NET OpenAPI
builder.Services.AddOpenApi();

// CORS Policy for Keshav Ecosystem + configured AllowedOrigins
const string CorsPolicy = "HinduPanchangCors";
var configuredOrigins = (builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [])
    .Select(o => o.Trim().TrimEnd('/'))
    .Where(o => !string.IsNullOrEmpty(o))
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrWhiteSpace(origin)) return false;
            var normalized = origin.Trim().TrimEnd('/');
            if (configuredOrigins.Contains(normalized)) return true;
            try
            {
                var uri = new Uri(normalized);
                if (uri.Host.Equals("keshavsingh.in", StringComparison.OrdinalIgnoreCase) ||
                    uri.Host.EndsWith(".keshavsingh.in", StringComparison.OrdinalIgnoreCase)) return true;
                if (uri.Host.Equals("keshavsingh3197.github.io", StringComparison.OrdinalIgnoreCase)) return true;
                if (uri.Host.Equals("keshav-hindu-panchang.onrender.com", StringComparison.OrdinalIgnoreCase)) return true;
                if (builder.Environment.IsDevelopment() && (uri.Host == "localhost" || uri.Host == "127.0.0.1")) return true;
                return false;
            }
            catch
            {
                return false;
            }
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// Auth (JWT validation trusting admin.keshavsingh.in / id.keshavsingh.in IdP)
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = !string.IsNullOrWhiteSpace(jwtSection["Issuer"]) ? jwtSection["Issuer"]!.Trim() : "keshavsingh-idp";
var audience = !string.IsNullOrWhiteSpace(jwtSection["Audience"]) ? jwtSection["Audience"]!.Trim() : "keshavsingh-apps";
var rawSigningKey = jwtSection["SigningKey"] ?? builder.Configuration["JWT_SIGNING_KEY"];
var hasConfiguredSigningKey = !string.IsNullOrWhiteSpace(rawSigningKey) && rawSigningKey.Trim().Length >= 32;
var signingKey = !string.IsNullOrWhiteSpace(rawSigningKey)
    ? rawSigningKey.Trim()
    : (builder.Environment.IsDevelopment()
        ? "dev_signing_key_for_local_keshavsingh_ecosystem_32chars!"
        : "__UNCONFIGURED_JWT_SIGNING_KEY_SET_ENV_ON_RENDER__");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                signingKey.Length >= 32 ? signingKey : signingKey.PadRight(32, '0'))),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = "name",
            ClockSkew = TimeSpan.FromSeconds(30)
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("HinduPanchang.Auth");
                if (!hasConfiguredSigningKey)
                {
                    logger.LogError(
                        "JWT validation failed ({Message}) because Jwt:SigningKey is unset/empty. Configure Jwt__SigningKey on Render to match admin-api.",
                        ctx.Exception.Message);
                }
                return Task.CompletedTask;
            },
            OnChallenge = async ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                ctx.Response.ContentType = "application/json";
                var err = ctx.Error ?? "invalid_token";
                ctx.Response.Headers.WWWAuthenticate = $"Bearer error=\"{err}\"";
                await ctx.Response.WriteAsJsonAsync(new
                {
                    error = err,
                    code = !hasConfiguredSigningKey ? "jwt_signing_key_unconfigured" : "invalid_or_expired_token",
                    message = !hasConfiguredSigningKey
                        ? "Server JWT SigningKey is not configured on hindu-panchang-api. Set Jwt__SigningKey on Render to match admin-api."
                        : "Your SSO session token could not be verified. Sign out & sign in again via SSO.",
                    hasConfiguredSigningKey
                });
            },
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is ClaimsIdentity identity)
                {
                    var shortRoles = identity.FindAll("role").Select(c => c.Value).ToList();
                    foreach (var role in shortRoles)
                    {
                        if (!identity.HasClaim(ClaimTypes.Role, role))
                            identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

// Dedicated health endpoint for Render health checks
app.MapGet("/health", (HybridDataStore store) => Results.Ok(new
{
    status = "Healthy",
    service = "HinduPanchang.Api",
    version = "2.0.0",
    storage = store.StorageMode,
    time = DateTime.UtcNow
}));

app.MapControllers();

app.Run();
