using OnlineCV.Api.Models;

namespace OnlineCV.Api.Services;

/// <summary>
/// Every addressable list section of the CV document. Adding a section here and mapping it in
/// <c>CvEndpoints</c> is all that a new CRUD surface requires.
/// </summary>
public static class CvSections
{
    public static readonly CvSection<Project> Projects = new(
        "projects",
        content => content.Projects,
        project => CvContentProcessor.Slugify(project.Id),
        "id",
        "Project",
        (project, slug) => project.Id = slug,
        project => CvContentProcessor.Slugify(project.Name));

    public static readonly CvSection<SkillGroup> Skills = new(
        "skillGroups",
        content => content.SkillGroups,
        skill => CvContentProcessor.Slugify(skill.Title),
        "title",
        "Skill group");

    public static readonly CvSection<ExperienceItem> Experience = new(
        "experience",
        content => content.Experience,
        item => CvContentProcessor.Slugify($"{item.Role} {item.Company}"),
        "role",
        "Experience entry");

    public static readonly CvSection<Credential> Credentials = new(
        "credentials",
        content => content.Credentials,
        credential => CvContentProcessor.Slugify(credential.Title),
        "title",
        "Credential");

    public static readonly CvSection<Metric> Metrics = new(
        "metrics",
        content => content.Metrics,
        metric => CvContentProcessor.Slugify(metric.Label),
        "label",
        "Metric");

    public static readonly CvSection<Achievement> Achievements = new(
        "achievements",
        content => content.Achievements,
        achievement => CvContentProcessor.Slugify(achievement.Title),
        "title",
        "Achievement");

    public static readonly CvSection<NavigationItem> Navigation = new(
        "navigation",
        content => content.Navigation,
        item => CvContentProcessor.Slugify(item.Label),
        "label",
        "Navigation entry");

    public static readonly CvSection<ContactMethod> ContactMethods = new(
        "contactMethods",
        content => content.ContactMethods,
        method => CvContentProcessor.Slugify(method.Label),
        "label",
        "Contact method");

    public static readonly CvSection<EngineeringStandard> EngineeringStandards = new(
        "engineeringStandards",
        content => content.EngineeringStandards,
        standard => CvContentProcessor.Slugify(standard.Title),
        "title",
        "Engineering standard");

    public static readonly CvSection<StackGroup> StackMatrix = new(
        "stackMatrix",
        content => content.StackMatrix,
        group => CvContentProcessor.Slugify(group.Area),
        "area",
        "Stack area");

    public static readonly CvSection<Detail> ProfessionalDetails = new(
        "professionalDetails",
        content => content.ProfessionalDetails,
        detail => CvContentProcessor.Slugify(detail.Label),
        "label",
        "Professional detail");

    public static readonly CvSection<WorkflowStep> Workflow = new(
        "workflow",
        content => content.Workflow,
        step => CvContentProcessor.Slugify(step.Label),
        "label",
        "Workflow step");

    public static readonly CvSection<TextItem> Strengths = new(
        "strengths",
        content => content.Strengths,
        item => CvContentProcessor.Slugify(item.Title),
        "title",
        "Strength");

    public static readonly CvSection<TextItem> RoleFit = new(
        "roleFit",
        content => content.RoleFit,
        item => CvContentProcessor.Slugify(item.Title),
        "title",
        "Role fit entry");

    public static readonly CvSection<TextItem> RoleSignals = new(
        "roleSignals",
        content => content.RoleSignals,
        item => CvContentProcessor.Slugify(item.Title),
        "title",
        "Role signal");

    public static readonly CvSection<TextItem> Languages = new(
        "languages",
        content => content.Languages,
        item => CvContentProcessor.Slugify(item.Title),
        "title",
        "Language");
}
