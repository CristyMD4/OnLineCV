using System.Text.Json;
using OnlineCV.Api.Models;
using OnlineCV.Api.Services;

namespace OnlineCV.Api.Endpoints;

/// <summary>
/// Maps the read and administration routes for a CV list section. Reads are public, writes require
/// an administrator token, and every write returns through <see cref="ApiResults"/> so validation,
/// conflict, and not-found responses stay identical across sections.
/// </summary>
internal static class CvSectionEndpoints
{
    public static IEndpointRouteBuilder MapSection<T>(
        this IEndpointRouteBuilder endpoints,
        CvSection<T> section,
        string route)
        where T : notnull
    {
        var group = endpoints.MapGroup($"/api/{route}").WithTags("CV Content");

        group.MapGet("", (CvService service) => Results.Ok(service.GetSection(section)))
            .WithSummary($"List {route}");

        group.MapGet("/{id}", (string id, CvService service) =>
            {
                var item = service.GetItem(section, id);
                return item is null ? Results.NotFound() : Results.Ok(item);
            })
            .WithSummary($"Get one entry from {route}");

        group.MapPost("", async (HttpContext context, CvService service) =>
            {
                var (item, error) = await ReadJsonAsync<T>(context);
                if (error is not null) return error;

                return ApiResults.From(
                    await service.AddItemAsync(section, item!, context.RequestAborted),
                    value => Results.Created($"/api/{route}/{section.Identity(value)}", value));
            })
            .Accepts<T>("application/json")
            .Produces<T>(StatusCodes.Status201Created)
            .RequireAuthorization("AdminOnly")
            .WithSummary($"Add an entry to {route}");

        group.MapPut("", async (HttpContext context, CvService service) =>
            {
                var (items, error) = await ReadJsonAsync<List<T>>(context);
                if (error is not null) return error;

                return ApiResults.From(
                    await service.ReplaceSectionAsync(section, items!, context.RequestAborted),
                    Results.Ok);
            })
            .Accepts<List<T>>("application/json")
            .Produces<List<T>>()
            .RequireAuthorization("AdminOnly")
            .WithSummary($"Replace every entry in {route}");

        group.MapPut("/{id}", async (string id, HttpContext context, CvService service) =>
            {
                var (item, error) = await ReadJsonAsync<T>(context);
                if (error is not null) return error;

                return ApiResults.From(
                    await service.UpdateItemAsync(section, id, item!, context.RequestAborted),
                    Results.Ok);
            })
            .Accepts<T>("application/json")
            .Produces<T>()
            .RequireAuthorization("AdminOnly")
            .WithSummary($"Update one entry in {route}");

        group.MapDelete("/{id}", async (string id, CvService service, CancellationToken cancellationToken) =>
                ApiResults.From(
                    await service.DeleteItemAsync(section, id, cancellationToken),
                    _ => Results.NoContent()))
            .RequireAuthorization("AdminOnly")
            .WithSummary($"Delete one entry from {route}");

        return endpoints;
    }

    /// <summary>
    /// Maps a plain list of strings. These entries carry no title to address them by, so they are
    /// read and replaced as a whole rather than edited item by item.
    /// </summary>
    public static IEndpointRouteBuilder MapTextList(
        this IEndpointRouteBuilder endpoints,
        string route,
        Func<CvContent, List<string>> selector)
    {
        var group = endpoints.MapGroup($"/api/{route}").WithTags("CV Content");

        group.MapGet("", (CvService service) => Results.Ok(selector(service.Content)))
            .WithSummary($"List {route}");

        group.MapPut("", async (List<string> values, CvService service, CancellationToken cancellationToken) =>
                ApiResults.From(
                    await service.ReplaceTextListAsync(selector, values, cancellationToken),
                    Results.Ok))
            .RequireAuthorization("AdminOnly")
            .WithSummary($"Replace every entry in {route}");

        return endpoints;
    }

    /// <summary>
    /// Reads the request body explicitly rather than declaring it as a handler parameter. A body
    /// parameter typed on an open generic crashes the ASP.NET route-handler analyzer, which then
    /// stops checking every other route in the project, so the binding is done here instead and
    /// the schema is declared with <c>Accepts</c>.
    /// </summary>
    private static async Task<(TBody? Value, IResult? Error)> ReadJsonAsync<TBody>(HttpContext context)
    {
        try
        {
            var value = await context.Request.ReadFromJsonAsync<TBody>(context.RequestAborted);
            return value is null
                ? (default, Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "A request body is required."))
                : (value, null);
        }
        catch (JsonException exception)
        {
            return (default, Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "The request body is not valid JSON.",
                detail: exception.Message));
        }
    }
}
