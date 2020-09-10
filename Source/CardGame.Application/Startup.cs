using System;
using System.Reactive.Subjects;
using CardGame.Application.Bus;
using CardGame.Application.Client;
using CardGame.Application.CommonState;
using CardGame.Application.DTO;
using CardGame.CommonModel.CommonState;
using CardGame.Domain.Abstractions.Game;
using CardGame.Domain.Game;
using CardGame.Utils.Abstractions.Bus;
using CardGame.Utils.Abstractions.DependencyInjection;
using CardGame.Utils.Bus;
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
            serviceCollection.AddTransient<IGameDal, FakeGameDal>();
            serviceCollection.AddTransient<IGameService, GameService>();
            serviceCollection.AddTransient<IPlayService, PlayService>();
            serviceCollection.AddTransient<IGameRepository, FakeGameRespository>();
            
            var commonEventSubject = new Subject<ICommonEvent>();
            serviceCollection.AddTransient<IBus, NaiveBus>(sp => 
                new NaiveBus(commonEventSubject, sp.GetService<IServiceCallRouter>(), sp.GetService<IEventConverter>()));
            serviceCollection.AddTransient<IEventConverter, EventConverter>();

            serviceCollection.AddTransient<IServiceCallRouter, ServiceCallRouter>();

            serviceCollection.AddSingleton<IServiceProvider>(serviceCollection.BuildServiceProvider());
        }

        public void Configure(IApplicationBuilder app)
        {
            // need to configure routing here
        }
    }
}