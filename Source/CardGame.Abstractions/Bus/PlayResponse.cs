using System;

namespace CardGame.CommonModel.Bus
{
    public class PlayResponse
    {
        public Guid CorrelationId { get; set; }
        public Guid EventId { get; set; }
    }
}