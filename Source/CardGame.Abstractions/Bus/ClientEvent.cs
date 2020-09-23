using System;

namespace CardGame.CommonModel.Bus
{
    public class ClientEvent : IEvent
    {
        public object Data { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid GameId { get; set; }
        public Guid EventId { get; set; }
        public string Type { get; set; }
        public string Topic { get; set; }
    }
}