using System.Text.Json;
using OnlineCV.Api.Models;
using OnlineCV.Api.Repositories;

namespace OnlineCV.Api.Tests.TestDoubles;

/// <summary>
/// Mirrors the production repository's copy-on-write contract without connecting to SQL Server:
/// <see cref="Get"/> hands out an isolated copy, <see cref="GetSnapshot"/> shares the live one, and
/// <see cref="ReplaceAsync"/> publishes a fresh document rather than editing in place.
/// </summary>
internal sealed class InMemoryCvRepository(CvContent content) : ICvRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private CvContent _content = Clone(content);

    public int WriteCount { get; private set; }

    public CvContent Get() => Clone(_content);

    public CvContent GetSnapshot() => _content;

    public Task ReplaceAsync(CvContent content, CancellationToken cancellationToken)
    {
        _content = Clone(content);
        WriteCount++;
        return Task.CompletedTask;
    }

    private static CvContent Clone(CvContent content) =>
        JsonSerializer.Deserialize<CvContent>(JsonSerializer.Serialize(content, JsonOptions), JsonOptions)!;
}
