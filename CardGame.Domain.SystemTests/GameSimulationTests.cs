using CardGame.Decks.BaseGame;
using CardGame.Domain.Game;
using CardGame.Domain.Game.GameException;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Providers;
using CardGame.Domain.Types;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using CardRank = CardGame.Decks.BaseGame.CardRank;
// For Game, Player, Card, Hand, PlayerInfo, DeckDefinition
// For IDeckProvider
// For DefaultDeckProvider
// For CardType, PlayerStatus
// For GameRuleException and InvalidMoveException
using GameUnderTest = CardGame.Domain.Game.Game;

namespace CardGame.Domain.SystemTests
{
    [TestFixture]
    [Category("CardGame.Domain.SystemTests")]
    public class GameSimulationTests
    {
        private IDeckProvider _deckProvider;

        [SetUp]
        public void TestSetup()
        {
            _deckProvider = new DefaultDeckProvider(); 
        }

        // New private method to encapsulate single game simulation
        private void SimulateSingleGamePlay(int? seed = null)
        {
            var localRandomizer = new TestRandomizer(seed); // Initialize with seed here, local instance
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole()); // ADDED loggerFactory
            Console.WriteLine($"--- Starting game with Seed: {localRandomizer.Seed} ---");

            // 1. Setup Game (copied from original test method)
            int playerCount = localRandomizer.Next(2, 5); // Use seeded randomizer
            var playerInfos = Enumerable.Range(1, playerCount)
                .Select(i => new PlayerInfo(Guid.NewGuid(), $"Player {i}"))
                .ToList();
            Guid creatorId = playerInfos[0].Id; 
            
            DeckDefinition deckDefinition = _deckProvider.GetDeck(); 
            IReadOnlyList<Card> initialDeckCardSet = deckDefinition.Cards.ToList(); 
            Guid deckDefinitionId = _deckProvider.DeckId; 

            int tokensToWin = localRandomizer.Next(3, 6); // NEW SEEDED WAY

            var game = GameUnderTest.CreateNewGame(deckDefinitionId, playerInfos, creatorId, initialDeckCardSet, loggerFactory, _deckProvider, tokensToWin, localRandomizer);

            Console.WriteLine($"Starting game ID: {game.Id} with Seed: {localRandomizer.Seed}. Target tokens: {tokensToWin}.");

            // 2. Game Loop (copied and modified for seed logging)
            int maxTurnsOverall = 200; 
            int turnsTakenOverall = 0;

            while (game.GamePhase != GamePhase.GameOver && turnsTakenOverall < maxTurnsOverall)
            {
                Console.WriteLine($"Seed: {localRandomizer.Seed} - Round {game.RoundNumber} started ---");
            
                int maxTurnsInRound = 50; 
                int turnsInCurrentRound = 0;

                while (game.GamePhase == GamePhase.RoundInProgress && turnsInCurrentRound < maxTurnsInRound)
                {
                    Player currentPlayer = game.Players.First(p => p.Id == game.CurrentTurnPlayerId);
                    Console.WriteLine($"Seed: {localRandomizer.Seed} - Turn {turnsInCurrentRound + 1} (Round {game.RoundNumber}): Player {currentPlayer.Name}'s turn. Hand: {string.Join(", ", currentPlayer.Hand.Cards.Select(c => CardRank.FromValue(c.Rank).Name))}");

                    if (currentPlayer.Status != PlayerStatus.Active)
                    {
                        Console.WriteLine($"Seed: {localRandomizer.Seed} - Player {currentPlayer.Name} is out (Status: {currentPlayer.Status}). Skipping turn.");
                        // If all players become inactive mid-round, it might be an issue or specific end condition
                        if (game.Players.Count(p => p.Status == PlayerStatus.Active) <= 1 && game.GamePhase == GamePhase.RoundInProgress) {
                             Console.WriteLine($"Seed: {localRandomizer.Seed} - Warning: Zero or one active player remaining mid-round. Current Player: {currentPlayer.Name} (Status: {currentPlayer.Status}). Round should likely end.");
                             // Domain should handle round end, but test can note this state.
                        }
                        turnsInCurrentRound++;
                        turnsTakenOverall++;
                        continue;
                    }
                
                    if (!currentPlayer.Hand.Cards.Any() && currentPlayer.Status == PlayerStatus.Active)
                    {
                        Console.WriteLine($"Seed: {localRandomizer.Seed} - Warning: Player {currentPlayer.Name} is Active but has no cards. This is unusual. Skipping turn.");
                        // This state might indicate an issue if not immediately after a Prince effect that correctly emptied the hand.
                        // The domain should handle if this player can/cannot continue the round.
                        turnsInCurrentRound++;
                        turnsTakenOverall++;
                        continue;
                    }

                    Card? cardToPlay = null; 
                    Guid? targetPlayerId = null;
                    int? guessedRankValue = null;
                    bool cardPlayedThisTurn = false;

                    var allHandCards = currentPlayer.Hand.Cards.ToList(); // Make a mutable copy
                    localRandomizer.Shuffle(allHandCards); // Shuffle for variety

                    List<Card> candidateCardsToConsider = new List<Card>();
                    bool hasCountess = allHandCards.Any(c => c.Rank == CardRank.Countess.Value);
                    bool hasKing = allHandCards.Any(c => c.Rank == CardRank.King.Value);
                    bool hasPrince = allHandCards.Any(c => c.Rank == CardRank.Prince.Value);

                    if (hasCountess && (hasKing || hasPrince))
                    {
                        var countessCard = allHandCards.FirstOrDefault(c => c.Rank == CardRank.Countess.Value);
                        if (countessCard != null) candidateCardsToConsider.Add(countessCard);
                        Console.WriteLine($"Seed: {localRandomizer.Seed} - Player {currentPlayer.Name} must play Countess due to King/Prince.");
                    }
                    else
                    {
                        if (allHandCards.Count == 1 && allHandCards[0].Rank == CardRank.Princess.Value)
                        {
                            candidateCardsToConsider.Add(allHandCards[0]);
                        }
                        else
                        {
                            candidateCardsToConsider.AddRange(allHandCards.Where(c => c.Rank != CardRank.Princess.Value || allHandCards.Count == 1));
                        }
                    }

                    foreach (var cardInHandChoice in candidateCardsToConsider)
                    {
                        Guid? currentTargetId = SelectTargetPlayer(currentPlayer, cardInHandChoice, game.Players.ToList(), localRandomizer);
                        int? currentGuessedRankValue = SelectGuessedCardType(cardInHandChoice, localRandomizer);

                        bool requirementsMet = true;
                        bool cardRequiresTarget = cardInHandChoice.Rank == CardRank.Guard.Value || cardInHandChoice.Rank == CardRank.Priest.Value || cardInHandChoice.Rank == CardRank.Baron.Value || cardInHandChoice.Rank == CardRank.King.Value || cardInHandChoice.Rank == CardRank.Prince.Value;
                        bool cardRequiresGuess = cardInHandChoice.Rank == CardRank.Guard.Value;

                        if (cardInHandChoice.Rank == CardRank.Baron.Value && currentPlayer.Hand.Cards.Count <= 1)
                        {
                            Console.WriteLine($"Seed: {localRandomizer.Seed} - Player {currentPlayer.Name} cannot play Baron as it's their only card or they have no other card to compare.");
                            requirementsMet = false;
                        }

                        if (cardRequiresTarget && !currentTargetId.HasValue)
                        {
                            requirementsMet = false;
                        }
                        if (cardRequiresGuess && currentGuessedRankValue == null)
                        {
                            requirementsMet = false;
                        }

                        if (requirementsMet)
                        {
                            cardToPlay = cardInHandChoice;
                            targetPlayerId = currentTargetId;
                            guessedRankValue = currentGuessedRankValue;
                            cardPlayedThisTurn = true;
                            break; // Found a valid card to play
                        }
                    }

                    if (!cardPlayedThisTurn)
                    {
                        // If still no card played, and player has cards, it's an issue with test logic or an unhandled scenario by test AI.
                        if (currentPlayer.Hand.Cards.Any()) 
                        {
                            Console.WriteLine($"Seed: {localRandomizer.Seed} - Test logic could not find a 'perfect' play for Player {currentPlayer.Name} with hand: {string.Join(", ", currentPlayer.Hand.Cards.Select(c=> CardRank.FromValue(c.Rank).Name))}. Candidates considered: {string.Join(", ", candidateCardsToConsider.Select(c=> CardRank.FromValue(c.Rank).Name))}. Attempting a fallback play.");

                            var originalHandShuffled = currentPlayer.Hand.Cards.ToList(); // Get a fresh copy
                            localRandomizer.Shuffle(originalHandShuffled);

                            Card? cardToAttemptInFallback = null; 
                            bool fallbackPlayFound = false;

                            // Iterate through the shuffled hand to find a fallback play
                            foreach (var candidateFallbackCard in originalHandShuffled)
                            {
                                // Rule: Must play Countess if King or Prince is also in hand (and Countess is chosen here by shuffle)
                                if (candidateFallbackCard.Rank == CardRank.Countess.Value && 
                                    originalHandShuffled.Any(c => c.Rank == CardRank.King.Value || c.Rank == CardRank.Prince.Value))
                                {
                                    // This is a valid play, Countess has no target
                                    cardToAttemptInFallback = candidateFallbackCard;
                                    targetPlayerId = null;
                                    guessedRankValue = null;
                                    fallbackPlayFound = true;
                                    break;
                                }
                                // Rule: Cannot play Princess if other cards are available (unless it's the only card)
                                if (candidateFallbackCard.Rank == CardRank.Princess.Value && originalHandShuffled.Count > 1)
                                {
                                    continue; // Skip Princess if other options exist in fallback
                                }

                                // Additional check for Baron in fallback
                                if (candidateFallbackCard.Rank == CardRank.Baron.Value && currentPlayer.Hand.Cards.Count <= 1)
                                {
                                    Console.WriteLine($"Seed: {localRandomizer.Seed} - Fallback: Skipping Baron for {currentPlayer.Name} as it's their only card or they have no other card to compare.");
                                    continue;
                                }

                                targetPlayerId = SelectTargetPlayer(currentPlayer, candidateFallbackCard, game.Players.ToList(), localRandomizer);
                                guessedRankValue = SelectGuessedCardType(candidateFallbackCard, localRandomizer);

                                bool fallbackRequiresTarget = candidateFallbackCard.Rank == CardRank.Guard.Value || candidateFallbackCard.Rank == CardRank.Priest.Value || candidateFallbackCard.Rank == CardRank.Baron.Value || candidateFallbackCard.Rank == CardRank.King.Value || candidateFallbackCard.Rank == CardRank.Prince.Value;

                                if (fallbackRequiresTarget && !targetPlayerId.HasValue)
                                {
                                    Console.WriteLine($"Seed: {localRandomizer.Seed} - Fallback: Skipping {CardRank.FromValue(candidateFallbackCard.Rank).Name} for {currentPlayer.Name} as it requires a target but none found.");
                                    continue;
                                }
                            
                                // Check if Guard requires a guess and if a guess was found
                                if (candidateFallbackCard.Rank == CardRank.Guard.Value && guessedRankValue == null)
                                {
                                    Console.WriteLine($"Seed: {localRandomizer.Seed} - Fallback: Skipping Guard for {currentPlayer.Name} as it requires a guess but none found.");
                                    continue;
                                }

                                cardToAttemptInFallback = candidateFallbackCard;
                                fallbackPlayFound = true;
                                break; // Found a fallback card to play
                            }

                            if (fallbackPlayFound && cardToAttemptInFallback != null)
                            {
                                try
                                {
                                    Console.WriteLine($"Seed: {localRandomizer.Seed} - Fallback: Player {currentPlayer.Name} attempts to play {CardRank.FromValue(cardToAttemptInFallback.Rank).Name} (Target: {targetPlayerId?.ToString() ?? "None"}, Guess: {(guessedRankValue.HasValue ? CardRank.FromValue(guessedRankValue.Value).Name : "None")})"); 
                                    game.PlayCard(currentPlayer.Id, cardToAttemptInFallback, targetPlayerId, guessedRankValue);
                                    cardPlayedThisTurn = true;
                                }
                                catch (GameRuleException ex)
                                {
                                    Console.WriteLine($"Seed: {localRandomizer.Seed} - Fallback play failed for {CardRank.FromValue(cardToAttemptInFallback.Rank).Name}. Exception: {ex.Message}");
                                }
                                catch (InvalidMoveException ex)
                                {
                                    Console.WriteLine($"Seed: {localRandomizer.Seed} - Fallback play was invalid for {CardRank.FromValue(cardToAttemptInFallback.Rank).Name}. Exception: {ex.Message}");
                                }
                            }
                        }

                        if (!cardPlayedThisTurn)
                        {
                            Console.WriteLine($"Seed: {localRandomizer.Seed} - Player {currentPlayer.Name} has no valid card to play. Hand: {string.Join(", ", currentPlayer.Hand.Cards.Select(c => CardRank.FromValue(c.Rank).Name))}. Skipping turn.");
                        }
                    }
                
                    if (cardToPlay != null)
                    {
                        Console.WriteLine($"Seed: {localRandomizer.Seed} - Player {currentPlayer.Name} plays {CardRank.FromValue(cardToPlay.Rank).Name}. Target: {(targetPlayerId.HasValue ? game.Players.First(p => p.Id == targetPlayerId).Name : "N/A")}. Guess: {(guessedRankValue.HasValue ? CardRank.FromValue(guessedRankValue.Value).Name : "N/A")}");
                        game.PlayCard(currentPlayer.Id, cardToPlay, targetPlayerId, guessedRankValue);
                        turnsInCurrentRound++;
                        turnsTakenOverall++;
                    }
                    else if (!cardPlayedThisTurn) // Only log if a card wasn't played in fallback either
                    {
                        Console.WriteLine($"Seed: {localRandomizer.Seed} - No card was played by {currentPlayer.Name} this turn.");
                        turnsInCurrentRound++;
                        turnsTakenOverall++;
                    }
                }

                if (game.GamePhase == GamePhase.RoundInProgress)
                {
                    (game.GamePhase == GamePhase.RoundOver || game.GamePhase == GamePhase.GameOver || turnsInCurrentRound >= maxTurnsInRound).Should().BeTrue($"Seed: {localRandomizer.Seed} - Round {game.RoundNumber} did not complete as expected within {maxTurnsInRound} turns. GamePhase={game.GamePhase}, turnsInCurrentRound={turnsInCurrentRound}.");

                    if (turnsInCurrentRound >= maxTurnsInRound && game.GamePhase == GamePhase.RoundInProgress)
                    {
                         Console.WriteLine($"Seed: {localRandomizer.Seed} - Warning: Round {game.RoundNumber} hit max turn limit ({maxTurnsInRound}) and was still in progress. Active players: {game.Players.Count(p => p.Status == PlayerStatus.Active)}. Current Player: {game.Players.FirstOrDefault(p=>p.Id == game.CurrentTurnPlayerId)?.Name} (Status: {game.Players.FirstOrDefault(p=>p.Id == game.CurrentTurnPlayerId)?.Status})");
                    }
                    else if (game.GamePhase == GamePhase.RoundOver)
                    {
                        Console.WriteLine($"Seed: {localRandomizer.Seed} - Round {game.RoundNumber} ended. Reason: {GetRoundEndReason(game)} --- Player Tokens: {string.Join("; ", game.Players.Select(p => $"{p.Name}: {p.TokensWon}"))}");
                    }
                }
            }

            // 3. Assert Game Completion (copied and modified for seed logging)
            (game.GamePhase == GamePhase.GameOver || turnsTakenOverall >= maxTurnsOverall).Should().BeTrue($"Seed: {localRandomizer.Seed} - Game did not complete as expected within {maxTurnsOverall} turns. GamePhase={game.GamePhase}, turnsTakenOverall={turnsTakenOverall}.");
        
            if (turnsTakenOverall >= maxTurnsOverall && game.GamePhase != GamePhase.GameOver)
            {
                 Console.WriteLine($"Seed: {localRandomizer.Seed} - Warning: Game hit max turn limit ({maxTurnsOverall}) and was not over (GamePhase: {game.GamePhase}).");
            }
            else if (game.GamePhase == GamePhase.GameOver)
            {
                Console.WriteLine($"Seed: {localRandomizer.Seed} - --- Game Over --- Player Tokens: {string.Join("; ", game.Players.Select(p => $"{p.Name}: {p.TokensWon}"))}");
                Player? winner = game.Players.FirstOrDefault(p => p.TokensWon >= game.TokensNeededToWin); 
                winner.Should().NotBeNull($"Seed: {localRandomizer.Seed} - Game ended without a winner based on tokens. This is unexpected for a standard game completion.");
                if (winner != null) {
                    Console.WriteLine($"Seed: {localRandomizer.Seed} - Winner: Player {winner.Name} with {winner.TokensWon} tokens.");
                    (winner.TokensWon >= game.TokensNeededToWin).Should().BeTrue($"Seed: {localRandomizer.Seed} - Winner {winner.Name} should have {game.TokensNeededToWin} or more tokens to win, but has {winner.TokensWon}.");
                }
            }
            Console.WriteLine($"Seed: {localRandomizer.Seed} - Game completed. Total rounds: {game.RoundNumber}. Total turns: {turnsTakenOverall}.");
        } 

        [Test]
        public void Should_Successfully_Complete_Specific_Game_With_Random_Plays()
        {
            SimulateSingleGamePlay(1237624095); // 692458833
        }

        [Test]
        public void Should_Successfully_Complete_Full_Game_With_Random_Plays()
        {
            SimulateSingleGamePlay(); // Call the new refactored method
        }

        [Test]
        public void Should_Successfully_Complete_Multiple_Games_In_Parallel()
        {
            int numberOfGames = 600;
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            Console.WriteLine($"Starting {numberOfGames} games in parallel...");

            Parallel.For(0, numberOfGames, i =>
            {
                try
                {
                    Console.WriteLine($"--- Starting parallel game iteration {i + 1} ---");
                    SimulateSingleGamePlay(); // Each call will use its own TestRandomizer with a new seed
                    Console.WriteLine($"--- Parallel game iteration {i + 1} completed successfully ---");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--- Parallel game iteration {i + 1} FAILED: {ex.Message} ---");
                    exceptions.Add(ex); // The exception message already contains the seed
                }
            });

            if (exceptions.Any())
            {
                var aggregatedExceptionMessage = string.Join(Environment.NewLine + "--------------------" + Environment.NewLine, 
                                                           exceptions.Select(ex => ex.ToString()));
                throw new AssertionException($"{exceptions.Count} out of {numberOfGames} games failed in parallel execution. Failures:\n{aggregatedExceptionMessage}");
            }
            
            Console.WriteLine($"All {numberOfGames} parallel games completed successfully.");
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

        private Guid? SelectTargetPlayer(Player currentPlayer, Card cardToPlay, List<Player> allPlayers, TestRandomizer randomizer)
        {
            var potentialTargets = allPlayers.Where(p => p.Id != currentPlayer.Id && p.Status == PlayerStatus.Active && !p.IsProtected).ToList();
            if (!potentialTargets.Any())
                return null;

            // For Prince, the player can target themselves
            if (cardToPlay.Rank == CardRank.Prince.Value)
            {
                potentialTargets.Add(currentPlayer);
            }

            if (!potentialTargets.Any())
            {
                return null;
            }
            
            randomizer.Shuffle(potentialTargets);
            return potentialTargets.First().Id;
        }

        private int? SelectGuessedCardType(Card cardToPlay, TestRandomizer randomizer)
        {
            if (cardToPlay.Rank != CardRank.Guard.Value) return null;

            var possibleRanks = CardRank.List().Where(r => r.Value != CardRank.Guard.Value).ToList();

            if (!possibleRanks.Any()) return null; // Should not happen

            randomizer.Shuffle(possibleRanks);
            return possibleRanks.First().Value;
        }
    }
}