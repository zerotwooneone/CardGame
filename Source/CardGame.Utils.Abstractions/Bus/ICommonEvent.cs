using System;
using System.Collections.Generic;

namespace CardGame.Utils.Abstractions.Bus
{
    public interface ICommonEvent
    {
        Guid EventId { get; }
        Guid? CorrelationId { get; }
        string Topic { get; }
        IReadOnlyDictionary<string, string> Values { get; }
    }
}
