using OnlineCV.Api.Models;

namespace OnlineCV.Api.Repositories;

public interface ICvRepository
{
    /// <summary>
    /// Returns an isolated copy that callers may mutate freely. Use this on write paths.
    /// </summary>
    CvContent Get();

    /// <summary>
    /// Returns the current document without copying it. Reads are safe because writes publish a
    /// fresh document rather than editing this one, so a caller keeps a consistent view even while
    /// a write completes. Callers must treat the result as read-only.
    /// </summary>
    CvContent GetSnapshot();

    Task ReplaceAsync(CvContent content, CancellationToken cancellationToken);
}
