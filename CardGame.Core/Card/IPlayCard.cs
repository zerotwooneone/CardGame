using System;

namespace CardGame.Core.Card
{
    public interface IPlayCard
    {
        Guid Id { get; }
        CardValue Value { get; }
    }
}