using OnlineCV.Api.Models;
using OnlineCV.Api.Services;

namespace OnlineCV.Api.Endpoints;

public static class ContactEndpoints
{
    public static IEndpointRouteBuilder MapContactEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/contact", async (
                ContactRequest request,
                ContactService service,
                CancellationToken cancellationToken) =>
            {
                var result = await service.SubmitAsync(request, cancellationToken);
                return ApiResults.From(result, submission => Results.Accepted(
                    $"/api/contact/submissions/{submission.Id}",
                    new { submission.Id, submission.Status, submission.CreatedAt }));
            })
            .RequireRateLimiting("contact")
            .WithTags("Contact")
            .WithSummary("Validate and store a contact message");

        var admin = endpoints.MapGroup("/api/contact/submissions")
            .RequireAuthorization("AdminOnly")
            .WithTags("Contact Admin");

        admin.MapGet("/", (ContactSubmissionStatus? status, ContactService service) =>
            Results.Ok(service.GetAll(status)));
        admin.MapGet("/{id:guid}", (Guid id, ContactService service) =>
        {
            var submission = service.GetById(id);
            return submission is null ? Results.NotFound() : Results.Ok(submission);
        });
        admin.MapPatch("/{id:guid}/status", async (
            Guid id,
            UpdateContactStatusRequest request,
            ContactService service,
            CancellationToken cancellationToken) =>
            ApiResults.From(
                await service.UpdateStatusAsync(id, request.Status, cancellationToken),
                Results.Ok));
        admin.MapDelete("/{id:guid}", async (
            Guid id,
            ContactService service,
            CancellationToken cancellationToken) =>
            ApiResults.From(
                await service.DeleteAsync(id, cancellationToken),
                _ => Results.NoContent()));

        return endpoints;
    }
}
