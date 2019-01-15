﻿using System;
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
                    var newCardId = PlayKing(playCard.Id, playerId, targetPlayer.Value, round);
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
                    var knownPlayerHand = PlayPriest(playCard.Id, playerId, targetPlayer.Value, targetCard.Value, turn, round);
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
        }

        public void PlayPrincess(Guid cardId, Guid playerId, IPlayRound round)
        {
            round.EliminatePlayer(playerId);
            round.Play(playerId, cardId);
        }

        public Guid? PlayKing(Guid cardId, Guid playerId, Guid targetId, IPlayRound round)
        {
            var result = round.TradeHands(playerId, targetId);
            round.Play(playerId, cardId);
            return result;
        }

        public void PlayPrince(Guid cardId, Guid playerId, Guid targetId, IPlayRound round)
        {
            round.DiscardAndDraw(targetId);
            round.Play(playerId, cardId);
        }

        public void PlayHandmaid(Guid cardId, Guid playerId, IPlayRound round)
        {
            round.AddPlayerProtection(playerId);
            round.Play(playerId, cardId);
        }

        public void PlayBaron(Guid cardId, Guid playerId, Guid targetId, CardValue targetHand, IPlayRound round)
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

            round.Play(playerId, cardId);
        }

        public KnownPlayerHand PlayPriest(Guid cardId, Guid playerId, Guid targetId, CardValue targetHand, IPlayTurn turn,
            IPlayRound round)
        {
            var knownPlayerHand = turn.RevealHand(targetId, targetHand);
            round.Play(playerId, cardId);
            return knownPlayerHand;
        }

        public void PlayGuard(Guid cardId, Guid playerId, Guid targetId, CardValue targetHand, IPlayRound round,
            CardValue guess)
        {
            if (guess == CardValue.Guard) throw new ArgumentException("Can not guess Guard Value", nameof(guess));
            if (targetHand == guess)
                round.EliminatePlayer(targetId);
            round.Play(playerId, cardId);
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