using System;
using System.Collections.Generic;
using CardGame.Utils.Abstractions.Bus;

namespace CardGame.Utils.Bus
{
    public class CommonEvent : ICommonEvent
    {
        public Guid EventId { get; set; }
        public Guid? CorrelationId { get; set; }
        public string Topic { get; set; }
        public IReadOnlyDictionary<string, string> Values { get; set; }
    }
}