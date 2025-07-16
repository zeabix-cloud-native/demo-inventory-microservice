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

        // Extract and validate API key using a safer approach
        var apiKeyResult = ExtractAndValidateApiKey();
        if (!apiKeyResult.IsValid)
        {
            return Task.FromResult(AuthenticateResult.Fail(apiKeyResult.ErrorMessage));
        }

        // Get API key from configuration - no fallback for security
        var validApiKey = GetConfigurationValue("Authentication:ApiKey");
        if (string.IsNullOrEmpty(validApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API key not configured"));
        }
        
        if (!string.Equals(apiKeyResult.ApiKey, validApiKey, StringComparison.Ordinal))
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

    /// <summary>
    /// Extracts and validates the API key from request headers
    /// </summary>
    /// <returns>Validation result containing the API key or error message</returns>
    private ApiKeyValidationResult ExtractAndValidateApiKey()
    {
        // Validate request and extract headers safely in a single operation
        var requestHeaders = Request?.Headers;
        if (requestHeaders == null)
        {
            return new ApiKeyValidationResult { IsValid = false, ErrorMessage = "Invalid request headers" };
        }

        if (!requestHeaders.TryGetValue(ApiKeyHeaderName, out var headerValues) || 
            headerValues.Count == 0 || 
            !headerValues.Any() ||
            headerValues.Any(string.IsNullOrWhiteSpace))
        {
            return new ApiKeyValidationResult { IsValid = false, ErrorMessage = "API Key not found in header" };
        }

        var apiKey = headerValues.FirstOrDefault();

        // Validate API key format and content
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new ApiKeyValidationResult { IsValid = false, ErrorMessage = "API Key is empty" };
        }

        // Enhanced validation for API key format
        if (apiKey.Length < 10 || apiKey.Length > 200 || !IsValidApiKeyFormat(apiKey))
        {
            return new ApiKeyValidationResult { IsValid = false, ErrorMessage = "API Key format is invalid" };
        }

        return new ApiKeyValidationResult { IsValid = true, ApiKey = apiKey };
    }

    /// <summary>
    /// Validation result for API key extraction
    /// </summary>
    private class ApiKeyValidationResult
    {
        public bool IsValid { get; set; }
        public string? ApiKey { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Validates the format of an API key
    /// </summary>
    /// <param name="apiKey">The API key to validate</param>
    /// <returns>True if the format is valid, false otherwise</returns>
    private static bool IsValidApiKeyFormat(string apiKey)
    {
        // API key should contain only alphanumeric characters, hyphens, and underscores
        return apiKey.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }

    /// <summary>
    /// Safely retrieves a configuration value
    /// </summary>
    /// <param name="key">The configuration key</param>
    /// <returns>The configuration value or null if not found</returns>
    private string? GetConfigurationValue(string key)
    {
        try
        {
            return _configuration?[key];
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Options for API Key authentication scheme
/// </summary>
public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
}