using System.Collections.Generic;
using CardGame.Domain.Card;

namespace CardGame.Domain.Round
{
    //todo: move this to abstractions
    public interface IDeckBuilder
    {
        IDeckComposition DeckComposition { get; }
        IEnumerable<CardId> Shuffle(IEnumerable<CardId> cards);
    }
}