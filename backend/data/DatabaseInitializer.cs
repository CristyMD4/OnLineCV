using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OnlineCV.Api.Models;

namespace OnlineCV.Api.Data;

public sealed class DatabaseInitializer(
    IDbContextFactory<OnlineCvDbContext> dbContextFactory,
    IConfiguration configuration,
    IWebHostEnvironment environment,
    ILogger<DatabaseInitializer> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (!await dbContext.CvDocuments.AnyAsync(cancellationToken))
        {
            var cvPath = ResolveSeedPath("SeedData:CvPath", "data/cv-content.json");
            var contentJson = await File.ReadAllTextAsync(cvPath, cancellationToken);
            _ = JsonSerializer.Deserialize<CvContent>(contentJson, JsonOptions)
                ?? throw new InvalidOperationException($"CV seed data in '{cvPath}' is invalid.");

            dbContext.CvDocuments.Add(new CvDocumentRecord
            {
                ContentJson = contentJson,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        if (!await dbContext.ContactSubmissions.AnyAsync(cancellationToken))
        {
            var contactPath = ResolveSeedPath("SeedData:ContactPath", "data/contact-submissions.json");
            if (File.Exists(contactPath))
            {
                var contactJson = await File.ReadAllTextAsync(contactPath, cancellationToken);
                var submissions = JsonSerializer.Deserialize<List<ContactSubmission>>(contactJson, JsonOptions) ?? [];
                dbContext.ContactSubmissions.AddRange(submissions);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("SQL Server database is ready and CV data is available.");
    }

    private string ResolveSeedPath(string configurationKey, string fallback)
    {
        var configuredPath = configuration[configurationKey] ?? fallback;
        return Path.GetFullPath(configuredPath, environment.ContentRootPath);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
