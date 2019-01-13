using System;
using System.Collections.Generic;
using System.Linq;

namespace CardGame.Core.Card
{
    public class PlayUtil
    {
        public readonly IEnumerable<CardValue> RequireGuessedCardValueToPlay = new[] {CardValue.Guard};

        public readonly IEnumerable<CardValue> RequireTargetHandToPlay = new[]
            {CardValue.Baron, CardValue.Priest, CardValue.Guard};

        public readonly IEnumerable<CardValue> RequireTargetIdToPlay = new[]
            {CardValue.King, CardValue.Prince, CardValue.Baron, CardValue.Priest, CardValue.Guard};

        public void Play(Guid playerId,
            Card playCard,
            Turn.Turn turn,
            Round.Round round,
            CardValue previousValue,
            CardValue drawnValue,
            Guid? targetPlayer = null,
            CardValue? guessedCardvalue = null,
            CardValue? targetCard = null)
        {
            var cardValue = playCard.Value;
            if (RequiresTargetPlayerToPlay(cardValue) && targetPlayer == null)
                throw new ArgumentException("Missing target player", nameof(targetPlayer));

            if (RequiresGuessedCardToPlay(cardValue) && guessedCardvalue == null)
                throw new ArgumentException("Missing guessed card value", nameof(guessedCardvalue));

            if(RequiresTargetHandToPlay(cardValue) && targetCard == null)
                throw new ArgumentException("Missing target card value", nameof(targetCard));

            if ((previousValue == CardValue.Countess ||
                 drawnValue == CardValue.Countess) &&
                (playCard.Value == CardValue.King ||
                 playCard.Value == CardValue.Prince))
            {
                throw new ArgumentException("Can not play King or Prince when Countess is in hand", nameof(playCard));
            }

            switch (cardValue)
            {
                case CardValue.Princess:
                    PlayPrincess(playCard.Id, playerId, round);
                    break;
                case CardValue.King:
                    PlayKing(playCard.Id, playerId, targetPlayer.Value, round);
                    break;
                case CardValue.Prince:
                    PlayPrince(playCard.Id, playerId, targetPlayer.Value, round);
                    break;
                case CardValue.Handmaid:
                    PlayHandmaid(playCard.Id, playerId, round);
                    break;
                case CardValue.Baron:
                    PlayBaron(playCard.Id, playerId, targetPlayer.Value, targetCard.Value, round);
                    break;
                case CardValue.Priest:
                    PlayPriest(playCard.Id, playerId, targetPlayer.Value, targetCard.Value, turn, round);
                    break;
                case CardValue.Guard:
                    PlayGuard(playCard.Id, playerId, targetPlayer.Value, targetCard.Value, round,
                        guessedCardvalue.Value);
                    break;
                case CardValue.Countess:
                    break; //nothing
                default:
                    throw new ArgumentException("Unknown card value", nameof(cardValue));
            }

            round.Discard(playCard.Id);
        }

        public void PlayPrincess(Guid cardId, Guid playerId, Round.Round round)
        {
            round.EliminatePlayer(playerId);
            Play(cardId, round);
        }

        private void Play(Guid cardId, Round.Round round)
        {
            round.Discard(cardId);
        }

        public void PlayKing(Guid cardId, Guid playerId, Guid targetId, Round.Round round)
        {
            round.TradeHands(playerId, targetId);
            Play(cardId, round);
        }

        public void PlayPrince(Guid cardId, Guid playerId, Guid targetId, Round.Round round)
        {
            round.DiscardAndDraw(targetId);
            Play(cardId, round);
        }

        public void PlayHandmaid(Guid cardId, Guid playerId, Round.Round round)
        {
            round.AddPlayerProtection(playerId);
            Play(cardId, round);
        }

        public void PlayBaron(Guid cardId, Guid playerId, Guid targetId, CardValue targetHand, Round.Round round)
        {
            if (CardValue.Baron == targetHand)
            {
                //we do nothing
            }
            else if (CardValue.Baron > targetHand)
            {
                round.EliminatePlayer(targetId);
            }
            else
            {
                round.EliminatePlayer(playerId);
            }

            Play(cardId, round);
        }

        public void PlayPriest(Guid cardId, Guid playerId, Guid targetId, CardValue targetHand, Turn.Turn turn,
            Round.Round round)
        {
            turn.RevealHand(targetId, targetHand);
            Play(cardId, round);
        }

        public void PlayGuard(Guid cardId, Guid playerId, Guid targetId, CardValue targetHand, Round.Round round,
            CardValue guess)
        {
            if (guess == CardValue.Guard) throw new ArgumentException("Can not guess Guard Value", nameof(guess));
            if (targetHand == guess)
                round.EliminatePlayer(targetId);
            Play(cardId, round);
        }

        public bool RequiresTargetPlayerToPlay(CardValue cardValue)
        {
            return RequireTargetIdToPlay.Contains(cardValue);
        }

        public bool RequiresTargetHandToPlay(CardValue cardValue)
        {
            return RequireTargetHandToPlay.Contains(cardValue);
        }

        public bool RequiresGuessedCardToPlay(CardValue cardValue)
        {
            return RequireGuessedCardValueToPlay.Contains(cardValue);
        }
    }
}