using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Core.Card;

namespace CardGame.Core.Round
{
    public class Round : IPlayRound
    {
        public delegate CardValue GetCardValue(Guid cardId);

        public delegate Guid? GetCurrentPlayerId();

        private readonly IList<Guid> _discarded;
        private readonly IList<Guid> _drawCards;
        private readonly IDictionary<Guid, Hand.Hand> _playerHands;
        private readonly IList<Guid> _protectedPlayers;
        private readonly IDictionary<Guid, string> _remainingPlayers;
        private readonly IList<Guid> _setAsideCards;

        private readonly Guid? _winningPlayerId;
        public readonly IEnumerable<Guid> Players;
        private ushort _nextRoundId;

        /// <summary>
        ///     Used for creating a new round during a game
        /// </summary>
        /// <param name="players"></param>
        /// <param name="deck"></param>
        /// <param name="id"></param>
        public Round(IEnumerable<Guid> players,
            IEnumerable<Guid> deck, Guid id)
        {
            Id = id;
            _drawCards = deck.ToList();
            _discarded = new List<Guid>();
            _setAsideCards = new List<Guid>();
            var enumerablePlayers = players as Guid[] ?? players.ToArray();
            Players = enumerablePlayers;
            _remainingPlayers = enumerablePlayers.ToDictionary(g => g, g => (string) null);
            _protectedPlayers = new List<Guid>();
            _winningPlayerId = null;
            _playerHands = enumerablePlayers.ToDictionary(p => p, p => (Hand.Hand) null);
            _nextRoundId = 0;
        }

        /// <summary>
        ///     Used for restoring a saved round
        /// </summary>
        /// <param name="playerOrder"></param>
        /// <param name="playerHands"></param>
        /// <param name="deck"></param>
        /// <param name="id"></param>
        /// <param name="nextRoundId"></param>
        /// <param name="protectedPlayers"></param>
        /// <param name="discarded"></param>
        /// <param name="setAsideCards"></param>
        public Round(IEnumerable<Guid> playerOrder,
            IDictionary<Guid, Hand.Hand> playerHands,
            IEnumerable<Guid> deck, Guid id,
            ushort nextRoundId,
            IEnumerable<Guid> protectedPlayers,
            IEnumerable<Guid> discarded,
            IEnumerable<Guid> setAsideCards,
            ushort currentTurnId)
        {
            Id = id;
            _drawCards = deck.ToList();
            _discarded = discarded.ToList();
            _setAsideCards = setAsideCards.ToList();
            Players = playerOrder;
            _remainingPlayers = playerHands
                .Where(kvp => kvp.Value != null)
                .ToDictionary(kvp => kvp.Key, kvp => (string) null);
            _protectedPlayers = protectedPlayers.ToList();
            _winningPlayerId = null;
            _playerHands = playerHands;
            _nextRoundId = nextRoundId;
            CurrentTurnId = currentTurnId;
        }

        public Guid Id { get; }
        public IEnumerable<Guid> Discarded => _discarded;
        public ushort? CurrentTurnId { get; private set; }
        public IEnumerable<Guid> RemainingPlayers => _remainingPlayers.Keys;

        public void DiscardAndDraw(Guid targetId)
        {
            if (_protectedPlayers.Contains(targetId)) return;
            var hand = _playerHands[targetId];
            Discard(hand.Previous);
            var newCard = RemoveTopCard();
            var newHand = new Hand.Hand(newCard);
            _playerHands[targetId] = newHand;
        }

        public Guid? TradeHands(Guid sourcePlayerId, Guid targetPlayerId)
        {
            if (_protectedPlayers.Contains(targetPlayerId)) return null;
            var sourceHand = _playerHands[sourcePlayerId];
            var targetHand = _playerHands[targetPlayerId];
            _playerHands[sourcePlayerId] = targetHand;
            _playerHands[targetPlayerId] = sourceHand;
            return targetHand.Previous;
        }

        public void EliminatePlayer(Guid playerId)
        {
            _remainingPlayers.Remove(playerId);
        }

        public void AddPlayerProtection(Guid playerId)
        {
            _protectedPlayers.Add(playerId);
        }

        public void Discard(Guid playCard)
        {
            var keyValuePair = _playerHands.FirstOrDefault(kvp =>
            {
                return kvp.Value != null &&
                       kvp.Value.Any(p => p == playCard);
            });
            if (!default(KeyValuePair<Guid, Hand.Hand>).Equals(keyValuePair))
            {
                _playerHands[keyValuePair.Key] = keyValuePair.Value.Discard(playCard);
                _discarded.Add(playCard);
            }
        }

        private Guid Draw(Guid playerId)
        {
            var cardId = RemoveTopCard();
            var newHand = _playerHands[playerId].Append(cardId);
            _playerHands[playerId] = newHand;
            return cardId;
        }

        private Guid RemoveTopCard()
        {
            var card = _drawCards.First();
            _drawCards.RemoveAt(0);
            return card;
        }

        public Hand.Hand GetPlayerHand(Guid currentPlayerId)
        {
            return _playerHands[currentPlayerId];
        }

        public Guid RevealHand(Guid targetPlayerId)
        {
            //crud. need two versions of this method. one which can be immune with _protectedPlayers
            return _playerHands[targetPlayerId].Previous;
        }

        public Guid? GetWinningPlayerId(GetCardValue getCardValue)
        {
            if (_winningPlayerId == null)
            {
                if (_remainingPlayers.Count == 1) return _remainingPlayers.Keys.First();

                var maxValue = _remainingPlayers.Max(p => _playerHands[p.Key].Max(x => getCardValue(x)));
                //we arbitrarily choose the last player (the first match might be the winner from the last round) that matches, but really we need a better way
                return _remainingPlayers.Last(p => _playerHands[p.Key].Max(x => getCardValue(x)) == maxValue).Key;
            }

            return _winningPlayerId;
        }

        private Turn.Turn GetNextTurn(Guid nextPlayerId, GetCardValue getCardValue)
        {
            if (CurrentTurnId == null)
            {
                _setAsideCards.Add(RemoveTopCard());
                foreach (var player in Players)
                {
                    var newDeltHand = RemoveTopCard();
                    var newHand = new Hand.Hand(newDeltHand);
                    _playerHands[player] = newHand;
                }
            }
            else
            {
                if (_remainingPlayers.Count <= 1 ||
                    _drawCards.Count <= 0)
                    return null;
            }

            CurrentTurnId = _nextRoundId;
            var result = new Turn.Turn(nextPlayerId, CurrentTurnId.Value);
            _nextRoundId++;

            Draw(nextPlayerId);
            var hand = GetPlayerHand(nextPlayerId);
            var previous = getCardValue(hand.Previous);
            var drawn = getCardValue(hand.Drawn.Value);
            result.MarkUnplayable(previous, drawn);
            _protectedPlayers.Remove(nextPlayerId);

            return result;
        }

        public IEnumerable<Turn.Turn> Start(GetCardValue getCardValue, GetCurrentPlayerId getCurrentPlayerId)
        {
            Turn.Turn turn;
            do
            {
                Guid nextplayerId;
                if (CurrentTurnId == null)
                {
                    nextplayerId = Players.First();
                }
                else
                {
                    var currentPlayerId = getCurrentPlayerId().Value;
                    var nextPlayerIndex = Players.ToList().IndexOf(currentPlayerId) + 1;
                    nextplayerId = Players.Concat(Players).Skip(nextPlayerIndex).First();
                }

                turn = GetNextTurn(nextplayerId, getCardValue);
                if (turn != null) yield return turn;
            } while (turn != null);
        }

        public void Cleanup()
        {
            foreach (var player in Players)
            {
                var handPrevious = _playerHands[player].Previous;
                Discard(handPrevious);
            }

            foreach (var setAsideCard in _setAsideCards) Discard(setAsideCard);
        }
    }
}