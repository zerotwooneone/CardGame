using System;
using CardGame.Application.Client;
using CardGame.Application.CommonState;
using CardGame.Application.DTO;
using CardGame.CommonModel.CommonState;
using CardGame.Domain.Abstractions.Game;
using CardGame.Utils.Abstractions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CardGame.Application
{
    public class Startup : IStartup
    {
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSignalR();

            serviceCollection.AddTransient<CommonStateHub>();
            serviceCollection.AddTransient<Func<CommonStateHub>>(sp => sp.GetRequiredService<CommonStateHub>);
            serviceCollection.AddTransient<ICommonStateModelFactory, CommonStateModelFactory>();

            serviceCollection.AddTransient<ClientHub>();

            serviceCollection.AddTransient<IGameConverter, GameConverter>();
            serviceCollection.AddTransient<IGameRepository, FakeGameRepo>();
        }

        public void Configure(IApplicationBuilder app)
        {
            // need to configure routing here
        }
    }
}