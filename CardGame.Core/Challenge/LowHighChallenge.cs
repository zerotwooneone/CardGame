using System;
using System.Text;
using Automatonymous;

namespace CardGame.Core.Challenge
{
    public class LowHighChallenge : SagaStateMachineInstance
    {
        public Guid Target { get; }
        public Guid Requester { get; }

        public LowHighChallenge(Guid requester, Guid target, Guid correlationId, int requesterValue, byte[] encryptedRequesterValue, byte[] requesterKey)
        {
            Target = target;
            RequesterValue = requesterValue;
            EncryptedRequesterValue = encryptedRequesterValue;
            RequesterKey = requesterKey;
            Requester = requester;
            CorrelationId = correlationId;
        }

        public LowHighChallenge(Guid requester, Guid target, Guid correlationId, byte[] encryptedRequesterValue, int targetValue, bool targetIsLowerThanRequester)
        {
            Target = target;
            Requester = requester;
            CorrelationId = correlationId;
            EncryptedRequesterValue = encryptedRequesterValue;
            TargetValue = targetValue;
            TargetIsLowerThanRequester = targetIsLowerThanRequester;
        }

        public Guid CorrelationId { get; set; }
        public byte[] EncryptedRequesterValue { get; private set; }
        public string CurrentState { get; set; }
        public int? RequesterValue { get; private set; }
        public int? TargetValue { get; private set; }
        public bool? TargetIsLowerThanRequester { get; private set; }
        public byte[] RequesterKey { get; private set; }
        public bool? Win { get; private set; }

        public void SetRequest(byte[] encrypted)
        {
            EncryptedRequesterValue = encrypted;
        }

        public void SetResponse(bool isLowerThan, int targetValue)
        {
            TargetIsLowerThanRequester = isLowerThan;
            TargetValue = targetValue;
            Win = !TargetWins();
        }

        public void SetResult(byte[] requesterKey, int requesterValue)
        {
            RequesterKey = requesterKey;
            RequesterValue = requesterValue;
            Win = TargetWins();
        }

        private bool TargetWins()
        {
            return TargetIsLowerThanRequester.Value ? TargetValue <= RequesterValue : RequesterValue > TargetValue;
        }
    }
}
