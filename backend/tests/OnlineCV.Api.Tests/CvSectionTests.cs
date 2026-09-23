using OnlineCV.Api.Models;
using OnlineCV.Api.Services;
using OnlineCV.Api.Tests.TestDoubles;

namespace OnlineCV.Api.Tests;

/// <summary>
/// Covers the generic section CRUD in <c>CvService</c>. Each section is exercised through the same
/// helper so that adding a section to <c>CvSections</c> is enough to cover it here too.
/// </summary>
public sealed class CvSectionTests
{
    private readonly InMemoryCvRepository _repository = new(CvContentFactory.Valid());
    private readonly CvService _service;

    public CvSectionTests() => _service = new CvService(_repository, new CvContentProcessor());

    [Fact]
    public async Task AddingAProjectAssignsASlugAndReturnsTheStoredEntry()
    {
        var result = await _service.AddItemAsync(
            CvSections.Projects,
            new Project
            {
                Name = "Billing Gateway",
                Challenge = "Payments failed silently.",
                Solution = "Added a retry pipeline."
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("billing-gateway", result.Value!.Id);
        Assert.Equal(2, _service.Projects.Count);
    }

    [Fact]
    public async Task AddingAProjectWithAnExistingIdConflicts()
    {
        var result = await _service.AddItemAsync(
            CvSections.Projects,
            new Project
            {
                Id = "ledger-api",
                Name = "Ledger API again",
                Challenge = "Challenge",
                Solution = "Solution"
            },
            CancellationToken.None);

        Assert.Equal(ServiceResultStatus.Conflict, result.Status);
        Assert.Equal("id", result.Errors[0].Field);
        Assert.Single(_service.Projects);
    }

    [Fact]
    public async Task AddingASkillWhoseTitleOnlyDiffersInCaseConflicts()
    {
        var result = await _service.AddItemAsync(
            CvSections.Skills,
            new SkillGroup { Title = "backend engineering", Score = 10 },
            CancellationToken.None);

        Assert.Equal(ServiceResultStatus.Conflict, result.Status);
    }

    [Fact]
    public void AnItemIsAddressableByASlugDerivedFromItsTitle()
    {
        // The route id arrives unslugged; the service has to slugify it before matching.
        Assert.NotNull(_service.GetItem(CvSections.Skills, "Backend Engineering"));
        Assert.NotNull(_service.GetItem(CvSections.Skills, "backend-engineering"));
        Assert.NotNull(_service.GetItem(CvSections.Experience, "Backend Engineer Example Ltd"));
        Assert.Null(_service.GetItem(CvSections.Skills, "frontend"));
    }

    [Fact]
    public async Task UpdatingAnEntryKeepsItsPositionAndRoutesById()
    {
        var result = await _service.UpdateItemAsync(
            CvSections.Experience,
            "backend-engineer-example-ltd",
            new ExperienceItem { Role = "Senior Backend Engineer", Company = "Example Ltd", Period = "2024-2026" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Senior Backend Engineer", result.Value!.Role);
        Assert.Single(_service.Experience);
    }

    [Fact]
    public async Task UpdatingAProjectWithoutAnIdKeepsTheRouteId()
    {
        var result = await _service.UpdateItemAsync(
            CvSections.Projects,
            "ledger-api",
            new Project { Name = "Renamed", Challenge = "Challenge", Solution = "Solution" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ledger-api", result.Value!.Id);
    }

    [Fact]
    public async Task UpdatingOntoAnotherEntrysIdentityConflicts()
    {
        await _service.AddItemAsync(
            CvSections.Credentials,
            new Credential { Title = "MSc Distributed Systems", Status = "Degree" },
            CancellationToken.None);

        var result = await _service.UpdateItemAsync(
            CvSections.Credentials,
            "msc-distributed-systems",
            new Credential { Title = "BSc Computer Science", Status = "Degree" },
            CancellationToken.None);

        Assert.Equal(ServiceResultStatus.Conflict, result.Status);
        Assert.Equal("title", result.Errors[0].Field);
    }

    [Fact]
    public async Task DeletingRemovesTheEntry()
    {
        var result = await _service.DeleteItemAsync(
            CvSections.Credentials,
            "azure-fundamentals",
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_service.Certificates);
        Assert.Single(_service.Education);
    }

    [Fact]
    public async Task ReplacingASectionSwapsEveryEntry()
    {
        var result = await _service.ReplaceSectionAsync(
            CvSections.Metrics,
            [
                new Metric { Label = "Services shipped", Value = "12" },
                new Metric { Label = "Uptime", Value = "99.9%" }
            ],
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["Services shipped", "Uptime"], _service.Metrics.Select(metric => metric.Label));
    }

    [Fact]
    public async Task ReplacingTheProjectsSectionAssignsMissingIds()
    {
        var result = await _service.ReplaceSectionAsync(
            CvSections.Projects,
            [new Project { Name = "Fleet Tracker", Challenge = "Challenge", Solution = "Solution" }],
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("fleet-tracker", result.Value!.Single().Id);
    }

    [Fact]
    public async Task AnInvalidEntryFailsValidationAndLeavesTheDocumentUntouched()
    {
        var before = _repository.WriteCount;

        var result = await _service.AddItemAsync(
            CvSections.Projects,
            new Project { Name = "Missing fields" },
            CancellationToken.None);

        Assert.Equal(ServiceResultStatus.ValidationFailed, result.Status);
        Assert.Contains(result.Errors, error => error.Field.EndsWith(".challenge", StringComparison.Ordinal));
        Assert.Single(_service.Projects);
        Assert.Equal(before, _repository.WriteCount);
    }

    [Fact]
    public async Task DeletingTheLastProjectIsRejectedByTheProjectCountRule()
    {
        var result = await _service.DeleteItemAsync(CvSections.Projects, "ledger-api", CancellationToken.None);

        Assert.Equal(ServiceResultStatus.ValidationFailed, result.Status);
        Assert.Single(_service.Projects);
    }

    [Theory]
    [MemberData(nameof(MissingIdCases))]
    public async Task AnUnknownIdIsReportedAsNotFound(
        string operation,
        Func<CvService, Task<ServiceResult<bool>>> act)
    {
        var result = await act(_service);

        Assert.Equal(ServiceResultStatus.NotFound, result.Status);
        Assert.Contains("nope", result.Errors[0].Message);
        Assert.Contains(operation, MissingIdCaseNames);
    }

    private static readonly string[] MissingIdCaseNames =
        ["update project", "delete project", "update skill", "delete experience", "delete credential"];

    public static TheoryData<string, Func<CvService, Task<ServiceResult<bool>>>> MissingIdCases() => new()
    {
        {
            "update project",
            async service => Discard(await service.UpdateItemAsync(
                CvSections.Projects, "nope", new Project { Name = "N" }, CancellationToken.None))
        },
        {
            "delete project",
            service => service.DeleteItemAsync(CvSections.Projects, "nope", CancellationToken.None)
        },
        {
            "update skill",
            async service => Discard(await service.UpdateItemAsync(
                CvSections.Skills, "nope", new SkillGroup { Title = "N" }, CancellationToken.None))
        },
        {
            "delete experience",
            service => service.DeleteItemAsync(CvSections.Experience, "nope", CancellationToken.None)
        },
        {
            "delete credential",
            service => service.DeleteItemAsync(CvSections.Credentials, "nope", CancellationToken.None)
        }
    };

    /// <summary>Re-shapes a typed failure so every case in the theory has one signature.</summary>
    private static ServiceResult<bool> Discard<T>(ServiceResult<T> result) => result.Status switch
    {
        ServiceResultStatus.NotFound => ServiceResult<bool>.NotFound(result.Errors[0].Message),
        ServiceResultStatus.Conflict => ServiceResult<bool>.Conflict(result.Errors[0].Message, result.Errors[0].Field),
        ServiceResultStatus.ValidationFailed => ServiceResult<bool>.Validation(result.Errors),
        _ => ServiceResult<bool>.Success(true)
    };

    [Fact]
    public async Task UpdatingTheProfileValidatesItsEmail()
    {
        var result = await _service.UpdateProfileAsync(
            new Profile { Name = "Ada", Role = "Engineer", Email = "nope", Summary = "Summary" },
            CancellationToken.None);

        Assert.Equal(ServiceResultStatus.ValidationFailed, result.Status);
        Assert.Contains(result.Errors, error => error.Field == "identity.email");
    }

    [Fact]
    public async Task ReplacingATextListDropsBlankEntries()
    {
        var result = await _service.ReplaceTextListAsync(
            content => content.Tools,
            ["Rider", "   ", "Docker"],
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["Rider", "Docker"], result.Value!);
    }

    [Fact]
    public async Task ReplacingThePageWordingWithAnEmptyDictionaryIsRejected()
    {
        var result = await _service.ReplaceUiTextAsync(
            new Dictionary<string, string>(),
            CancellationToken.None);

        Assert.Equal(ServiceResultStatus.ValidationFailed, result.Status);
        Assert.Contains(result.Errors, error => error.Field == "uiText");
    }

    [Fact]
    public async Task ASnapshotTakenBeforeAWriteIsNotChangedByIt()
    {
        var before = _service.Content;

        await _service.AddItemAsync(
            CvSections.Achievements,
            new Achievement { Title = "Shipped v2", Detail = "On time." },
            CancellationToken.None);

        Assert.Single(before.Achievements);
        Assert.Equal(2, _service.Content.Achievements.Count);
    }
}
