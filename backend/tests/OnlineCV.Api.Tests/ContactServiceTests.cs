using OnlineCV.Api.Models;
using OnlineCV.Api.Services;
using OnlineCV.Api.Tests.TestDoubles;

namespace OnlineCV.Api.Tests;

public sealed class ContactServiceTests
{
    private readonly InMemoryContactRepository _repository = new();
    private readonly ContactService _service;

    public ContactServiceTests() => _service = new ContactService(_repository);

    private static ContactRequest ValidRequest() => new()
    {
        Name = "Grace Hopper",
        Email = "grace@example.com",
        Message = "I would like to discuss a backend role."
    };

    [Fact]
    public async Task AValidMessageIsStoredAsNew()
    {
        var result = await _service.SubmitAsync(ValidRequest(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ContactSubmissionStatus.New, result.Value!.Status);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Single(_service.GetAll());
    }

    [Fact]
    public async Task SubmissionTrimsInputAndLowercasesTheEmail()
    {
        var request = ValidRequest();
        request.Name = "  Grace Hopper  ";
        request.Email = "  Grace@Example.COM ";

        var result = await _service.SubmitAsync(request, CancellationToken.None);

        Assert.Equal("Grace Hopper", result.Value!.Name);
        Assert.Equal("grace@example.com", result.Value.Email);
    }

    [Theory]
    [InlineData("A", "name")]
    [InlineData("", "name")]
    public async Task ANameShorterThanTwoCharactersIsRejected(string name, string field)
    {
        var request = ValidRequest();
        request.Name = name;

        await AssertRejected(request, field);
    }

    [Fact]
    public async Task ANameLongerThanOneHundredCharactersIsRejected()
    {
        var request = ValidRequest();
        request.Name = new string('a', 101);

        await AssertRejected(request, "name");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    public async Task AnInvalidEmailIsRejected(string email)
    {
        var request = ValidRequest();
        request.Email = email;

        await AssertRejected(request, "email");
    }

    [Fact]
    public async Task AMessageShorterThanTenCharactersIsRejected()
    {
        var request = ValidRequest();
        request.Message = "too short";

        await AssertRejected(request, "message");
    }

    [Fact]
    public async Task AMessageLongerThanThreeThousandCharactersIsRejected()
    {
        var request = ValidRequest();
        request.Message = new string('a', 3001);

        await AssertRejected(request, "message");
    }

    [Fact]
    public async Task ARejectedMessageIsNotStored()
    {
        await _service.SubmitAsync(new ContactRequest(), CancellationToken.None);

        Assert.Empty(_service.GetAll());
    }

    [Fact]
    public async Task StatusCanMoveFromNewToReadToArchived()
    {
        var submitted = await _service.SubmitAsync(ValidRequest(), CancellationToken.None);
        var id = submitted.Value!.Id;

        var read = await _service.UpdateStatusAsync(id, ContactSubmissionStatus.Read, CancellationToken.None);
        Assert.Equal(ContactSubmissionStatus.Read, read.Value!.Status);
        Assert.NotNull(read.Value.UpdatedAt);

        var archived = await _service.UpdateStatusAsync(id, ContactSubmissionStatus.Archived, CancellationToken.None);
        Assert.Equal(ContactSubmissionStatus.Archived, archived.Value!.Status);
    }

    [Fact]
    public async Task SubmissionsCanBeFilteredByStatus()
    {
        var first = await _service.SubmitAsync(ValidRequest(), CancellationToken.None);
        await _service.SubmitAsync(ValidRequest(), CancellationToken.None);
        await _service.UpdateStatusAsync(first.Value!.Id, ContactSubmissionStatus.Archived, CancellationToken.None);

        Assert.Single(_service.GetAll(ContactSubmissionStatus.New));
        Assert.Single(_service.GetAll(ContactSubmissionStatus.Archived));
        Assert.Equal(2, _service.GetAll().Count);
    }

    [Fact]
    public async Task DeletingRemovesTheSubmission()
    {
        var submitted = await _service.SubmitAsync(ValidRequest(), CancellationToken.None);

        Assert.True((await _service.DeleteAsync(submitted.Value!.Id, CancellationToken.None)).IsSuccess);
        Assert.Empty(_service.GetAll());
    }

    [Fact]
    public async Task AnUnknownSubmissionIsReportedAsNotFound()
    {
        Assert.Equal(
            ServiceResultStatus.NotFound,
            (await _service.DeleteAsync(Guid.NewGuid(), CancellationToken.None)).Status);
        Assert.Equal(
            ServiceResultStatus.NotFound,
            (await _service.UpdateStatusAsync(Guid.NewGuid(), ContactSubmissionStatus.Read, CancellationToken.None)).Status);
    }

    private async Task AssertRejected(ContactRequest request, string field)
    {
        var result = await _service.SubmitAsync(request, CancellationToken.None);

        Assert.Equal(ServiceResultStatus.ValidationFailed, result.Status);
        Assert.Contains(result.Errors, error => error.Field == field);
    }
}
