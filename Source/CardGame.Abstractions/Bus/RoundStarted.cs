using System;

namespace CardGame.CommonModel.Bus
{
    public class RoundStarted
    {
        public Guid GameId { get; set; }
        public int RoundId { get; set; }

        public RoundStarted(Guid gameId, int roundId)
        {
            GameId = gameId;
            RoundId = roundId;
        }
    }
}