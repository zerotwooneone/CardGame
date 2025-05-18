using CardGame.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CardGame.Infrastructure.Services;

/// <summary>
/// Adapts the IServiceCollection to the IDeckProviderCollection interface,
/// allowing domain-level registrars to add deck providers without directly
/// depending on Microsoft.Extensions.DependencyInjection.
/// </summary>
public class ServiceCollectionDeckProviderAdapter : IDeckProviderCollection
{
    private readonly IServiceCollection _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceCollectionDeckProviderAdapter"/> class.
    /// </summary>
    /// <param name="services">The service collection to adapt.</param>
    public ServiceCollectionDeckProviderAdapter(IServiceCollection services)
    {
        _services = services;
    }

    /// <inheritdoc />
    public void AddProvider<TProvider>() where TProvider : class, IDeckProvider
    {
        _services.AddTransient<IDeckProvider, TProvider>();
    }
}
