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
                    RevealedCardAppearanceId = log.RevealedCardAppearanceId, 
                    RevealedCardValue = log.RevealedCardType?.Value,
                    IsPrivate = log.IsPrivate, // Will be false here
                    Message = log.Message,

                    PlayedCardAppearanceId = log.PlayedCardAppearanceId, 
                    PlayedCardValue = log.PlayedCardType?.Value,

                    GuessedCardAppearanceId = log.GuessedCardAppearanceId, 
                    GuessedCardValue = log.GuessedCardType?.Value,
                    WasGuessCorrect = log.WasGuessCorrect,

                    Player1ComparedCardAppearanceId = log.Player1ComparedCardAppearanceId, 
                    Player1ComparedCardValue = log.Player1ComparedCardType?.Value,
                    Player2ComparedCardAppearanceId = log.Player2ComparedCardAppearanceId, 
                    Player2ComparedCardValue = log.Player2ComparedCardType?.Value,
                    BaronLoserPlayerId = log.BaronLoserPlayerId,

                    DiscardedByPrinceCardAppearanceId = log.DiscardedByPrinceCardAppearanceId, 
                    DiscardedByPrinceCardValue = log.DiscardedByPrinceCardType?.Value,

                    CardResponsibleForEliminationAppearanceId = log.CardResponsibleForEliminationAppearanceId, 
                    CardResponsibleForEliminationValue = log.CardResponsibleForElimination?.Value,

                    FizzleReason = log.FizzleReason,

                    WinnerPlayerId = log.WinnerPlayerId,
                    RoundEndReason = log.RoundEndReason,
                    RoundPlayerSummaries = log.RoundPlayerSummaries?.Select(s => new RoundEndPlayerSummaryDto
                    {
                        PlayerId = s.PlayerId,
                        PlayerName = s.PlayerName,
                        CardsHeld = s.CardsHeld.Select(c => new CardDto { Rank = c.Rank, AppearanceId = c.AppearanceId }).ToList(),
                        TokensWon = s.Score 
                    }).ToList(), 
                    TokensHeld = log.TokensHeld,
                    
                    CardDrawnAppearanceId = log.CardDrawnAppearanceId, 
                    CardDrawnValue = log.CardDrawnType?.Value
                })
                .OrderByDescending(log => log.Timestamp) // Ensure logs are newest first
                .ToList()
        };

        return spectatorDto;
    }
}