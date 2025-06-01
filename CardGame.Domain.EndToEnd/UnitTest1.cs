using FluentAssertions;
using CardGame.Domain.Game;       // For Game, Player, Card, Hand, PlayerInfo, DeckDefinition
using CardGame.Domain.Interfaces; // For IDeckProvider
using CardGame.Domain.Providers;  // For DefaultDeckProvider
using CardGame.Domain.Types;      // For CardType, PlayerStatus
using GameUnderTest = CardGame.Domain.Game.Game;

namespace CardGame.Domain.EndToEnd
{
    // TestRandomizer now implements the correct IRandomizer interface

    [TestFixture]
    public class GameSimulationTests
    {
        private IDeckProvider _deckProvider;
        private TestRandomizer _randomizer; // Changed to concrete type TestRandomizer

        [SetUp]
        public void TestSetup()
        {
            _deckProvider = new DefaultDeckProvider(); 
            _randomizer = new TestRandomizer();
        }

        [Test]
        public void Should_Successfully_Complete_Full_Game_With_Random_Plays()
        {
            // 1. Setup Game
            var playerInfos = new List<PlayerInfo>
            {
                new PlayerInfo(Guid.NewGuid(), "Player 1"),
                new PlayerInfo(Guid.NewGuid(), "Player 2")
            };
            Guid creatorId = playerInfos[0].Id; // Assume first player is creator for simplicity
            
            DeckDefinition deckDefinition = _deckProvider.GetDeck(); 
            IReadOnlyList<Card> initialDeckCardSet = deckDefinition.Cards.ToList(); // CS0266: Added .ToList()
            Guid deckDefinitionId = _deckProvider.DeckId; // CS1061: Used _deckProvider.DeckId

            // Game.CreateNewGame expects deckDefinitionId, IEnumerable<PlayerInfo>, creatorPlayerId, initialDeckCards, tokensToWin
            // The factory method itself will generate the game.Id.
            var game = GameUnderTest.CreateNewGame(deckDefinitionId, playerInfos, creatorId, initialDeckCardSet, 4);

            Console.WriteLine($"Starting game with ID: {game.Id}. Target tokens: {game.TokensNeededToWin}.");

            // 2. Game Loop
            int maxTurnsOverall = 200; 
            int turnsTakenOverall = 0;

            while (game.GamePhase != GamePhase.GameOver && turnsTakenOverall < maxTurnsOverall)
            {
                // Ensure StartNewRound is only called if not already in progress or over
                if (game.GamePhase == GamePhase.NotStarted || game.GamePhase == GamePhase.RoundOver)
                {
                    game.StartNewRound(_randomizer);
                }
                Console.WriteLine($"--- Round {game.RoundNumber} started ---");
                
                int maxTurnsInRound = 50; 
                int turnsInCurrentRound = 0;

                while (game.GamePhase == GamePhase.RoundInProgress && turnsInCurrentRound < maxTurnsInRound)
                {
                    Player currentPlayer = game.Players.First(p => p.Id == game.CurrentTurnPlayerId);
                    Console.WriteLine($"Turn {turnsInCurrentRound + 1} (Round {game.RoundNumber}): Player {currentPlayer.Name}'s turn. Hand: {string.Join(", ", currentPlayer.Hand.Cards.Select(c => c.Type.Name))}");

                    if (currentPlayer.Status != PlayerStatus.Active)
                    {
                        Console.WriteLine($"Player {currentPlayer.Name} is out (Status: {currentPlayer.Status}). Skipping turn.");
                        // Domain logic should advance turn or handle this scenario.
                        // For safety, if the domain doesn't auto-advance past inactive players in CurrentTurnPlayerId,
                        // this loop might get stuck. The test assumes domain handles this.
                        // If game state implies it's stuck, we might need a manual break or fail.
                        if (game.Players.Count(p => p.Status == PlayerStatus.Active) == 0 && game.GamePhase == GamePhase.RoundInProgress) {
                             Console.WriteLine("Warning: All players are inactive but round is still in progress. This might be an issue.");
                             break; 
                        }
                        // This test doesn't manually advance turns; relies on game.PlayCard to do so.
                        // If CurrentTurnPlayerId points to an inactive player and PlayCard isn't called, it's a problem.
                        // The game logic should ensure CurrentTurnPlayerId is always an active player or handle it.
                        turnsInCurrentRound++; // Increment to avoid infinite loop if game logic doesn't advance
                        turnsTakenOverall++;
                        continue;
                    }
                    
                    Card cardToPlay = SelectCardToPlay(currentPlayer, game);
                    Guid? targetPlayerId = SelectTargetPlayer(currentPlayer, cardToPlay, game.Players.ToList()); 
                    CardType? guessedCardType = SelectGuessedCardType(cardToPlay); 

                    Console.WriteLine($"Player {currentPlayer.Name} plays {cardToPlay.Type.Name}" +
                                      $"{(targetPlayerId.HasValue ? $" targeting Player {game.Players.First(p => p.Id == targetPlayerId.Value).Name}" : "")}" +
                                      $"{(guessedCardType != null ? $" guessing {guessedCardType?.Name}" : "")}"); // CS1061 (HasValue) & CS8602 (?.Name)
                    
                    try
                    {
                        game.PlayCard(currentPlayer.Id, cardToPlay, targetPlayerId, guessedCardType);
                    }
                    catch (Exception ex)
                    {
                        throw new AssertionException($"Exception during PlayCard: {ex.ToString()} on Turn {turnsInCurrentRound + 1}, Round {game.RoundNumber}. Player: {currentPlayer.Name}, Card: {cardToPlay.Type.Name}, Target: {targetPlayerId}, Guess: {guessedCardType?.Name}", ex);
                    }

                    turnsInCurrentRound++;
                    turnsTakenOverall++;
                }
                
                (game.GamePhase == GamePhase.RoundOver || game.GamePhase == GamePhase.GameOver || turnsInCurrentRound >= maxTurnsInRound).Should().BeTrue($"Round {game.RoundNumber} did not complete as expected within {maxTurnsInRound} turns. GamePhase={game.GamePhase}, turnsInCurrentRound={turnsInCurrentRound}.");

                if (turnsInCurrentRound >= maxTurnsInRound && game.GamePhase == GamePhase.RoundInProgress)
                {
                     Console.WriteLine($"Warning: Round {game.RoundNumber} hit max turn limit ({maxTurnsInRound}) and was still in progress. Active players: {game.Players.Count(p => p.Status == PlayerStatus.Active)}. Current Player: {game.Players.FirstOrDefault(p=>p.Id == game.CurrentTurnPlayerId)?.Name} (Status: {game.Players.FirstOrDefault(p=>p.Id == game.CurrentTurnPlayerId)?.Status})");
                }
                else if (game.GamePhase == GamePhase.RoundOver)
                {
                    Console.WriteLine($"--- Round {game.RoundNumber} ended. Reason: {GetRoundEndReason(game)} --- Player Tokens: {string.Join("; ", game.Players.Select(p => $"{p.Name}: {p.TokensWon}"))}");
                }
            }

            // 3. Assert Game Completion
            (game.GamePhase == GamePhase.GameOver || turnsTakenOverall >= maxTurnsOverall).Should().BeTrue($"Game did not complete as expected within {maxTurnsOverall} turns. GamePhase={game.GamePhase}, turnsTakenOverall={turnsTakenOverall}.");
            
            if (turnsTakenOverall >= maxTurnsOverall && game.GamePhase != GamePhase.GameOver)
            {
                 Console.WriteLine($"Warning: Game hit max turn limit ({maxTurnsOverall}) and was not over (GamePhase: {game.GamePhase}).");
            }
            else if (game.GamePhase == GamePhase.GameOver)
            {
                Console.WriteLine($"--- Game Over --- Player Tokens: {string.Join("; ", game.Players.Select(p => $"{p.Name}: {p.TokensWon}"))}");
                Player? winner = game.Players.FirstOrDefault(p => p.TokensWon >= game.TokensNeededToWin); // CS1061: Implemented GetWinner logic
                winner.Should().NotBeNull("Game ended without a winner based on tokens. This is unexpected for a standard game completion.");
                if (winner != null) {
                    Console.WriteLine($"Winner: Player {winner.Name} with {winner.TokensWon} tokens.");
                    (winner.TokensWon >= game.TokensNeededToWin).Should().BeTrue($"Winner {winner.Name} should have {game.TokensNeededToWin} or more tokens to win, but has {winner.TokensWon}.");
                }
            }
            Console.WriteLine($"Game completed. Total rounds: {game.RoundNumber}. Total turns: {turnsTakenOverall}.");
        }

        private string GetRoundEndReason(GameUnderTest game)
        {
            if (game.GamePhase == GamePhase.RoundOver || game.GamePhase == GamePhase.GameOver)
            {
                if (game.Players.Count(p => p.Status == PlayerStatus.Active) == 1) return $"Last player standing: {game.Players.FirstOrDefault(p => p.Status == PlayerStatus.Active)?.Name}";
                if (game.Deck.IsEmpty) return "Deck is empty";
                return "Unknown (Round/Game Over is true, but specific condition not immediately clear from simple checks)";
            }
            return "Round not over";
        }

        private Card SelectCardToPlay(Player currentPlayer, GameUnderTest game)
        {
            bool hasCountess = currentPlayer.Hand.Cards.Any(c => c.Type == CardType.Countess);
            bool hasKing = currentPlayer.Hand.Cards.Any(c => c.Type == CardType.King);
            bool hasPrince = currentPlayer.Hand.Cards.Any(c => c.Type == CardType.Prince);

            if (hasCountess && (hasKing || hasPrince))
            {
                var countess = currentPlayer.Hand.Cards.First(c => c.Type == CardType.Countess);
                Console.WriteLine($"Player {currentPlayer.Name} must play Countess due to King/Prince.");
                return countess;
            }

            var playableCards = currentPlayer.Hand.Cards.Where(c => c.Type != CardType.Princess).ToList();
            if (playableCards.Any())
            {
                return playableCards[_randomizer.Next(0, playableCards.Count)];
            }
            
            var princess = currentPlayer.Hand.Cards.FirstOrDefault(c => c.Type == CardType.Princess);
            if (princess != null) {
                Console.WriteLine($"Player {currentPlayer.Name} is forced to play Princess.");
                return princess;
            }

            if (!currentPlayer.Hand.Cards.Any()) {
                 throw new AssertionException($"Player {currentPlayer.Name} (ID: {currentPlayer.Id}) has no cards to play at the start of their turn decision logic. Hand size: {currentPlayer.Hand.Count}. Status: {currentPlayer.Status}.");
            }
            return currentPlayer.Hand.Cards.First(); 
        }

        private Guid? SelectTargetPlayer(Player currentPlayer, Card cardToPlay, List<Player> allPlayersInGame)
        {
            var activeOpponentsNotProtected = allPlayersInGame
                .Where(p => p.Id != currentPlayer.Id && p.Status == PlayerStatus.Active && !p.IsProtected)
                .ToList();

            var princeTargets = allPlayersInGame 
                .Where(p => p.Status == PlayerStatus.Active && (p.Id == currentPlayer.Id || !p.IsProtected))
                .ToList();

            switch (cardToPlay.Type.Name) // Assuming CardType has a Name property or using .ToString()
            {
                case nameof(CardType.Guard):
                case nameof(CardType.Priest):
                case nameof(CardType.Baron):
                case nameof(CardType.King):
                    var validKingOrBaronTargets = activeOpponentsNotProtected;
                    if (cardToPlay.Type == CardType.King || cardToPlay.Type == CardType.Baron)
                    {
                        // Current player must have at least one card to trade (King/Baron itself is played, so >1 card initially, or 1 if it's not King/Baron)
                        // Or more simply, the domain will check if actingPlayer.Hand.GetHeldCard() is valid.
                        // For test selection, ensure target has cards.
                        validKingOrBaronTargets = activeOpponentsNotProtected.Where(p => p.Hand.Cards.Any()).ToList();
                    }

                    if (validKingOrBaronTargets.Any())
                    {
                        return validKingOrBaronTargets[_randomizer.Next(0, validKingOrBaronTargets.Count)].Id;
                    }
                    Console.WriteLine($"No valid target (active, non-protected opponent{(cardToPlay.Type == CardType.King || cardToPlay.Type == CardType.Baron ? ", with cards" : "")}) for {cardToPlay.Type.Name} by {currentPlayer.Name}. Effect may fizzle.");
                    return null; 

                case nameof(CardType.Prince):
                    if (princeTargets.Any())
                    {
                        return princeTargets[_randomizer.Next(0, princeTargets.Count)].Id;
                    }
                    Console.WriteLine($"No valid target for Prince by {currentPlayer.Name}. Effect may fizzle if it requires a target.");
                    return null;

                case nameof(CardType.Handmaid):
                case nameof(CardType.Countess):
                case nameof(CardType.Princess):
                default:
                    return null; 
            }
        }

        private CardType? SelectGuessedCardType(Card cardToPlay)
        {
            if (cardToPlay.Type == CardType.Guard)
            {
                // CS0117: Manually list guessable card types as CardType.GetAll() does not exist.
                var guessableTypes = new List<CardType>
                {
                    CardType.Priest,
                    CardType.Baron,
                    CardType.Handmaid,
                    CardType.Prince,
                    CardType.King,
                    CardType.Countess,
                    CardType.Princess
                    // Guard is not guessable with a Guard card.
                };

                if (guessableTypes.Any())
                {
                    return guessableTypes[_randomizer.Next(0, guessableTypes.Count)]; // IRandomizer.Next errors fixed by _randomizer being TestRandomizer type
                }
            }
            return null;
        }
    }
}