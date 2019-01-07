﻿using System;
using System.Linq;
using CardGame.Core.Round;

namespace CardGame.Core.Card
{
    public class Card
    {
        public readonly Guid Id;
        public readonly string Name;
        public readonly Action<RoundContext, Player.Player, CardValue?> OnDiscard;
        public readonly Action<RoundContext> OnDraw;
        public readonly string TypeName;
        public readonly CardValue Value;

        public Card(Guid id,
            CardValue value,
            string name,
            string typeName,
            Action<RoundContext> specialOnDraw = null,
            Action<RoundContext, Player.Player, CardValue?> specialOnDiscard = null)
        {
            Id = id;
            Value = value;
            Name = name;
            TypeName = typeName;
            OnDraw = GetOnDraw(Value, specialOnDraw);
            OnDiscard = GetOnDiscard(Value, specialOnDiscard);
        }

        public Action<RoundContext> GetOnDraw(CardValue value, Action<RoundContext> specialOnDraw)
        {
            return roundContext =>
            {
                specialOnDraw?.Invoke(roundContext);
                switch (value)
                {
                    case CardValue.Countess:
                        HandleCountessOnDraw(roundContext);
                        break;
                    default:
                        HandleNonCountessOnDraw(roundContext);
                        break;
                }
            };
        }
        
        public Action<RoundContext, Player.Player, CardValue?> GetOnDiscard(CardValue value,
            Action<RoundContext, Player.Player, CardValue?> specialOnDiscard)
        {
            return (roundContext, target, guessValue) =>
            {
                //switch (value)
                //{
                //    case CardValue.King:
                //    case CardValue.Prince:
                //    case CardValue.Baron:
                //    case CardValue.Priest:
                //    case CardValue.Guard:
                //        if (target == null) throw new Exception("target must be set");
                //        break;
                //    case CardValue.Princess:
                //    case CardValue.Handmaid:
                //    case CardValue.Countess:
                //    default:
                //        break;
                //}

                //if (value == CardValue.Guard &&
                //    guessValue == null)
                //{
                //    throw new Exception("guessValue must be set");
                //}

                specialOnDiscard?.Invoke(roundContext, target, guessValue);
                switch (value)
                {
                    case CardValue.Princess:
                        HandlePrincessOnDiscard(roundContext);
                        break;
                    case CardValue.King:
                        if (target == null) break;
                        HandleKingOnDiscard(roundContext, target);
                        break;
                    case CardValue.Prince:
                        if (target == null) break;
                        HandlePrinceOnDiscard(roundContext, target);
                        break;
                    case CardValue.Handmaid:
                        HandleHandMaidOnDiscard(roundContext);
                        break;
                    case CardValue.Baron:
                        if (target == null) break;
                        HandleBaronOnDiscard(roundContext, target);
                        break;
                    case CardValue.Priest:
                        if (target == null) break;
                        HandlePriestOnDiscard(roundContext, target);
                        break;
                    case CardValue.Guard:
                        if (target == null) break;
                        HandleGuardOnDiscard(roundContext, target, guessValue.Value);
                        break;
                    case CardValue.Countess:
                    default:
                        break;
                }
            };
        }

        private void HandleGuardOnDiscard(RoundContext roundContext, Player.Player target, CardValue guessValue)
        {
            if (guessValue != CardValue.Guard &&
                target.Hand.First().Value == guessValue)
                roundContext.Eliminate(target.Id);
        }

        private void HandlePriestOnDiscard(RoundContext roundContext, Player.Player target)
        {
            roundContext.CurrentTurn.RevealHand(target);
        }

        private void HandleBaronOnDiscard(RoundContext roundContext, Player.Player target)
        {
            var other = target.Hand.First();
            if (CardValue.Baron == other.Value)
            {
                //we do nothing
            }
            else if (CardValue.Baron > other.Value)
            {
                roundContext.Eliminate(target.Id);
            }
            else
            {
                var currentPlayer = roundContext.CurrentTurn.CurrentPlayer;
                roundContext.Eliminate(currentPlayer.Id);
            }
        }

        private void HandleHandMaidOnDiscard(RoundContext roundContext)
        {
            roundContext.AddCurrentPlayerProtection();
        }

        private void HandlePrinceOnDiscard(RoundContext roundContext, Player.Player target)
        {
            roundContext.DiscardAndDraw(target);
        }

        private void HandleKingOnDiscard(RoundContext roundContext, Player.Player target)
        {
            roundContext.CurrentTurn.TradeHands(target);
        }

        private void HandlePrincessOnDiscard(RoundContext roundContext)
        {
            roundContext.EliminateCurrentPlayer();
        }

        public void HandleCountessOnDraw(RoundContext roundContext)
        {
            var unplayable = roundContext.CurrentTurn?.CurrentPlayer.Hand.FirstOrDefault(c=> c.Value == CardValue.King || c.Value == CardValue.Prince);
            if (unplayable != null)
                roundContext.CurrentTurn.MarkUnplayable(unplayable);
        }

        private void HandleNonCountessOnDraw(RoundContext roundContext)
        {
            var countess = roundContext.CurrentTurn?.CurrentPlayer.Hand.FirstOrDefault(c=>c.Value == CardValue.Countess);
            if (countess != null)
                roundContext.CurrentTurn.MarkUnplayable(this);
        }
    }
}