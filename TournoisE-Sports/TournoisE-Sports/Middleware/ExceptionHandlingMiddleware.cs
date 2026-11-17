using Esport.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace TournoisE_Sports.Middleware;

/// <summary>
/// Middleware global qui convertit les exceptions de domaine en r\u00e9ponses HTTP appropri\u00e9es :
/// - NotFoundException   -> 404
/// - ConflictException   -> 409
/// - BadRequestException -> 400
/// - exception inattendue -> 500
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur non g\u00e9r�e : {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, title) = ex switch
        {
            NotFoundException   => (StatusCodes.Status404NotFound, "Not Found"),
            ConflictException   => (StatusCodes.Status409Conflict, "Conflict"),
            BadRequestException => (StatusCodes.Status400BadRequest, "Bad Request"),
            _                   => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problem = new
        {
            type = $"https://tools.ietf.org/html/rfc9110#section-15.{statusCode switch { 400 => "5.1", 404 => "5.4", 409 => "5.6", _ => "5.6" }}",
            title,
            status = statusCode,
            detail = ex.Message
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
