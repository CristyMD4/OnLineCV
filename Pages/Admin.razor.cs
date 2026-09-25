using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace OnLineCV.Pages;

public partial class Admin
{
    #region Configuration

    private const string TokenKey = "onlinecv-admin-token";
    private static readonly JsonSerializerOptions PrettyJson = new() { WriteIndented = true };
    private static readonly Dictionary<string, string> SectionMeta = new(StringComparer.Ordinal)
    {
        ["identity"] = "Personal profile",
        ["projects"] = "Projects",
        ["experience"] = "Experience",
        ["skillGroups"] = "Skills",
        ["credentials"] = "Education & credentials",
        ["achievements"] = "Achievements",
        ["metrics"] = "Career metrics",
        ["strengths"] = "Strengths",
        ["engineeringStandards"] = "Engineering standards",
        ["workflow"] = "Workflow",
        ["collaboration"] = "Collaboration",
        ["professionalDetails"] = "Professional details",
        ["languages"] = "Languages",
        ["tools"] = "Tools",
        ["stackMatrix"] = "Technology stack",
        ["roleFit"] = "Role fit",
        ["roleSignals"] = "Role signals",
        ["summaryHighlights"] = "Summary highlights",
        ["navigation"] = "Navigation",
        ["uiText"] = "Page wording",
    };

    #endregion

    #region Injected services and state

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private JsonObject? _content;
    private JsonObject? _savedContent;
    private string _token = "";
    private string _username = "admin";
    private string _password = "";
    private string _selectedSection = "identity";
    private string _sectionFilter = "";
    private string _jsonDraft = "";
    private string? _jsonError;
    private string _status = "";
    private string _statusKind = "neutral";
    private bool _jsonMode;
    private bool _busy;

    #endregion

    #region Derived state

    private IEnumerable<string> FilteredSections => SectionMeta.Keys
        .Where(key => SectionMeta[key].Contains(_sectionFilter, StringComparison.OrdinalIgnoreCase));

    private bool HasChanges => _savedContent is not null && _content?.ToJsonString() != _savedContent.ToJsonString();
    private int ChangeCount => SectionMeta.Keys.Count(IsChanged);

    #endregion

    #region Lifecycle and authentication

    protected override async Task OnInitializedAsync()
    {
        _token = await JS.InvokeAsync<string?>("cvUi.sessionGet", TokenKey) ?? "";
        if (!string.IsNullOrWhiteSpace(_token))
        {
            await LoadAsync();
        }
    }

    private async Task LoginAsync()
    {
        _busy = true;
        SetStatus("Signing in...", "neutral");

        try
        {
            using var response = await Http.PostAsJsonAsync("/api/auth/login", new { username = _username, password = _password });
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Invalid username or password");
            }

            var result = await response.Content.ReadFromJsonAsync<JsonObject>()
                ?? throw new InvalidOperationException("Login response was empty");
            _token = result["token"]?.GetValue<string>()
                ?? throw new InvalidOperationException("Login token was missing");

            await JS.InvokeVoidAsync("cvUi.sessionSet", TokenKey, _token);
            _password = "";
            SetStatus("Signed in", "success");
            await LoadAsync();
        }
        catch (Exception exception)
        {
            SetStatus(exception.Message, "error");
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task SignOutAsync()
    {
        await JS.InvokeVoidAsync("cvUi.sessionRemove", TokenKey);
        _token = "";
        _content = null;
        _savedContent = null;
    }

    #endregion

    #region API operations

    private async Task LoadAsync()
    {
        _busy = true;

        try
        {
            using var response = await Http.GetAsync("/api/cv");
            response.EnsureSuccessStatusCode();
            _content = await response.Content.ReadFromJsonAsync<JsonObject>() ?? new();
            _savedContent = JsonNode.Parse(_content.ToJsonString())!.AsObject();
            _jsonError = null;
            SetStatus("Content loaded", "success");
            SyncDraft();
        }
        catch (Exception exception)
        {
            SetStatus(exception.Message, "error");
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task SaveAsync()
    {
        if (_content is null || !HasChanges || _jsonError is not null)
        {
            return;
        }

        _busy = true;
        SetStatus("Saving changes...", "neutral");

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, "/api/cv")
            {
                Content = JsonContent.Create(_content),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            using var response = await Http.SendAsync(request);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
            {
                await SignOutAsync();
                throw new InvalidOperationException("Your session expired. Sign in again.");
            }

            response.EnsureSuccessStatusCode();
            _content = await response.Content.ReadFromJsonAsync<JsonObject>() ?? _content;
            _savedContent = JsonNode.Parse(_content.ToJsonString())!.AsObject();
            SetStatus("Changes saved", "success");
            SyncDraft();
        }
        catch (Exception exception)
        {
            SetStatus(exception.Message, "error");
        }
        finally
        {
            _busy = false;
        }
    }

    #endregion

    #region Editor operations

    private void ApplyJson(ChangeEventArgs _)
    {
        if (_content is null)
        {
            return;
        }

        try
        {
            _content[_selectedSection] = JsonNode.Parse(_jsonDraft)
                ?? throw new FormatException("JSON value is empty.");
            _jsonError = null;
            SetStatus("Unsaved changes", "neutral");
        }
        catch (Exception exception)
        {
            _jsonError = exception.Message;
        }
    }

    private void SelectSection(string section)
    {
        _selectedSection = section;
        _jsonMode = false;
        _jsonError = null;
        SyncDraft();
    }

    private void SetJsonMode(bool enabled)
    {
        _jsonMode = enabled;
        _jsonError = null;
        if (enabled)
        {
            SyncDraft();
        }
    }

    private void SyncDraft()
    {
        _jsonDraft = _content?[_selectedSection]?.ToJsonString(PrettyJson) ?? "null";
    }

    private bool IsChanged(string section) =>
        _savedContent is not null && _content?[section]?.ToJsonString() != _savedContent[section]?.ToJsonString();

    private string SectionTitle(string section) => SectionMeta.TryGetValue(section, out var title) ? title : section;

    private string SectionDescription(string section) =>
        $"Edit the {SectionTitle(section).ToLowerInvariant()} content used by the CV.";

    private void SetStatus(string message, string kind)
    {
        _status = message;
        _statusKind = kind;
    }

    #endregion
}
