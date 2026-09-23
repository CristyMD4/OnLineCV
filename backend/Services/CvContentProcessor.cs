using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using OnlineCV.Api.Models;

namespace OnlineCV.Api.Services;

public sealed partial class CvContentProcessor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public CvContent Normalize(CvContent content)
    {
        var node = JsonSerializer.SerializeToNode(content, JsonOptions)
            ?? throw new InvalidOperationException("CV content could not be normalized.");
        TrimStrings(node);
        var normalized = node.Deserialize<CvContent>(JsonOptions)
            ?? throw new InvalidOperationException("CV content could not be normalized.");

        normalized.Collaboration = CleanList(normalized.Collaboration);
        normalized.SummaryHighlights = CleanList(normalized.SummaryHighlights);
        normalized.Tools = CleanList(normalized.Tools);

        foreach (var project in normalized.Projects ?? [])
        {
            project.Preview ??= new ProjectPreview();
            project.Id = string.IsNullOrWhiteSpace(project.Id) ? Slugify(project.Name) : Slugify(project.Id);
            project.RepoUrl = string.IsNullOrWhiteSpace(project.RepoUrl) ? null : project.RepoUrl;
            project.Stack = CleanList(project.Stack);
            project.Scope = CleanList(project.Scope);
            project.Deliverables = CleanList(project.Deliverables);
            project.Highlights = CleanList(project.Highlights);
            project.Preview.Stats = CleanList(project.Preview.Stats);
        }

        foreach (var skill in normalized.SkillGroups ?? [])
        {
            skill.Items = CleanList(skill.Items);
        }

        foreach (var group in normalized.StackMatrix ?? [])
        {
            group.Tools = CleanList(group.Tools);
        }

        foreach (var experience in normalized.Experience ?? [])
        {
            experience.Bullets = CleanList(experience.Bullets);
        }

        return normalized;
    }

    public IReadOnlyList<ServiceError> Validate(CvContent content)
    {
        var errors = new List<ServiceError>();
        ValidateRequiredSections(content, errors);
        if (errors.Count > 0)
        {
            return errors;
        }

        Require(content.Identity.Name, "identity.name", "Profile name is required.", errors);
        Require(content.Identity.Role, "identity.role", "Profile role is required.", errors);
        Require(content.Identity.Email, "identity.email", "Profile email is required.", errors);
        Require(content.Identity.Summary, "identity.summary", "Profile summary is required.", errors);
        if (!string.IsNullOrWhiteSpace(content.Identity.Email)
            && !MailAddress.TryCreate(content.Identity.Email, out _))
        {
            errors.Add(new ServiceError("invalid_email", "Enter a valid profile email address.", "identity.email"));
        }

        if (content.Projects.Count is < 1 or > 50)
        {
            errors.Add(new ServiceError("invalid_count", "The CV must contain between 1 and 50 projects.", "projects"));
        }

        var projectIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < content.Projects.Count; index++)
        {
            var project = content.Projects[index];
            var prefix = $"projects[{index}]";
            Require(project.Id, $"{prefix}.id", "Project id is required.", errors);
            Require(project.Name, $"{prefix}.name", "Project name is required.", errors);
            Require(project.Challenge, $"{prefix}.challenge", "Project challenge is required.", errors);
            Require(project.Solution, $"{prefix}.solution", "Project solution is required.", errors);

            if (!string.IsNullOrWhiteSpace(project.Id) && !SlugPattern().IsMatch(project.Id))
            {
                errors.Add(new ServiceError("invalid_slug", "Project id must use lowercase letters, numbers, and hyphens.", $"{prefix}.id"));
            }
            if (!string.IsNullOrWhiteSpace(project.Id) && !projectIds.Add(project.Id))
            {
                errors.Add(new ServiceError("duplicate", "Project ids must be unique.", $"{prefix}.id"));
            }
            if (!IsOptionalHttpUrl(project.RepoUrl))
            {
                errors.Add(new ServiceError("invalid_url", "Repository URL must be an absolute HTTP or HTTPS URL.", $"{prefix}.repoUrl"));
            }
        }

        var skillTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < content.SkillGroups.Count; index++)
        {
            var skill = content.SkillGroups[index];
            Require(skill.Title, $"skillGroups[{index}].title", "Skill group title is required.", errors);
            if (skill.Score is < 0 or > 100)
            {
                errors.Add(new ServiceError("invalid_score", "Skill score must be between 0 and 100.", $"skillGroups[{index}].score"));
            }
            if (!string.IsNullOrWhiteSpace(skill.Title) && !skillTitles.Add(skill.Title))
            {
                errors.Add(new ServiceError("duplicate", "Skill group titles must be unique.", $"skillGroups[{index}].title"));
            }
        }

        var experienceKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < content.Experience.Count; index++)
        {
            var experience = content.Experience[index];
            Require(experience.Role, $"experience[{index}].role", "Experience role is required.", errors);
            Require(experience.Company, $"experience[{index}].company", "Experience company is required.", errors);

            var key = CvSections.Experience.Identity(experience);
            if (key.Length > 0 && !experienceKeys.Add(key))
            {
                errors.Add(new ServiceError(
                    "duplicate",
                    "Experience entries must be unique by role and company.",
                    $"experience[{index}].role"));
            }
        }

        var navigationKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < content.Navigation.Count; index++)
        {
            Require(content.Navigation[index].Label, $"navigation[{index}].label", "Navigation label is required.", errors);
            if (!IsNavigationTarget(content.Navigation[index].Href))
            {
                errors.Add(new ServiceError("invalid_link", "Navigation target must be an anchor or relative path.", $"navigation[{index}].href"));
            }

            var key = CvSections.Navigation.Identity(content.Navigation[index]);
            if (key.Length > 0 && !navigationKeys.Add(key))
            {
                errors.Add(new ServiceError("duplicate", "Navigation labels must be unique.", $"navigation[{index}].label"));
            }
        }

        var contactKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < content.ContactMethods.Count; index++)
        {
            Require(content.ContactMethods[index].Label, $"contactMethods[{index}].label", "Contact label is required.", errors);
            Require(content.ContactMethods[index].Value, $"contactMethods[{index}].value", "Contact value is required.", errors);
            if (!IsContactTarget(content.ContactMethods[index].Href))
            {
                errors.Add(new ServiceError("invalid_link", "Contact link must use HTTP, HTTPS, mailto, tel, or an anchor.", $"contactMethods[{index}].href"));
            }

            var key = CvSections.ContactMethods.Identity(content.ContactMethods[index]);
            if (key.Length > 0 && !contactKeys.Add(key))
            {
                errors.Add(new ServiceError("duplicate", "Contact labels must be unique.", $"contactMethods[{index}].label"));
            }
        }

        ValidateIdentified(CvSections.Credentials, content.Credentials, item => item.Title, errors);
        ValidateIdentified(CvSections.Metrics, content.Metrics, item => item.Label, errors);
        ValidateIdentified(CvSections.Achievements, content.Achievements, item => item.Title, errors);
        ValidateIdentified(CvSections.EngineeringStandards, content.EngineeringStandards, item => item.Title, errors);
        ValidateIdentified(CvSections.StackMatrix, content.StackMatrix, item => item.Area, errors);
        ValidateIdentified(CvSections.ProfessionalDetails, content.ProfessionalDetails, item => item.Label, errors);
        ValidateIdentified(CvSections.Workflow, content.Workflow, item => item.Label, errors);
        ValidateIdentified(CvSections.Strengths, content.Strengths, item => item.Title, errors);
        ValidateIdentified(CvSections.RoleFit, content.RoleFit, item => item.Title, errors);
        ValidateIdentified(CvSections.RoleSignals, content.RoleSignals, item => item.Title, errors);
        ValidateIdentified(CvSections.Languages, content.Languages, item => item.Title, errors);

        for (var index = 0; index < content.Metrics.Count; index++)
        {
            Require(content.Metrics[index].Value, $"metrics[{index}].value", "Metric value is required.", errors);
        }

        if (content.UiText.Count == 0)
        {
            errors.Add(new ServiceError("required", "Page wording cannot be empty.", "uiText"));
        }

        return errors;
    }

    public static string Slugify(string value)
    {
        var normalized = NonSlugCharacters().Replace(value.Trim().ToLowerInvariant(), "-");
        return RepeatedHyphens().Replace(normalized, "-").Trim('-');
    }

    private static void ValidateRequiredSections(CvContent content, List<ServiceError> errors)
    {
        if (content.Identity is null) errors.Add(new ServiceError("required", "Profile is required.", "identity"));
        if (content.Achievements is null) errors.Add(new ServiceError("required", "Achievements are required.", "achievements"));
        if (content.Collaboration is null) errors.Add(new ServiceError("required", "Collaboration is required.", "collaboration"));
        if (content.ContactMethods is null) errors.Add(new ServiceError("required", "Contact methods are required.", "contactMethods"));
        if (content.Credentials is null) errors.Add(new ServiceError("required", "Credentials are required.", "credentials"));
        if (content.EngineeringStandards is null) errors.Add(new ServiceError("required", "Engineering standards are required.", "engineeringStandards"));
        if (content.Experience is null) errors.Add(new ServiceError("required", "Experience is required.", "experience"));
        if (content.Languages is null) errors.Add(new ServiceError("required", "Languages are required.", "languages"));
        if (content.Metrics is null) errors.Add(new ServiceError("required", "Metrics are required.", "metrics"));
        if (content.Navigation is null) errors.Add(new ServiceError("required", "Navigation is required.", "navigation"));
        if (content.ProfessionalDetails is null) errors.Add(new ServiceError("required", "Professional details are required.", "professionalDetails"));
        if (content.Projects is null) errors.Add(new ServiceError("required", "Projects are required.", "projects"));
        if (content.RoleFit is null) errors.Add(new ServiceError("required", "Role fit is required.", "roleFit"));
        if (content.RoleSignals is null) errors.Add(new ServiceError("required", "Role signals are required.", "roleSignals"));
        if (content.SkillGroups is null) errors.Add(new ServiceError("required", "Skills are required.", "skillGroups"));
        if (content.StackMatrix is null) errors.Add(new ServiceError("required", "Technology stack is required.", "stackMatrix"));
        if (content.Strengths is null) errors.Add(new ServiceError("required", "Strengths are required.", "strengths"));
        if (content.SummaryHighlights is null) errors.Add(new ServiceError("required", "Summary highlights are required.", "summaryHighlights"));
        if (content.Tools is null) errors.Add(new ServiceError("required", "Tools are required.", "tools"));
        if (content.UiText is null) errors.Add(new ServiceError("required", "Page wording is required.", "uiText"));
        if (content.Workflow is null) errors.Add(new ServiceError("required", "Workflow is required.", "workflow"));
    }

    private static List<string> CleanList(IEnumerable<string>? values) =>
        values?.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToList() ?? [];

    /// <summary>
    /// Enforces the rules a section needs to stay addressable: every entry carries a title or
    /// label, that text produces a usable slug, and no two entries share one. Granular routes
    /// resolve entries by this slug, so duplicates would make an update or delete ambiguous.
    /// </summary>
    private static void ValidateIdentified<T>(
        CvSection<T> section,
        IReadOnlyList<T> items,
        Func<T, string?> titleOf,
        List<ServiceError> errors)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < items.Count; index++)
        {
            var field = $"{section.Name}[{index}].{section.IdentityField}";
            var title = titleOf(items[index]);
            Require(title, field, $"{section.DisplayName} {section.IdentityField} is required.", errors);
            if (string.IsNullOrWhiteSpace(title)) continue;

            var key = section.Identity(items[index]);
            if (key.Length == 0)
            {
                errors.Add(new ServiceError(
                    "invalid_slug",
                    $"{section.DisplayName} {section.IdentityField} must contain letters or numbers.",
                    field));
            }
            else if (!keys.Add(key))
            {
                errors.Add(new ServiceError(
                    "duplicate",
                    $"{section.DisplayName} {section.IdentityField}s must be unique.",
                    field));
            }
        }
    }

    private static void Require(string? value, string field, string message, List<ServiceError> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new ServiceError("required", message, field));
        }
    }

    private static bool IsOptionalHttpUrl(string? value) =>
        string.IsNullOrWhiteSpace(value)
        || (Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https");

    private static bool IsNavigationTarget(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && (value.StartsWith('#') || value.StartsWith('/'));

    private static bool IsContactTarget(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value.StartsWith('#')) return true;
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && uri.Scheme is "http" or "https" or "mailto" or "tel";
    }

    private static void TrimStrings(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject.ToList())
            {
                if (property.Value is JsonValue value && value.TryGetValue<string>(out var text))
                {
                    jsonObject[property.Key] = text.Trim();
                }
                else if (property.Value is not null)
                {
                    TrimStrings(property.Value);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray.Where(item => item is not null))
            {
                TrimStrings(item!);
            }
        }
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugCharacters();

    [GeneratedRegex("-+")]
    private static partial Regex RepeatedHyphens();
}
