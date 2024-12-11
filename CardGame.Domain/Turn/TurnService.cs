using Microsoft.Extensions.Logging;

namespace CardGame.Domain.Turn;

public class TurnService : ITurnService
{
    private readonly ILogger<TurnService> _logger;
    public TurnService(ILogger<TurnService> logger)
    {
        _logger = logger;
    }
    public async Task<Turn> Play(
        ITurnRepository turnRepository, 
        IPlayableCardRepository playableCardRepository,
        GameId gameId, 
        PlayerId playerId, 
        CardId cardId, 
        PlayParams playParams, 
        IInspectNotificationService inspectNotificationService,
        IRoundFactory roundFactory, 
        IShuffleService shuffleService)
    {
        var turn = await turnRepository.GetCurrentTurn(gameId).ConfigureAwait(false);
        if (turn == null)
        {
            throw new Exception($"No turn found for game {gameId}");
        }

        if (turn.CurrentPlayer.Id != playerId)
        {
            throw new Exception($"it's not player {playerId} turn");
        }

        var cardEffect = await playableCardRepository.Get(gameId, cardId).ConfigureAwait(false);
        if (cardEffect == null)
        {
            throw new Exception($"card effect not found {cardId}");
        }
        
        await turn.Play(
            cardEffect,
            playParams, 
            inspectNotificationService).ConfigureAwait(false);
        if (!turn.Round.Complete)
        {
            await turn.NextTurn(roundFactory, shuffleService).ConfigureAwait(false);
        }
        
        await turnRepository.Save(turn).ConfigureAwait(false);
        
        return turn;
    }
}