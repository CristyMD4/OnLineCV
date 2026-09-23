using OnlineCV.Api.Models;

namespace OnlineCV.Api.Services;

/// <summary>
/// Describes how to reach a list section of the CV document and how to identify its items.
/// One descriptor replaces a hand-written CRUD trio per section.
/// </summary>
/// <param name="Name">Section name used in messages, for example <c>experience</c>.</param>
/// <param name="Items">Selects the backing list from a CV document.</param>
/// <param name="Identity">Derives the slug that addresses an item in routes.</param>
/// <param name="IdentityField">Field reported on conflict errors, for example <c>title</c>.</param>
/// <param name="DisplayName">Human-readable item name used in not-found and conflict messages.</param>
/// <param name="AssignIdentity">
/// Writes the derived slug back onto the item. Only sections that persist their own id, such as
/// projects, need this; every other section derives identity from a title or label.
/// </param>
/// <param name="DeriveIdentity">
/// Produces a slug for an item that carries no identity of its own, for example a new project
/// posted without an id. On updates the route id is used instead.
/// </param>
public sealed record CvSection<T>(
    string Name,
    Func<CvContent, List<T>> Items,
    Func<T, string> Identity,
    string IdentityField,
    string DisplayName,
    Action<T, string>? AssignIdentity = null,
    Func<T, string>? DeriveIdentity = null);
