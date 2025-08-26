using System.Text.Json;
using DocHub.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DocHub.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly IMemoryCache _cache;
    private readonly RateLimitOptions _options;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        IMemoryCache cache,
        RateLimitOptions options)
    {
        _next = next;
        _logger = logger;
        _cache = cache;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var endpoint = GetEndpointIdentifier(context);

        if (!await IsRequestAllowedAsync(clientId, endpoint))
        {
            await HandleRateLimitExceededAsync(context, clientId, endpoint);
            return;
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID from JWT token first
        var userId = context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user_{userId}";
        }

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip_{ipAddress}";
    }

    private string GetEndpointIdentifier(HttpContext context)
    {
        var controller = context.Request.RouteValues["controller"]?.ToString() ?? "unknown";
        var action = context.Request.RouteValues["action"]?.ToString() ?? "unknown";
        var method = context.Request.Method;
        
        return $"{method}_{controller}_{action}";
    }

    private async Task<bool> IsRequestAllowedAsync(string clientId, string endpoint)
    {
        var cacheKey = $"rate_limit_{clientId}_{endpoint}";
        
        if (_cache.TryGetValue(cacheKey, out RateLimitInfo rateLimitInfo))
        {
            // Check if window has expired
            if (DateTime.UtcNow >= rateLimitInfo.WindowEnd)
            {
                // Reset for new window
                rateLimitInfo = new RateLimitInfo
                {
                    RequestCount = 1,
                    WindowStart = DateTime.UtcNow,
                    WindowEnd = DateTime.UtcNow.AddMinutes(_options.WindowMinutes)
                };
            }
            else
            {
                // Check if limit exceeded
                if (rateLimitInfo.RequestCount >= _options.MaxRequestsPerWindow)
                {
                    return false;
                }
                
                // Increment request count
                rateLimitInfo.RequestCount++;
            }
        }
        else
        {
            // First request in new window
            rateLimitInfo = new RateLimitInfo
            {
                RequestCount = 1,
                WindowStart = DateTime.UtcNow,
                WindowEnd = DateTime.UtcNow.AddMinutes(_options.WindowMinutes)
            };
        }

        // Cache the rate limit info
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.WindowMinutes + 1)
        };
        
        _cache.Set(cacheKey, rateLimitInfo, cacheOptions);
        
        return true;
    }

    private async Task HandleRateLimitExceededAsync(HttpContext context, string clientId, string endpoint)
    {
        context.Response.StatusCode = 429; // Too Many Requests
        context.Response.ContentType = "application/json";
        
        // Add rate limit headers
        context.Response.Headers.Add("X-RateLimit-Limit", _options.MaxRequestsPerWindow.ToString());
        context.Response.Headers.Add("X-RateLimit-Remaining", "0");
        context.Response.Headers.Add("X-RateLimit-Reset", DateTime.UtcNow.AddMinutes(_options.WindowMinutes).ToString("R"));

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = "Rate limit exceeded",
            Errors = new List<string> 
            { 
                $"Too many requests. Limit: {_options.MaxRequestsPerWindow} requests per {_options.WindowMinutes} minutes.",
                "Please try again later."
            }
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _logger.LogWarning("Rate limit exceeded for {ClientId} on {Endpoint}", clientId, endpoint);
        
        await context.Response.WriteAsync(jsonResponse);
    }
}

public class RateLimitInfo
{
    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; }
    public DateTime WindowEnd { get; set; }
}

public class RateLimitOptions
{
    public int MaxRequestsPerWindow { get; set; } = 100;
    public int WindowMinutes { get; set; } = 1;
}

// Extension method for easy registration
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder, RateLimitOptions options)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>(options);
    }
}
