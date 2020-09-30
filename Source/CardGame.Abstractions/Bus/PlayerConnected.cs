using System;

namespace CardGame.CommonModel.Bus
{
    public class PlayerConnected
    {
        internal PlayerConnected(){}
        public PlayerConnected(Guid gameId, 
            Guid playerId,
            Guid correlationId)
        {
            GameId = gameId;
            PlayerId = playerId;
            CorrelationId = correlationId;
        }

        public Guid GameId { get; set; }
        public Guid PlayerId { get; set; }
        public Guid CorrelationId { get; set; }
    }
}