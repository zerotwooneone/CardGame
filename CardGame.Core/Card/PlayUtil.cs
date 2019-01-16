using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Core.Round;
using CardGame.Core.Turn;

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
            IPlayCard playCard,
            IPlayTurn turn,
            IPlayRound round,
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

            if (RequiresTargetHandToPlay(cardValue) && targetCard == null)
                throw new ArgumentException("Missing target card value", nameof(targetCard));

            if ((previousValue == CardValue.Countess ||
                 drawnValue == CardValue.Countess) &&
                (playCard.Value == CardValue.King ||
                 playCard.Value == CardValue.Prince))
                throw new ArgumentException("Can not play King or Prince when Countess is in hand", nameof(playCard));

            switch (cardValue)
            {
                case CardValue.Princess:
                    round.PlayPrincess(playCard.Id, playerId);
                    break;
                case CardValue.King:
                    var newCardId = round.PlayKing(playCard.Id, playerId, targetPlayer.Value);
                    break;
                case CardValue.Prince:
                    round.PlayPrince(playCard.Id, playerId, targetPlayer.Value);
                    break;
                case CardValue.Handmaid:
                    round.PlayHandmaid(playCard.Id, playerId);
                    break;
                case CardValue.Baron:
                    round.PlayBaron(playCard.Id, playerId, targetPlayer.Value, targetCard.Value);
                    break;
                case CardValue.Priest:
                    if (round.PlayPriest(playCard.Id, playerId, targetPlayer.Value))
                    {
                        var knownPlayerHand = turn.PlayPriest(targetPlayer.Value, targetCard.Value);
                    }

                    break;
                case CardValue.Guard:
                    round.PlayGuard(playCard.Id, playerId, targetPlayer.Value, targetCard.Value,
                        guessedCardvalue.Value);
                    break;
                case CardValue.Countess:
                    break; //nothing
                default:
                    throw new ArgumentException("Unknown card value", nameof(cardValue));
            }
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