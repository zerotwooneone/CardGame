using CardGame.Application.DTOs;
using CardGame.Domain.Interfaces;
using MediatR;

namespace CardGame.Application.Queries;

/// <summary>
/// Handles the GetSpectatorGameStateQuery.
/// </summary>
public class GetSpectatorGameStateQueryHandler : IRequestHandler<GetSpectatorGameStateQuery, SpectatorGameStateDto?>
{
    private readonly IGameRepository _gameRepository;

    // Constructor injection for dependencies
    public GetSpectatorGameStateQueryHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
    }

    public async Task<SpectatorGameStateDto?> Handle(GetSpectatorGameStateQuery request, CancellationToken cancellationToken)
    {
        // 1. Load the Game aggregate
        var game = await _gameRepository.GetByIdAsync(request.GameId).ConfigureAwait(false); // Assuming repository method exists

        if (game == null)
        {
            return null; // Game not found
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
                Type = card.Type.Value, 
                Id = card.Id
            }).ToList(),
            Players = game.Players.Select(player => new SpectatorPlayerDto
            {
                PlayerId = player.Id,
                Name = player.Name,
                Status = player.Status.Value, 
                HandCardCount = player.Hand.Count, // Only the count
                PlayedCardTypes = player.PlayedCards.Select(cardType => cardType.Value).ToList(), // Only the types played
                TokensWon = player.TokensWon,
                IsProtected = player.IsProtected
            }).ToList(),
            LogEntries = game.LogEntries
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
                    RevealedCardId = log.RevealedCardId,
                    RevealedCardType = log.RevealedCardType,
                    IsPrivate = log.IsPrivate, // Will be false here
                    Message = log.Message
                })
                .OrderByDescending(log => log.Timestamp) // Ensure logs are newest first
                .ToList()
        };

        return spectatorDto;
    }
}