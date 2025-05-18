using CardGame.Domain.Interfaces; // For IDeckProviderCollection

namespace CardGame.Domain.Interfaces; // Updated namespace

/// <summary>
/// Defines a contract for modules or libraries to register their IDeckProvider implementations.
/// </summary>
public interface IDeckProviderRegistrar
{
    /// <summary>
    /// Called by the application during startup to allow this module/library
    /// to register its IDeckProvider implementations using an abstraction.
    /// </summary>
    /// <param name="providerCollection">The collection to add deck providers to.</param>
    void RegisterDeckProviders(IDeckProviderCollection providerCollection);
}
