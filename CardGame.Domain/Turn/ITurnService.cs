namespace CardGame.Domain.Turn;

public interface ITurnService
{
    Task<Turn> Play(
        ITurnRepository turnRepository, 
        IPlayEffectRepository playEffectRepository,
        GameId gameId, 
        PlayerId playerId, 
        CardId cardId, 
        PlayParams playParams, 
        IInspectNotificationService inspectNotificationService,
        IRoundFactory roundFactory);
}