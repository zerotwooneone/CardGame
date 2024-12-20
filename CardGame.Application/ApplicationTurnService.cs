﻿using CardGame.Domain.Turn;
using Microsoft.Extensions.Logging;

namespace CardGame.Application;

public class ApplicationTurnService: IApplicationTurnService
{
    private readonly ILogger<ApplicationTurnService> _logger;
    private readonly TurnService _domainService;
    private readonly ITurnRepository _turnRepository;
    private readonly IInspectNotificationService _inspectNotificationService;
    private readonly IRoundFactory _roundFactory;
    private readonly IShuffleService _shuffleService;

    public ApplicationTurnService(
        ILogger<ApplicationTurnService> logger,
        TurnService domainService,
        ITurnRepository turnRepository, 
        IInspectNotificationService inspectNotificationService,
        IRoundFactory roundFactory, 
        IShuffleService shuffleService)
    {
        _logger = logger;
        _domainService = domainService;
        _turnRepository = turnRepository;
        _inspectNotificationService = inspectNotificationService;
        _roundFactory = roundFactory;
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
            (GameId) gameId,
            (PlayerId) playerId,
            (CardId) cardId,
            playParams,
            _inspectNotificationService,
            _roundFactory,
            _shuffleService).ConfigureAwait(false);
    }
}