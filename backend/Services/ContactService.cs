using System.Net.Mail;
using OnlineCV.Api.Models;
using OnlineCV.Api.Repositories;

namespace OnlineCV.Api.Services;

public sealed class ContactService(IContactRepository repository)
{
    public IReadOnlyList<ContactSubmission> GetAll(ContactSubmissionStatus? status = null)
    {
        var submissions = repository.GetAll();
        return status is null
            ? submissions
            : submissions.Where(item => item.Status == status).ToArray();
    }

    public ContactSubmission? GetById(Guid id) => repository.GetById(id);

    public async Task<ServiceResult<ContactSubmission>> SubmitAsync(
        ContactRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim() ?? "";
        var email = request.Email?.Trim().ToLowerInvariant() ?? "";
        var message = request.Message?.Trim() ?? "";
        var errors = new List<ServiceError>();

        ValidateLength(name, 2, 100, "name", "Name", errors);
        ValidateLength(email, 5, 254, "email", "Email", errors);
        ValidateLength(message, 10, 3000, "message", "Message", errors);
        if (!string.IsNullOrWhiteSpace(email) && !MailAddress.TryCreate(email, out _))
        {
            errors.Add(new ServiceError("invalid_email", "Enter a valid email address.", "email"));
        }

        if (errors.Count > 0)
        {
            return ServiceResult<ContactSubmission>.Validation(errors);
        }

        var submission = new ContactSubmission
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Message = message,
            Status = ContactSubmissionStatus.New,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await repository.AddAsync(submission, cancellationToken);
        return ServiceResult<ContactSubmission>.Success(submission);
    }

    public async Task<ServiceResult<ContactSubmission>> UpdateStatusAsync(
        Guid id,
        ContactSubmissionStatus status,
        CancellationToken cancellationToken)
    {
        var updated = await repository.UpdateStatusAsync(id, status, cancellationToken);
        return updated
            ? ServiceResult<ContactSubmission>.Success(repository.GetById(id)!)
            : ServiceResult<ContactSubmission>.NotFound($"Contact submission '{id}' was not found.");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);
        return deleted
            ? ServiceResult<bool>.Success(true)
            : ServiceResult<bool>.NotFound($"Contact submission '{id}' was not found.");
    }

    private static void ValidateLength(
        string value,
        int minimum,
        int maximum,
        string field,
        string label,
        List<ServiceError> errors)
    {
        if (value.Length < minimum)
        {
            errors.Add(new ServiceError("too_short", $"{label} must contain at least {minimum} characters.", field));
        }
        else if (value.Length > maximum)
        {
            errors.Add(new ServiceError("too_long", $"{label} cannot exceed {maximum} characters.", field));
        }
    }
}
