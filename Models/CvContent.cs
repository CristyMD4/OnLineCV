namespace OnLineCV.Models;

public sealed class CvContent
{
    public UiContent Ui { get; set; } = new();
    public IdentityContent Identity { get; set; } = new();
    public List<string> SummaryStrip { get; set; } = [];
    public List<MetricItem> Metrics { get; set; } = [];
    public List<SkillGroup> SkillGroups { get; set; } = [];
    public List<TextDetail> Achievements { get; set; } = [];
    public List<TextDetail> Strengths { get; set; } = [];
    public List<TextDetail> RoleFit { get; set; } = [];
    public List<TextDetail> RoleSignals { get; set; } = [];
    public List<EngineeringStandard> EngineeringStandards { get; set; } = [];
    public List<StackMatrixGroup> StackMatrix { get; set; } = [];
    public List<ProjectItem> Projects { get; set; } = [];
    public List<ExperienceItem> Experience { get; set; } = [];
    public List<WorkflowStep> Workflow { get; set; } = [];
    public List<string> Tools { get; set; } = [];
    public List<CredentialItem> Credentials { get; set; } = [];
    public List<string> Collaboration { get; set; } = [];
    public ProfessionalDetails ProfessionalDetails { get; set; } = new();
    public List<TextDetail> Languages { get; set; } = [];
}

public sealed class UiContent
{
    public NavLabels Nav { get; set; } = new();
    public ActionLabels Actions { get; set; } = new();
    public AriaLabels Aria { get; set; } = new();
    public UiLabels Labels { get; set; } = new();
    public SectionLabels Sections { get; set; } = new();
}

public sealed class NavLabels
{
    public string Projects { get; set; } = "";
    public string Experience { get; set; } = "";
    public string Skills { get; set; } = "";
    public string Stack { get; set; } = "";
    public string Standards { get; set; } = "";
    public string Impact { get; set; } = "";
    public string Fit { get; set; } = "";
    public string Details { get; set; } = "";
    public string Contact { get; set; } = "";
}

public sealed class ActionLabels
{
    public string DownloadPdf { get; set; } = "";
    public string PrintCv { get; set; } = "";
    public string ViewRepository { get; set; } = "";
}

public sealed class AriaLabels
{
    public string Navigation { get; set; } = "";
    public string Highlights { get; set; } = "";
    public string ContactLinks { get; set; } = "";
    public string RoleFocus { get; set; } = "";
    public string CareerSnapshot { get; set; } = "";
    public string Avatar { get; set; } = "";
    public string CapabilityOverview { get; set; } = "";
    public string ProjectCases { get; set; } = "";
    public string ProjectPreview { get; set; } = "";
    public string ContactMethods { get; set; } = "";
    public string LanguageSwitcher { get; set; } = "";
    public string Proficiency { get; set; } = "";
}

public sealed class UiLabels
{
    public string DeveloperProfile { get; set; } = "";
    public string Open { get; set; } = "";
    public string Role { get; set; } = "";
    public string Duration { get; set; } = "";
    public string Challenge { get; set; } = "";
    public string Solution { get; set; } = "";
    public string Highlights { get; set; } = "";
    public string Scope { get; set; } = "";
    public string Deliverables { get; set; } = "";
    public string TechStack { get; set; } = "";
    public string Email { get; set; } = "";
    public string Github { get; set; } = "";
    public string Location { get; set; } = "";
}

public sealed class SectionLabels
{
    public string ProfileEyebrow { get; set; } = "";
    public string ImpactEyebrow { get; set; } = "";
    public string ImpactTitle { get; set; } = "";
    public string ImpactSubtitle { get; set; } = "";
    public string StrengthsEyebrow { get; set; } = "";
    public string StrengthsTitle { get; set; } = "";
    public string StrengthsSubtitle { get; set; } = "";
    public string ProjectsEyebrow { get; set; } = "";
    public string ProjectsTitle { get; set; } = "";
    public string ProjectsSubtitle { get; set; } = "";
    public string ExperienceEyebrow { get; set; } = "";
    public string ExperienceTitle { get; set; } = "";
    public string EducationEyebrow { get; set; } = "";
    public string EducationTitle { get; set; } = "";
    public string EducationSubtitle { get; set; } = "";
    public string WorkflowEyebrow { get; set; } = "";
    public string WorkflowTitle { get; set; } = "";
    public string WorkflowSubtitle { get; set; } = "";
    public string CollaborationEyebrow { get; set; } = "";
    public string CollaborationTitle { get; set; } = "";
    public string DetailsEyebrow { get; set; } = "";
    public string DetailsTitle { get; set; } = "";
    public string SkillsEyebrow { get; set; } = "";
    public string SkillsTitle { get; set; } = "";
    public string StackEyebrow { get; set; } = "";
    public string StackTitle { get; set; } = "";
    public string StandardsEyebrow { get; set; } = "";
    public string StandardsTitle { get; set; } = "";
    public string FitEyebrow { get; set; } = "";
    public string FitTitle { get; set; } = "";
    public string ToolsEyebrow { get; set; } = "";
    public string ToolsTitle { get; set; } = "";
    public string LanguagesEyebrow { get; set; } = "";
    public string LanguagesTitle { get; set; } = "";
    public string ContactEyebrow { get; set; } = "";
    public string ContactTitle { get; set; } = "";
}

public class IdentityContent
{
    public string Role { get; set; } = "";
    public string Location { get; set; } = "";
    public string Availability { get; set; } = "";
    public string WorkMode { get; set; } = "";
    public string Summary { get; set; } = "";
    public string ProfileSummary { get; set; } = "";
}

public sealed class IdentityView : IdentityContent
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Github { get; set; } = "";
    public string Linkedin { get; set; } = "";
}

public sealed class MetricItem
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

public sealed class TextDetail
{
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";
    public string Detail { get; set; } = "";
}

public sealed class EngineeringStandard
{
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
    public string Proof { get; set; } = "";
}

public sealed class StackMatrixGroup
{
    public string Area { get; set; } = "";
    public List<string> Tools { get; set; } = [];
}

public sealed class ProjectItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Role { get; set; } = "";
    public string Duration { get; set; } = "";
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

public sealed class WorkflowStep
{
    public string Label { get; set; } = "";
    public string Text { get; set; } = "";
}

public sealed class CredentialItem
{
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
    public string Focus { get; set; } = "";
    public string Status { get; set; } = "";
    public string? Period { get; set; }
}

public sealed class ProfessionalDetails
{
    public string Location { get; set; } = "";
    public string Availability { get; set; } = "";
    public string WorkMode { get; set; } = "";
    public string Focus { get; set; } = "";
    public string FocusValue { get; set; } = "";
}
