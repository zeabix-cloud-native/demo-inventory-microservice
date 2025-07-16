using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace DemoInventory.API.Authentication;

/// <summary>
/// API Key authentication handler for validating requests
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private readonly IConfiguration _configuration;

    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, IConfiguration configuration) : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Allow requests to Swagger endpoints without authentication
        if (Request.Path.StartsWithSegments("/swagger") || 
            Request.Path.StartsWithSegments("/openapi.json"))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "swagger"),
                new Claim(ClaimTypes.Role, "swagger")
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        // Check if API key header exists with proper validation
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var headerValues) || 
            headerValues.Count == 0)
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key not found in header"));
        }

        var apiKey = headerValues.FirstOrDefault();

        // Validate API key format and content
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key is empty"));
        }

        if (apiKey.Length < 10 || apiKey.Length > 200)
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key format is invalid"));
        }

        // Get API key from configuration with secure fallback
        var validApiKey = _configuration["Authentication:ApiKey"];
        if (string.IsNullOrEmpty(validApiKey))
        {
            // Fallback for demo purposes - in production this should come from secure configuration
            validApiKey = "demo-inventory-api-key-2024";
            Logger.LogWarning("Using fallback API key configuration. Configure Authentication:ApiKey in production.");
        }
        
        if (!string.Equals(apiKey, validApiKey, StringComparison.Ordinal))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        var userClaims = new[]
        {
            new Claim(ClaimTypes.Name, "api-user"),
            new Claim(ClaimTypes.Role, "user")
        };
        var userIdentity = new ClaimsIdentity(userClaims, Scheme.Name);
        var userPrincipal = new ClaimsPrincipal(userIdentity);
        var authTicket = new AuthenticationTicket(userPrincipal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(authTicket));
    }
}

/// <summary>
/// Options for API Key authentication scheme
/// </summary>
public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
}