namespace CardGame.Domain.Turn;

public interface ITurnService
{
    Task<Turn> Play(
        ITurnRepository turnRepository, 
        GameId gameId, 
        PlayerId playerId, 
        CardId cardId, 
        PlayParams playParams, 
        IInspectNotificationService inspectNotificationService,
        IRoundFactory roundFactory, 
        IShuffleService shuffleService);
}