using CardGame.Application.Common.Interfaces;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Providers;

namespace CardGame.Infrastructure.Services;

/// <summary>
/// Registers the core deck providers (DefaultDeckProvider, CthulhuDeckProvider, etc.)
/// using the domain's IDeckProviderCollection abstraction.
/// </summary>
public class CoreDeckProviderRegistrar : IDeckProviderRegistrar
{
    /// <inheritdoc />
    public void RegisterDeckProviders(IDeckProviderCollection providerCollection)
    {
        // Register core IDeckProvider implementations.
        // These will be discovered by the assembly scanning logic.
        // The adapter will handle the actual IServiceCollection registration.
        providerCollection.AddProvider<DefaultDeckProvider>();
        providerCollection.AddProvider<CthulhuDeckProvider>();
        // Add registrations for other core deck providers here if needed in the future.
    }
}
