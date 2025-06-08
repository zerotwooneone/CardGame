using CardGame.Domain.BaseGame;
using CardGame.Domain.Game;
using CardGame.Domain.Game.GameException;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Providers;
using CardGame.Domain.Types;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using CardRank = CardGame.Domain.BaseGame.CardRank;
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

            var game = GameUnderTest.CreateNewGame(deckDefinitionId, playerInfos, creatorId, initialDeckCardSet, loggerFactory, tokensToWin, localRandomizer);

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
                    Console.WriteLine($"Seed: {localRandomizer.Seed} - Turn {turnsInCurrentRound + 1} (Round {game.RoundNumber}): Player {currentPlayer.Name}'s turn. Hand: {string.Join(", ", currentPlayer.Hand.Cards.Select(c => c.Rank.Name))}");

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

                    Card? cardToPlay = null; // MODIFIED to nullable
                    Guid? targetPlayerId = null;
                    CardRank? guessedCardType = null;
                    bool cardPlayedThisTurn = false;

                    var allHandCards = currentPlayer.Hand.Cards.ToList(); // Make a mutable copy
                    localRandomizer.Shuffle(allHandCards); // Shuffle for variety

                    List<Card> candidateCardsToConsider = new List<Card>();
                    bool hasCountess = allHandCards.Any(c => c.Rank == CardRank.Countess);
                    bool hasKing = allHandCards.Any(c => c.Rank == CardRank.King);
                    bool hasPrince = allHandCards.Any(c => c.Rank == CardRank.Prince);

                    if (hasCountess && (hasKing || hasPrince))
                    {
                        var countessCard = allHandCards.FirstOrDefault(c => c.Rank == CardRank.Countess);
                        if (countessCard != null) candidateCardsToConsider.Add(countessCard);
                        Console.WriteLine($"Seed: {localRandomizer.Seed} - Player {currentPlayer.Name} must play Countess due to King/Prince.");
                    }
                    else
                    {
                        if (allHandCards.Count == 1 && allHandCards[0].Rank == CardRank.Princess)
                        {
                            candidateCardsToConsider.Add(allHandCards[0]);
                        }
                        else
                        {
                            candidateCardsToConsider.AddRange(allHandCards.Where(c => c.Rank != CardRank.Princess || allHandCards.Count == 1));
                        }
                    }

                    foreach (var cardInHandChoice in candidateCardsToConsider)
                    {
                        Guid? currentTargetId = SelectTargetPlayer(currentPlayer, cardInHandChoice, game.Players.ToList(), localRandomizer);
                        CardRank? currentGuessedType = SelectGuessedCardType(cardInHandChoice, localRandomizer);

                        bool requirementsMet = true;
                        bool cardRequiresTarget = cardInHandChoice.Rank == CardRank.Guard || cardInHandChoice.Rank == CardRank.Priest || cardInHandChoice.Rank == CardRank.Baron || cardInHandChoice.Rank == CardRank.King || cardInHandChoice.Rank == CardRank.Prince;
                        bool cardRequiresGuess = cardInHandChoice.Rank == CardRank.Guard;

                        if (cardInHandChoice.Rank == CardRank.Baron && currentPlayer.Hand.Cards.Count <= 1)
                        {
                            Console.WriteLine($"Seed: {localRandomizer.Seed} - Player {currentPlayer.Name} cannot play Baron as it's their only card or they have no other card to compare.");
                            requirementsMet = false;
                        }

                        if (cardRequiresTarget && !currentTargetId.HasValue)
                        {
                            requirementsMet = false;
                        }
                        if (cardRequiresGuess && currentGuessedType == null)
                        {
                            requirementsMet = false;
                        }

                        if (requirementsMet)
                        {
                            cardToPlay = cardInHandChoice;
                            targetPlayerId = currentTargetId;
                            guessedCardType = currentGuessedType;
                            cardPlayedThisTurn = true;
                            break; // Found a valid card to play
                        }
                    }

                    if (!cardPlayedThisTurn)
                    {
                        // If still no card played, and player has cards, it's an issue with test logic or an unhandled scenario by test AI.
                        if (currentPlayer.Hand.Cards.Any()) 
                        {
                            Console.WriteLine($"Seed: {localRandomizer.Seed} - Test logic could not find a 'perfect' play for Player {currentPlayer.Name} with hand: {string.Join(", ", currentPlayer.Hand.Cards.Select(c=>c.Rank.Name))}. Candidates considered: {string.Join(", ", candidateCardsToConsider.Select(c=>c.Rank.Name))}. Attempting a fallback play.");

                            var originalHandShuffled = currentPlayer.Hand.Cards.ToList(); // Get a fresh copy
                            localRandomizer.Shuffle(originalHandShuffled);

                            Card? cardToAttemptInFallback = null; // MODIFIED to nullable
                            bool fallbackPlayFound = false;

                            // Iterate through the shuffled hand to find a fallback play
                            foreach (var candidateFallbackCard in originalHandShuffled)
                            {
                                // Rule: Must play Countess if King or Prince is also in hand (and Countess is chosen here by shuffle)
                                if (candidateFallbackCard.Rank == CardRank.Countess && 
                                    originalHandShuffled.Any(c => c.Rank == CardRank.King || c.Rank == CardRank.Prince))
                                {
                                    // This is a valid play, Countess has no target
                                    cardToAttemptInFallback = candidateFallbackCard;
                                    targetPlayerId = null;
                                    guessedCardType = null;
                                    fallbackPlayFound = true;
                                    break;
                                }
                                // Rule: Cannot play Princess if other cards are available (unless it's the only card)
                                if (candidateFallbackCard.Rank == CardRank.Princess && originalHandShuffled.Count > 1)
                                {
                                    continue; // Skip Princess if other options exist in fallback
                                }

                                // Additional check for Baron in fallback
                                if (candidateFallbackCard.Rank == CardRank.Baron && currentPlayer.Hand.Cards.Count <= 1)
                                {
                                    Console.WriteLine($"Seed: {localRandomizer.Seed} - Fallback: Skipping Baron for {currentPlayer.Name} as it's their only card or they have no other card to compare.");
                                    continue;
                                }

                                targetPlayerId = SelectTargetPlayer(currentPlayer, candidateFallbackCard, game.Players.ToList(), localRandomizer);
                                guessedCardType = SelectGuessedCardType(candidateFallbackCard, localRandomizer);

                                // Check if card requires a target and if a target was found
                                bool requiresTarget = candidateFallbackCard.Rank == CardRank.Guard ||
                                                candidateFallbackCard.Rank == CardRank.Priest ||
                                                candidateFallbackCard.Rank == CardRank.Baron ||
                                                candidateFallbackCard.Rank == CardRank.King ||
                                                candidateFallbackCard.Rank == CardRank.Prince;

                                if (requiresTarget && targetPlayerId == null)
                                {
                                    // This card needs a target, but none was found. Cannot use it for fallback.
                                    Console.WriteLine($"Seed: {localRandomizer.Seed} - Fallback: Skipping {candidateFallbackCard.Rank.Name} for {currentPlayer.Name} as it requires a target but none found.");
                                    continue;
                                }
                            
                                // Check if Guard requires a guess and if a guess was found
                                if (candidateFallbackCard.Rank == CardRank.Guard && guessedCardType == null)
                                {
                                    Console.WriteLine($"Seed: {localRandomizer.Seed} - Fallback: Skipping Guard for {currentPlayer.Name} as it requires a guess but none found.");
                                    continue;
                                }

                                // If we reach here, this card is a viable fallback candidate
                                cardToAttemptInFallback = candidateFallbackCard;
                                fallbackPlayFound = true;
                                break;
                            }

                            if (fallbackPlayFound && cardToAttemptInFallback != null) // ADDED null check for cardToAttemptInFallback
                            {
                                try
                                {
                                    Console.WriteLine($"Seed: {localRandomizer.Seed} - Fallback: Player {currentPlayer.Name} attempts to play {cardToAttemptInFallback.Rank.Name} (Target: {targetPlayerId?.ToString() ?? "None"}, Guess: {guessedCardType?.Name ?? "None"})"); // MODIFIED: guessedCardType?.Name
                                    game.PlayCard(currentPlayer.Id, cardToAttemptInFallback, targetPlayerId, guessedCardType, _deckProvider); 
                                    cardPlayedThisTurn = true;
                                }
                                catch (GameRuleException ex)
                                {
                                    var actingPlayerTestView = game.Players.FirstOrDefault(p => p.Id == currentPlayer.Id);
                                    var targetPlayerTestView = targetPlayerId.HasValue ? game.Players.FirstOrDefault(p => p.Id == targetPlayerId.Value) : null;

                                    string actingPlayerHand = actingPlayerTestView != null ? string.Join(", ", actingPlayerTestView.Hand.Cards.Select(c => c.Rank.Name)) : "N/A";
                                    string targetPlayerHand = targetPlayerTestView != null ? string.Join(", ", targetPlayerTestView.Hand.Cards.Select(c => c.Rank.Name)) : "N/A";
                                    string targetPlayerStatus = targetPlayerTestView?.Status.Name ?? "N/A";

                                    string detailedMessage = $"Seed: {localRandomizer.Seed} - GameRuleException on Turn {(turnsInCurrentRound + 1)}, Round {game.RoundNumber}. Player {currentPlayer.Name} ({currentPlayer.Id}) playing {cardToAttemptInFallback.Rank.Name} ({cardToAttemptInFallback.AppearanceId.Substring(0,4)}) targeting {(targetPlayerTestView?.Name ?? "None")} ({(targetPlayerTestView?.Id.ToString() ?? "N/A")}).\nTestView Acting Player: Hand: [{actingPlayerHand}], Status: {actingPlayerTestView?.Status.Name ?? "N/A"}.\nTestView Target Player: Hand: [{targetPlayerHand}], Status: {targetPlayerStatus}.\nOriginal Exception: {ex.Message}";
                                    Console.WriteLine(detailedMessage);
                                    throw new Exception(detailedMessage, ex); 
                                }
                                catch (InvalidMoveException ex)
                                {
                                    var actingPlayerTestView = game.Players.FirstOrDefault(p => p.Id == currentPlayer.Id);
                                    var targetPlayerTestView = targetPlayerId.HasValue ? game.Players.FirstOrDefault(p => p.Id == targetPlayerId.Value) : null;

                                    string actingPlayerHand = actingPlayerTestView != null ? string.Join(", ", actingPlayerTestView.Hand.Cards.Select(c => c.Rank.Name)) : "N/A";
                                    string targetPlayerHand = targetPlayerTestView != null ? string.Join(", ", targetPlayerTestView.Hand.Cards.Select(c => c.Rank.Name)) : "N/A";
                                    string targetPlayerStatus = targetPlayerTestView?.Status.Name ?? "N/A";

                                    string detailedMessage = $"Seed: {localRandomizer.Seed} - InvalidMoveException on Turn {(turnsInCurrentRound + 1)}, Round {game.RoundNumber}. Player {currentPlayer.Name} ({currentPlayer.Id}) attempting to play {cardToAttemptInFallback?.Rank.Name ?? "UnknownCard"} ({cardToAttemptInFallback?.AppearanceId.Substring(0,4) ?? "N/A"}) targeting {(targetPlayerTestView?.Name ?? "None")} ({(targetPlayerTestView?.Id.ToString() ?? "N/A")}).\nTestView Acting Player: Hand: [{actingPlayerHand}], Status: {actingPlayerTestView?.Status.Name ?? "N/A"}.\nTestView Target Player: Hand: [{targetPlayerHand}], Status: {targetPlayerStatus}.\nOriginal Exception: {ex.Message}";
                                    Console.WriteLine(detailedMessage);
                                    throw new Exception(detailedMessage, ex);
                                }
                            }
                            else
                            {
                                // If no fallback play could be constructed (e.g. only card is Princess with others, or all cards require targets but none available)
                                throw new AssertionException($"Seed: {localRandomizer.Seed} - Test logic FATAL: Could not select any fallback card for Player {currentPlayer.Name} with hand: {string.Join(", ", currentPlayer.Hand.Cards.Select(c=>c.Rank.Name))}. No valid move possible according to test AI even in fallback.");
                            }
                        }
                        else 
                        { // Player has no cards - this should have been caught by the check at the start of the turn.
                            Console.WriteLine($"Seed: {localRandomizer.Seed} - Player {currentPlayer.Name} has no cards and cannot play. (Should be caught earlier)");
                            turnsInCurrentRound++;
                            turnsTakenOverall++;
                            continue;
                        }
                    }
                
                    if (cardToPlay != null) // ADDED null check
                    {
                        try
                        {
                            Console.WriteLine($"Seed: {localRandomizer.Seed} - Player {currentPlayer.Name} plays {cardToPlay.Rank.Name} (Target: {targetPlayerId?.ToString() ?? "None"}, Guess: {guessedCardType?.Name ?? "None"})"); // MODIFIED: guessedCardType?.Name
                            game.PlayCard(currentPlayer.Id, cardToPlay, targetPlayerId, guessedCardType, _deckProvider); 
                        }
                        catch (GameRuleException ex)
                        {
                            var actingPlayerTestView = game.Players.FirstOrDefault(p => p.Id == currentPlayer.Id);
                            var targetPlayerTestView = targetPlayerId.HasValue ? game.Players.FirstOrDefault(p => p.Id == targetPlayerId.Value) : null;

                            string actingPlayerHand = actingPlayerTestView != null ? string.Join(", ", actingPlayerTestView.Hand.Cards.Select(c => c.Rank.Name)) : "N/A";
                            string targetPlayerHand = targetPlayerTestView != null ? string.Join(", ", targetPlayerTestView.Hand.Cards.Select(c => c.Rank.Name)) : "N/A";
                            string targetPlayerStatus = targetPlayerTestView?.Status.Name ?? "N/A";

                            string detailedMessage = $"Seed: {localRandomizer.Seed} - GameRuleException on Turn {(turnsInCurrentRound + 1)}, Round {game.RoundNumber}. Player {currentPlayer.Name} ({currentPlayer.Id}) playing {cardToPlay.Rank.Name} ({cardToPlay.AppearanceId.Substring(0,4)}) targeting {(targetPlayerTestView?.Name ?? "None")} ({(targetPlayerTestView?.Id.ToString() ?? "N/A")}).\nTestView Acting Player: Hand: [{actingPlayerHand}], Status: {actingPlayerTestView?.Status.Name ?? "N/A"}.\nTestView Target Player: Hand: [{targetPlayerHand}], Status: {targetPlayerStatus}.\nOriginal Exception: {ex.Message}";
                            Console.WriteLine(detailedMessage);
                            throw new Exception(detailedMessage, ex); 
                        }
                        catch (InvalidMoveException ex)
                        {
                            var actingPlayerTestView = game.Players.FirstOrDefault(p => p.Id == currentPlayer.Id);
                            var targetPlayerTestView = targetPlayerId.HasValue ? game.Players.FirstOrDefault(p => p.Id == targetPlayerId.Value) : null;

                            string actingPlayerHand = actingPlayerTestView != null ? string.Join(", ", actingPlayerTestView.Hand.Cards.Select(c => c.Rank.Name)) : "N/A";
                            string targetPlayerHand = targetPlayerTestView != null ? string.Join(", ", targetPlayerTestView.Hand.Cards.Select(c => c.Rank.Name)) : "N/A";
                            string targetPlayerStatus = targetPlayerTestView?.Status.Name ?? "N/A";

                            string detailedMessage = $"Seed: {localRandomizer.Seed} - InvalidMoveException on Turn {(turnsInCurrentRound + 1)}, Round {game.RoundNumber}. Player {currentPlayer.Name} ({currentPlayer.Id}) attempting to play {cardToPlay?.Rank.Name ?? "UnknownCard"} ({cardToPlay?.AppearanceId.Substring(0,4) ?? "N/A"}) targeting {(targetPlayerTestView?.Name ?? "None")} ({(targetPlayerTestView?.Id.ToString() ?? "N/A")}).\nTestView Acting Player: Hand: [{actingPlayerHand}], Status: {actingPlayerTestView?.Status.Name ?? "N/A"}.\nTestView Target Player: Hand: [{targetPlayerHand}], Status: {targetPlayerStatus}.\nOriginal Exception: {ex.Message}";
                            Console.WriteLine(detailedMessage);
                            throw new Exception(detailedMessage, ex);
                        }

                        turnsInCurrentRound++;
                        turnsTakenOverall++;
                    }
                } // ADDED closing brace for the inner while loop (while (game.GamePhase == GamePhase.RoundInProgress...))
            
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

        private Guid? SelectTargetPlayer(Player currentPlayer, Card cardToPlay, List<Player> allPlayersInGame, TestRandomizer randomizer) // Added randomizer param
        {
            var activeOpponentsNotProtected = allPlayersInGame
                .Where(p => p.Id != currentPlayer.Id && p.Status == PlayerStatus.Active && !p.IsProtected)
                .ToList();

            var princeTargets = new List<Player>();
            // Add self if active and has cards
            if (currentPlayer.Status == PlayerStatus.Active && currentPlayer.Hand.Cards.Count > 1)
            {
                princeTargets.Add(currentPlayer);
            }
            // Add active, non-protected opponents who have cards
            princeTargets.AddRange(activeOpponentsNotProtected.Where(p => p.Hand.Cards.Any()));

            switch (cardToPlay.Rank.Name) // Assuming CardType has a Name property or using .ToString()
            {
                case nameof(CardRank.Guard):
                case nameof(CardRank.Priest):
                case nameof(CardRank.Baron):
                case nameof(CardRank.King):
                    var validKingOrBaronTargets = activeOpponentsNotProtected;
                    if (cardToPlay.Rank == CardRank.King || cardToPlay.Rank == CardRank.Baron)
                    {
                        // Current player must have at least one card to trade (King/Baron itself is played, so >1 card initially, or 1 if it's not King/Baron)
                        // Or more simply, the domain will check if actingPlayer.Hand.GetHeldCard() is valid.
                        // For test selection, ensure target has cards.
                        validKingOrBaronTargets = activeOpponentsNotProtected.Where(p => p.Hand.Cards.Any()).ToList();
                    }

                    if (validKingOrBaronTargets.Any())
                    {
                        return validKingOrBaronTargets[randomizer.Next(0, validKingOrBaronTargets.Count)].Id; // Use passed randomizer
                    }
                    Console.WriteLine($"Seed: {randomizer.Seed} - No valid target (active, non-protected opponent{(cardToPlay.Rank == CardRank.King || cardToPlay.Rank == CardRank.Baron ? ", with cards" : "")}) for {cardToPlay.Rank.Name} by {currentPlayer.Name}. Effect may fizzle.");
                    return null; 

                case nameof(CardRank.Prince):
                    var validPrinceTargets = allPlayersInGame
                        .Where(p => p.Status == PlayerStatus.Active &&
                                     (
                                         (p.Id != currentPlayer.Id && p.Hand.Cards.Any() && !p.IsProtected) || // Opponent: active, has cards, not protected
                                         (p.Id == currentPlayer.Id && currentPlayer.Hand.Cards.Count > 1)      // Self: active, has Prince + at least one other card to discard
                                     )
                      )
                        .ToList();

                    if (validPrinceTargets.Any())
                    {
                        return validPrinceTargets[randomizer.Next(0, validPrinceTargets.Count)].Id;
                    }
                    Console.WriteLine($"Seed: {randomizer.Seed} - No valid target (active, non-protected if opponent, with cards) for Prince by {currentPlayer.Name}. Effect may fizzle or be invalid.");
                    return null;

                case nameof(CardRank.Countess):
                case nameof(CardRank.Handmaid):
                case nameof(CardRank.Princess):
                default:
                    return null; 
            }
        }

        private CardRank? SelectGuessedCardType(Card cardToPlay, TestRandomizer randomizer) // Added randomizer param
        {
            if (cardToPlay.Rank != CardRank.Guard) return null;

            var guessableCardTypes = new List<CardRank>
            {
                CardRank.Priest, CardRank.Baron, CardRank.Handmaid, CardRank.Prince, CardRank.King, /*CardType.Countess,*/ CardRank.Princess // Countess is rarely a good guess
            };
            randomizer.Shuffle(guessableCardTypes);
            return guessableCardTypes.First();
        }
    }
}