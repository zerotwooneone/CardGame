using CardGame.Utils.Abstractions.DependencyInjection;
using System.Collections.Generic;
using CardGame.Application.Client;
using CardGame.Application.CommonState;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CardGame.Server.DependencyInjection
{
    public class StartupRegistry
    {
        private readonly IList<IStartup> _startups = new List<IStartup>();
        public void Initialize()
        {
            // todo: find startups by reflection or config
            _startups.Add(new Application.Startup());
        }

        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            foreach (var startup in _startups)
            {
                startup.ConfigureServices(serviceCollection);
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            //todo: this should be done in the app library
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapHub<CommonStateHub>("/commonState");
                endpoints.MapHub<ClientHub>("/client");
            });

            foreach (var startup in _startups)
            {
                startup.Configure(app);
            }
        }
    }
}
