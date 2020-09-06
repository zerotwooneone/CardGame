using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CardGame.Utils.Abstractions.DependencyInjection
{
    public interface IStartup
    {
        void ConfigureServices(IServiceCollection serviceCollection);
        void Configure(IApplicationBuilder app);
    }
}
