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
        public IEnumerable<Guid> ProtectedPlayers => _protectedPlayers;

        public void PlayPrince(Guid cardId, Guid playerId, Guid targetId)
        {
            if (_protectedPlayers.Contains(targetId)) return;
            Play(playerId, cardId);

            Discard(targetId);
            var newCard = RemoveTopCard();
            var newHand = new Hand.Hand(newCard);
            _playerHands[targetId] = newHand;
        }

        public Guid? PlayKing(Guid cardId, Guid playerId, Guid targetId)
        {
            if (_protectedPlayers.Contains(targetId)) return null;
            if (playerId == targetId) throw new ArgumentException("Cannot target self", nameof(targetId));
            Play(playerId, cardId);
            var sourceHand = _playerHands[playerId];
            var targetHand = _playerHands[targetId];
            _playerHands[playerId] = targetHand;
            _playerHands[targetId] = sourceHand;
            return targetHand.Previous;
        }

        public void PlayBaron(Guid cardId, Guid playerId, Guid targetId, CardValue targetHand)
        {
            if (_protectedPlayers.Contains(targetId)) return;
            if (playerId == targetId) throw new ArgumentException("Cannot target self", nameof(targetId));
            if (CardValue.Baron == targetHand)
            {
                //we do nothing
            }
            else if (CardValue.Baron > targetHand)
            {
                EliminatePlayer(targetId);
            }
            else
            {
                EliminatePlayer(playerId);
            }

            Play(playerId, cardId);
        }

        public bool PlayPriest(Guid playCardId, Guid playerId, Guid targetId)
        {
            if (playerId == targetId) throw new ArgumentException("Cannot target self", nameof(targetId));
            Play(playerId, playCardId);
            return !_protectedPlayers.Contains(targetId);
        }

        public void PlayGuard(Guid cardId, Guid playerId, Guid targetId, CardValue targetHand, CardValue guess)
        {
            if (_protectedPlayers.Contains(targetId)) return;
            if (playerId == targetId) throw new ArgumentException("Cannot target self", nameof(targetId));
            if (guess == CardValue.Guard) throw new ArgumentException("Can not guess Guard Value", nameof(guess));
            if (targetHand == guess) EliminatePlayer(targetId);
            Play(playerId, cardId);
        }

        public void PlayPrincess(Guid cardId, Guid playerId)
        {
            EliminatePlayer(playerId);
            Play(playerId, cardId);
        }

        public void PlayHandmaid(Guid cardId, Guid playerId)
        {
            _protectedPlayers.Add(playerId);
            Play(playerId, cardId);
        }

        private void EliminatePlayer(Guid playerId)
        {
            _remainingPlayers.Remove(playerId);
        }

        public void Play(Guid playerId, Guid playCard)
        {
            var hand = GetPlayerHand(playerId);
            _playerHands[playerId] = hand.Discard(playCard);
            _discarded.Add(playCard);
        }

        public void Discard(Guid playerId)
        {
            var hand = GetPlayerHand(playerId);
            _playerHands[playerId] = null;
            foreach (var cardId in hand) _discarded.Add(cardId);
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

        public Hand.Hand GetPlayerHand(Guid playerId)
        {
            return _playerHands[playerId];
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
            foreach (var playerId in Players) Discard(playerId);

            foreach (var setAsideCard in _setAsideCards) _discarded.Add(setAsideCard);
            _setAsideCards.Clear();
        }
    }
}