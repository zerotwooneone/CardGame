using System.ComponentModel.DataAnnotations;
using CardGame.Application.Common.Interfaces;
using CardGame.Domain.Common;
using CardGame.Domain.Game;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Providers;
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
    private readonly IDeckRegistry _deckRegistry; // Changed from IDeckProvider to IDeckRegistry

    public CreateGameCommandHandler(
        IGameRepository gameRepository,
        IUserRepository userRepository,
        IRandomizer randomizer,
        IDomainEventPublisher domainEventPublisher, // Add publisher dependency
        ILogger<CreateGameCommandHandler> logger,
        IDeckRegistry deckRegistry) // Changed from IDeckProvider to IDeckRegistry
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _randomizer = randomizer ?? throw new ArgumentNullException(nameof(randomizer));
        _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher)); // Assign publisher
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _deckRegistry = deckRegistry ?? throw new ArgumentNullException(nameof(deckRegistry)); // Assign IDeckRegistry
    }

    public async Task<Guid> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        // --- Validation ---
        _logger.LogInformation("Handling CreateGameCommand for Creator {CreatorId} with DeckId {DeckId}", request.CreatorPlayerId, request.DeckId);
        var playerInfosForGame = new List<PlayerInfo>();
        foreach(var playerId in request.PlayerIds)
        {
            var user = await _userRepository.GetUserByIdAsync(playerId).ConfigureAwait(false);
            if (user == null) throw new ValidationException($"Player ID '{playerId}' not found.");
            playerInfosForGame.Add(new PlayerInfo(user.PlayerId, user.Username));
        }
        if (playerInfosForGame.Select(p => p.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != playerInfosForGame.Count)
             throw new InvalidOperationException("Duplicate usernames found for distinct player IDs.");
        // --- End Validation ---

        // --- Resolve Deck --- 
        _logger.LogDebug("Resolving deck provider for DeckId {DeckId}", request.DeckId);
        var deckProvider = _deckRegistry.GetProvider(request.DeckId);
        if (deckProvider == null)
        {
            _logger.LogWarning("Deck provider not found for DeckId {DeckId}", request.DeckId);
            throw new ValidationException($"Deck with ID '{request.DeckId}' not found or not registered.");
        }
        var deckDefinition = deckProvider.GetDeck();
        if (deckDefinition == null || !deckDefinition.Cards.Any())
        {
            _logger.LogWarning("Deck definition is null or empty for DeckId {DeckId} from provider {ProviderType}", request.DeckId, deckProvider.GetType().Name);
            throw new InvalidOperationException($"Deck provider for '{request.DeckId}' returned an invalid or empty deck definition.");
        }
        _logger.LogInformation("Successfully resolved deck provider {ProviderType} for DeckId {DeckId}", deckProvider.GetType().Name, request.DeckId);

        // --- Create Game ---
        _logger.LogDebug("Creating new game aggregate...");
        // Pass playerInfos, creatorPlayerId, the actual cards from deckDefinition, and tokensToWin
        // Also pass request.DeckId as the deckDefinitionId
        var game = Game.CreateNewGame(request.DeckId, playerInfosForGame, request.CreatorPlayerId, deckDefinition.Cards, request.TokensToWin ?? 4, _randomizer);
        // GameCreated event is now in game.DomainEvents

        // --- Start First Round ---
        _logger.LogDebug("Starting first round for game {GameId}...", game.Id);
        // StartNewRound uses the initialDeckCardSet provided during CreateNewGame and the randomizer
        game.StartNewRound(); 
        // RoundStarted, TurnStarted etc. events are now also in game.DomainEvents

        // --- Save Final State ---
        _logger.LogDebug("Saving game state for {GameId}...", game.Id);
        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);
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
            await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken).ConfigureAwait(false);
            _logger.LogTrace("Published domain event {EventType} ({EventId})", domainEvent.GetType().Name, domainEvent.EventId);
        }
        _logger.LogInformation("Finished publishing domain events for game {GameId}.", game.Id);
        // --- End Event Publishing ---


        return game.Id;
    }
}