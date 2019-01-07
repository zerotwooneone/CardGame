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
        private readonly IList<Card.Card> _setAsideCards;
        private readonly IList<Guid> _protectedPlayers;
        private readonly IDictionary<Guid, Player.Player> _remainingPlayers;
        public readonly IEnumerable<Player.Player> Players;

        private readonly Guid? _winningPlayerId;

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
        }

        public void Init()
        {
            _setAsideCards.Add(Draw());
            foreach (var player in Players)
            {
                player.SetHand(new Hand.Hand(Draw()));
            }
        }

        public IEnumerable<Card.Card> Discarded => _discarded;

        public Turn.Turn CurrentTurn { get; private set; }
        public IEnumerable<Player.Player> RemainingPlayers => _remainingPlayers.Values;

        private Card.Card Draw()
        {
            var card = _drawCards.First();
            _drawCards.RemoveAt(0);
            card.OnDraw(CreateRoundContext()); //there is going to be an issue when being forced to draw countess while holding king or prince
            return card;
        }

        public RoundContext CreateRoundContext()
        {
            var result = new RoundContext(OnForcedDiscard, AddPlayerProtection, Draw, EliminatePlayer, CurrentTurn);
            return result;
        }

        private void EliminatePlayer(Guid playerId)
        {
            _remainingPlayers.Remove(playerId);
        }

        private void AddPlayerProtection(Guid playerId)
        {
            _protectedPlayers.Add(playerId);
        }

        private void OnForcedDiscard(Card.Card card)
        {
            card.OnDiscard(CreateRoundContext(), null, null);
            Discard(card);
        }

        public Guid? GetWinningPlayerId()
        {
            if (_winningPlayerId == null)
            {
                if (_remainingPlayers.Count == 1)
                {
                    return _remainingPlayers.Values.First().Id;
                }

                var maxValue = _remainingPlayers.Max(p => p.Value.Hand.First().Value);
                //we arbitrarily choose the last player (the first match might be the winner from the last round) that matches, but really we need a better way
                return _remainingPlayers.Last(p => p.Value.Hand.First().Value == maxValue).Value.Id;
            }

            return _winningPlayerId;
        }

        private Turn.Turn GetNextTurn()
        {
            if (CurrentTurn == null)
            {
                CurrentTurn = new Turn.Turn(Players.First());
            }
            else
            {
                if (_remainingPlayers.Count <= 1 ||
                    _drawCards.Count <= 0)
                    return null;
                var nextPlayerIndex = Players.ToList().IndexOf(CurrentTurn.CurrentPlayer) + 1;
                var nextPlayer = Players.Concat(Players).Skip(nextPlayerIndex).First();
                nextPlayer.SetHand(new Hand.Hand(nextPlayer.Hand.Previous, Draw()));
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
                    var card = Draw();
                    turn.Init(card);
                    yield return turn;
                }
                
            } while (turn != null);
        }

        public void Cleanup()
        {
            foreach (var player in Players)
            {
                var handPrevious = player.Hand.Previous;
                if (handPrevious != null)
                {
                    Discard(handPrevious);
                }
            }

            foreach (var setAsideCard in _setAsideCards)
            {
                Discard(setAsideCard);
            }
        }

        public void Discard(Card.Card playCard)
        {
            _discarded.Add(playCard);
        }
    }
}