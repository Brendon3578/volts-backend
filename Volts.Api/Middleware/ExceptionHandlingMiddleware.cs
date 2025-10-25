using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Volts.Application.Exceptions;

namespace Volts.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception caught by middleware");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            int status = exception switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                ArgumentException => StatusCodes.Status400BadRequest,
                UserHasNotPermissionException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            string message = exception switch
            {
                NotFoundException => exception.Message ?? "Resource not found",
                UserHasNotPermissionException => exception.Message ?? "User has not allowed permission",
                UnauthorizedAccessException => exception.Message ?? "Forbidden",
                ArgumentException => exception.Message ?? "Bad request",
                _ => "An unexpected error occurred"
            };

            var payload = JsonSerializer.Serialize(new { status, message });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = status;

            return context.Response.WriteAsync(payload);
        }
    }
}
