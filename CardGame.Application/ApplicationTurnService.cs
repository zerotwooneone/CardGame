﻿using CardGame.Domain.Turn;
using Microsoft.Extensions.Logging;

namespace CardGame.Application;

public class ApplicationTurnService: IApplicationTurnService
{
    private readonly ILogger<ApplicationTurnService> _logger;
    private readonly TurnService _domainService;
    private readonly ITurnRepository _turnRepository;
    private readonly IPlayEffectRepository _playEffectRepository;
    private readonly IInspectNotificationService _inspectNotificationService;
    private readonly IRoundFactory _roundFactory;
    private readonly IForcedDiscardEffectRepository _forcedDiscardEffectRepository;
    private readonly IShuffleService _shuffleService;

    public ApplicationTurnService(
        ILogger<ApplicationTurnService> logger,
        TurnService domainService,
        ITurnRepository turnRepository, 
        IPlayEffectRepository playEffectRepository, 
        IInspectNotificationService inspectNotificationService,
        IRoundFactory roundFactory, 
        IForcedDiscardEffectRepository forcedDiscardEffectRepository,
        IShuffleService shuffleService)
    {
        _logger = logger;
        _domainService = domainService;
        _turnRepository = turnRepository;
        _playEffectRepository = playEffectRepository;
        _inspectNotificationService = inspectNotificationService;
        _roundFactory = roundFactory;
        _forcedDiscardEffectRepository = forcedDiscardEffectRepository;
        _shuffleService = shuffleService;
    }

    public async Task<Turn> Play(
        ulong gameId, 
        ulong playerId, 
        uint cardId, 
        PlayParams playParams)
    {
        return await _domainService.Play(
            _turnRepository,
            _playEffectRepository,
            (GameId) gameId,
            (PlayerId) playerId,
            (CardId) cardId,
            playParams,
            _inspectNotificationService,
            _roundFactory,
            _forcedDiscardEffectRepository,
            _shuffleService).ConfigureAwait(false);
    }
}