using System.ComponentModel.DataAnnotations;
using CardGame.Application.Common.Interfaces;
using CardGame.Domain.Common;
using CardGame.Domain.Game;
using CardGame.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.Commands;

/// <summary>
/// Handles the CreateGameCommand. Creates the game, immediately starts the first round,
/// persists the state, and publishes domain events.
/// </summary>
public class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, Guid>
{
    private readonly IGameRepository _gameRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRandomizer _randomizer;
    private readonly IDomainEventPublisher _domainEventPublisher; // Inject publisher
    private readonly ILogger<CreateGameCommandHandler> _logger; // Inject logger

    public CreateGameCommandHandler(
        IGameRepository gameRepository,
        IUserRepository userRepository,
        IRandomizer randomizer,
        IDomainEventPublisher domainEventPublisher, // Add publisher dependency
        ILogger<CreateGameCommandHandler> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _randomizer = randomizer ?? throw new ArgumentNullException(nameof(randomizer));
        _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher)); // Assign publisher
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        // --- Validation ---
        _logger.LogInformation("Handling CreateGameCommand for Creator {CreatorId}", request.CreatorPlayerId);
        var playerInfosForGame = new List<PlayerInfo>();
        foreach(var playerId in request.PlayerIds)
        {
            var user = await _userRepository.GetUserByIdAsync(playerId);
            if (user == null) throw new ValidationException($"Player ID '{playerId}' not found.");
            playerInfosForGame.Add(new PlayerInfo(user.PlayerId, user.Username));
        }
        if (playerInfosForGame.Select(p => p.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != playerInfosForGame.Count)
             throw new InvalidOperationException("Duplicate usernames found for distinct player IDs.");
        // --- End Validation ---


        // --- Create Game ---
        _logger.LogDebug("Creating new game aggregate...");
        var game = Game.CreateNewGame(playerInfosForGame, request.TokensToWin ?? 4);
        // GameCreated event is now in game.DomainEvents

        // --- Start First Round ---
        _logger.LogDebug("Starting first round for game {GameId}...", game.Id);
        game.StartNewRound(_randomizer);
        // RoundStarted, TurnStarted etc. events are now also in game.DomainEvents

        // --- Save Final State ---
        _logger.LogDebug("Saving game state for {GameId}...", game.Id);
        await _gameRepository.SaveAsync(game, cancellationToken);
        _logger.LogInformation("Game {GameId} created and saved.", game.Id);

        // --- Publish Domain Events ---
        // Retrieve events collected within the aggregate during CreateNewGame and StartNewRound
        var eventsToPublish = game.DomainEvents.ToList(); // Copy the list
        game.ClearDomainEvents(); // Clear events from the aggregate

        _logger.LogDebug("Publishing {EventCount} domain events for game {GameId}...", eventsToPublish.Count, game.Id);
        foreach (var domainEvent in eventsToPublish)
        {
            // Use the injected publisher. Its implementation (e.g., MediatRDomainEventPublisher)
            // will handle wrapping the event (e.g., GameCreated) in DomainEventNotification<GameCreated>
            // before sending it via MediatR.
            await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
            _logger.LogTrace("Published domain event {EventType} ({EventId})", domainEvent.GetType().Name, domainEvent.EventId);
        }
        _logger.LogInformation("Finished publishing domain events for game {GameId}.", game.Id);
        // --- End Event Publishing ---


        return game.Id;
    }
}