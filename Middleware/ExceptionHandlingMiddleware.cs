using IdentityCore.DTOs;
using IdentityCore.Exceptions;
using System.Net;
using System.Text.Json;

namespace IdentityCore.Middleware
{
    /// <summary>
    /// Catches the unhandled exceptions, logs details and sends generic errors back to the client.
    /// no need for try/catch in the controllers.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex)
            {
                _logger.LogWarning(
                    "Domain exception on {Method} {Path}: [{StatusCode}] {Message}",
                    context.Request.Method, context.Request.Path, ex.StatusCode, ex.Message);

                await WriteJsonAsync(context, ex.StatusCode,
                    new ErrorResponse(ex.Message, ex.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                await WriteJsonAsync(context, (int)HttpStatusCode.InternalServerError,
                    new ErrorResponse("An unexpected error occurred. Please try again later."));
            }
        }

        private static Task WriteJsonAsync(HttpContext context, int statusCode, ErrorResponse body)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonSerializer.Serialize(body, _jsonOptions));
        }
    }
}
