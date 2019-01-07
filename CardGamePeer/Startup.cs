using System;
using Autofac;
using CardGame.Core.Card;

namespace CardGamePeer
{
    public class Startup
    {
        public void Setup(ContainerBuilder builder)
        {
            var assembly = typeof(ProgramViewmodel).Assembly;
            builder.RegisterAssemblyTypes(assembly)
                .PublicOnly()
                .AsSelf()
                .AsImplementedInterfaces();

            var coreAssembly = typeof(Card).Assembly;
            builder.RegisterAssemblyTypes(coreAssembly)
                .PublicOnly()
                .AsSelf()
                .AsImplementedInterfaces();

            builder
                .Register(c =>
                {
                    return new Random(222);
                })
                .SingleInstance();
        }
    }
}