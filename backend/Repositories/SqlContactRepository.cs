using Microsoft.EntityFrameworkCore;
using OnlineCV.Api.Data;
using OnlineCV.Api.Models;

namespace OnlineCV.Api.Repositories;

public sealed class SqlContactRepository(IDbContextFactory<OnlineCvDbContext> dbContextFactory)
    : IContactRepository
{
    public IReadOnlyList<ContactSubmission> GetAll()
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        return dbContext.ContactSubmissions
            .AsNoTracking()
            .OrderByDescending(item => item.CreatedAt)
            .ToArray();
    }

    public ContactSubmission? GetById(Guid id)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        return dbContext.ContactSubmissions.AsNoTracking().SingleOrDefault(item => item.Id == id);
    }

    public async Task AddAsync(ContactSubmission submission, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.ContactSubmissions.Add(submission);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UpdateStatusAsync(
        Guid id,
        ContactSubmissionStatus status,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var submission = await dbContext.ContactSubmissions.FindAsync([id], cancellationToken);
        if (submission is null)
        {
            return false;
        }

        submission.Status = status;
        submission.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var deleted = await dbContext.ContactSubmissions
            .Where(item => item.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
        return deleted > 0;
    }
}
