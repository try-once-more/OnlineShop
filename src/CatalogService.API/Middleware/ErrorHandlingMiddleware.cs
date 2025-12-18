using CatalogService.Application.Exceptions;

namespace CatalogService.API.Middleware;

internal class ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (ex is CategoryNotFoundException or ProductNotFoundException)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status404NotFound);
        }
        catch (Exception ex) when (ex is CategoryHasProductsException or CategoryHasChildCategoriesException)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status409Conflict);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex, int statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var errorResponse = new
        {
            error = ex.Message,
            type = ex.GetType().Name,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsJsonAsync(errorResponse);
    }
}
