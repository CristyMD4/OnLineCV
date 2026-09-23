using OnlineCV.Api.Models;
using OnlineCV.Api.Services;

namespace OnlineCV.Api.Endpoints;

public static class CvEndpoints
{
    public static IEndpointRouteBuilder MapCvEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var cv = endpoints.MapGroup("/api").WithTags("CV Content");

        cv.MapGet("/cv", (CvService service) => Results.Ok(service.Content))
            .WithSummary("Get the complete CV document");

        cv.MapPut("/cv", async (CvContent content, CvService service, CancellationToken cancellationToken) =>
            ApiResults.From(
                await service.UpdateAsync(content, cancellationToken),
                Results.Ok))
            .RequireAuthorization("AdminOnly")
            .WithSummary("Validate and replace the complete CV document");

        cv.MapGet("/profile", (CvService service) => Results.Ok(service.Profile))
            .WithSummary("Get the profile section");
        cv.MapPut("/profile", async (Profile profile, CvService service, CancellationToken cancellationToken) =>
            ApiResults.From(
                await service.UpdateProfileAsync(profile, cancellationToken),
                Results.Ok))
            .RequireAuthorization("AdminOnly")
            .WithSummary("Update the profile section");

        // Education and certificates are views over credentials, split on the Status field, so they
        // stay read-only. Write to /api/credentials instead.
        cv.MapGet("/education", (CvService service) => Results.Ok(service.Education))
            .WithSummary("List credentials that are not certificates");
        cv.MapGet("/certificates", (CvService service) => Results.Ok(service.Certificates))
            .WithSummary("List credentials marked as certificates");

        endpoints.MapSection(CvSections.Projects, "projects");
        endpoints.MapSection(CvSections.Skills, "skills");
        endpoints.MapSection(CvSections.Experience, "experience");
        endpoints.MapSection(CvSections.Credentials, "credentials");
        endpoints.MapSection(CvSections.Metrics, "metrics");
        endpoints.MapSection(CvSections.Achievements, "achievements");
        endpoints.MapSection(CvSections.Navigation, "navigation");
        endpoints.MapSection(CvSections.ContactMethods, "contact-methods");
        endpoints.MapSection(CvSections.EngineeringStandards, "engineering-standards");
        endpoints.MapSection(CvSections.StackMatrix, "stack-matrix");
        endpoints.MapSection(CvSections.ProfessionalDetails, "professional-details");
        endpoints.MapSection(CvSections.Workflow, "workflow");
        endpoints.MapSection(CvSections.Strengths, "strengths");
        endpoints.MapSection(CvSections.RoleFit, "role-fit");
        endpoints.MapSection(CvSections.RoleSignals, "role-signals");
        endpoints.MapSection(CvSections.Languages, "languages");

        endpoints.MapTextList("collaboration", content => content.Collaboration);
        endpoints.MapTextList("summary-highlights", content => content.SummaryHighlights);
        endpoints.MapTextList("tools", content => content.Tools);

        cv.MapGet("/ui-text", (CvService service) => Results.Ok(service.Content.UiText))
            .WithSummary("Get the page wording dictionary");
        cv.MapPut("/ui-text", async (
                Dictionary<string, string> values,
                CvService service,
                CancellationToken cancellationToken) =>
            ApiResults.From(
                await service.ReplaceUiTextAsync(values, cancellationToken),
                Results.Ok))
            .RequireAuthorization("AdminOnly")
            .WithSummary("Replace the page wording dictionary");

        return endpoints;
    }
}
