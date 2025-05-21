using CardGame.Application.DTOs;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Game;
using MediatR;

namespace CardGame.Application.Queries;

/// <summary>
/// Handles the GetSpectatorGameStateQuery.
/// </summary>
public class GetSpectatorGameStateQueryHandler : IRequestHandler<GetSpectatorGameStateQuery, SpectatorGameStateDto?>
{
    private readonly IGameRepository _gameRepository;
    private readonly IDeckRegistry _deckRegistry; 

    // Constructor injection for dependencies
    public GetSpectatorGameStateQueryHandler(IGameRepository gameRepository, IDeckRegistry deckRegistry) 
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _deckRegistry = deckRegistry ?? throw new ArgumentNullException(nameof(deckRegistry)); 
    }

    public async Task<SpectatorGameStateDto?> Handle(GetSpectatorGameStateQuery request, CancellationToken cancellationToken)
    {
        // 1. Load the Game aggregate
        var game = await _gameRepository.GetByIdAsync(request.GameId).ConfigureAwait(false); // Assuming repository method exists

        if (game == null)
        {
            return null; // Game not found
        }

        // 1.5 Load DeckDefinition
        DeckDefinition? deckDefinition = null; // Initialize as nullable
        var deckProvider = _deckRegistry.GetProvider(game.DeckDefinitionId); // Get the provider
        if (deckProvider != null)
        {
            deckDefinition = deckProvider.GetDeck(); // Get the DeckDefinition from the provider
        }
        
        if (deckDefinition == null)
        {
            // Handle case where deck definition is not found - perhaps log and return error or partial DTO
            // For now, we'll proceed, but played card appearances might be missing or default
            // Consider throwing an exception or returning a more specific error DTO if this is critical
        }

        var spectatorDto = new SpectatorGameStateDto
        {
            GameId = game.Id,
            RoundNumber = game.RoundNumber,
            GamePhase = game.GamePhase.Value, 
            CurrentTurnPlayerId = game.CurrentTurnPlayerId,
            TokensNeededToWin = game.TokensNeededToWin,
            DeckCardsRemaining = game.Deck.CardsRemaining,
            DiscardPile = game.DiscardPile.Select(card => new CardDto
            {
                Rank = card.Type.Value, 
                AppearanceId = card.AppearanceId
            }).ToList(),
            Players = game.Players.Select(player => new SpectatorPlayerDto
            {
                PlayerId = player.Id,
                Name = player.Name,
                Status = player.Status.Value, 
                HandCardCount = player.Hand.Count, // Only the count
                PlayedCards = player.PlayedCards.Select(cardType => 
                {
                    var cardDef = deckDefinition?.Cards.FirstOrDefault(cd => cd.Type == cardType);
                    return new CardDto
                    {
                        Rank = cardType.Value,
                        AppearanceId = cardDef?.AppearanceId ?? string.Empty // Use string.Empty if not found
                    };
                }).ToList(),
                TokensWon = player.TokensWon,
                IsProtected = player.IsProtected
            }).ToList(),
            GameLog = game.LogEntries
                .Where(log => !log.IsPrivate) // Filter out private logs for spectator view
                .Select(log => new GameLogEntryDto
                {
                    Id = log.Id,
                    Timestamp = log.Timestamp,
                    EventType = log.EventType,
                    ActingPlayerId = log.ActingPlayerId,
                    ActingPlayerName = log.ActingPlayerName,
                    TargetPlayerId = log.TargetPlayerId,
                    TargetPlayerName = log.TargetPlayerName,
                    IsPrivate = log.IsPrivate, // Will be false here
                    Message = log.Message,

                    // --- Map from Domain.GameLogEntry.Card to Application.DTOs.CardDto ---
                    PlayedCard = log.PlayedCard != null ? new CardDto { Rank = log.PlayedCard.Type.Value, AppearanceId = log.PlayedCard.AppearanceId } : null,
                    DrawnCard = log.DrawnCard != null ? new CardDto { Rank = log.DrawnCard.Type.Value, AppearanceId = log.DrawnCard.AppearanceId } : null,
                    DiscardedCard = log.DiscardedCard != null ? new CardDto { Rank = log.DiscardedCard.Type.Value, AppearanceId = log.DiscardedCard.AppearanceId } : null,
                    RevealedPlayerCard = log.RevealedPlayerCard != null ? new CardDto { Rank = log.RevealedPlayerCard.Type.Value, AppearanceId = log.RevealedPlayerCard.AppearanceId } : null,
                    ActingPlayerBaronCard = log.ActingPlayerBaronCard != null ? new CardDto { Rank = log.ActingPlayerBaronCard.Type.Value, AppearanceId = log.ActingPlayerBaronCard.AppearanceId } : null,
                    TargetPlayerBaronCard = log.TargetPlayerBaronCard != null ? new CardDto { Rank = log.TargetPlayerBaronCard.Type.Value, AppearanceId = log.TargetPlayerBaronCard.AppearanceId } : null,
                    TargetDiscardedCard = log.TargetDiscardedCard != null ? new CardDto { Rank = log.TargetDiscardedCard.Type.Value, AppearanceId = log.TargetDiscardedCard.AppearanceId } : null,
                    TargetNewCardAfterPrince = log.TargetNewCardAfterPrince != null ? new CardDto { Rank = log.TargetNewCardAfterPrince.Type.Value, AppearanceId = log.TargetNewCardAfterPrince.AppearanceId } : null,
                    RevealedTradedCard = log.RevealedTradedCard != null ? new CardDto { Rank = log.RevealedTradedCard.Type.Value, AppearanceId = log.RevealedTradedCard.AppearanceId } : null,
                    RevealedCardOnElimination = log.RevealedCardOnElimination != null ? new CardDto { Rank = log.RevealedCardOnElimination.Type.Value, AppearanceId = log.RevealedCardOnElimination.AppearanceId } : null,
                    GuessedPlayerActualCard = log.GuessedPlayerActualCard != null ? new CardDto { Rank = log.GuessedPlayerActualCard.Type.Value, AppearanceId = log.GuessedPlayerActualCard.AppearanceId } : null,
                    
                    // Guard specific
                    GuessedRank = log.GuessedRank?.Value,
                    WasGuessCorrect = log.WasGuessCorrect,

                    // Baron specific
                    BaronLoserPlayerId = log.BaronLoserPlayerId,

                    // Fizzle reason
                    FizzleReason = log.FizzleReason,

                    // Round/Game End
                    WinnerPlayerId = log.WinnerPlayerId,
                    WinnerPlayerName = log.WinnerPlayerName, // Ensure this is mapped from domain log if it exists
                    RoundEndReason = log.RoundEndReason,
                    RoundPlayerSummaries = log.RoundPlayerSummaries?.Select(s => new RoundEndPlayerSummaryDto
                    {
                        PlayerId = s.PlayerId,
                        PlayerName = s.PlayerName,
                        CardsHeld = s.CardsHeld.Select(c => new CardDto { Rank = c.Type.Value, AppearanceId = c.AppearanceId }).ToList(), // Corrected: c.Type.Value
                        TokensWon = s.Score 
                    }).ToList(), 
                    TokensHeld = log.TokensHeld
                })
                .OrderByDescending(log => log.Timestamp) // Ensure logs are newest first
                .ToList()
        };

        return spectatorDto;
    }
}