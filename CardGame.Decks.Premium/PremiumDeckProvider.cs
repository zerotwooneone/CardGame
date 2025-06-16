using CardGame.Domain;
using CardGame.Domain.Game;
using CardGame.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Domain.Types;

namespace CardGame.Decks.Premium
{
    public class PremiumDeckProvider : IDeckProvider
    {
        private readonly ILogger<PremiumDeckProvider> _logger;
        private static readonly string _jesterToken = $"JesterToken{Guid.Parse("0005F11F-4760-4A2C-9B6C-4F9F1CF9A111")}";
        private static readonly string _assassinMarkKey = $"AssassinMark{Guid.Parse("0005F11F-4760-4A2C-9B6C-4F9F1CF9A222")}";
        private static readonly List<Card> _allPremiumCards = new List<Card>();

        public Guid DeckId => new Guid("A9A5F11F-4760-4A2C-9B6C-4F9F1CF9A717");
        public string DisplayName => "Love Letter Premium Deck";
        public string Description => "The premium expansion deck for Love Letter, featuring additional roles and more complex interactions.";

        static PremiumDeckProvider()
        {
            InitializeAllCards();
        }

        public PremiumDeckProvider(ILogger<PremiumDeckProvider> logger)
        {
            _logger = logger;
        }

        private static void InitializeAllCards()
        {
            if (_allPremiumCards.Any()) return;

            foreach (var rank in PremiumCardRank.All())
            {
                var appearanceId = $"premium-{rank.Name.ToLowerInvariant().Replace(' ', '-')}";
                for (int i = 0; i < rank.QuantityInDeck; i++)
                {
                    _allPremiumCards.Add(new Card(rank.Value, appearanceId));
                }
            }
        }

        public DeckDefinition GetDeck()
        {
            return new DeckDefinition(GetAllCards(), "PremiumBackAppearanceId");
        }

        // Method to get all cards, ensuring it's populated
        private List<Card> GetAllCards()
        {
            // Ensure cards are initialized (static constructor should handle this, but as a safeguard)
            if (!_allPremiumCards.Any())
            {
                InitializeAllCards();
            }
            return new List<Card>(_allPremiumCards); // Return a copy
        }

        public void ExecuteCardEffect(
            IGameOperations gameOperations,
            Player actingPlayer,
            Card cardPlayed,
            Player? targetPlayer1,
            int? guessedCardRankValue,
            Player? targetPlayer2) 
        {
            var premiumRankPlayed = GetPremiumRankFromCard(cardPlayed);
            PremiumCardRank? guessedPremiumRank = guessedCardRankValue.HasValue 
                ? PremiumCardRank.FromValue(guessedCardRankValue.Value) 
                : null;

            // Validate targetPlayer1 based on card requirements
            if (premiumRankPlayed.RequiresTarget && targetPlayer1 == null)
            {
                _logger.LogWarning("Card {PremiumRankName} requires a target, but none was provided for targetPlayer1.", premiumRankPlayed.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played {premiumRankPlayed.Name}, but no primary target was provided when one was required.")
                {
                    PlayedCard = cardPlayed,
                    FizzleReason = "No primary target provided when required."
                });
                return;
            }

            // Specific checks for targetPlayer1 if it's provided
            if (targetPlayer1 != null)
            {
                if (targetPlayer1.Id == actingPlayer.Id && !premiumRankPlayed.CanTargetSelf)
                {
                    _logger.LogWarning("Card {PremiumRankName} cannot target self, but acting player was targeted as targetPlayer1.", premiumRankPlayed.Name);
                    gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} attempted to play {premiumRankPlayed.Name} targeting self with the primary target, which is not allowed.")
                    {
                        PlayedCard = cardPlayed,
                        TargetPlayerId = targetPlayer1.Id,
                        TargetPlayerName = targetPlayer1.Name,
                        FizzleReason = "Cannot target self with primary target."
                    });
                    return; 
                }
                // Handmaid Protection Check for targetPlayer1
                // Sycophant is a special case that can bypass Handmaid for its *initial* marking effect.
                // Other effects targeting a Handmaid-protected player should fizzle.
                if (targetPlayer1.IsProtected && premiumRankPlayed.Name != PremiumCardRank.Sycophant.Name) 
                {
                    _logger.LogInformation("Primary target player {TargetName} is protected by Handmaid effect. Card {PremiumRankName} has no effect on them.", targetPlayer1.Name, premiumRankPlayed.Name);
                    gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, targetPlayer1.Id, targetPlayer1.Name, $"{actingPlayer.Name} played {premiumRankPlayed.Name} targeting {targetPlayer1.Name}, but {targetPlayer1.Name} is protected. No effect.")
                    {
                        PlayedCard = cardPlayed,
                        FizzleReason = "Target is protected by Handmaid."
                    });
                    // If an effect *only* targets targetPlayer1 and it's protected, the entire effect might fizzle here.
                    // For cards that might also have targetPlayer2 or other effects, we might only skip action on targetPlayer1.
                    // For simplicity and common single-target card behavior, we return.
                    return; 
                }
            }
            
            // TODO: Add similar validation for targetPlayer2 if premiumRankPlayed indicates a second target is required or possible,
            // including null checks, CanTargetSelf (if applicable to a second target), and IsProtected.

            _logger.LogInformation("Executing effect for card {PremiumRankName} played by {PlayerName}", premiumRankPlayed.Name, actingPlayer.Name);
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.CardPlayed, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played {premiumRankPlayed.Name} ({premiumRankPlayed.Description}).")
            {
                PlayedCard = cardPlayed
            });

            // Effect logic based on the card's rank name
            // This assumes unique names for each PremiumCardRank instance
            switch (premiumRankPlayed.Name)
            {
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Guard.Name: // Value 1
                    Effect_Guard(gameOperations, actingPlayer, targetPlayer1, guessedPremiumRank, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Priest.Name: // Value 2 (Original)
                    Effect_Priest(gameOperations, actingPlayer, targetPlayer1, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Cardinal.Name: // Value 2 (New)
                    // Cardinal targets two players. Ensure targets list is appropriate.
                    if (targetPlayer1 != null && targetPlayer2 != null)
                    {
                        Effect_Cardinal(gameOperations, actingPlayer, targetPlayer1, targetPlayer2, cardPlayed); // Pass cardPlayed
                    }
                    else
                    {
                        _logger.LogWarning("Cardinal played by {PlayerName} requires 2 targets, but {TargetCount} were provided.", actingPlayer.Name, targetPlayer1 != null ? 1 : 0);
                        gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Cardinal but did not target exactly two players.") { PlayedCard = cardPlayed, FizzleReason = "Cardinal requires exactly two targets." });
                    }
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Baron.Name: // Value 3 (Original)
                    Effect_Baron(gameOperations, actingPlayer, targetPlayer1, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Baroness.Name: // Value 3 (New)
                    if (targetPlayer1 != null)
                    {
                        Effect_Baroness(gameOperations, actingPlayer, targetPlayer1, targetPlayer2, cardPlayed);
                    }
                    else
                    {
                        _logger.LogWarning("Baroness played by {PlayerName} requires 1 or 2 targets, but {TargetCount} were provided.", actingPlayer.Name, targetPlayer1 != null ? 1 : 0);
                        gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Baroness but did not target one or two players.") { PlayedCard = cardPlayed, FizzleReason = "Baroness requires one or two targets." });
                    }
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Handmaid.Name: // Value 4 (Original)
                    Effect_Handmaid(gameOperations, actingPlayer, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Sycophant.Name: // Value 4 (New)
                    Effect_Sycophant(gameOperations, actingPlayer, targetPlayer1, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Prince.Name: // Value 5 (Original)
                    Effect_Prince(gameOperations, actingPlayer, targetPlayer1, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Count.Name: // Value 5 (New)
                    Effect_Count(gameOperations, actingPlayer, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.King.Name: // Value 6 (Original)
                    Effect_King(gameOperations, actingPlayer, targetPlayer1, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Constable.Name: // Value 6 (New)
                    Effect_Constable(gameOperations, actingPlayer, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Countess.Name: // Value 7 (Original)
                    Effect_Countess(gameOperations, actingPlayer, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.DowagerQueen.Name: // Value 7 (New)
                    Effect_DowagerQueen(gameOperations, actingPlayer, targetPlayer1, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Princess.Name: // Value 8 (Original)
                    // Princess effect is primarily on discard, handled by Prince or game rules.
                    // Playing her directly might have no immediate effect beyond being played.
                    _logger.LogInformation("Princess played by {PlayerName}. No direct effect on play, but discard rules apply.", actingPlayer.Name);
                    gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.CardPlayed, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Princess. If this was a forced discard (e.g. by Prince), they are out of the round.") { PlayedCard = cardPlayed });
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Bishop.Name: // Value 9 (New)
                    Effect_Bishop(gameOperations, actingPlayer, targetPlayer1, guessedPremiumRank, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Jester.Name: // Value 0 (New)
                    Effect_Jester(gameOperations, actingPlayer, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Assassin.Name: // Value 0 (New)
                     Effect_Assassin(gameOperations, actingPlayer, targetPlayer1, cardPlayed);
                    break;
                default:
                    // This case should ideally not be reached if all cards are implemented.
                    // Throwing an exception is more appropriate for an unhandled card type
                    // than trying to log it as a player-facing "no-op".
                    throw new InvalidOperationException($"No effect logic defined for card {premiumRankPlayed.Name} (Value: {premiumRankPlayed.Value}). This indicates an unimplemented card effect in PremiumDeckProvider.");
            }
        }

        private PremiumCardRank GetPremiumRankFromCard(Card card)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card), "Input card cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(card.AppearanceId) || !card.AppearanceId.StartsWith("premium-"))
            {
                throw new ArgumentException($"Card AppearanceId '{card.AppearanceId}' is not in the expected 'premium-<name>' format or is null/empty.", nameof(card.AppearanceId));
            }

            var namePart = card.AppearanceId.Substring("premium-".Length);
            if (string.IsNullOrWhiteSpace(namePart))
            {
                throw new ArgumentException($"Card AppearanceId '{card.AppearanceId}' has no name part after 'premium-'.", nameof(card.AppearanceId));
            }
            
            var potentialRankByName = PremiumCardRank.FromName(namePart.Replace('-', ' '));

            if (potentialRankByName != null)
            { 
                if (potentialRankByName.Value == card.Rank)
                {
                    return potentialRankByName;
                }
                else
                {
                    // Name matched, but rank value did not. This is a critical mismatch.
                    throw new InvalidOperationException(
                        $"Card AppearanceId '{card.AppearanceId}' (parsed as '{potentialRankByName.Name}') mapped to PremiumCardRank with value {potentialRankByName.Value}, but the card's actual Rank is {card.Rank}. This indicates a data inconsistency.");
                }
            }
            else
            {
                // Name did not match any known PremiumCardRank. 
                // Attempting to find by value alone is risky if multiple cards share a value or if AppearanceId is the primary key.
                // Given the expectation that AppearanceId should resolve, this is an error.
                throw new InvalidOperationException(
                    $"Could not find a PremiumCardRank matching the name part '{namePart}' (derived from AppearanceId '{card.AppearanceId}'). Card Rank value was {card.Rank}.");
            }
        }

        // Individual card effect methods (assuming these are largely correct as per previous review)
        // Ensure they use gameOperations for all state changes and logging.

        private void Effect_Guard(IGameOperations gameOperations, Player actingPlayer, Player? target, PremiumCardRank? guessedRank, Card cardPlayed)
        {
            if (target == null || guessedRank == null)
            {
                _logger.LogWarning("Guard effect failed: Target or guessed rank null for player {PlayerName}.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, "Played Guard but target or guessed rank was invalid.")
                {
                    PlayedCard = cardPlayed,
                    FizzleReason = "Target or guessed rank was null for Guard."
                });
                return;
            }

            // Assassin's Mark Check: If the target is marked, the assassin is eliminated.
            var assassinMarkerIdStr = gameOperations.GetPlayerDeckStatus(target.Id, DeckId, _assassinMarkKey);
            if (!string.IsNullOrEmpty(assassinMarkerIdStr) && Guid.TryParse(assassinMarkerIdStr, out var assassinPlayerId))
            {
                var assassinPlayer = gameOperations.GetPlayer(assassinPlayerId);
                _logger.LogInformation("Assassin's mark triggered. Guard played by {GuardingPlayer} against {TargetPlayer} caused {AssassinPlayer} to be eliminated.", actingPlayer.Name, target.Name, assassinPlayer?.Name ?? "Unknown");

                gameOperations.EliminatePlayer(assassinPlayerId, $"Eliminated because their Assassin's mark on {target.Name} was triggered by a Guard.", cardPlayed);
                gameOperations.AddLogEntry(new GameLogEntry(
                    eventType: GameLogEventType.PlayerEliminated,
                    actingPlayerId: actingPlayer.Id,
                    actingPlayerName: actingPlayer.Name,
                    message: $"{actingPlayer.Name} played the Princess and is eliminated!",
                    isPrivate: false)
                {
                    PlayedCard = cardPlayed,
                    RevealedCardOnElimination = target.Hand.GetHeldCard(),
                });

                // Clear the mark after it's triggered
                gameOperations.ClearPlayerDeckStatus(target.Id, DeckId, _assassinMarkKey);
                return; // The Guard's main effect does not proceed
            }

            if (guessedRank.Name == PremiumCardRank.Guard.Name)
            {
                 _logger.LogWarning("Guard effect failed: Player {PlayerName} guessed Guard.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, "Played Guard but incorrectly guessed Guard (not allowed).")
                {
                    PlayedCard = cardPlayed,
                    GuessedRank = guessedRank.Value,
                    FizzleReason = "Cannot guess Guard with Guard."
                });
                return;
            }

            var targetCard = gameOperations.GetPlayer(target.Id)?.Hand.GetHeldCard(); 
            if (targetCard != null)
            {
                var targetPremiumRank = GetPremiumRankFromCard(targetCard);
                if (targetPremiumRank != null && targetPremiumRank.Name == guessedRank.Name)
                {
                    _logger.LogInformation("Guard by {PlayerName} eliminated {TargetPlayerName}. Guessed: {GuessedRankName}", actingPlayer.Name, target.Name, guessedRank.Name);
                    gameOperations.EliminatePlayer(target.Id, $"Eliminated by {actingPlayer.Name}'s Guard (guessed {guessedRank.Name}).", cardPlayed);
                    gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.GuardHit, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name}'s Guard successfully targeted {target.Name} (guessed {guessedRank.Name}).")
                    {
                        PlayedCard = cardPlayed,
                        GuessedRank = guessedRank.Value,
                        WasGuessCorrect = true,
                        RevealedCardOnElimination = targetCard 
                    });
                }
                else
                {
                    _logger.LogInformation("Guard by {PlayerName} no effect on {TargetPlayerName}. Guessed: {GuessedRankName}, Had: {ActualRankName}", actingPlayer.Name, target.Name, guessedRank.Name, targetPremiumRank?.Name ?? "Unknown");
                    gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.GuardMiss, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name}'s Guard had no effect on {target.Name} (guessed {guessedRank.Name}, had {(targetPremiumRank != null ? targetPremiumRank.Name : "unknown")}).")
                    {
                        PlayedCard = cardPlayed,
                        GuessedRank = guessedRank.Value,
                        WasGuessCorrect = false,
                        GuessedPlayerActualCard = targetCard 
                    });
                }
            }
            else
            {
                _logger.LogWarning("Guard target {TargetPlayerName} had no card in hand or hand unknown for {PlayerName}'s Guard.", target.Name, actingPlayer.Name);
                 gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.GuardMiss, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name}'s Guard targeted {target.Name}, but their hand was empty or unknown.")
                 {
                    PlayedCard = cardPlayed,
                    GuessedRank = guessedRank.Value,
                    WasGuessCorrect = false
                 });
            }
        }

        private void Effect_Priest(IGameOperations gameOperations, Player actingPlayer, Player? target, Card cardPlayed)
        {
            if (target == null)
            {
                _logger.LogWarning("Priest effect failed for player {PlayerName}: Target null.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, "Played Priest but target was invalid.")
                {
                    PlayedCard = cardPlayed,
                    FizzleReason = "Invalid target for Priest."
                });
                return;
            }

            var targetActualPlayer = gameOperations.GetPlayer(target.Id);
            var targetCardInHand = targetActualPlayer?.Hand.GetHeldCard();

            if (targetCardInHand != null)
            {
                var targetPremiumRank = GetPremiumRankFromCard(targetCardInHand);
                _logger.LogInformation("Priest effect by {PlayerName}: viewed {TargetPlayerName}'s hand: {CardName} (Rank {CardRank})", actingPlayer.Name, target.Name, targetPremiumRank?.Name ?? "Unknown", targetCardInHand.Rank);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.PriestEffect, actingPlayer.Id, actingPlayer.Name, $"You used Priest to look at {target.Name}'s hand, revealing {targetPremiumRank?.Name ?? "Unknown"} (Rank {targetCardInHand.Rank}).", true) // isPrivate = true
                {
                    PlayedCard = cardPlayed,
                    TargetPlayerId = target.Id,
                    TargetPlayerName = target.Name,
                    RevealedPlayerCard = targetCardInHand
                });
            }
            else
            {
                _logger.LogInformation("Priest effect by {PlayerName}: viewed {TargetPlayerName}'s hand: Empty/Unknown", actingPlayer.Name, target.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.PriestEffect, actingPlayer.Id, actingPlayer.Name, $"You used Priest to look at {target.Name}'s hand, but it was empty/unknown.", true) // isPrivate = true
                {
                    PlayedCard = cardPlayed,
                    TargetPlayerId = target.Id,
                    TargetPlayerName = target.Name
                });
            }
        }

        private void Effect_Cardinal(IGameOperations gameOperations, Player actingPlayer, Player target1, Player target2, Card cardPlayed)
        {
            if (target1.Id == target2.Id)
            {
                _logger.LogInformation("Cardinal effect by {PlayerName}: targeted the same player {TargetName} twice. No trade.", actingPlayer.Name, target1.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, $"Played Cardinal targeting the same player ({target1.Name}) twice. No card trade occurred.")
                {
                    PlayedCard = cardPlayed,
                    TargetPlayerId = target1.Id,
                    TargetPlayerName = target1.Name,
                    FizzleReason = "Cannot target the same player twice for Cardinal trade."
                });
                return;
            }

            gameOperations.SwapPlayerHands(target1.Id, target2.Id);
            _logger.LogInformation("Cardinal effect by {PlayerName}: made {Target1Name} and {Target2Name} trade hands.", actingPlayer.Name, target1.Name, target2.Name);
            
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.CardinalEffect, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Cardinal. {target1.Name} and {target2.Name} traded hands.")
            {
                PlayedCard = cardPlayed
                // Additional target info is in the message for now.
            });

            Player? playerToRevealTo = null;
            Player? handOfPlayerSeen = null;

            if (actingPlayer.Id == target1.Id) { playerToRevealTo = actingPlayer; handOfPlayerSeen = target2; }
            else if (actingPlayer.Id == target2.Id) { playerToRevealTo = actingPlayer; handOfPlayerSeen = target1; }

            if (playerToRevealTo != null && handOfPlayerSeen != null)
            {
                var revealedCard = gameOperations.GetPlayer(handOfPlayerSeen.Id)?.Hand.GetHeldCard();
                if (revealedCard != null)
                {
                    var revealedPremiumRank = GetPremiumRankFromCard(revealedCard);
                    _logger.LogInformation("Cardinal view by {PlayerName}: saw {TargetName}'s new hand: {CardName} (Rank {CardRank})", playerToRevealTo.Name, handOfPlayerSeen.Name, revealedPremiumRank?.Name ?? "Unknown", revealedCard.Rank);
                    gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.CardinalEffect, playerToRevealTo.Id, playerToRevealTo.Name, $"As Cardinal player, you now see {handOfPlayerSeen.Name}'s new hand: {revealedPremiumRank?.Name ?? "Unknown"} (Rank {revealedCard.Rank}).", true) // isPrivate = true
                    {
                        PlayedCard = cardPlayed,
                        TargetPlayerId = handOfPlayerSeen.Id,
                        TargetPlayerName = handOfPlayerSeen.Name,
                        RevealedPlayerCard = revealedCard
                    });
                }
                else
                {
                    _logger.LogInformation("Cardinal view by {PlayerName}: saw {TargetName}'s new hand: Empty/Unknown", playerToRevealTo.Name, handOfPlayerSeen.Name);
                    gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.CardinalEffect, playerToRevealTo.Id, playerToRevealTo.Name, $"As Cardinal player, you attempt to see {handOfPlayerSeen.Name}'s new hand, but it's empty/unknown.", true) // isPrivate = true
                    {
                        PlayedCard = cardPlayed,
                        TargetPlayerId = handOfPlayerSeen.Id,
                        TargetPlayerName = handOfPlayerSeen.Name
                    });
                }
            }
        }

        private void Effect_Baron(IGameOperations gameOperations, Player actingPlayer, Player? target, Card cardPlayed)
        {
            if (target == null)
            {
                _logger.LogWarning("Baron effect failed for player {PlayerName}: Target null.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, "Played Baron but target was invalid.")
                {
                    PlayedCard = cardPlayed,
                    FizzleReason = "Invalid target for Baron."
                });
                return;
            }

            var actingPlayerActual = gameOperations.GetPlayer(actingPlayer.Id);
            var targetActual = gameOperations.GetPlayer(target.Id);

            var actingPlayerCardInHand = actingPlayerActual?.Hand.GetHeldCard();
            var targetPlayerCardInHand = targetActual?.Hand.GetHeldCard();

            if (actingPlayerCardInHand == null || targetPlayerCardInHand == null)
            {
                _logger.LogWarning("Baron effect failed for {PlayerName} against {TargetName}: One or both hands empty/unknown.", actingPlayer.Name, target.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Baron against {target.Name}, but one or both hands were empty/unknown. No comparison.")
                {
                    PlayedCard = cardPlayed,
                    TargetPlayerId = target.Id,
                    TargetPlayerName = target.Name,
                    FizzleReason = "One or both hands empty for Baron comparison."
                });
                return;
            }
            
            var actingPlayerRank = GetPremiumRankFromCard(actingPlayerCardInHand);
            var targetPlayerRank = GetPremiumRankFromCard(targetPlayerCardInHand);

            _logger.LogInformation("Baron comparison by {PlayerName} ({Card1Rank}) vs {TargetName} ({Card2Rank})", actingPlayer.Name, actingPlayerCardInHand.Rank, target.Name, targetPlayerCardInHand.Rank);

            // Private logs for players involved in comparison
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BaronCompare, actingPlayer.Id, actingPlayer.Name, $"You (Baron) are comparing your {actingPlayerRank?.Name ?? "Unknown"} (Rank {actingPlayerCardInHand.Rank}) with {target.Name}'s {targetPlayerRank?.Name ?? "Unknown"} (Rank {targetPlayerCardInHand.Rank}).", true) 
            {
                PlayedCard = cardPlayed,
                TargetPlayerId = target.Id, 
                TargetPlayerName = target.Name,
                ActingPlayerBaronCard = actingPlayerCardInHand, 
                TargetPlayerBaronCard = targetPlayerCardInHand 
            });
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BaronCompare, target.Id, target.Name, $"{actingPlayer.Name}'s Baron is comparing their {actingPlayerRank?.Name ?? "Unknown"} (Rank {actingPlayerCardInHand.Rank}) with your {targetPlayerRank?.Name ?? "Unknown"} (Rank {targetPlayerCardInHand.Rank}).", true) 
            {
                PlayedCard = cardPlayed,
                ActingPlayerId = actingPlayer.Id, 
                ActingPlayerName = actingPlayer.Name,
                ActingPlayerBaronCard = actingPlayerCardInHand, 
                TargetPlayerBaronCard = targetPlayerCardInHand 
            });

            if (actingPlayerCardInHand.Rank > targetPlayerCardInHand.Rank)
            {
                _logger.LogInformation("Baron result: {TargetName} eliminated by {ActingPlayerName}'s Baron.", target.Name, actingPlayer.Name);
                gameOperations.EliminatePlayer(target.Id, $"{actingPlayer.Name}'s Baron eliminated {target.Name} ({actingPlayerRank?.Name} > {targetPlayerRank?.Name}).", cardPlayed);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BaronCompare, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name}'s Baron eliminated {target.Name} ({actingPlayerRank?.Name} > {targetPlayerRank?.Name}).") 
                { 
                    PlayedCard = cardPlayed, 
                    BaronLoserPlayerId = target.Id,
                    RevealedCardOnElimination = targetPlayerCardInHand, // The loser's card is revealed
                    ActingPlayerBaronCard = actingPlayerCardInHand, 
                    TargetPlayerBaronCard = targetPlayerCardInHand 
                });
            }
            else if (targetPlayerCardInHand.Rank > actingPlayerCardInHand.Rank)
            {
                _logger.LogInformation("Baron result: {ActingPlayerName} eliminated by {TargetName} in Baron comparison.", actingPlayer.Name, target.Name);
                gameOperations.EliminatePlayer(actingPlayer.Id, $"{target.Name}'s Baron comparison eliminated {actingPlayer.Name} ({targetPlayerRank?.Name} > {actingPlayerRank?.Name}).", cardPlayed);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BaronCompare, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{target.Name} eliminated {actingPlayer.Name} with Baron ({targetPlayerRank?.Name} > {actingPlayerRank?.Name}).") 
                { 
                    PlayedCard = cardPlayed, 
                    BaronLoserPlayerId = actingPlayer.Id,
                    RevealedCardOnElimination = actingPlayerCardInHand, // The loser's card is revealed
                    ActingPlayerBaronCard = actingPlayerCardInHand, 
                    TargetPlayerBaronCard = targetPlayerCardInHand 
                });
            }
            else
            {
                _logger.LogInformation("Baron result: Tie between {ActingPlayerName} and {TargetName}. No elimination.", actingPlayer.Name, target.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BaronCompare, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name}'s Baron comparison with {target.Name} resulted in a tie ({actingPlayerRank?.Name} vs {targetPlayerRank?.Name}). No one is eliminated.") 
                { 
                    PlayedCard = cardPlayed, 
                    ActingPlayerBaronCard = actingPlayerCardInHand, 
                    TargetPlayerBaronCard = targetPlayerCardInHand 
                });
            }
        }

        private void Effect_Baroness(IGameOperations gameOperations, Player actingPlayer, Player? target1, Player? target2, Card cardPlayed)
        {
            if (target1 == null)
            {
                _logger.LogWarning("Baroness effect failed for player {PlayerName}: Target1 null.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, "Played Baroness but the primary target was invalid.")
                {
                    PlayedCard = cardPlayed,
                    FizzleReason = "Invalid primary target for Baroness."
                });
                return;
            }

            var target1Actual = gameOperations.GetPlayer(target1.Id);
            var target1CardInHand = target1Actual?.Hand.GetHeldCard();

            if (target1CardInHand != null)
            {
                var target1Rank = GetPremiumRankFromCard(target1CardInHand);
                _logger.LogInformation("Baroness effect by {PlayerName}: viewed {TargetName1}'s hand: {CardName1} (Rank {CardRank1})", actingPlayer.Name, target1.Name, target1Rank?.Name ?? "Unknown", target1CardInHand.Rank);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BaronessEffect, actingPlayer.Id, actingPlayer.Name, $"You used Baroness to look at {target1.Name}'s hand, revealing {target1Rank?.Name ?? "Unknown"} (Rank {target1CardInHand.Rank}).", true) // isPrivate = true
                {
                    PlayedCard = cardPlayed,
                    TargetPlayerId = target1.Id,
                    TargetPlayerName = target1.Name,
                    RevealedPlayerCard = target1CardInHand
                });
            }
            else
            {
                _logger.LogInformation("Baroness effect by {PlayerName}: viewed {TargetName1}'s hand: Empty/Unknown", actingPlayer.Name, target1.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BaronessEffect, actingPlayer.Id, actingPlayer.Name, $"You used Baroness to look at {target1.Name}'s hand, but it was empty/unknown.", true) // isPrivate = true
                {
                    PlayedCard = cardPlayed,
                    TargetPlayerId = target1.Id,
                    TargetPlayerName = target1.Name
                });
            }

            if (target2 != null)
            {
                var target2Actual = gameOperations.GetPlayer(target2.Id);
                var target2CardInHand = target2Actual?.Hand.GetHeldCard();
                if (target2CardInHand != null)
                {
                    var target2Rank = GetPremiumRankFromCard(target2CardInHand);
                    _logger.LogInformation("Baroness effect by {PlayerName}: also viewed {TargetName2}'s hand: {CardName2} (Rank {CardRank2})", actingPlayer.Name, target2.Name, target2Rank?.Name ?? "Unknown", target2CardInHand.Rank);
                    gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BaronessEffect, actingPlayer.Id, actingPlayer.Name, $"You used Baroness to also look at {target2.Name}'s hand, revealing {target2Rank?.Name ?? "Unknown"} (Rank {target2CardInHand.Rank}).", true) // isPrivate = true
                    {
                        PlayedCard = cardPlayed,
                        TargetPlayerId = target2.Id,
                        TargetPlayerName = target2.Name,
                        RevealedPlayerCard = target2CardInHand
                    });
                }
                else
                {
                    _logger.LogInformation("Baroness effect by {PlayerName}: also viewed {TargetName2}'s hand: Empty/Unknown", actingPlayer.Name, target2.Name);
                    gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BaronessEffect, actingPlayer.Id, actingPlayer.Name, $"You used Baroness to also look at {target2.Name}'s hand, but it was empty/unknown.", true) // isPrivate = true
                    {
                        PlayedCard = cardPlayed,
                        TargetPlayerId = target2.Id,
                        TargetPlayerName = target2.Name
                    });
                }
            }
        }

        private void Effect_Handmaid(IGameOperations gameOperations, Player actingPlayer, Card cardPlayed)
        {
            actingPlayer.SetProtection(true);
            _logger.LogInformation("{PlayerName} played Handmaid and is protected until their next turn.", actingPlayer.Name);
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.HandmaidProtection, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Handmaid and is protected until their next turn.")
            {
                PlayedCard = cardPlayed,
                TargetPlayerId = actingPlayer.Id, // Handmaid protects the acting player
                TargetPlayerName = actingPlayer.Name
            });
        }

        private void Effect_Sycophant(IGameOperations gameOperations, Player actingPlayer, Player? target, Card cardPlayed)
        {
            if (target == null)
            {
                _logger.LogWarning("Sycophant effect failed for player {PlayerName}: Target null.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, "Played Sycophant but target was invalid.")
                {
                    PlayedCard = cardPlayed,
                    FizzleReason = "Invalid target for Sycophant."
                });
                return;
            }

            // The game logic for Sycophant (forcing target) is likely handled by game rules when effects are resolved against a Sycophant-marked player.
            // Here, we just log that the Sycophant has marked a target.
            _logger.LogInformation("{PlayerName} played Sycophant, marking {TargetName}.", actingPlayer.Name, target.Name);
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.SycophantEffect, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Sycophant, marking {target.Name}. {target.Name} must target {actingPlayer.Name} with their next card effect, if possible.")
            {
                PlayedCard = cardPlayed,
                TargetPlayerId = target.Id,
                TargetPlayerName = target.Name
                // Potentially add a property like MarkedBySycophantTargetId = target.Id if GameLogEntry supports it and it's useful for UI.
            });
            // Actual enforcement of Sycophant's mark would be in gameOperations or target selection logic.
        }

        private void Effect_Prince(IGameOperations gameOperations, Player actingPlayer, Player? target, Card cardPlayed)
        {
            if (target == null)
            {
                _logger.LogWarning("Prince effect failed for player {PlayerName}: Target null.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, "Played Prince but target was invalid.")
                {
                    PlayedCard = cardPlayed,
                    FizzleReason = "Invalid target for Prince."
                });
                return;
            }

            var currentTargetState = gameOperations.GetPlayer(target.Id);
            if (currentTargetState == null) 
            {
                _logger.LogError("Prince effect failed: Target player {TargetPlayerId} not found after initial check.", target.Id);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, "Played Prince but target became unexpectedly invalid.")
                {
                    PlayedCard = cardPlayed,
                    FizzleReason = "Target player data inconsistent."
                });
                return;
            }

            if (currentTargetState.IsProtected && currentTargetState.Id != actingPlayer.Id)
            {
                _logger.LogInformation("Prince effect by {PlayerName} on {TargetName} fizzled due to Handmaid protection.", actingPlayer.Name, currentTargetState.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, $"The Prince's effect on {currentTargetState.Name} was blocked by Handmaid protection.")
                {
                    PlayedCard = cardPlayed,
                    TargetPlayerId = currentTargetState.Id,
                    TargetPlayerName = currentTargetState.Name,
                    FizzleReason = "Target is protected by Handmaid."
                });
                return;
            }

            Card? discardedCard = target.DiscardHand(); 

            if (discardedCard == null) 
            {
                _logger.LogInformation("Prince effect by {PlayerName} on {TargetName}: Target had no card to discard or DiscardHeldCard returned null.", actingPlayer.Name, currentTargetState.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Prince on {currentTargetState.Name}, but they had no card to discard.")
                {
                    PlayedCard = cardPlayed,
                    TargetPlayerId = currentTargetState.Id,
                    TargetPlayerName = currentTargetState.Name,
                    FizzleReason = "Target has no cards to discard for Prince or discard failed."
                });
                return;
            }

            var discardedPremiumRank = GetPremiumRankFromCard(discardedCard); // Throws on invalid card

            _logger.LogInformation("Prince effect by {PlayerName}: {TargetName} discarded {DiscardedCardName} (Rank {DiscardedCardRank}).", 
                actingPlayer.Name, currentTargetState.Name, discardedPremiumRank?.Name ?? "Unknown Card", discardedCard.Rank);

            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.PrinceDiscard, actingPlayer.Id, actingPlayer.Name,
                $"{actingPlayer.Name} used the Prince to make {currentTargetState.Name} discard their {discardedPremiumRank?.Name ?? "card"}.")
            {
                PlayedCard = cardPlayed,
                TargetPlayerId = currentTargetState.Id,
                TargetPlayerName = currentTargetState.Name,
                TargetDiscardedCard = discardedCard,
                IsPrivate = (discardedPremiumRank?.Name == PremiumCardRank.Princess.Name) 
            });

            if (discardedPremiumRank?.Name == PremiumCardRank.Princess.Name)
            {
                _logger.LogInformation("{TargetName} discarded Princess due to Prince effect by {PlayerName} and is eliminated.", currentTargetState.Name, actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.PlayerEliminated, currentTargetState.Id, currentTargetState.Name, 
                    $"{currentTargetState.Name} discarded the Princess and is eliminated!")
                {
                    PlayedCard = cardPlayed, // The Prince card that caused the discard
                    RevealedCardOnElimination = discardedCard // The Princess card itself
                });
                gameOperations.EliminatePlayer(currentTargetState.Id, "discarded the Princess", cardPlayed);
                return; // Effect ends here
            }

            // If player is not eliminated, they draw a new card
            if (currentTargetState.Status != PlayerStatus.Eliminated)
            {
                var drawnCard = gameOperations.DrawCardForPlayer(currentTargetState.Id);
                if (drawnCard != null)
                {
                    _logger.LogInformation("{TargetName} drew a new card after discarding due to Prince effect by {PlayerName}.", currentTargetState.Name, actingPlayer.Name);
                    gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.PlayerDrewCard, currentTargetState.Id, currentTargetState.Name,
                        $"{currentTargetState.Name} drew a new card after discarding due to the Prince.")
                    {
                        DrawnCard = drawnCard, // For private logs to the player
                        IsPrivate = true       // Only the drawing player should know what they drew
                    });
                }
                else
                {
                    _logger.LogInformation("Deck is empty. {TargetName} could not draw a new card after discarding due to Prince effect by {PlayerName}.", currentTargetState.Name, actingPlayer.Name);
                    gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.DeckEmpty, currentTargetState.Id, currentTargetState.Name,
                        $"{currentTargetState.Name} could not draw a new card as the deck is empty.")
                    {
                        PlayedCard = cardPlayed // Context: this happened because of the Prince
                    });
                }
            }
        }

        private void Effect_Count(IGameOperations gameOperations, Player actingPlayer, Card cardPlayed)
        {
            var actingPlayerActual = gameOperations.GetPlayer(actingPlayer.Id);
            var cardInHand = actingPlayerActual?.Hand.GetHeldCard();

            if (cardInHand != null)
            {
                var premiumRankInHand = GetPremiumRankFromCard(cardInHand);
                _logger.LogInformation("{PlayerName} played Count and looked at their own hand: {CardName} (Rank {CardRank}).", actingPlayer.Name, premiumRankInHand?.Name ?? "Unknown", cardInHand.Rank);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.CountEffect, actingPlayer.Id, actingPlayer.Name, $"You played Count and looked at your hand, revealing {premiumRankInHand?.Name ?? "Unknown"} (Rank {cardInHand.Rank}).", true) // isPrivate = true
                {
                    PlayedCard = cardPlayed,
                    RevealedPlayerCard = cardInHand // The card they looked at in their own hand
                });

                // Note: The logic for Countess being forced by Count (if Countess is also in hand)
                // is typically handled by game rules or a separate check after a card like Count is played/resolved,
                // rather than directly within the Count's effect method here. This method just logs the Count's primary effect.
            }
            else
            {
                _logger.LogInformation("{PlayerName} played Count but their hand was empty/unknown.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.CountEffect, actingPlayer.Id, actingPlayer.Name, "You played Count, but your hand was empty/unknown.", true) // isPrivate = true
                {
                    PlayedCard = cardPlayed
                });
            }
        }

        private void Effect_King(IGameOperations gameOperations, Player actingPlayer, Player? target, Card cardPlayed)
        {
            if (target == null)
            {
                _logger.LogWarning("King effect failed for player {PlayerName}: Target null.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, "Played King but target was invalid.")
                {
                    PlayedCard = cardPlayed,
                    FizzleReason = "Invalid target for King."
                });
                return;
            }

            if (actingPlayer.Id == target.Id)
            {
                _logger.LogInformation("King effect by {PlayerName}: Targeted self. No hand swap.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played King targeting themself. No hand swap occurred.")
                {
                    PlayedCard = cardPlayed,
                    TargetPlayerId = target.Id,
                    TargetPlayerName = target.Name,
                    FizzleReason = "Cannot target self with King for hand swap."
                });
                return;
            }

            gameOperations.SwapPlayerHands(actingPlayer.Id, target.Id);
            _logger.LogInformation("{PlayerName} played King and traded hands with {TargetName}.", actingPlayer.Name, target.Name);
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.KingTrade, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played King and traded hands with {target.Name}.")
            {
                PlayedCard = cardPlayed,
                TargetPlayerId = target.Id,
                TargetPlayerName = target.Name
            });
        }

        private void Effect_Constable(IGameOperations gameOperations, Player actingPlayer, Card cardPlayed)
        {
            // Constable's effect is passive: If you eliminate another player while you have the Constable, take a Jester token.
            // Playing the Constable itself doesn't grant a token immediately.
            // The actual Jester token gain would be logged when the condition is met (elimination).
            _logger.LogInformation("{PlayerName} played Constable. Their passive ability is now active.", actingPlayer.Name);
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.CardPlayed, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Constable. If they eliminate another player holding a Jester token, they will gain a Jester token.")
            {
                PlayedCard = cardPlayed
                // No specific target for playing Constable itself. Its effect is conditional on future eliminations.
            });
            // Note: Game logic elsewhere should check if an eliminating player has Constable and the eliminated player has a Jester token.
        }

        private void Effect_Countess(IGameOperations gameOperations, Player actingPlayer, Card cardPlayed)
        {
            // Countess's main effect is being forced to be played if held with King, Prince, or Count.
            // Playing her otherwise has no direct effect beyond being played.
            _logger.LogInformation("{PlayerName} played Countess.", actingPlayer.Name);
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.CardPlayed, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Countess. If held with a King, Prince, or Count, it must be played.")
            {
                PlayedCard = cardPlayed
            });
        }

        private void Effect_DowagerQueen(IGameOperations gameOperations, Player actingPlayer, Player? target, Card cardPlayed)
        {
            if (target == null)
            {
                _logger.LogWarning("Dowager Queen effect failed for player {PlayerName}: Target null.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, "Played Dowager Queen but target was invalid.")
                {
                    PlayedCard = cardPlayed,
                    FizzleReason = "Invalid target for Dowager Queen."
                });
                return;
            }

            var actingPlayerActual = gameOperations.GetPlayer(actingPlayer.Id);
            var targetActual = gameOperations.GetPlayer(target.Id);

            var actingPlayerCardInHand = actingPlayerActual?.Hand.GetHeldCard();
            var targetPlayerCardInHand = targetActual?.Hand.GetHeldCard();

            if (actingPlayerCardInHand == null || targetPlayerCardInHand == null)
            {
                _logger.LogWarning("Dowager Queen effect failed for {PlayerName} against {TargetName}: One or both hands empty/unknown.", actingPlayer.Name, target.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Dowager Queen against {target.Name}, but one or both hands were empty/unknown. No comparison.")
                {
                    PlayedCard = cardPlayed,
                    TargetPlayerId = target.Id,
                    TargetPlayerName = target.Name,
                    FizzleReason = "One or both hands empty for Dowager Queen comparison."
                });
                return;
            }

            var actingPlayerRank = GetPremiumRankFromCard(actingPlayerCardInHand);
            var targetPlayerRank = GetPremiumRankFromCard(targetPlayerCardInHand);

            _logger.LogInformation("Dowager Queen comparison by {PlayerName} ({Card1Rank}) vs {TargetName} ({Card2Rank})", 
                actingPlayer.Name, actingPlayerCardInHand.Rank, target.Name, targetPlayerCardInHand.Rank);
            
            // Private log for acting player
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.DowagerQueenCompare, actingPlayer.Id, actingPlayer.Name, $"You (Dowager Queen) compared your {actingPlayerRank?.Name ?? "Unknown"} (Rank {actingPlayerCardInHand.Rank}) with {target.Name}'s {targetPlayerRank?.Name ?? "Unknown"} (Rank {targetPlayerCardInHand.Rank}).", true)
            {
                PlayedCard = cardPlayed,
                TargetPlayerId = target.Id,
                TargetPlayerName = target.Name
                // ActingPlayerCard and TargetPlayerCard could be added if GameLogEntry supports them for this event.
            });
            // Private log for target player
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.DowagerQueenCompare, target.Id, target.Name, $"{actingPlayer.Name} (Dowager Queen) compared their {actingPlayerRank?.Name ?? "Unknown"} (Rank {actingPlayerCardInHand.Rank}) with your {targetPlayerRank?.Name ?? "Unknown"} (Rank {targetPlayerCardInHand.Rank}).", true)
            {
                PlayedCard = cardPlayed,
                ActingPlayerId = actingPlayer.Id,
                ActingPlayerName = actingPlayer.Name
            });

            if (actingPlayerCardInHand.Rank > targetPlayerCardInHand.Rank)
            {
                _logger.LogInformation("Dowager Queen result: {ActingPlayerName} has the higher card.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.DowagerQueenCompare, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name}'s Dowager Queen revealed they have a higher card ({actingPlayerRank?.Name}) than {target.Name} ({targetPlayerRank?.Name}).")
                {
                    PlayedCard = cardPlayed,
                    WinnerPlayerId = actingPlayer.Id
                });
            }
            else if (targetPlayerCardInHand.Rank > actingPlayerCardInHand.Rank)
            {
                _logger.LogInformation("Dowager Queen result: {TargetName} has the higher card.", target.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.DowagerQueenCompare, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{target.Name} revealed they have a higher card ({targetPlayerRank?.Name}) than {actingPlayer.Name} ({actingPlayerRank?.Name}) in Dowager Queen comparison.")
                {
                    PlayedCard = cardPlayed,
                    WinnerPlayerId = target.Id
                });
            }
            else
            {
                _logger.LogInformation("Dowager Queen result: Tie between {ActingPlayerName} and {TargetName}.", actingPlayer.Name, target.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.DowagerQueenCompare, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name}'s Dowager Queen comparison with {target.Name} resulted in a tie ({actingPlayerRank?.Name} vs {targetPlayerRank?.Name}).")
                {
                    PlayedCard = cardPlayed
                    // DowagerQueenWinnerId remains null for a tie
                });
            }
        }

        // Princess effect is handled in ExecuteCardEffect as it's about being discarded.

        private void Effect_Bishop(IGameOperations gameOperations, Player actingPlayer, Player? target, PremiumCardRank? guessedPremiumRank, Card cardPlayed)
        {
            if (target == null)
            {
                _logger.LogWarning("Bishop effect failed for player {PlayerName}: Target null.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, "Played Bishop but target was invalid.")
                {
                    PlayedCard = cardPlayed,
                    FizzleReason = "Invalid target for Bishop."
                });
                return;
            }

            if (guessedPremiumRank == null)
            {
                _logger.LogWarning("Bishop effect failed for player {PlayerName}: Guessed rank null.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, "Played Bishop but no card rank was guessed.")
                {
                    PlayedCard = cardPlayed,
                    TargetPlayerId = target.Id,
                    TargetPlayerName = target.Name,
                    FizzleReason = "No card rank guessed for Bishop."
                });
                return;
            }
            
            var targetActual = gameOperations.GetPlayer(target.Id);
            var targetCardInHand = targetActual?.Hand.GetHeldCard();

            if (targetCardInHand == null)
            {
                _logger.LogInformation("Bishop effect by {PlayerName} on {TargetName}: Target has no card to check.", actingPlayer.Name, target.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Bishop on {target.Name}, but they had no card.")
                {
                    PlayedCard = cardPlayed,
                    TargetPlayerId = target.Id,
                    TargetPlayerName = target.Name,
                    GuessedRank = guessedPremiumRank.Value,
                    FizzleReason = "Target has no card for Bishop to check."
                });
                return;
            }

            var targetActualPremiumRank = GetPremiumRankFromCard(targetCardInHand);
            bool guessCorrect = targetActualPremiumRank?.Value == guessedPremiumRank.Value;

            _logger.LogInformation("Bishop effect by {PlayerName} on {TargetName}: Guessed {GuessedRankName} ({GuessedRankValue}). Target has {ActualRankName} ({ActualRankValue}). Correct: {IsCorrect}", 
                actingPlayer.Name, target.Name, guessedPremiumRank.Name, guessedPremiumRank.Value, targetActualPremiumRank?.Name ?? "Unknown", targetActualPremiumRank?.Value ?? -1, guessCorrect);

            // Private log for acting player about their guess
            gameOperations.AddLogEntry(new GameLogEntry(guessCorrect ? GameLogEventType.BishopGuessCorrect : GameLogEventType.BishopGuessIncorrect, actingPlayer.Id, actingPlayer.Name, $"You played Bishop and guessed {guessedPremiumRank.Name} for {target.Name}'s card. Your guess was {(guessCorrect ? "correct" : "incorrect")}.", true)
            {
                PlayedCard = cardPlayed,
                TargetPlayerId = target.Id,
                TargetPlayerName = target.Name,
                GuessedRank = guessedPremiumRank.Value,
                RevealedPlayerCard = targetCardInHand, // Reveal to Bishop player what the card was
                WasGuessCorrect = guessCorrect,
                IsPrivate = true
            });

            if (guessCorrect)
            {
                AssignJesterToken(gameOperations, target);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BishopGuessCorrect, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name}'s Bishop correctly guessed {target.Name}'s card ({guessedPremiumRank.Name}). {target.Name} gains a Jester token.")
                {
                    PlayedCard = cardPlayed,
                    GuessedRank = guessedPremiumRank.Value,
                    WasGuessCorrect = true
                    // RevealedPlayerCard is not for public log
                });
            }
            else
            {
                // Public log for incorrect guess (optional, could just be silent or a generic message)
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BishopGuessIncorrect, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name}'s Bishop incorrectly guessed {target.Name}'s card.")
                {
                    PlayedCard = cardPlayed,
                    GuessedRank = guessedPremiumRank.Value,
                    WasGuessCorrect = false
                });
            }
        }

        private void AssignJesterToken(IGameOperations gameOperations, Player target)
        {
            gameOperations.SetPlayerDeckStatus(target.Id,DeckId,_jesterToken, "true");
        }

        private void Effect_Jester(IGameOperations gameOperations, Player actingPlayer, Card cardPlayed)
        {
            if (gameOperations.GetPlayerDeckStatus(actingPlayer.Id, DeckId, _jesterToken) != "true")
            {
                AssignJesterToken(gameOperations, actingPlayer);
                _logger.LogInformation("{PlayerName} played Jester and gained a Jester token.", actingPlayer.Name);
                // The AssignJesterToken method is expected to create the primary JesterTokenAssigned log entry.
                // We can add a CardPlayed log here to signify the Jester was the source, if desired, or rely on AssignJesterToken's context.
            }
            else
            {
                _logger.LogInformation("{PlayerName} played Jester, but already had a Jester token. No new token assigned.", actingPlayer.Name);
            }
        }

         private void Effect_Assassin(IGameOperations gameOperations, Player actingPlayer, Player? target, Card cardPlayed)
         {
             if (target == null)
             {
                _logger.LogWarning("Assassin effect failed for player {PlayerName}: Target null.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, "Played Assassin but the target was invalid.")
                {
                    PlayedCard = cardPlayed,
                    FizzleReason = "Invalid target for Assassin."
                });
                return;
             }

            MarkPlayerWithAssassin(gameOperations, target, actingPlayer);
            _logger.LogInformation("{PlayerName} played Assassin, marking {TargetName}.", actingPlayer.Name, target.Name);
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.AssassinMarked, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name} played Assassin, marking {target.Name}. If {target.Name} is targeted by a Guard, {actingPlayer.Name} will be eliminated.")
            {
                PlayedCard = cardPlayed
            });
         }

        private void MarkPlayerWithAssassin(IGameOperations gameOperations, Player target, Player source)
        {
            gameOperations.SetPlayerDeckStatus(target.Id, DeckId, _assassinMarkKey, source.Id.ToString());
        }
    }
}