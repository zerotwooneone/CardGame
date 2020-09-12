using System;
using System.Collections.Generic;

namespace CardGame.Utils.Abstractions.Bus
{
    public interface IRequestRegistryBuilder
    {
        void Configure(IReadOnlyDictionary<string, RequestConfiguration> registry, Func<Type, object> resolve);
    }
}