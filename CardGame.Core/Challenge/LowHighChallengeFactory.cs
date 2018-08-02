using System;
using System.Text;
using MassTransit;

namespace CardGame.Core.Challenge
{
    public class LowHighChallengeFactory
    {
        private readonly ICryptoService _cryptoService;
        private readonly Random _random;

        public LowHighChallengeFactory(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
            _random = new Random();
        }
        public LowHighChallenge CreateFromRequest(Guid requester, Guid target, Guid correlationId, byte[] encryptedRequesterValue)
        {
            int targetValue = GetRandomInt();
            bool targetIsLowerThanRequester = GetRandomBool();
            var lowHighChallenge = new LowHighChallenge(requester, target, correlationId, encryptedRequesterValue, targetValue, targetIsLowerThanRequester);
            return lowHighChallenge;
        }
        public LowHighChallenge CreateRequest(Guid requester, Guid target, IPublishEndpoint publishEndpoint)
        {
            var correlationId = Guid.NewGuid();
            int requesterValue = GetRandomInt();
            byte[] requesterKey = Encoding.UTF8.GetBytes("super secrete key");
            byte[] encryptedRequesterValue = _cryptoService.Encrypt(requesterValue, requesterKey);
            var lowHighChallenge = new LowHighChallenge(requester, target, correlationId, requesterValue, encryptedRequesterValue, requesterKey);
            
            Console.WriteLine($"{nameof(correlationId)}:{correlationId}");
            publishEndpoint.Publish(new LowHighChallengeRequestEvent(
                lowHighChallenge.EncryptedRequesterValue, lowHighChallenge.Target, lowHighChallenge.Requester, correlationId));
            return lowHighChallenge;
        }

        private int GetRandomInt()
        {
            return _random.Next(int.MinValue, int.MaxValue - 1);
        }
        
        private bool GetRandomBool()
        {
            var value = _random.NextDouble();
            return value > 0.5;
        }
    }
}