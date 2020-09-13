using System;

namespace CardGame.CommonModel.Bus
{
    public class ServiceCall : IRequest
    {
        public object Param { get; set; }
        public Guid EventId { get; set; }
        public Guid CorrelationId { get; set; }
        public string RequestTopic { get; set; }
    }
}