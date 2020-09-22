using System;
using System.Collections.Generic;

namespace CardGame.CommonModel.Bus
{
    public class ClientEvent : IEvent
    {
        public IDictionary<string, object> Data { get; set; }
        public Guid CorrelationId { get; set; }
    }
}