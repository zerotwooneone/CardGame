using System;

namespace CardGame.CommonModel.Bus
{
    [Obsolete("it might not make sense for services to deal with a generic error event")]
    public class Rejected : IEvent
    {
        public Guid CorrelationId { get; set; }
        public Guid OriginalEventId { get; set; }
        public string Reason { get; set; }
    }
}