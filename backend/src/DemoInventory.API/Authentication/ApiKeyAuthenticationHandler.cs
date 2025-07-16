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

        if (!Request.Headers.ContainsKey(ApiKeyHeaderName))
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key not found in header"));
        }

        var apiKey = Request.Headers[ApiKeyHeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key is empty"));
        }

        // Get API key from configuration
        var validApiKey = _configuration["Authentication:ApiKey"] ?? "demo-inventory-api-key-2024";
        
        if (apiKey != validApiKey)
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