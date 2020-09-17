using System;

namespace CardGame.Domain.Abstractions.Card
{
    public interface ICardId : IEquatable<ICardId>
    {
        ICardValue CardValue { get; }
        int Variant { get; }
    }
}