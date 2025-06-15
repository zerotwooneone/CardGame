using System.Collections.Generic;
using System;
using CardGame.Domain.Interfaces;

namespace CardGame.Domain.Game
{
    public class DeckDefinition
    {
        /// <summary>
        /// Gets the cards in the deck.
        /// </summary>
        public IReadOnlyList<Card> Cards { get; }
        
        /// <summary>
        /// Gets the appearance ID for the back of the cards.
        /// </summary>
        public string BackAppearanceId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeckDefinition"/> class.
        /// </summary>
        /// <param name="cards">The cards in the deck.</param>
        /// <param name="backAppearanceId">The appearance ID for the back of the cards.</param>
        /// <param name="provider">The provider that created this deck definition.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are null.</exception>
        public DeckDefinition(IEnumerable<Card> cards, string backAppearanceId)
        {
            Cards = new List<Card>(cards ?? throw new ArgumentNullException(nameof(cards))).AsReadOnly();
            BackAppearanceId = backAppearanceId ?? throw new ArgumentNullException(nameof(backAppearanceId));
        }
    }
}
