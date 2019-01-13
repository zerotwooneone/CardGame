using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Core.Card;

namespace CardGame.Core.Game
{
    public class Game
    {
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

        private Round.Round GetNextRound(Round.Round previousRound, Func<Guid, CardValue> getCardValue, Guid nextPlayerId)
        {
            if (_playerScores.Values.Any(s => s >= WinningScore)) return null;

            var deck = Shuffle();

            int playerIndex;
            if (previousRound == null)
            {
                var dummy = _randomService.GetInclusive(0, 1);
                
            }
            else
            {
                _playerScores[nextPlayerId] = _playerScores[nextPlayerId] + 1;
                previousRound.Cleanup();
            }
            playerIndex = Players.ToList().IndexOf(nextPlayerId);

            var players = Players.Skip(playerIndex).Concat(Players.Take(playerIndex));
            previousRound = new Round.Round(players, deck, Guid.NewGuid());
            CurrentRoundId = previousRound.Id;
            return previousRound;
        }

        public IEnumerable<Round.Round> Start(Guid firstPlayerId, Func<Guid, CardValue> getCardValue)
        {
            Round.Round round = null;
            do
            {
                var nextPlayerId = round == null ? firstPlayerId : round.GetWinningPlayerId(getCardValue).Value;
                round = GetNextRound(round, getCardValue, nextPlayerId);
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