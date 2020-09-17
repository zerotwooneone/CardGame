using System;
using CardGame.Utils.Abstractions.Value;

namespace CardGame.Domain.Abstractions.Game
{
    public interface IGameId : IStructValue<Guid>, IEquatable<IGameId>
    {
    }
}