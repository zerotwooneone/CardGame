using CardGame.Utils.Abstractions.DependencyInjection;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace CardGame.Server.DependencyInjection
{
    public class StartupRegistry
    {
        private readonly IList<IStartup> _startups = new List<IStartup>();
        public void Initialize()
        {
            // todo: find startups by reflection?
            _startups.Add(new CardGame.Application.Startup());
        }

        public void RegisterDi(IServiceCollection serviceCollection)
        {
            foreach (var startup in _startups)
            {
                startup.ConfigureServices(serviceCollection);
            }
        }
    }
}
