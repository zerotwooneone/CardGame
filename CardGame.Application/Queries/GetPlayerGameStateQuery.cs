using CardGame.Application.DTOs;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using MediatR;

namespace CardGame.Application.Queries;

/// <summary>
    /// Query to retrieve the game state from a specific player's perspective.
    /// </summary>
    /// <param name="GameId">The ID of the game.</param>
    /// <param name="RequestingPlayerId">The ID of the player requesting their state.</param>
    public record GetPlayerGameStateQuery(Guid GameId, Guid RequestingPlayerId) : IRequest<PlayerGameStateDto?>;

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
                    Type = card.Type.Value,
                    Id = card.Id
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
                                    Type = card.Type.Value,
                                    Id = card.Id
                                }).ToList()
                             : new List<CardDto>() // Empty list if eliminated
            };

            return playerStateDto;
        }
    }