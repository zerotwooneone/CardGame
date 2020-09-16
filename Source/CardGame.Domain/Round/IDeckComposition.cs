using System.Collections.Generic;
using CardGame.Domain.Card;

namespace CardGame.Domain.Round
{
    //todo: move this to abstractions
    public interface IDeckComposition : IReadOnlyDictionary<CardId, int> {}
}