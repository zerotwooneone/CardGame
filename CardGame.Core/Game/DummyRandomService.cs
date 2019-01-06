using System;

namespace CardGame.Core.Game
{
    public class DummyRandomService : IRandomService
    {
        private readonly Random _random;

        public DummyRandomService(Random random)
        {
            _random = random;
        }

        public int GetInclusive(int minInclusive, int maxInclusive)
        {
            return _random.Next(maxInclusive, maxInclusive + 1);
        }
    }
}