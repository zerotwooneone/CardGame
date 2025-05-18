using CardGame.Application.DTOs;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using MediatR;

namespace CardGame.Application.Queries;

/// <summary>
/// Handles the GetPlayerGameStateQuery.
/// </summary>
public class GetPlayerGameStateQueryHandler : IRequestHandler<GetPlayerGameStateQuery, PlayerGameStateDto?>
{
    private readonly IGameRepository _gameRepository;

    public GetPlayerGameStateQueryHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
    }

    public async Task<PlayerGameStateDto?> Handle(GetPlayerGameStateQuery request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(request.GameId).ConfigureAwait(false);
        if (game == null) return null;

        // Find the requesting player within the game
        var requestingPlayer = game.Players.FirstOrDefault(p => p.Id == request.RequestingPlayerId);
        if (requestingPlayer == null)
        {
            // This player isn't part of this game, return null or throw?
            // Returning null might be confused with game not found. Maybe throw an exception?
            // For now, return null, controller should handle authorization primarily.
            return null;
        }

        // Filter and map log entries
        var gameLogDtos = game.LogEntries
            .Where(log => !log.IsPrivate || (log.IsPrivate && (log.ActingPlayerId == request.RequestingPlayerId || log.TargetPlayerId == request.RequestingPlayerId)))
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
                IsPrivate = log.IsPrivate,
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
            .ToList();

        // Map to DTO
        var playerStateDto = new PlayerGameStateDto
        {
            GameId = game.Id,
            RoundNumber = game.RoundNumber,
            GamePhase = game.GamePhase.Name,
            CurrentTurnPlayerId = game.CurrentTurnPlayerId,
            TokensNeededToWin = game.TokensNeededToWin,
            DeckCardsRemaining = game.Deck.CardsRemaining,
            DiscardPile = game.DiscardPile.Select(card => new CardDto
            {
                Rank = card.Rank,
                AppearanceId = card.AppearanceId
            }).ToList(),

            // Map player info - show hand count for everyone
            Players = game.Players.Select(player => new PlayerHandInfoDto
            {
                PlayerId = player.Id,
                Name = player.Name,
                Status = player.Status.Value,
                HandCardCount = player.Hand.Count, // Show count for all
                PlayedCardTypes = player.PlayedCards.Select(cardType => cardType.Value).ToList(),
                TokensWon = player.TokensWon,
                IsProtected = player.IsProtected
            }).ToList(),

            // Populate the specific hand for the requesting player
            // Only include hand if player is active (eliminated players have no hand / shouldn't see it)
            PlayerHand = (requestingPlayer.Status == PlayerStatus.Active)
                ? requestingPlayer.Hand.GetCards().Select(card => new CardDto
                {
                    Rank = card.Rank,
                    AppearanceId = card.AppearanceId
                }).ToList()
                : new List<CardDto>(), // Empty list if eliminated
            GameLog = gameLogDtos // Assign the mapped and filtered log entries
        };

        return playerStateDto;
    }
}