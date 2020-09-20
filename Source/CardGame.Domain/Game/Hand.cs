﻿using System.Collections.Generic;
using System.Linq;
using CardGame.Domain.Abstractions.Card;
using CardGame.Domain.Card;
using CardGame.Utils.Factory;
using CardGame.Utils.Validation;
using CardGame.Utils.Value;

namespace CardGame.Domain.Game
{
    public class Hand : Value
    {
        public CardId Card1 { get; }
        public CardId Card2 { get; }
        public IEnumerable<CardId> Cards { get; }

        protected Hand(CardId card1, CardId card2, IEnumerable<CardId> cards)
        {
            Card1 = card1;
            Card2 = card2;
            Cards = cards;
        }
        public override int GetHashCode()
        {
            return (magicAdd * magicFactor)
                   + GetHashCode(Cards);

        }

        public override bool Equals(object obj)
        {
            var other = obj as Hand;
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var cards = Cards.ToArray();
            var otherCards = other.Cards.ToArray();
            return cards.Length == otherCards.Length &&
                   cards.SequenceEqual(otherCards);
        }

        public static FactoryResult<Hand> Factory(IEnumerable<CardId> cards = null)
        {
            var ca = (cards ?? new CardId[0]).ToArray();
            const int MaxHandSize = 2;
            if (ca.Length > MaxHandSize)
            {
                return FactoryResult<Hand>.Error($"Handsize {ca.Length} exceeds max of {MaxHandSize}");
            }

            var card1 = ca.Length > 0
                ? ca[0]
                : null;
            var card2 = ca.Length > 1
                ? ca[1]
                : null;
            return FactoryResult<Hand>.Success(new Hand(card1, card2, ca));
        }

        public bool HasCard(ICardId cardId)
        {
            return cardId.Equals(Card1) || cardId.Equals(Card2);
        }
        public bool HasCard(CardStrength cardStrength)
        {
            return Cards.Any(c => c.CardValue.Value == cardStrength);
        }

        public Hand Discard(ICardId cardId, Notification note)
        {
            if (!HasCard(cardId))
            {
                note.AddError($"Hand does not contain card:{cardId}");
                return this;
            }

            var cards = Cards.ToList();
            const int minCards = 2;
            if (cards.Count < minCards)
            {
                note.AddError($"Cannot discard the last card in hand card:{cardId}");
            }

            var match = cards.FirstOrDefault(c => c.Equals(cardId));
            cards.Remove(match);
            var result = Factory(cards);
            if (result.IsError)
            {
                note.AddError(result.ErrorMessage);
                return this;
            }

            note.AddStateChange(nameof(Hand));
            return result.Value;
        }


        public Hand Replace(CardId cardId, Notification note)
        {
            var result = Factory(new[] {cardId});
            if (result.IsError)
            {
                note.AddError(result.ErrorMessage);
                return this;
            }
            note.AddStateChange(nameof(Hand));
            return result.Value;
        }

        public bool IsWeaker(Hand targetHand, Notification note)
        {
            if (Card2 != null || targetHand.Card2 != null)
            {
                note.AddError("Cant compare hands with more than one card");
                return false;
            }

            if (Card1 == null || targetHand.Card1 == null)
            {
                note.AddError("Cant compare empty hands");
                return false;
            }

            return Card1.IsWeaker(targetHand.Card1, note);
        }

        public Hand Draw(CardId cardId, Notification note)
        {
            var result = Factory(Cards.Append(cardId));
            if (result.IsError)
            {
                note.AddError(result.ErrorMessage);
                return this;
            }
            note.AddStateChange(nameof(Hand));
            return result.Value;
        }
    }
}