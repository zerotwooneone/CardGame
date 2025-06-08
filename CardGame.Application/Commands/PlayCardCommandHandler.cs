using CardGame.Domain.Game.GameException;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using CardRank = CardGame.Domain.BaseGame.CardRank;

namespace CardGame.Application.Commands;

/// <summary>
/// Handles the PlayCardCommand. Loads the game, validates the move, executes domain logic,
/// saves the state, and publishes resulting domain events.
/// </summary>
public class PlayCardCommandHandler : IRequestHandler<PlayCardCommand>
{
    private readonly IGameRepository _gameRepository;
    private readonly IDomainEventPublisher _domainEventPublisher; 
    private readonly ILogger<PlayCardCommandHandler> _logger; 
    private readonly IDeckRegistry _deckRegistry;

    public PlayCardCommandHandler(
        IGameRepository gameRepository,
        IDomainEventPublisher domainEventPublisher, 
        ILogger<PlayCardCommandHandler> logger,
        IDeckRegistry deckRegistry)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _domainEventPublisher =
            domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher)); // Assign publisher
        _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Assign logger
        _deckRegistry = deckRegistry ?? throw new ArgumentNullException(nameof(deckRegistry));
    }

    public async Task Handle(PlayCardCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling PlayCardCommand for Game {GameId}, Player {PlayerId}, Card {CardId}",
            request.GameId, request.PlayerId, request.CardToPlayId);

        // 1. Load Game Aggregate
        var game = await _gameRepository.GetByIdAsync(request.GameId).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game not found: {GameId}", request.GameId);
            throw new KeyNotFoundException($"Game with ID {request.GameId} not found.");
        }

        // Resolve IDeckProvider using the game's DeckDefinitionId
        var deckProvider = _deckRegistry.GetProvider(game.DeckDefinitionId);
        if (deckProvider == null)
        {
            _logger.LogError("Could not resolve IDeckProvider for DeckDefinitionId {DeckDefinitionId} in Game {GameId}", game.DeckDefinitionId, request.GameId);
            // Consider throwing a specific exception or handling this scenario appropriately
            throw new InvalidOperationException($"Deck provider not found for deck definition ID {game.DeckDefinitionId}.");
        }

        // --- Fail Fast Validations ---
        _logger.LogDebug("Performing pre-domain logic validations for Game {GameId}...", request.GameId);

        // 2. Check Game Phase
        if (game.GamePhase != GamePhase.RoundInProgress)
        {
            throw new InvalidOperationException(
                $"Cannot play card. Game {request.GameId} is not in progress (Phase: {game.GamePhase.Name}).");
        }

        // 3. Check if it's the requesting player's turn
        if (game.CurrentTurnPlayerId != request.PlayerId)
        {
            throw new InvalidOperationException(
                $"It is not player {request.PlayerId}'s turn in game {request.GameId}. Current turn: {game.CurrentTurnPlayerId}");
        }

        // 4. Check if player exists and get player object
        var player = game.Players.FirstOrDefault(p => p.Id == request.PlayerId);
        if (player == null)
        {
            // This shouldn't happen if CurrentTurnPlayerId is valid, but good check
            throw new InvalidOperationException($"Player {request.PlayerId} not found in game {request.GameId}.");
        }

        // 5. Find the specific card instance in the player's hand using the ID
        var cardToPlayInstance = player.Hand.GetCards().FirstOrDefault(c => c.AppearanceId == request.CardToPlayId);
        if (cardToPlayInstance == null)
        {
            // Throw specific exception if card ID not found in hand
            throw new CardNotFoundInHandException(
                $"Card with ID {request.CardToPlayId} not found in player {request.PlayerId}'s hand.");
        }

        // 6. Check hand size - player must have 2 cards to be able to choose one to play
        if (player.Hand.Count < 2)
        {
            throw new InvalidOperationException(
                $"Player {request.PlayerId} must have 2 cards to play, but has {player.Hand.Count} in game {request.GameId}.");
        }

        // 7. Perform validation that depends on the actual card type being played (e.g., Guard guess)
        if (cardToPlayInstance.Rank == CardRank.Guard)
        {
            if (request.GuessedCardType == null)
                throw new ValidationException("A card type must be guessed when playing a Guard.");
            if (request.GuessedCardType == CardRank.Guard)
                throw new ValidationException("Cannot guess Guard when playing a Guard.");
            if (request.TargetPlayerId == null)
                throw new ValidationException("Target player must be specified when playing a Guard.");
        }

        _logger.LogDebug("Pre-domain logic validations passed for Game {GameId}.", request.GameId);
        // --- End Fail Fast Validations ---


        // 8. Execute the domain logic by calling the aggregate method
        _logger.LogDebug("Executing Game.PlayCard for Game {GameId}...", request.GameId);
        // This call might throw DomainExceptions (InvalidMove, GameRule)
        game.PlayCard(
            request.PlayerId,
            cardToPlayInstance, // Pass the found instance
            request.TargetPlayerId,
            request.GuessedCardType,
            deckProvider
        );
        _logger.LogDebug("Game.PlayCard executed for Game {GameId}. {EventCount} domain events to raise.", request.GameId,
            game.DomainEvents.Count);
        
        // 9. Save the updated game state
        _logger.LogDebug("Saving game state for {GameId}...", request.GameId);
        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Game state saved for {GameId}.", request.GameId);

        var eventsToPublish = game.DomainEvents.ToList(); 
        game.ClearDomainEvents(); 

        _logger.LogDebug("Publishing {EventCount} domain events for game {GameId}...", eventsToPublish.Count,
            request.GameId);
        foreach (var domainEvent in eventsToPublish)
        {
            await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken).ConfigureAwait(false);
            _logger.LogTrace("Published domain event {EventType} ({EventId}) for game {GameId}",
                domainEvent.GetType().Name, domainEvent.EventId, request.GameId);
        }

        if (eventsToPublish.Any())
            _logger.LogInformation("Finished publishing {EventCount} domain events for game {GameId}.",
                eventsToPublish.Count, request.GameId);
    }
}