using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OnlineCV.Api.Data;
using OnlineCV.Api.Models;

namespace OnlineCV.Api.Repositories;

public sealed class SqlCvRepository : ICvRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly IDbContextFactory<OnlineCvDbContext> _dbContextFactory;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private CvContent _content;

    public SqlCvRepository(IDbContextFactory<OnlineCvDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        using var dbContext = _dbContextFactory.CreateDbContext();
        var document = dbContext.CvDocuments.AsNoTracking().Single(item => item.Id == 1);
        _content = Deserialize(document.ContentJson);
    }

    public CvContent Get() => Clone(Volatile.Read(ref _content));

    public CvContent GetSnapshot() => Volatile.Read(ref _content);

    public async Task ReplaceAsync(CvContent content, CancellationToken cancellationToken)
    {
        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            var json = JsonSerializer.Serialize(content, JsonOptions);
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var document = await dbContext.CvDocuments.SingleAsync(
                item => item.Id == 1,
                cancellationToken);
            document.ContentJson = json;
            document.UpdatedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            Volatile.Write(ref _content, Clone(content));
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private static CvContent Clone(CvContent content) =>
        Deserialize(JsonSerializer.Serialize(content, JsonOptions));

    private static CvContent Deserialize(string json) =>
        JsonSerializer.Deserialize<CvContent>(json, JsonOptions)
        ?? throw new InvalidOperationException("CV content stored in SQL Server is invalid.");
}
