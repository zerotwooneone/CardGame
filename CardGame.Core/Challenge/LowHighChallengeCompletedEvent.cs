using System;

namespace CardGame.Core.Challenge
{
    public class LowHighChallengeCompletedEvent
    {
        public Guid CorrelationId { get; }
        public bool RequesterWins { get; }

        public LowHighChallengeCompletedEvent(Guid correlationId, bool requesterWins)
        {
            CorrelationId = correlationId;
            RequesterWins = requesterWins;
        }
    }
}