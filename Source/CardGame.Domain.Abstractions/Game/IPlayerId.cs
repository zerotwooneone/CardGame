using System;
using CardGame.Utils.Abstractions.Value;

namespace CardGame.Domain.Abstractions.Game
{
    public interface IPlayerId : IStructValue<Guid>, IEquatable<IPlayerId>
    {
    }
}