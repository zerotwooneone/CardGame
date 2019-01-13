using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Core.Card;

namespace CardGame.Core.Round
{
    public class Round
    {
        private readonly IList<Guid> _discarded;
        private readonly IList<Guid> _drawCards;
        private readonly IDictionary<Guid, Hand.Hand> _hands;
        private readonly IList<Guid> _protectedPlayers;
        private readonly IDictionary<Guid, string> _remainingPlayers;
        private readonly IList<Guid> _setAsideCards;

        private readonly Guid? _winningPlayerId;
        public readonly IEnumerable<Guid> Players;

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
            _hands = enumerablePlayers.ToDictionary(p => p, p => (Hand.Hand) null);
        }

        public Guid Id { get; }
        public IEnumerable<Guid> Discarded => _discarded;
        public Turn.Turn CurrentTurn { get; private set; }
        public IEnumerable<Guid> RemainingPlayers => _remainingPlayers.Keys;

        public Guid Draw(Guid playerId, Func<Guid, CardValue> getCardValue)
        {
            var cardId = RemoveTopCard();
            var newHand = _hands[playerId].Append(cardId);
            _hands[playerId] = newHand;
            var previous = getCardValue(newHand.Previous);
            var drawn = getCardValue(newHand.Drawn.Value);
            CurrentTurn.MarkUnplayable(previous, drawn);
            return cardId;
        }

        private Guid RemoveTopCard()
        {
            var card = _drawCards.First();
            _drawCards.RemoveAt(0);
            return card;
        }

        public void DiscardAndDraw(Guid targetId)
        {
            if (_protectedPlayers.Contains(targetId)) return;
            var hand = _hands[targetId];
            Discard(hand.Previous);
            var newCard = RemoveTopCard();
            var newHand = new Hand.Hand(newCard);
            _hands[targetId] = newHand;
        }

        public Hand.Hand GetCurrentPlayerHand()
        {
            return _hands[CurrentTurn.CurrentPlayerId];
        }

        public Guid RevealHand(Guid targetPlayerId)
        {
            //crud. need two versions of this method. one which can be immune with _protectedPlayers
            return _hands[targetPlayerId].Previous;
        }

        public void TradeHands(Guid sourcePlayerId, Guid targetPlayerId)
        {
            if (_protectedPlayers.Contains(targetPlayerId)) return;
            var sourceHand = _hands[sourcePlayerId];
            var targetHand = _hands[targetPlayerId];
            _hands[sourcePlayerId] = targetHand;
            _hands[targetPlayerId] = sourceHand;
        }

        public void EliminatePlayer(Guid playerId)
        {
            _remainingPlayers.Remove(playerId);
        }

        public void AddPlayerProtection(Guid playerId)
        {
            _protectedPlayers.Add(playerId);
        }

        public Guid? GetWinningPlayerId(Func<Guid, CardValue> getCardValue)
        {
            if (_winningPlayerId == null)
            {
                if (_remainingPlayers.Count == 1) return _remainingPlayers.Keys.First();

                var maxValue = _remainingPlayers.Max(p => _hands[p.Key].Max(getCardValue));
                //we arbitrarily choose the last player (the first match might be the winner from the last round) that matches, but really we need a better way
                return _remainingPlayers.Last(p => _hands[p.Key].Max(getCardValue) == maxValue).Key;
            }

            return _winningPlayerId;
        }

        private Turn.Turn GetNextTurn(Guid nextPlayerId, Func<Guid, CardValue> getCardValue)
        {
            if (CurrentTurn == null)
            {
                CurrentTurn = new Turn.Turn(nextPlayerId);
                _setAsideCards.Add(RemoveTopCard());
                foreach (var player in Players)
                {
                    var newDeltHand = RemoveTopCard();
                    var newHand = new Hand.Hand(newDeltHand);
                    _hands[player] = newHand;
                }
                var drawn = Draw(CurrentTurn.CurrentPlayerId, getCardValue);
            }
            else
            {
                if (_remainingPlayers.Count <= 1 ||
                    _drawCards.Count <= 0)
                    return null;
                var drawn = Draw(nextPlayerId, getCardValue);
                CurrentTurn = new Turn.Turn(nextPlayerId);
            }
            _protectedPlayers.Remove(CurrentTurn.CurrentPlayerId);

            return CurrentTurn;
        }

        public IEnumerable<Turn.Turn> Start(Func<Guid, CardValue> getCardValue)
        {
            Turn.Turn turn;
            do
            {
                Guid nextplayerId;
                if (CurrentTurn == null)
                    nextplayerId = Players.First();
                else
                {
                    var nextPlayerIndex = Players.ToList().IndexOf(CurrentTurn.CurrentPlayerId) + 1;
                    nextplayerId = Players.Concat(Players).Skip(nextPlayerIndex).First();
                }
                turn = GetNextTurn(nextplayerId, getCardValue);
                if (turn != null)
                {
                    yield return turn;
                }
            } while (turn != null);
        }

        public void Cleanup()
        {
            foreach (var player in Players)
            {
                var handPrevious = _hands[player].Previous;
                Discard(handPrevious);
            }

            foreach (var setAsideCard in _setAsideCards) Discard(setAsideCard);
        }

        public void Discard(Guid playCard)
        {
            var keyValuePair = _hands.FirstOrDefault(kvp =>
            {
                return kvp.Value != null &&
                       kvp.Value.Any(p => p == playCard);
            });
            if (!default(KeyValuePair<Guid, Hand.Hand>).Equals(keyValuePair))
            {
                _hands[keyValuePair.Key] = keyValuePair.Value.Discard(playCard);
                _discarded.Add(playCard);
            }
        }
    }
}