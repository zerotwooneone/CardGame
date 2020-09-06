using System;
using CardGame.Application.Client;
using CardGame.Application.CommonState;
using CardGame.CommonModel.CommonState;
using CardGame.Utils.Abstractions.DependencyInjection;
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
        }
    }
}