using System;

namespace CardGame.Core.Turn
{
    public class KnownPlayerHand
    {
        public KnownPlayerHand(Guid playerId, Guid cardId)
        {
            PlayerId = playerId;
            CardId = cardId;
        }

        public Guid PlayerId { get; }
        public Guid CardId { get; }
    }
}