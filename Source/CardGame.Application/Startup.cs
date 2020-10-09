using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using CardGame.Application.Bus;
using CardGame.Application.Client;
using CardGame.Application.DTO;
using CardGame.Domain.Abstractions.Game;
using CardGame.Domain.Game;
using CardGame.Utils.Abstractions.Bus;
using CardGame.Utils.Bus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IStartup = CardGame.Utils.Abstractions.DependencyInjection.IStartup;

namespace CardGame.Application
{
    public class Startup : IStartup
    {
        private IRequestRegistryBuilder _requestRegistry;
        private Subject<ICommonEvent> _commonEventSubject;
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSignalR();
            
            serviceCollection.AddTransient<ClientHub>();

            serviceCollection.AddTransient<IGameConverter, GameConverter>();
            serviceCollection.AddSingleton<IGameDal, FakeGameDal>();
            serviceCollection.AddTransient<IGameService, GameService>();
            serviceCollection.AddSingleton<IGameRepository, FakeGameRespository>();
            
            //bus config
            _commonEventSubject = new Subject<ICommonEvent>();
            serviceCollection.AddSingleton<IBus, NaiveBus>();
            serviceCollection.AddTransient<IEventConverter, EventConverter>();
            var requestRegistry = new RequestRegistry();
            _requestRegistry = requestRegistry;
            serviceCollection.AddSingleton<IResponseRegistry>(requestRegistry);

            serviceCollection.AddTransient<IServiceCallRouter, ServiceCallRouter>();

            serviceCollection.AddSingleton<IServiceProvider>(serviceCollection.BuildServiceProvider());

            serviceCollection.AddSingleton<ClientRouter>();

            // Add service and create Policy with options
            serviceCollection.AddCors(options =>
            {
                options.AddPolicy("DevPolicy",
                    builder => builder.WithOrigins("http://localhost:4200/", "http://localhost:4200")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetService<IHostingEnvironment>();
            if (env.IsDevelopment())
            {
                app.UseCors("DevPolicy");
            }

            // need to configure routing here
            var logger = app.ApplicationServices.GetService<ILogger<Subject<ICommonEvent>>>();
            _commonEventSubject.Subscribe(e =>
            {
                logger.LogTrace($"Common Event topic:{e.Topic} eventId:{e.EventId} correlationId:{e.CorrelationId} values:{string.Join($",{Environment.NewLine}",e.Values.Select(kvp=>$"{kvp.Key}:{kvp.Value}"))}");
            });

            //todo: move this to a config file
            var requestRegistry = new Dictionary<string, RequestConfiguration>
            {
                {"CardGame.Domain.Abstractions.Game.IGameService:Play", new RequestConfiguration
                    (
                        "CardGame.Domain.Abstractions.Game.IGameService", 
                        "Play", 
                        "CardPlayed"
                    )
                },
            };

            object Resolve(Type type)
            {
                return app.ApplicationServices.GetService(type);
            }
            _requestRegistry.Configure(requestRegistry, Resolve);

            //todo find a way to avoid calling these setup methods in configure

            var serviceCallRouter = app.ApplicationServices.GetService<IServiceCallRouter>();
            serviceCallRouter.Configure();

            var clientRouter = app.ApplicationServices.GetService<ClientRouter>();
            clientRouter.Init();

        }
    }
}