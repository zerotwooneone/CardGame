using System;

namespace CardGame.CommonModel.Bus
{
    public class NextRoundRequest
    {
        public Guid GameId { get; set; }
        public Guid WinningPlayer { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid EventId { get; set; }
    }
}