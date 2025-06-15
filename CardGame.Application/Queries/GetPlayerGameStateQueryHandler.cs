using CardGame.Application.DTOs;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Game;
using CardGame.Domain.Types;
using MediatR;

namespace CardGame.Application.Queries;

/// <summary>
/// Handles the GetPlayerGameStateQuery.
/// </summary>
public class GetPlayerGameStateQueryHandler : IRequestHandler<GetPlayerGameStateQuery, PlayerGameStateDto?>
{
    private readonly IGameRepository _gameRepository;
    private readonly IDeckRegistry _deckRegistry;

    public GetPlayerGameStateQueryHandler(IGameRepository gameRepository, IDeckRegistry deckRegistry)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _deckRegistry = deckRegistry ?? throw new ArgumentNullException(nameof(deckRegistry));
    }

    public async Task<PlayerGameStateDto?> Handle(GetPlayerGameStateQuery request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(request.GameId).ConfigureAwait(false);
        if (game == null) return null;

        // Find the requesting player within the game
        var requestingPlayer = game.Players.FirstOrDefault(p => p.Id == request.RequestingPlayerId);
        if (requestingPlayer == null)
        {
            return null;
        }

        // Load DeckDefinition (similar to GetSpectatorGameStateQueryHandler)
        DeckDefinition? deckDefinition = null;
        var deckProvider = _deckRegistry.GetProvider(game.DeckDefinitionId);
        if (deckProvider != null)
        {
            deckDefinition = deckProvider.GetDeck();
        }

        // Filter and map log entries
        var gameLogDtos = game.LogEntries
            .Where(log => !log.IsPrivate || log.ActingPlayerId == requestingPlayer.Id || log.TargetPlayerId == requestingPlayer.Id)
            .Select(log => new GameLogEntryDto
            {
                Id = log.Id,
                Timestamp = log.Timestamp,
                EventType = log.EventType,
                ActingPlayerId = log.ActingPlayerId,
                ActingPlayerName = log.ActingPlayerName,
                TargetPlayerId = log.TargetPlayerId,
                TargetPlayerName = log.TargetPlayerName,
                IsPrivate = log.IsPrivate && !(log.ActingPlayerId == requestingPlayer.Id || log.TargetPlayerId == requestingPlayer.Id), // Adjust IsPrivate based on player involvement
                Message = log.Message,

                // --- Map from Domain.GameLogEntry.Card to Application.DTOs.CardDto ---
                PlayedCard = log.PlayedCard != null ? new CardDto { RankValue =log.PlayedCard.Rank, AppearanceId = log.PlayedCard.AppearanceId } : null,
                DrawnCard = log.DrawnCard != null ? new CardDto { RankValue = log.DrawnCard.Rank, AppearanceId = log.DrawnCard.AppearanceId } : null,
                DiscardedCard = log.DiscardedCard != null ? new CardDto { RankValue = log.DiscardedCard.Rank, AppearanceId = log.DiscardedCard.AppearanceId } : null,
                RevealedPlayerCard = log.RevealedPlayerCard != null ? new CardDto { RankValue = log.RevealedPlayerCard.Rank, AppearanceId = log.RevealedPlayerCard.AppearanceId } : null,
                ActingPlayerBaronCard = log.ActingPlayerBaronCard != null ? new CardDto { RankValue = log.ActingPlayerBaronCard.Rank, AppearanceId = log.ActingPlayerBaronCard.AppearanceId } : null,
                TargetPlayerBaronCard = log.TargetPlayerBaronCard != null ? new CardDto { RankValue = log.TargetPlayerBaronCard.Rank, AppearanceId = log.TargetPlayerBaronCard.AppearanceId } : null,
                TargetDiscardedCard = log.TargetDiscardedCard != null ? new CardDto { RankValue = log.TargetDiscardedCard.Rank, AppearanceId = log.TargetDiscardedCard.AppearanceId } : null,
                TargetNewCardAfterPrince = log.TargetNewCardAfterPrince != null ? new CardDto { RankValue = log.TargetNewCardAfterPrince.Rank, AppearanceId = log.TargetNewCardAfterPrince.AppearanceId } : null,
                RevealedTradedCard = log.RevealedTradedCard != null ? new CardDto { RankValue = log.RevealedTradedCard.Rank, AppearanceId = log.RevealedTradedCard.AppearanceId } : null,
                RevealedCardOnElimination = log.RevealedCardOnElimination != null ? new CardDto { RankValue = log.RevealedCardOnElimination.Rank, AppearanceId = log.RevealedCardOnElimination.AppearanceId } : null,
                GuessedPlayerActualCard = log.GuessedPlayerActualCard != null ? new CardDto { RankValue = log.GuessedPlayerActualCard.Rank, AppearanceId = log.GuessedPlayerActualCard.AppearanceId } : null,

                // Guard specific
                GuessedRank = log.GuessedRank,
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
                    CardsHeld = s.CardsHeld.Select(c => new CardDto { RankValue = c.Rank, AppearanceId = c.AppearanceId }).ToList(), // Corrected: c.Type.Value
                    TokensWon = s.Score
                }).ToList(),
                TokensHeld = log.TokensHeld
            })
            .OrderByDescending(log => log.Timestamp) // Ensure logs are newest first
            .ToList();

        // Map to DTO
        var playerStateDto = new PlayerGameStateDto
        {
            GameId = game.Id,
            DeckDefinitionId = game.DeckDefinitionId, // Populate DeckDefinitionId
            RoundNumber = game.RoundNumber,
            GamePhase = game.GamePhase.Value,
            CurrentTurnPlayerId = game.CurrentTurnPlayerId,
            TokensNeededToWin = game.TokensNeededToWin,
            DeckCardsRemaining = game.Deck.CardsRemaining,

            // Map player info - show hand count for everyone
            Players = game.Players.Select(player => new PlayerHandInfoDto
            {
                PlayerId = player.Id,
                Name = player.Name,
                Status = player.Status.Value, // Corrected: player.Status.Value
                HandCardCount = player.Hand.GetCards().Count(), // Corrected: Added () to Count
                PlayedCards = player.PlayedCards.Select(playedCard => 
                {
                    return new CardDto
                    {
                        RankValue = playedCard.Rank, 
                        AppearanceId = playedCard.AppearanceId
                    };
                }).ToList(),
                TokensWon = player.TokensWon, // Corrected: player.TokensWon
                IsProtected = player.IsProtected
            }).ToList(),

            // Populate the specific hand for the requesting player
            // Only include hand if player is active (eliminated players have no hand / shouldn't see it)
            PlayerHand = (requestingPlayer.Status == PlayerStatus.Active)
                ? requestingPlayer.Hand.GetCards().Select(card => new CardDto
                {
                    RankValue = card.Rank,
                    AppearanceId = card.AppearanceId
                }).ToList()
                : new List<CardDto>(), // Empty list if eliminated
            GameLog = gameLogDtos // Assign the mapped and filtered log entries
        };

        return playerStateDto;
    }
}