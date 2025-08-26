using System.Text.Json;
using DocHub.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DocHub.API.Middleware;

public class RequestValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestValidationMiddleware> _logger;

    public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Validate request size
            if (!await ValidateRequestSizeAsync(context))
            {
                return;
            }

            // Validate content type for POST/PUT requests
            if (!await ValidateContentTypeAsync(context))
            {
                return;
            }

            // Validate request headers
            if (!await ValidateHeadersAsync(context))
            {
                return;
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in request validation middleware");
            await HandleValidationErrorAsync(context, "Request validation failed", ex.Message);
        }
    }

    private async Task<bool> ValidateRequestSizeAsync(HttpContext context)
    {
        var maxRequestSize = 50 * 1024 * 1024; // 50MB default
        
        if (context.Request.ContentLength > maxRequestSize)
        {
            await HandleValidationErrorAsync(context, "Request too large", 
                $"Request size exceeds maximum allowed size of {maxRequestSize / (1024 * 1024)}MB");
            return false;
        }

        return true;
    }

    private async Task<bool> ValidateContentTypeAsync(HttpContext context)
    {
        if (context.Request.Method == "POST" || context.Request.Method == "PUT")
        {
            var contentType = context.Request.ContentType;
            
            if (string.IsNullOrEmpty(contentType))
            {
                await HandleValidationErrorAsync(context, "Invalid content type", 
                    "Content-Type header is required for POST/PUT requests");
                return false;
            }

            // Allow multipart/form-data for file uploads
            if (contentType.StartsWith("multipart/form-data"))
            {
                return true;
            }

            // Allow application/json for JSON requests
            if (contentType.StartsWith("application/json"))
            {
                return true;
            }

            // Allow application/x-www-form-urlencoded for form data
            if (contentType.StartsWith("application/x-www-form-urlencoded"))
            {
                return true;
            }

            await HandleValidationErrorAsync(context, "Unsupported content type", 
                $"Content-Type '{contentType}' is not supported");
            return false;
        }

        return true;
    }

    private async Task<bool> ValidateHeadersAsync(HttpContext context)
    {
        // Validate required headers for API requests
        var requiredHeaders = new[] { "User-Agent" };
        
        foreach (var header in requiredHeaders)
        {
            if (!context.Request.Headers.ContainsKey(header))
            {
                await HandleValidationErrorAsync(context, "Missing required header", 
                    $"Header '{header}' is required");
                return false;
            }
        }

        // Validate User-Agent format
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        if (string.IsNullOrEmpty(userAgent) || userAgent.Length < 10)
        {
            await HandleValidationErrorAsync(context, "Invalid User-Agent", 
                "User-Agent header must be a valid string");
            return false;
        }

        return true;
    }

    private async Task HandleValidationErrorAsync(HttpContext context, string title, string message)
    {
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = title,
            Errors = new List<string> { message }
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

// Extension method for easy registration
public static class RequestValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestValidationMiddleware>();
    }
}
