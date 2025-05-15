using System.Collections.Generic;
using System;

namespace CardGame.Domain.Game
{
    public class DeckDefinition
    {
        public IEnumerable<Card> Cards { get; }
        public string BackAppearanceId { get; }

        public DeckDefinition(IEnumerable<Card> cards, string backAppearanceId)
        {
            Cards = cards ?? throw new ArgumentNullException(nameof(cards));
            BackAppearanceId = backAppearanceId ?? throw new ArgumentNullException(nameof(backAppearanceId));
        }
    }
}
