using System.Text.Json.Serialization;

namespace OnlineCV.Api.Models;

public sealed class CvContent
{
    public List<Achievement> Achievements { get; set; } = [];
    public List<string> Collaboration { get; set; } = [];
    public List<ContactMethod> ContactMethods { get; set; } = [];
    public List<Credential> Credentials { get; set; } = [];
    public List<EngineeringStandard> EngineeringStandards { get; set; } = [];
    public List<ExperienceItem> Experience { get; set; } = [];
    public Profile Identity { get; set; } = new();
    public List<Metric> Metrics { get; set; } = [];
    public List<NavigationItem> Navigation { get; set; } = [];
    public List<Detail> ProfessionalDetails { get; set; } = [];
    public List<Project> Projects { get; set; } = [];
    public List<TextItem> RoleFit { get; set; } = [];
    public List<TextItem> RoleSignals { get; set; } = [];
    public List<SkillGroup> SkillGroups { get; set; } = [];
    public List<StackGroup> StackMatrix { get; set; } = [];
    public List<TextItem> Strengths { get; set; } = [];
    public List<string> SummaryHighlights { get; set; } = [];
    public List<string> Tools { get; set; } = [];
    public List<TextItem> Languages { get; set; } = [];
    public Dictionary<string, string> UiText { get; set; } = [];
    public List<WorkflowStep> Workflow { get; set; } = [];
}

public sealed class NavigationItem
{
    public string Label { get; set; } = "";
    public string Href { get; set; } = "";
}

public sealed class Profile
{
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Location { get; set; } = "";

    [JsonPropertyName("github")]
    public string GitHub { get; set; } = "";

    [JsonPropertyName("linkedin")]
    public string LinkedIn { get; set; } = "";

    public string Availability { get; set; } = "";
    public string WorkMode { get; set; } = "";
    public string Summary { get; set; } = "";
}

public sealed class Metric
{
    public string Value { get; set; } = "";
    public string Label { get; set; } = "";
}

public sealed class SkillGroup
{
    public string Title { get; set; } = "";
    public int Score { get; set; }
    public List<string> Items { get; set; } = [];
}

public sealed class Achievement
{
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
}

public sealed class TextItem
{
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";
}

public sealed class EngineeringStandard
{
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
    public string Proof { get; set; } = "";
}

public sealed class StackGroup
{
    public string Area { get; set; } = "";
    public List<string> Tools { get; set; } = [];
}

public sealed class Project
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Role { get; set; } = "";
    public string Duration { get; set; } = "";
    public string? RepoUrl { get; set; }
    public List<string> Stack { get; set; } = [];
    public string Metric { get; set; } = "";
    public ProjectPreview Preview { get; set; } = new();
    public List<string> Scope { get; set; } = [];
    public string Challenge { get; set; } = "";
    public string Solution { get; set; } = "";
    public string Impact { get; set; } = "";
    public List<string> Deliverables { get; set; } = [];
    public List<string> Highlights { get; set; } = [];
}

public sealed class ProjectPreview
{
    public string Label { get; set; } = "";
    public string Title { get; set; } = "";
    public List<string> Stats { get; set; } = [];
}

public sealed class ExperienceItem
{
    public string Role { get; set; } = "";
    public string Company { get; set; } = "";
    public string Period { get; set; } = "";
    public string Stack { get; set; } = "";
    public List<string> Bullets { get; set; } = [];
}

public sealed class Credential
{
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
    public string Focus { get; set; } = "";
    public string Status { get; set; } = "";
    public string Period { get; set; } = "";
}

public sealed class WorkflowStep
{
    public string Label { get; set; } = "";
    public string Text { get; set; } = "";
}

public sealed class Detail
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
}

public sealed class ContactMethod
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string Href { get; set; } = "";
}
