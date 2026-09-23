using OnlineCV.Api.Models;

namespace OnlineCV.Api.Repositories;

public interface IContactRepository
{
    IReadOnlyList<ContactSubmission> GetAll();
    ContactSubmission? GetById(Guid id);
    Task AddAsync(ContactSubmission submission, CancellationToken cancellationToken);
    Task<bool> UpdateStatusAsync(Guid id, ContactSubmissionStatus status, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
