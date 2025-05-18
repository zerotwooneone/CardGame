using CardGame.Domain.Interfaces; // For IDeckProvider

namespace CardGame.Domain.Interfaces;

/// <summary>
/// Abstracts the collection and registration of IDeckProvider implementations.
/// This allows the domain to define how providers are registered without depending
/// directly on IServiceCollection or other DI framework types.
/// </summary>
public interface IDeckProviderCollection
{
    /// <summary>
    /// Registers an implementation of IDeckProvider.
    /// </summary>
    /// <typeparam name="TProvider">The type of the deck provider, must implement IDeckProvider.</typeparam>
    void AddProvider<TProvider>() where TProvider : class, IDeckProvider;
    // We use 'class' constraint so it can be resolved as a concrete type by DI
}
