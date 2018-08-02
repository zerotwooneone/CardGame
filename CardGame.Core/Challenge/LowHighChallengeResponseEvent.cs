using System;

namespace CardGame.Core.Challenge
{
    public class LowHighChallengeResponseEvent
    {
        public LowHighChallengeResponseEvent(Guid correlationId, int value, bool isLowerThan, Guid requester)
        {
            CorrelationId = correlationId;
            Value = value;
            IsLowerThan = isLowerThan;
            Requester = requester;
        }

        public Guid CorrelationId { get; }
        public Guid Requester { get; set; }
        public int Value { get; }
        public bool IsLowerThan { get; }
    }
}