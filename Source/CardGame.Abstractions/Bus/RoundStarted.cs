using System;

namespace CardGame.CommonModel.Bus
{
    public class RoundStarted : IEvent
    {
        public Guid GameId { get; set; }
        public int RoundId { get; set; }
        public Guid CorrelationId { get; set; }

        public RoundStarted(Guid gameId, int roundId, Guid correlationId)
        {
            GameId = gameId;
            RoundId = roundId;
            CorrelationId = correlationId;
        }
    }
}