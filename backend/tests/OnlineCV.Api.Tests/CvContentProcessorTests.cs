using OnlineCV.Api.Models;
using OnlineCV.Api.Services;

namespace OnlineCV.Api.Tests;

public sealed class CvContentProcessorTests
{
    private readonly CvContentProcessor _processor = new();

    [Fact]
    public void ValidDocumentPassesValidation()
    {
        Assert.Empty(_processor.Validate(_processor.Normalize(CvContentFactory.Valid())));
    }

    [Fact]
    public void NormalizeTrimsStringsAnywhereInTheDocument()
    {
        var content = CvContentFactory.Valid();
        content.Identity.Name = "  Ada Lovelace  ";
        content.Projects[0].Name = "  Ledger API  ";

        var normalized = _processor.Normalize(content);

        Assert.Equal("Ada Lovelace", normalized.Identity.Name);
        Assert.Equal("Ledger API", normalized.Projects[0].Name);
    }

    [Fact]
    public void NormalizeDropsBlankListEntries()
    {
        var content = CvContentFactory.Valid();
        content.Tools = ["Rider", "   ", "", "Docker"];
        content.Projects[0].Stack = ["C#", "  "];

        var normalized = _processor.Normalize(content);

        Assert.Equal(["Rider", "Docker"], normalized.Tools);
        Assert.Equal(["C#"], normalized.Projects[0].Stack);
    }

    [Fact]
    public void NormalizeDerivesAProjectIdFromTheNameWhenItIsMissing()
    {
        var content = CvContentFactory.Valid();
        content.Projects[0].Id = "";
        content.Projects[0].Name = "Ledger API v2";

        Assert.Equal("ledger-api-v2", _processor.Normalize(content).Projects[0].Id);
    }

    [Fact]
    public void NormalizeSlugifiesASuppliedProjectId()
    {
        var content = CvContentFactory.Valid();
        content.Projects[0].Id = "Ledger API";

        Assert.Equal("ledger-api", _processor.Normalize(content).Projects[0].Id);
    }

    [Fact]
    public void NormalizeTurnsABlankRepositoryUrlIntoNull()
    {
        var content = CvContentFactory.Valid();
        content.Projects[0].RepoUrl = "   ";

        Assert.Null(_processor.Normalize(content).Projects[0].RepoUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    public void ProfileEmailMustBePresentAndValid(string email)
    {
        var content = CvContentFactory.Valid();
        content.Identity.Email = email;

        AssertFails(content, "identity.email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ProfileNameIsRequired(string name)
    {
        var content = CvContentFactory.Valid();
        content.Identity.Name = name;

        AssertFails(content, "identity.name");
    }

    [Fact]
    public void ACvNeedsAtLeastOneProject()
    {
        var content = CvContentFactory.Valid();
        content.Projects.Clear();

        AssertFails(content, "projects");
    }

    [Fact]
    public void ACvCannotHoldMoreThanFiftyProjects()
    {
        var content = CvContentFactory.Valid();
        content.Projects.Clear();
        for (var index = 0; index < 51; index++)
        {
            content.Projects.Add(new Project
            {
                Id = $"project-{index}",
                Name = $"Project {index}",
                Challenge = "Challenge",
                Solution = "Solution"
            });
        }

        AssertFails(content, "projects");
    }

    [Fact]
    public void ProjectIdsMustBeUnique()
    {
        var content = CvContentFactory.Valid();
        content.Projects.Add(new Project
        {
            Id = "ledger-api",
            Name = "Ledger API copy",
            Challenge = "Challenge",
            Solution = "Solution"
        });

        AssertFails(content, "projects[1].id");
    }

    [Theory]
    [InlineData("example.com/repo")]
    [InlineData("ftp://example.com/repo")]
    [InlineData("/relative/path")]
    public void ARepositoryUrlMustBeAbsoluteHttp(string repoUrl)
    {
        var content = CvContentFactory.Valid();
        content.Projects[0].RepoUrl = repoUrl;

        AssertFails(content, "projects[0].repoUrl");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void ASkillScoreMustSitBetweenZeroAndOneHundred(int score)
    {
        var content = CvContentFactory.Valid();
        content.SkillGroups[0].Score = score;

        AssertFails(content, "skillGroups[0].score");
    }

    [Fact]
    public void SkillTitlesMustBeUnique()
    {
        var content = CvContentFactory.Valid();
        content.SkillGroups.Add(new SkillGroup { Title = "Backend Engineering", Score = 50 });

        AssertFails(content, "skillGroups[1].title");
    }

    [Fact]
    public void ExperienceEntriesMustBeUniqueByRoleAndCompany()
    {
        var content = CvContentFactory.Valid();
        content.Experience.Add(new ExperienceItem { Role = "Backend Engineer", Company = "Example Ltd" });

        AssertFails(content, "experience[1].role");
    }

    [Fact]
    public void ExperienceRequiresACompany()
    {
        var content = CvContentFactory.Valid();
        content.Experience[0].Company = "";

        AssertFails(content, "experience[0].company");
    }

    [Theory]
    [InlineData("projects")]
    [InlineData("https://example.com")]
    [InlineData("")]
    public void NavigationMustPointAtAnAnchorOrRelativePath(string href)
    {
        var content = CvContentFactory.Valid();
        content.Navigation[0].Href = href;

        AssertFails(content, "navigation[0].href");
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("example.com")]
    [InlineData("")]
    public void AContactLinkMustUseASupportedScheme(string href)
    {
        var content = CvContentFactory.Valid();
        content.ContactMethods[0].Href = href;

        AssertFails(content, "contactMethods[0].href");
    }

    [Fact]
    public void PageWordingCannotBeEmpty()
    {
        var content = CvContentFactory.Valid();
        content.UiText.Clear();

        AssertFails(content, "uiText");
    }

    [Fact]
    public void CredentialTitlesMustBeUnique()
    {
        var content = CvContentFactory.Valid();
        content.Credentials.Add(new Credential { Title = "BSc Computer Science", Status = "Degree" });

        AssertFails(content, "credentials[2].title");
    }

    [Fact]
    public void MetricLabelsMustBeUnique()
    {
        var content = CvContentFactory.Valid();
        content.Metrics.Add(new Metric { Label = "Years of experience", Value = "5" });

        AssertFails(content, "metrics[1].label");
    }

    [Fact]
    public void AMetricNeedsAValue()
    {
        var content = CvContentFactory.Valid();
        content.Metrics[0].Value = "";

        AssertFails(content, "metrics[0].value");
    }

    [Fact]
    public void ATitleThatProducesNoSlugIsRejected()
    {
        var content = CvContentFactory.Valid();
        content.Strengths[0].Title = "!!!";

        AssertFails(content, "strengths[0].title");
    }

    [Fact]
    public void EachIdentifiedSectionRejectsDuplicates()
    {
        var content = CvContentFactory.Valid();
        content.Achievements.Add(new Achievement { Title = "Cut deploy time", Detail = "Again." });
        content.StackMatrix.Add(new StackGroup { Area = "Backend" });
        content.Workflow.Add(new WorkflowStep { Label = "Discover", Text = "Again." });
        content.Languages.Add(new TextItem { Title = "English", Text = "Native" });

        var fields = Fields(content);

        Assert.Contains("achievements[1].title", fields);
        Assert.Contains("stackMatrix[1].area", fields);
        Assert.Contains("workflow[1].label", fields);
        Assert.Contains("languages[1].title", fields);
    }

    private void AssertFails(CvContent content, string expectedField) =>
        Assert.Contains(expectedField, Fields(content));

    private string[] Fields(CvContent content) =>
        _processor.Validate(_processor.Normalize(content)).Select(error => error.Field).ToArray();
}
