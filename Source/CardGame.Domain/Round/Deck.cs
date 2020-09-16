using System.Collections.Generic;
using System.Linq;
using CardGame.Domain.Card;
using CardGame.Utils.Factory;
using CardGame.Utils.Validation;
using CardGame.Utils.Value;

namespace CardGame.Domain.Round
{
    public class Deck : Value
    {
        public IEnumerable<CardId> Cards { get; }
        protected Deck(IEnumerable<CardId> cards)
        {
            Cards = cards;
        }

        public override int GetHashCode()
        {
            return GetHashCode(Cards);
        }

        public static FactoryResult<Deck> Factory(IDeckBuilder deckBuilder, IEnumerable<CardId> cards = null)
        {
            var c = (cards ?? Build(deckBuilder)).ToArray();
            return Factory(c);
        }

        public static FactoryResult<Deck> Factory(IEnumerable<CardId> cards)
        {
            return FactoryResult<Deck>.Success(new Deck(cards.ToArray()));
        }

        public Deck Draw(Notification note, out CardId card)
        {
            if (Cards.ToArray().Length <= 0)
            {
                note.AddError("Cannot draw from an empty deck");
                card = null;
                return this;
            }

            var result = Factory(Cards.Skip(1));
            if (result.IsError)
            {
                note.AddError(result.ErrorMessage);
                card = null;
                return this;
            }
            card = Cards.First();
            return result.Value;
        }

        private static IEnumerable<CardId> Build(IDeckBuilder deckBuilder)
        {
            var cards = deckBuilder.DeckComposition.Aggregate(new List<CardId>(), (list, kvp) =>
            {
                list.AddRange(Enumerable.Repeat(kvp.Key, kvp.Value));
                return list;
            });
            var shuffled = deckBuilder.Shuffle(cards);
            return shuffled;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Deck;
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var oa = other.Cards.ToArray();
            var cards = Cards.ToArray();
            if (oa.Length != cards.Length)
            {
                return false;
            }

            return GetHashCode() != other.GetHashCode();
        }

        public bool IsEmpty()
        {
            return !Cards.Any();
        }
    }
}