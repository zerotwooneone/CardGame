using System;

namespace CardGame.Core.Challenge
{
    public class LowHighChallengeResultEvent
    {
        public LowHighChallengeResultEvent(Guid correlationId, byte[] key, Guid target)
        {
            CorrelationId = correlationId;
            Key = key;
            Target = target;
        }

        public Guid CorrelationId { get; }
        public byte[] Key { get; }
        public Guid Target { get; }
    }
}