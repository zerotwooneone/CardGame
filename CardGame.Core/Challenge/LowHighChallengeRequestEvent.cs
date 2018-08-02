using System;

namespace CardGame.Core.Challenge
{
    public class LowHighChallengeRequestEvent
    {
        public LowHighChallengeRequestEvent(byte[] encrypted, Guid target, Guid requester, Guid correlationId)
        {
            Encrypted = encrypted;
            Target = target;
            Requester = requester;
            CorrelationId = correlationId;
        }

        public Guid Target { get; set; }
        public byte[] Encrypted { get; }
        public Guid Requester { get; }
        public Guid CorrelationId { get; }
    }
}