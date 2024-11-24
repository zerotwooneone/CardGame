﻿using Microsoft.Extensions.Logging;

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
        IPlayEffectRepository playEffectRepository,
        GameId gameId, 
        PlayerId playerId, 
        CardId cardId, 
        PlayParams playParams, 
        IInspectNotificationService inspectNotificationService,
        IRoundFactory roundFactory, 
        IForcedDiscardEffectRepository forcedDiscardEffectRepository)
    {
        var turn = await turnRepository.GetCurrentTurn(gameId).ConfigureAwait(false);
        if (turn == null)
        {
            throw new Exception($"No turn found for game {gameId}");
        }

        if (turn.Player.Id != playerId)
        {
            throw new Exception($"it's not player {playerId} turn");
        }

        var cardEffect = await playEffectRepository.Get(gameId, cardId, playParams).ConfigureAwait(false);
        if (cardEffect == null)
        {
            throw new Exception($"card effect not found {cardId}");
        }
        
        await turn.Play(cardEffect,forcedDiscardEffectRepository, playParams, inspectNotificationService, roundFactory).ConfigureAwait(false);
        
        await turnRepository.Save(turn).ConfigureAwait(false);
        
        return turn;
    }
}