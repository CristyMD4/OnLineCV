namespace OnlineCV.Api.Models;

public enum ContactSubmissionStatus
{
    New,
    Read,
    Archived
}

public sealed class ContactSubmission
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Message { get; set; } = "";
    public ContactSubmissionStatus Status { get; set; } = ContactSubmissionStatus.New;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class UpdateContactStatusRequest
{
    public ContactSubmissionStatus Status { get; set; }
}
