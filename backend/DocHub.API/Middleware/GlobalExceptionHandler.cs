using System.Net;
using System.Text.Json;
using DocHub.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace DocHub.API.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        RequestDelegate next,
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ApiResponse<object>
        {
            Success = false,
            Data = null
        };

        switch (exception)
        {
            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "Access denied. Please authenticate.";
                response.Errors = new List<string> { "Authentication required" };
                break;
                
            case ArgumentException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = exception.Message;
                response.Errors = new List<string> { exception.Message };
                break;
                
            case InvalidOperationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = exception.Message;
                response.Errors = new List<string> { exception.Message };
                break;
                
            case FileNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = "The requested resource was not found.";
                response.Errors = new List<string> { "Resource not found" };
                break;
                
            case NotSupportedException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = exception.Message;
                response.Errors = new List<string> { exception.Message };
                break;

            case DbUpdateException dbEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Database operation failed.";
                response.Errors = new List<string> { "Database error occurred" };
                if (_environment.IsDevelopment())
                {
                    response.Errors.Add(dbEx.InnerException?.Message ?? dbEx.Message);
                }
                break;

            case JsonException jsonEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Invalid JSON format.";
                response.Errors = new List<string> { "JSON parsing error" };
                if (_environment.IsDevelopment())
                {
                    response.Errors.Add(jsonEx.Message);
                }
                break;

            case TimeoutException:
                context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                response.Message = "The operation timed out.";
                response.Errors = new List<string> { "Request timeout" };
                break;

            case OutOfMemoryException:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "System resources are insufficient.";
                response.Errors = new List<string> { "Out of memory" };
                break;

            case StackOverflowException:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "A system error occurred.";
                response.Errors = new List<string> { "Stack overflow" };
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An unexpected error occurred. Please try again later.";
                response.Errors = new List<string> { "Internal server error" };
                
                if (_environment.IsDevelopment())
                {
                    response.Errors.Add(exception.Message);
                    response.Errors.Add(exception.StackTrace ?? "No stack trace available");
                }
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

// Extension method for easy registration
public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandler>();
    }
}
