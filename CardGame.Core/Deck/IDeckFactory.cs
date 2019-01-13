using System.Collections.Generic;

namespace CardGame.Core.Deck
{
    public interface IDeckFactory
    {
        IEnumerable<Card.Card> Create();
    }
}