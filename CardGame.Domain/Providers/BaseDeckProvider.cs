using CardGame.Domain.Game;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Game.GameException;
using System.Collections.Immutable;
using CardGame.Domain.Types;

namespace CardGame.Domain.Providers
{
    public abstract class BaseDeckProvider : IDeckProvider
    {
        protected record CardQuantity(int Rank, int Count);

        // Abstract properties to be implemented by derived classes
        public abstract Guid DeckId { get; }
        public abstract string DisplayName { get; }
        public abstract string Description { get; }
        protected abstract string ThemeName { get; }
        protected abstract string DeckBackAppearanceId { get; }
 
        /// <summary>
        /// Gets the list of card types and their quantities for this deck.
        /// </summary>
        protected abstract IEnumerable<CardQuantity> GetCardQuantities();

        /// <summary>
        /// Gets the appearance ID for a given card type, based on the theme.
        /// Derived classes can override for more complex appearance logic.
        /// </summary>
        protected abstract string GetCardAppearanceId(int rank, int index);

        public void ExecuteCardEffect(
            IGameOperations game, 
            Player actingPlayer, 
            Card card,
            Player? targetPlayer, 
            int? guessedRankValue)
        {

            ValidateCardEffect(actingPlayer, card, targetPlayer, guessedRankValue); 
            PerformCardEffect(game, actingPlayer, card, targetPlayer, guessedRankValue);
        }

        public DeckDefinition GetDeck()
        {
            var cards = new List<Card>();
            foreach (var quantity in GetCardQuantities())
            {
                for (int i = 0; i < quantity.Count; i++)
                {
                    var appearanceId = GetCardAppearanceId(quantity.Rank, i);
                    cards.Add(new Card( quantity.Rank,appearanceId));
                }
            }
            return new DeckDefinition(cards.ToImmutableList(), DeckBackAppearanceId);
        }

        protected abstract void ValidateCardEffect(
            Player actingPlayer,
            Card card,
            Player? targetPlayer,
            int? guessedRankValue);


        /// <summary>
        /// Performs the actual effect of the card after validation has passed.
        /// Derived classes must implement this to define card behaviors.
        /// </summary>
        /// <summary>
        /// Performs the actual effect of the card after validation has passed.
        /// Derived classes must implement this to define card behaviors.
        /// </summary>
        protected abstract void PerformCardEffect(IGameOperations game, Player actingPlayer, Card card,
            Player? targetPlayer, int? guessedCardRankValue);
    

        
    }
}
