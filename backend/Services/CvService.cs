using OnlineCV.Api.Models;
using OnlineCV.Api.Repositories;

namespace OnlineCV.Api.Services;

public sealed class CvService(ICvRepository repository, CvContentProcessor processor)
{
    private readonly SemaphoreSlim _businessLock = new(1, 1);

    private CvContent Snapshot => repository.GetSnapshot();

    public CvContent Content => Snapshot;
    public Profile Profile => Snapshot.Identity;
    public IReadOnlyList<Metric> Metrics => Snapshot.Metrics;
    public IReadOnlyList<SkillGroup> Skills => Snapshot.SkillGroups;
    public IReadOnlyList<Project> Projects => Snapshot.Projects;
    public IReadOnlyList<ExperienceItem> Experience => Snapshot.Experience;

    public IReadOnlyList<Credential> Education =>
        Snapshot.Credentials.Where(item => !IsCertificate(item)).ToArray();

    public IReadOnlyList<Credential> Certificates =>
        Snapshot.Credentials.Where(IsCertificate).ToArray();

    public Project? GetProjectById(string id) => GetItem(CvSections.Projects, id);

    public SkillGroup? GetSkillById(string id) => GetItem(CvSections.Skills, id);

    public IReadOnlyList<T> GetSection<T>(CvSection<T> section) => section.Items(Snapshot);

    public T? GetItem<T>(CvSection<T> section, string id)
    {
        var key = CvContentProcessor.Slugify(id);
        return section.Items(Snapshot).FirstOrDefault(item => section.Identity(item) == key);
    }

    public async Task<ServiceResult<CvContent>> UpdateAsync(CvContent content, CancellationToken cancellationToken)
    {
        await _businessLock.WaitAsync(cancellationToken);
        try
        {
            return await ValidateAndSaveAsync(content, cancellationToken);
        }
        finally
        {
            _businessLock.Release();
        }
    }

    public Task<ServiceResult<Profile>> UpdateProfileAsync(Profile profile, CancellationToken cancellationToken) =>
        MutateAsync(content => content.Identity = profile, content => content.Identity, cancellationToken);

    public async Task<ServiceResult<T>> AddItemAsync<T>(
        CvSection<T> section,
        T item,
        CancellationToken cancellationToken)
    {
        await _businessLock.WaitAsync(cancellationToken);
        try
        {
            var content = repository.Get();
            var items = section.Items(content);

            var key = section.Identity(item);
            if (key.Length == 0) key = section.DeriveIdentity?.Invoke(item) ?? "";
            if (Collides(section, items, key, skipIndex: -1))
            {
                return Duplicate<T, T>(section, key);
            }

            section.AssignIdentity?.Invoke(item, key);
            var index = items.Count;
            items.Add(item);

            var saved = await ValidateAndSaveAsync(content, cancellationToken);
            return saved.IsSuccess
                ? ServiceResult<T>.Success(section.Items(saved.Value!)[index])
                : ConvertFailure<T>(saved);
        }
        finally
        {
            _businessLock.Release();
        }
    }

    public async Task<ServiceResult<T>> UpdateItemAsync<T>(
        CvSection<T> section,
        string id,
        T item,
        CancellationToken cancellationToken)
    {
        var routeKey = CvContentProcessor.Slugify(id);

        await _businessLock.WaitAsync(cancellationToken);
        try
        {
            var content = repository.Get();
            var items = section.Items(content);
            var index = items.FindIndex(existing => section.Identity(existing) == routeKey);
            if (index < 0) return NotFound<T, T>(section, id);

            var key = section.Identity(item);
            if (key.Length == 0) key = routeKey;
            if (Collides(section, items, key, index))
            {
                return Duplicate<T, T>(section, key);
            }

            section.AssignIdentity?.Invoke(item, key);
            items[index] = item;

            var saved = await ValidateAndSaveAsync(content, cancellationToken);
            return saved.IsSuccess
                ? ServiceResult<T>.Success(section.Items(saved.Value!)[index])
                : ConvertFailure<T>(saved);
        }
        finally
        {
            _businessLock.Release();
        }
    }

    public async Task<ServiceResult<bool>> DeleteItemAsync<T>(
        CvSection<T> section,
        string id,
        CancellationToken cancellationToken)
    {
        var routeKey = CvContentProcessor.Slugify(id);

        await _businessLock.WaitAsync(cancellationToken);
        try
        {
            var content = repository.Get();
            var removed = section.Items(content).RemoveAll(item => section.Identity(item) == routeKey);
            if (removed == 0) return NotFound<bool, T>(section, id);

            var saved = await ValidateAndSaveAsync(content, cancellationToken);
            return saved.IsSuccess ? ServiceResult<bool>.Success(true) : ConvertFailure<bool>(saved);
        }
        finally
        {
            _businessLock.Release();
        }
    }

    public Task<ServiceResult<IReadOnlyList<T>>> ReplaceSectionAsync<T>(
        CvSection<T> section,
        IEnumerable<T> items,
        CancellationToken cancellationToken) =>
        MutateAsync(
            content =>
            {
                var target = section.Items(content);
                target.Clear();
                target.AddRange(items);

                if (section.AssignIdentity is null) return;

                foreach (var item in target)
                {
                    var key = section.Identity(item);
                    if (key.Length == 0) key = section.DeriveIdentity?.Invoke(item) ?? "";
                    section.AssignIdentity(item, key);
                }
            },
            content => (IReadOnlyList<T>)section.Items(content),
            cancellationToken);

    public Task<ServiceResult<IReadOnlyList<string>>> ReplaceTextListAsync(
        Func<CvContent, List<string>> selector,
        IEnumerable<string> values,
        CancellationToken cancellationToken) =>
        MutateAsync(
            content =>
            {
                var target = selector(content);
                target.Clear();
                target.AddRange(values);
            },
            content => (IReadOnlyList<string>)selector(content),
            cancellationToken);

    public Task<ServiceResult<IReadOnlyDictionary<string, string>>> ReplaceUiTextAsync(
        IDictionary<string, string> values,
        CancellationToken cancellationToken) =>
        MutateAsync(
            content => content.UiText = new Dictionary<string, string>(values),
            content => (IReadOnlyDictionary<string, string>)content.UiText,
            cancellationToken);

    private async Task<ServiceResult<T>> MutateAsync<T>(
        Action<CvContent> mutation,
        Func<CvContent, T> selectResult,
        CancellationToken cancellationToken)
    {
        await _businessLock.WaitAsync(cancellationToken);
        try
        {
            var content = repository.Get();
            mutation(content);
            var saved = await ValidateAndSaveAsync(content, cancellationToken);
            return saved.IsSuccess
                ? ServiceResult<T>.Success(selectResult(saved.Value!))
                : ConvertFailure<T>(saved);
        }
        finally
        {
            _businessLock.Release();
        }
    }

    private async Task<ServiceResult<CvContent>> ValidateAndSaveAsync(CvContent content, CancellationToken cancellationToken)
    {
        var normalized = processor.Normalize(content);
        var errors = processor.Validate(normalized);
        if (errors.Count > 0) return ServiceResult<CvContent>.Validation(errors);

        await repository.ReplaceAsync(normalized, cancellationToken);
        return ServiceResult<CvContent>.Success(repository.Get());
    }

    private static bool IsCertificate(Credential credential) =>
        string.Equals(credential.Status, "Certificate", StringComparison.OrdinalIgnoreCase);

    private static bool Collides<T>(CvSection<T> section, List<T> items, string key, int skipIndex)
    {
        if (key.Length == 0) return false;

        for (var index = 0; index < items.Count; index++)
        {
            if (index != skipIndex && section.Identity(items[index]) == key) return true;
        }

        return false;
    }

    private static ServiceResult<TResult> Duplicate<TResult, TItem>(CvSection<TItem> section, string key) =>
        ServiceResult<TResult>.Conflict(
            $"{section.DisplayName} '{key}' already exists.",
            section.IdentityField);

    private static ServiceResult<TResult> NotFound<TResult, TItem>(CvSection<TItem> section, string id) =>
        ServiceResult<TResult>.NotFound($"{section.DisplayName} '{id}' was not found.");

    private static ServiceResult<T> ConvertFailure<T>(ServiceResult<CvContent> result) =>
        result.Status switch
        {
            ServiceResultStatus.NotFound => ServiceResult<T>.NotFound(result.Errors[0].Message),
            ServiceResultStatus.Conflict => ServiceResult<T>.Conflict(result.Errors[0].Message, result.Errors[0].Field),
            _ => ServiceResult<T>.Validation(result.Errors)
        };
}
