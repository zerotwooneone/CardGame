using CardGame.Domain.Turn;

namespace CardGame.Application;

public interface IApplicationTurnService
{
    Task<Turn> Play(
        ulong gameId,
        ulong playerId,
        uint cardId,
        PlayParams playParams);
}