using CardGame.Domain.Interfaces;

namespace CardGame.Infrastructure.Services;

/// <summary>
/// Manages a collection of IDeckProvider implementations, allowing lookup by DeckId.
/// </summary>
public class DeckRegistry : IDeckRegistry
{
    private readonly IReadOnlyDictionary<Guid, IDeckProvider> _providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeckRegistry"/> class.
    /// </summary>
    /// <param name="providersEnumerable">An enumerable collection of IDeckProvider instances to register.</param>
    /// <exception cref="ArgumentNullException">Thrown if providersEnumerable is null.</exception>
    /// <exception cref="ArgumentException">Thrown if a duplicate DeckId is found among providers.</exception>
    public DeckRegistry(IEnumerable<IDeckProvider> providersEnumerable)
    {
        if (providersEnumerable == null)
        {
            throw new ArgumentNullException(nameof(providersEnumerable));
        }

        // Ensure no duplicate DeckIds are present, which would indicate a configuration error.
        var providersList = providersEnumerable.ToList();
        var duplicateKeys = providersList.GroupBy(p => p.DeckId)
                                       .Where(g => g.Count() > 1)
                                       .Select(g => g.Key)
                                       .ToList();

        if (duplicateKeys.Any())
        {
            throw new ArgumentException($"Duplicate DeckId(s) found in IDeckProvider registrations: {string.Join(", ", duplicateKeys)}", nameof(providersEnumerable));
        }

        _providers = providersList.ToDictionary(p => p.DeckId);
    }

    /// <inheritdoc />
    public IDeckProvider? GetProvider(Guid deckId)
    {
        _providers.TryGetValue(deckId, out var provider);
        return provider;
    }
}
