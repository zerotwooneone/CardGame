using CardGame.Application;
using CardGame.Domain.Turn;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.WriteLine("Hello, World!");

var appBuilder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(configBuilder =>
    {
        configBuilder.AddCommandLine(args);
    })
    .ConfigureServices(serviceCollection =>
    {
        serviceCollection.AddHostedService<ConsoleService>();
        
        serviceCollection.AddSingleton<IApplicationTurnService, ApplicationTurnService>();
        serviceCollection.AddSingleton<TurnService>();
        
        var logFactory = serviceCollection.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
        serviceCollection.AddSingleton<IInspectNotificationService>(new LoggerNotificationService(logFactory.CreateLogger<LoggerNotificationService>()));
        serviceCollection.AddSingleton<IShuffleService, ShuffleService>();
        
        serviceCollection.AddSingleton<DummyRepository>();
        serviceCollection.AddSingleton<ITurnRepository>(s=>s.GetRequiredService<DummyRepository>());
        serviceCollection.AddSingleton<IRoundFactory>(s=>s.GetRequiredService<DummyRepository>());
    });

appBuilder.Build().Run();