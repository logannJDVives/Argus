using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Argus.Api.Middleware
{
    /// <summary>
    /// Global exception handler that provides consistent error responses
    /// and logs unhandled exceptions throughout the API pipeline.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment environment)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var traceId = httpContext.TraceIdentifier;

            _logger.LogError(
                exception,
                "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
                traceId,
                httpContext.Request.Path,
                httpContext.Request.Method);

            var (statusCode, title) = MapExceptionToResponse(exception);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = $"https://httpstatuses.com/{statusCode}",
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["traceId"] = traceId;

            // Only include exception details in Development environment
            if (_environment.IsDevelopment())
            {
                problemDetails.Detail = exception.Message;
                problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }
            else
            {
                // In production, provide a generic message
                problemDetails.Detail = "An error occurred while processing your request.";
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            await httpContext.Response.WriteAsJsonAsync(problemDetails, options, cancellationToken);

            return true; // Exception handled
        }

        private static (int StatusCode, string Title) MapExceptionToResponse(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException => (StatusCodes.Status400BadRequest, "Bad Request"),
                ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
                UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden"),
                InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
                NotSupportedException => (StatusCodes.Status501NotImplemented, "Not Implemented"),
                TimeoutException => (StatusCodes.Status504GatewayTimeout, "Gateway Timeout"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };
        }
    }
}
