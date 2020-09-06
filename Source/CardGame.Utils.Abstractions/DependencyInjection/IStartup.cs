using Microsoft.Extensions.DependencyInjection;

namespace CardGame.Utils.Abstractions.DependencyInjection
{
    public interface IStartup
    {
        void ConfigureServices(IServiceCollection serviceCollection);
    }
}
