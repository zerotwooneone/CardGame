using System;
using CardGame.Utils.Abstractions.Value;
using CardGame.Utils.Validation;

namespace CardGame.Domain.Abstractions.Card
{
    public interface ICardValue : IStructValue<CardStrength>, IEquatable<ICardValue>
    {
        bool IsWeaker(ICardValue cardValue, Notification note);
    }
}