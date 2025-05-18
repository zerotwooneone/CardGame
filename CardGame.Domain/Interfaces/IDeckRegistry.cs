using CardGame.Domain.Interfaces; // IDeckProvider is already in this namespace
using System;

namespace CardGame.Domain.Interfaces; // Updated namespace

/// <summary>
/// Defines a contract for a registry that holds and provides access to deck providers.
/// </summary>
public interface IDeckRegistry
{
    /// <summary>
    /// Gets the deck provider associated with the specified deck ID.
    /// </summary>
    /// <param name="deckId">The unique identifier of the deck.</param>
    /// <returns>The <see cref="IDeckProvider"/> if found; otherwise, <c>null</c> or throw depending on implementation choice.</returns>
    IDeckProvider? GetProvider(Guid deckId);
}
