using OnlineCV.Api.Models;
using OnlineCV.Api.Repositories;

namespace OnlineCV.Api.Tests.TestDoubles;

internal sealed class InMemoryContactRepository : IContactRepository
{
    private readonly List<ContactSubmission> _submissions = [];

    public IReadOnlyList<ContactSubmission> GetAll() =>
        _submissions.OrderByDescending(item => item.CreatedAt).ToArray();

    public ContactSubmission? GetById(Guid id) =>
        _submissions.FirstOrDefault(item => item.Id == id);

    public Task AddAsync(ContactSubmission submission, CancellationToken cancellationToken)
    {
        _submissions.Add(submission);
        return Task.CompletedTask;
    }

    public Task<bool> UpdateStatusAsync(Guid id, ContactSubmissionStatus status, CancellationToken cancellationToken)
    {
        var submission = _submissions.FirstOrDefault(item => item.Id == id);
        if (submission is null) return Task.FromResult(false);

        submission.Status = status;
        submission.UpdatedAt = DateTimeOffset.UtcNow;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_submissions.RemoveAll(item => item.Id == id) > 0);
}
