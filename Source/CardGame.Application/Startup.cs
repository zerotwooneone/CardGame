using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Extensions.Logging;

namespace CardGame.Application
{
    public class Startup : IStartup
    {
        private IRequestRegistryBuilder _requestRegistry;
        private Subject<ICommonEvent> _commonEventSubject;
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
            
            //bus config
            _commonEventSubject = new Subject<ICommonEvent>();
            serviceCollection.AddSingleton<IBus, NaiveBus>(sp => 
                new NaiveBus(_commonEventSubject, sp.GetService<IEventConverter>(), sp.GetService<IResponseRegistry>()));
            serviceCollection.AddTransient<IEventConverter, EventConverter>();
            var requestRegistry = new RequestRegistry();
            _requestRegistry = requestRegistry;
            serviceCollection.AddSingleton<IResponseRegistry>(requestRegistry);

            serviceCollection.AddTransient<IServiceCallRouter, ServiceCallRouter>();

            serviceCollection.AddSingleton<IServiceProvider>(serviceCollection.BuildServiceProvider());
        }

        public void Configure(IApplicationBuilder app)
        {
            // need to configure routing here
            var logger = app.ApplicationServices.GetService<ILogger<Subject<ICommonEvent>>>();
            _commonEventSubject.Subscribe(e =>
            {
                logger.LogTrace($"Common Event topic:{e.Topic} eventId:{e.EventId} correlationId:{e.CorrelationId} values:{string.Join($",{Environment.NewLine}",e.Values.Select(kvp=>$"{kvp.Key}:{kvp.Value}"))}");
            });

            //todo: move this to a config file
            var requestRegistry = new Dictionary<string, RequestConfiguration>
            {
                {"CardGame.Domain.Abstractions.Game.IGameService:NextRound", new RequestConfiguration
                    (
                        "CardGame.Domain.Abstractions.Game.IGameService", 
                        "NextRound", 
                        "RoundStarted"
                    )
                },
                {"CardGame.Domain.Abstractions.Game.IPlayService:Play", new RequestConfiguration
                    (
                        "CardGame.Domain.Abstractions.Game.IPlayService", 
                        "Play", 
                        "CardPlayed"
                    )
                }
            };

            object Resolve(Type type)
            {
                return app.ApplicationServices.GetService(type);
            }
            _requestRegistry.Configure(requestRegistry, Resolve);

            var serviceCallRouter = app.ApplicationServices.GetService<IServiceCallRouter>();
            serviceCallRouter.Configure();
        }
    }
}