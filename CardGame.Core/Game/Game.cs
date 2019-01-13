using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Core.Card;

namespace CardGame.Core.Game
{
    public class Game
    {
        public delegate CardValue GetCardValue(Guid cardId);

        private const int MaxPoints = 13;

        private readonly IEnumerable<Guid> _deck;
        private readonly Dictionary<Guid, int> _playerScores;
        private readonly IRandomService _randomService;
        public readonly int WinningScore;

        public Game(Guid id, IEnumerable<Guid> players, IRandomService randomService, IEnumerable<Guid> deck)
        {
            _randomService = randomService;
            Id = id;
            _playerScores = players.ToDictionary(p => p, p => 0);
            _deck = deck;
            WinningScore = MaxPoints / _playerScores.Count + 1;
        }

        public Guid Id { get; }
        public Guid? CurrentRoundId { get; private set; }
        public IEnumerable<Guid> Players => _playerScores.Keys;
        public IReadOnlyDictionary<Guid, int> PlayerScores => _playerScores;
        public Guid? FirstPlayerId { get; private set; }

        private Round.Round GetNextRound(Guid nextPlayerId)
        {
            if (_playerScores.Values.Any(s => s >= WinningScore)) return null;

            if (FirstPlayerId.HasValue)
            {
                _playerScores[FirstPlayerId.Value] = _playerScores[FirstPlayerId.Value] + 1;
            }
            FirstPlayerId = nextPlayerId;

            var deck = Shuffle();
            var playerIndex = Players.ToList().IndexOf(nextPlayerId);

            var players = Players.Skip(playerIndex).Concat(Players.Take(playerIndex));
            var newRound = new Round.Round(players, deck, Guid.NewGuid());
            CurrentRoundId = newRound.Id;
            return newRound;
        }

        public IEnumerable<Round.Round> Start(Guid firstPlayerId, GetCardValue getCardValue)
        {
            
            Round.Round round = null;
            do
            {
                if (round == null)
                {
                    
                }
                else
                {
                    firstPlayerId = round.GetWinningPlayerId(x => getCardValue(x)).Value;
                    round.Cleanup();
                }

                round = GetNextRound(firstPlayerId);
                if (round != null) yield return round;
            } while (round != null);
        }

        private IEnumerable<Guid> Shuffle()
        {
            return ShuffleIterator(_deck, _randomService);
        }

        private static IEnumerable<T> ShuffleIterator<T>(
            IEnumerable<T> source, IRandomService randomService)
        {
            var buffer = source.ToArray();
            for (var i = 0; i < buffer.Length; i++)
            {
                var j = randomService.GetInclusive(i, buffer.Length - 1);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }
    }
}