using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Core.Card;

namespace CardGame.Core.Round
{
    public class Round
    {
        private readonly IList<Card.Card> _discarded;
        private readonly IList<Card.Card> _drawCards;
        private readonly IDictionary<Guid, Hand.Hand> _hands;
        private readonly IList<Guid> _protectedPlayers;
        private readonly IDictionary<Guid, Player.Player> _remainingPlayers;
        private readonly IList<Card.Card> _setAsideCards;

        private readonly Guid? _winningPlayerId;
        public readonly IEnumerable<Player.Player> Players;

        public Round(IEnumerable<Player.Player> players,
            IEnumerable<Card.Card> deck)
        {
            _drawCards = deck.ToList();
            _discarded = new List<Card.Card>();
            _setAsideCards = new List<Card.Card>();
            var enumerablePlayers = players as Player.Player[] ?? players.ToArray();
            Players = enumerablePlayers;
            _remainingPlayers = enumerablePlayers.ToDictionary(p => p.Id);
            _protectedPlayers = new List<Guid>();
            _winningPlayerId = null;
            _hands = enumerablePlayers.ToDictionary(p => p.Id, p => (Hand.Hand) null);
        }

        public IEnumerable<Card.Card> Discarded => _discarded;

        public Turn.Turn CurrentTurn { get; private set; }
        public IEnumerable<Player.Player> RemainingPlayers => _remainingPlayers.Values;

        private Card.Card Draw()
        {
            var card = RemoveTopCard();
            var drawContext = CreateDrawContext();
            card.OnDraw(
                drawContext);
            return card;
        }

        private Card.Card RemoveTopCard()
        {
            var card = _drawCards.First();
            _drawCards.RemoveAt(0);
            return card;
        }

        private DrawContext CreateDrawContext()
        {
            return new DrawContext(CurrentTurn, GetCurrentPlayerHand);
        }

        public PlayContext CreatePlayContext(Guid? targetPlayerId, CardValue? guessedCardvalue)
        {
            var result = new PlayContext(AddPlayerProtection,
                EliminatePlayer, CurrentTurn, TradeHands, RevealHand,
                DiscardAndDraw, targetPlayerId, guessedCardvalue);
            return result;
        }

        private void DiscardAndDraw(Guid targetId)
        {
            if (_protectedPlayers.Contains(targetId)) return;
            var hand = _hands[targetId];
            Discard(hand.Previous);
            var newCard = Draw();
            var newHand = new Hand.Hand(newCard);
            _hands[targetId] = newHand;
        }

        public Hand.Hand GetCurrentPlayerHand()
        {
            return _hands[CurrentTurn.CurrentPlayer.Id];
        }

        private CardValue RevealHand(Guid targetPlayerId)
        {
            //crud. need two versions of this method. one which can be immune with _protectedPlayers
            return _hands[targetPlayerId].Previous.Value;
        }

        private void TradeHands(Guid sourcePlayerId, Guid targetPlayerId)
        {
            if (_protectedPlayers.Contains(targetPlayerId)) return;
            var sourceHand = _hands[sourcePlayerId];
            var targetHand = _hands[targetPlayerId];
            _hands[sourcePlayerId] = targetHand;
            _hands[targetPlayerId] = sourceHand;
        }

        private void EliminatePlayer(Guid playerId)
        {
            _remainingPlayers.Remove(playerId);
        }

        private void AddPlayerProtection(Guid playerId)
        {
            _protectedPlayers.Add(playerId);
        }

        public Guid? GetWinningPlayerId()
        {
            if (_winningPlayerId == null)
            {
                if (_remainingPlayers.Count == 1) return _remainingPlayers.Values.First().Id;

                var maxValue = _remainingPlayers.Max(p => _hands[p.Key].Max(c => c.Value));
                //we arbitrarily choose the last player (the first match might be the winner from the last round) that matches, but really we need a better way
                return _remainingPlayers.Last(p => _hands[p.Key].Max(c => c.Value) == maxValue).Value.Id;
            }

            return _winningPlayerId;
        }

        private Turn.Turn GetNextTurn()
        {
            if (CurrentTurn == null)
            {
                CurrentTurn = new Turn.Turn(Players.First());
                _setAsideCards.Add(RemoveTopCard());
                foreach (var player in Players)
                {
                    var drawn = RemoveTopCard();
                    var newHand = new Hand.Hand(drawn);
                    _hands[player.Id] = newHand;
                }
            }
            else
            {
                if (_remainingPlayers.Count <= 1 ||
                    _drawCards.Count <= 0)
                    return null;
                var nextPlayerIndex = Players.ToList().IndexOf(CurrentTurn.CurrentPlayer) + 1;
                var nextPlayer = Players.Concat(Players).Skip(nextPlayerIndex).First();
                var drawn = Draw();
                var newHand = _hands[nextPlayer.Id].Append(drawn);
                _hands[nextPlayer.Id] = newHand;
                CurrentTurn = new Turn.Turn(nextPlayer);
            }

            return CurrentTurn;
        }

        public IEnumerable<Turn.Turn> Start()
        {
            Turn.Turn turn;
            do
            {
                turn = GetNextTurn();
                if (turn != null)
                {
                    _protectedPlayers.Remove(CurrentTurn.CurrentPlayer.Id);
                    var drawn = Draw();
                    var hand = GetCurrentPlayerHand();
                    _hands[CurrentTurn.CurrentPlayer.Id] = hand.Append(drawn);
                    yield return turn;
                }
            } while (turn != null);
        }

        public void Cleanup()
        {
            foreach (var player in Players)
            {
                var handPrevious = _hands[player.Id].Previous;
                if (handPrevious != null) Discard(handPrevious);
            }

            foreach (var setAsideCard in _setAsideCards) Discard(setAsideCard);
        }

        public void Discard(Card.Card playCard)
        {
            var keyValuePair = _hands.FirstOrDefault(kvp =>
            {
                return kvp.Value != null && 
                       kvp.Value.Any(p =>
                {
                    return p.Id == playCard.Id;
                });
            });
            if (!default(KeyValuePair<Guid, Hand.Hand>).Equals(keyValuePair))
            {
                _hands[keyValuePair.Key] = keyValuePair.Value.Discard(playCard.Id);
                _discarded.Add(playCard);
            }
        }
    }
}