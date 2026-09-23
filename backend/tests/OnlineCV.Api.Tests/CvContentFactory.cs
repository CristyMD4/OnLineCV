using OnlineCV.Api.Models;

namespace OnlineCV.Api.Tests;

/// <summary>
/// Builds a CV document that satisfies every rule in <c>CvContentProcessor.Validate</c>, so a test
/// can break exactly one rule and be sure that is the only reason validation failed.
/// </summary>
internal static class CvContentFactory
{
    public static CvContent Valid() => new()
    {
        Identity = new Profile
        {
            Name = "Ada Lovelace",
            Role = "Backend Engineer",
            Email = "ada@example.com",
            Summary = "Builds and maintains service backends.",
            Location = "Remote"
        },
        Projects =
        [
            new Project
            {
                Id = "ledger-api",
                Name = "Ledger API",
                Challenge = "Postings were reconciled by hand.",
                Solution = "Introduced an append-only posting service.",
                RepoUrl = "https://example.com/ledger-api"
            }
        ],
        SkillGroups =
        [
            new SkillGroup { Title = "Backend Engineering", Score = 90, Items = ["C#", "ASP.NET Core"] }
        ],
        Experience =
        [
            new ExperienceItem { Role = "Backend Engineer", Company = "Example Ltd", Period = "2022-2026" }
        ],
        Credentials =
        [
            new Credential { Title = "BSc Computer Science", Status = "Degree" },
            new Credential { Title = "Azure Fundamentals", Status = "Certificate" }
        ],
        Metrics =
        [
            new Metric { Label = "Years of experience", Value = "4" }
        ],
        Navigation =
        [
            new NavigationItem { Label = "Projects", Href = "#projects" }
        ],
        ContactMethods =
        [
            new ContactMethod { Label = "Email", Value = "ada@example.com", Href = "mailto:ada@example.com" }
        ],
        Achievements = [new Achievement { Title = "Cut deploy time", Detail = "From 40 to 6 minutes." }],
        EngineeringStandards = [new EngineeringStandard { Title = "Reviewed changes", Detail = "Every change is reviewed." }],
        StackMatrix = [new StackGroup { Area = "Backend", Tools = ["C#", "PostgreSQL"] }],
        ProfessionalDetails = [new Detail { Label = "Availability", Value = "Two weeks" }],
        Workflow = [new WorkflowStep { Label = "Discover", Text = "Agree the problem first." }],
        Strengths = [new TextItem { Title = "Debugging", Text = "Finds root causes quickly." }],
        RoleFit = [new TextItem { Title = "Backend", Text = "Service and data work." }],
        RoleSignals = [new TextItem { Title = "Ownership", Text = "Follows work to production." }],
        Languages = [new TextItem { Title = "English", Text = "Fluent" }],
        Collaboration = ["Pairs on tricky changes."],
        SummaryHighlights = ["Four years on production backends."],
        Tools = ["Rider", "Docker"],
        UiText = new Dictionary<string, string> { ["heroTitle"] = "Backend Engineer" }
    };
}
