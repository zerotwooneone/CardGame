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
        private static readonly Guid _deckId = new("A9A5F11F-4760-4A2C-9B6C-4F9F1CF9A717");
        private const string _sycophantMarkKey = "SycophantMarkedBy"; // Player ID of the Sycophant who marked them
        private const string _jesterTokenKey = "JesterToken"; // Token indicating a player has a Jester token
        private static readonly List<Card> _allPremiumCards = new List<Card>();

        public Guid DeckId => _deckId;
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

            Player? effectiveTarget1 = targetPlayer1;
            Player? effectiveTarget2 = targetPlayer2;

            // --- Sycophant Mark Redirection --- 
            // This must happen BEFORE Handmaid protection is checked for the *effective* target.
            if (effectiveTarget1 != null && premiumRankPlayed.MinTargets > 0 && effectiveTarget1.Id != actingPlayer.Id && premiumRankPlayed.Name != PremiumCardRank.Sycophant.Name) // Card targets an opponent, and is not Sycophant itself placing a mark
            {
                var allPlayers = gameOperations.GetGameState().Players; 
                foreach (var p in allPlayers)
                {
                    var sycophantMarkerPlayerIdStr = gameOperations.GetPlayerDeckStatus(p.Id, DeckId, _sycophantMarkKey);
                    if (!string.IsNullOrEmpty(sycophantMarkerPlayerIdStr)) // Player p is the victim of a Sycophant
                    {
                        var sycophantVictimPlayer = gameOperations.GetPlayer(p.Id);
                        if (sycophantVictimPlayer != null && sycophantVictimPlayer.Id != actingPlayer.Id) // Sycophant victim is an opponent of actingPlayer
                        {
                            bool victimIsValidForThisCard = !(sycophantVictimPlayer.Id == actingPlayer.Id && !premiumRankPlayed.CanTargetSelf);
                            // Potentially add other checks if 'premiumRankPlayed' has specific targeting restrictions beyond 'opponent' and 'self'

                            if (victimIsValidForThisCard)
                            {
                                _logger.LogInformation("Sycophant mark active on {SycophantVictimName}. {ActingPlayerName}'s {CardPlayedName} targeting {OriginalTargetName} will be redirected to {SycophantVictimName}.",
                                    sycophantVictimPlayer.Name, actingPlayer.Name, premiumRankPlayed.Name, effectiveTarget1.Name, sycophantVictimPlayer.Name);
                                
                                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.SycophantEffect, 
                                    actingPlayer.Id, 
                                    actingPlayer.Name,
                                    $@"{actingPlayer.Name}'s {premiumRankPlayed.Name} play, originally targeting {effectiveTarget1.Name}, was redirected to {sycophantVictimPlayer.Name} due to a Sycophant's influence.")
                                {
                                    PlayedCard = cardPlayed, 
                                    TargetPlayerId = sycophantVictimPlayer.Id, 
                                    TargetPlayerName = sycophantVictimPlayer.Name
                                });

                                effectiveTarget1 = sycophantVictimPlayer; // THE REDIRECTION
                                gameOperations.ClearPlayerDeckStatus(sycophantVictimPlayer.Id, DeckId, _sycophantMarkKey); // Mark is consumed
                                break; // Only one Sycophant mark can redirect a single play
                            }
                        }
                    }
                }
            }

            // --- Overall Target Count Validation (based on initially provided targets) ---
            int providedTargetsCount = (targetPlayer1 != null ? 1 : 0) + (targetPlayer2 != null ? 1 : 0);

            if (providedTargetsCount < premiumRankPlayed.MinTargets || providedTargetsCount > premiumRankPlayed.MaxTargets)
            {
                throw new ArgumentException($"Card '{premiumRankPlayed.Name}' requires between {premiumRankPlayed.MinTargets} and {premiumRankPlayed.MaxTargets} targets, but {providedTargetsCount} were provided.");
            }

            // --- Target 1 Specific Validation (if it was provided and card can have targets) ---
            if (premiumRankPlayed.MaxTargets > 0 && targetPlayer1 != null)
            {
                if (targetPlayer1.Id == actingPlayer.Id && !premiumRankPlayed.CanTargetSelf)
                {
                    throw new ArgumentException($"Card '{premiumRankPlayed.Name}' cannot target self, but acting player was targeted as targetPlayer1.", nameof(targetPlayer1));
                }
                if (targetPlayer1.IsProtected && premiumRankPlayed.Name != PremiumCardRank.Sycophant.Name) 
                {
                    _logger.LogInformation("Primary target player {TargetName} is protected by Handmaid. Effect on them will be skipped.", targetPlayer1.Name);
                    gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played {premiumRankPlayed.Name} targeting {targetPlayer1.Name}, but {targetPlayer1.Name} is protected. Effect on this target is skipped.")
                    {
                        PlayedCard = cardPlayed,
                        TargetPlayerId = targetPlayer1.Id,
                        TargetPlayerName = targetPlayer1.Name,
                        FizzleReason = "Target is protected by Handmaid."
                    });
                    effectiveTarget1 = null; 
                }
            }

            // --- Target 2 Specific Validation (if it was provided and card can have a second target) ---
            if (premiumRankPlayed.MaxTargets > 1 && targetPlayer2 != null) // MaxTargets > 1 implies a second target slot is potentially valid
            {
                // Specific validation for Cardinal: targets must be different.
                if (premiumRankPlayed.Name == PremiumCardRank.Cardinal.Name && targetPlayer1 != null && targetPlayer1.Id == targetPlayer2.Id)
                {
                    throw new ArgumentException($"Cardinal requires two different players to trade hands, but {targetPlayer1.Name} was targeted twice.");
                }

                if (targetPlayer2.Id == actingPlayer.Id && !premiumRankPlayed.CanTargetSelf)
                {
                    throw new ArgumentException($"Card '{premiumRankPlayed.Name}' cannot target self, but acting player was targeted as targetPlayer2.", nameof(targetPlayer2));
                }
                if (targetPlayer2.IsProtected && premiumRankPlayed.Name != PremiumCardRank.Sycophant.Name) 
                {
                    _logger.LogInformation("Secondary target player {TargetName} is protected by Handmaid. Effect on them will be skipped.", targetPlayer2.Name);
                    gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played {premiumRankPlayed.Name} targeting {targetPlayer2.Name}, but {targetPlayer2.Name} is protected. Effect on this target is skipped.")
                    {
                        PlayedCard = cardPlayed,
                        TargetPlayerId = targetPlayer2.Id,
                        TargetPlayerName = targetPlayer2.Name,
                        FizzleReason = "Target is protected by Handmaid."
                    });
                    effectiveTarget2 = null; 
                }
            }

            // --- Final Check: Do we still have enough *effective* targets after protection? ---
            int effectiveTargetsCount = (effectiveTarget1 != null ? 1 : 0) + (effectiveTarget2 != null ? 1 : 0);
            if (effectiveTargetsCount < premiumRankPlayed.MinTargets)
            {
                _logger.LogWarning("Card {PremiumRankName} requires at least {MinTargets} effective target(s) after protection checks, but only {EffectiveCount} remain valid.", 
                                 premiumRankPlayed.Name, premiumRankPlayed.MinTargets, effectiveTargetsCount);
                // Only add a new fizzle log if one wasn't already added for initial target count mismatch.
                if (providedTargetsCount >= premiumRankPlayed.MinTargets) 
                {
                    gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played {premiumRankPlayed.Name}, but all required targets became invalid (e.g., due to protection or other reasons). Effect cannot proceed.")
                    {
                        PlayedCard = cardPlayed,
                        FizzleReason = "Required targets became invalid after validation."
                    });
                }
                return;
            }
            
            // If we've reached here, basic target validation has passed or adjusted targets.
            _logger.LogDebug("Executing effect for card {PremiumRankName} played by {PlayerName}. EffectiveTarget1: {Target1Name}, EffectiveTarget2: {Target2Name}", 
                           premiumRankPlayed.Name, actingPlayer.Name, effectiveTarget1?.Name ?? "None", effectiveTarget2?.Name ?? "None");
            
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.CardPlayed, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played {premiumRankPlayed.Name} ({premiumRankPlayed.Description}).")
            {
                PlayedCard = cardPlayed
            });

            // Effect logic based on the card's rank name
            switch (premiumRankPlayed.Name)
            {
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Guard.Name: 
                    Effect_Guard(gameOperations, actingPlayer, effectiveTarget1!, guessedPremiumRank, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Priest.Name: 
                    Effect_Priest(gameOperations, actingPlayer, effectiveTarget1!, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Cardinal.Name: 
                    // MinTargets for Cardinal is 2. The validation above ensures effectiveTarget1 and effectiveTarget2 are non-null if we reach here.
                    Effect_Cardinal(gameOperations, actingPlayer, effectiveTarget1!, effectiveTarget2!, cardPlayed); 
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Baron.Name: 
                    Effect_Baron(gameOperations, actingPlayer, effectiveTarget1!, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Baroness.Name: 
                    // Baroness MinTargets is 1, MaxTargets is 2. Effect_Baroness should handle if effectiveTarget2 is null.
                    Effect_Baroness(gameOperations, actingPlayer, effectiveTarget1, effectiveTarget2, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Handmaid.Name: 
                    Effect_Handmaid(gameOperations, actingPlayer, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Sycophant.Name: 
                    Effect_Sycophant(gameOperations, actingPlayer, effectiveTarget1!, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Prince.Name: 
                    Effect_Prince(gameOperations, actingPlayer, effectiveTarget1!, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Count.Name: 
                    Effect_Count(gameOperations, actingPlayer, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.King.Name: 
                    Effect_King(gameOperations, actingPlayer, effectiveTarget1!, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Constable.Name:
                    Effect_Constable(gameOperations, actingPlayer, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Countess.Name: 
                    Effect_Countess(gameOperations, actingPlayer, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.DowagerQueen.Name: 
                    Effect_DowagerQueen(gameOperations, actingPlayer, effectiveTarget1!, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Bishop.Name: 
                    Effect_Bishop(gameOperations, actingPlayer, effectiveTarget1!, guessedPremiumRank, cardPlayed);
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Jester.Name: 
                    Effect_Jester(gameOperations, actingPlayer, cardPlayed); 
                    break;
                case var _ when premiumRankPlayed.Name == PremiumCardRank.Assassin.Name: 
                    Effect_Assassin(gameOperations, actingPlayer, cardPlayed);
                    break;
                default:
                    // This case indicates a programming error where a new PremiumCardRank was added
                    // without its corresponding effect logic in this switch statement.
                    throw new NotImplementedException($"The card effect for '{premiumRankPlayed.Name}' has not been implemented in PremiumDeckProvider.ExecuteCardEffect.");
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

        private void Effect_Guard(IGameOperations gameOperations, Player actingPlayer, Player target, PremiumCardRank guessedRank, Card cardPlayed)
        {
            var currentTargetState = gameOperations.GetPlayer(target.Id) 
                ?? throw new InvalidOperationException($"Guard target player {target.Name} ({target.Id}) not found in game state.");
            
            var targetCardInHand = currentTargetState.Hand.GetHeldCard() 
                ?? throw new InvalidOperationException($"Guard target {target.Name} ({currentTargetState.Name}) has no card to check.");

            // Assassin Reaction Check (as per CardEffects.md)
            var targetHeldRank = GetPremiumRankFromCard(targetCardInHand);
            if (targetHeldRank.Name == PremiumCardRank.Assassin.Name)
            {
                _logger.LogInformation("Assassin revealed! {TargetPlayerName} held Assassin against {ActingPlayerName}'s Guard. {ActingPlayerName} is eliminated.", target.Name, actingPlayer.Name, actingPlayer.Name);
                
                gameOperations.EliminatePlayer(actingPlayer.Id, $"Eliminated by {target.Name}'s revealed Assassin when playing Guard.", cardPlayed); // cardPlayed is the Guard
                target.DiscardHand();
                gameOperations.DrawCardForPlayer(target.Id);

                gameOperations.AddLogEntry(new GameLogEntry(
                    GameLogEventType.PlayerEliminated, 
                    target.Id, // The Assassin holder is the "actor" of this specific event outcome
                    target.Name,
                    $@"{actingPlayer.Name} was eliminated by {target.Name}'s revealed Assassin. {target.Name} discarded Assassin and drew a new card.")
                {
                    PlayedCard = cardPlayed, // Guard
                    TargetPlayerId = actingPlayer.Id, // Player who was eliminated
                    TargetPlayerName = actingPlayer.Name,
                    RevealedCardOnElimination = targetCardInHand // The Assassin
                });
                return; // Guard's main effect does not proceed
            }

            // Rule: Cannot guess Guard with a Guard.
            if (guessedRank.Name == PremiumCardRank.Guard.Name)
            {
                _logger.LogDebug("Guard effect fizzled: Player {PlayerName} guessed Guard.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Guard but incorrectly guessed Guard (not allowed).")
                {
                    PlayedCard = cardPlayed,
                    TargetPlayerId = target.Id,
                    TargetPlayerName = target.Name,
                    GuessedRank = guessedRank.Value,
                    FizzleReason = "Cannot guess Guard with Guard."
                });
                return;
            }

            var targetPremiumRank = GetPremiumRankFromCard(targetCardInHand); // Re-get rank in case it wasn't Assassin
            if (targetPremiumRank != null && targetPremiumRank.Name == guessedRank.Name)
            {
                _logger.LogDebug("Guard by {PlayerName} eliminated {TargetPlayerName}. Guessed: {GuessedRankName}", actingPlayer.Name, target.Name, guessedRank.Name);
                gameOperations.EliminatePlayer(target.Id, $"Eliminated by {actingPlayer.Name}'s Guard (guessed {guessedRank.Name}).", cardPlayed);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.GuardHit, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name}'s Guard successfully targeted {target.Name} (guessed {guessedRank.Name}).")
                {
                    PlayedCard = cardPlayed,
                    GuessedRank = guessedRank.Value,
                    WasGuessCorrect = true,
                    RevealedCardOnElimination = targetCardInHand 
                });
            }
            else
            {
                _logger.LogDebug("Guard by {PlayerName} no effect on {TargetPlayerName}. Guessed: {GuessedRankName}, Had: {ActualRankName}", actingPlayer.Name, target.Name, guessedRank.Name, targetPremiumRank?.Name ?? "Unknown");
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.GuardMiss, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name}'s Guard had no effect on {target.Name} (guessed {guessedRank.Name}, had {(targetPremiumRank != null ? targetPremiumRank.Name : "unknown")}).")
                {
                    PlayedCard = cardPlayed,
                    GuessedRank = guessedRank.Value,
                    WasGuessCorrect = false,
                    GuessedPlayerActualCard = targetCardInHand 
                });
            }
        }

        private void Effect_Priest(IGameOperations gameOperations, Player actingPlayer, Player target, Card cardPlayed)
        {
            var targetPlayerActual = gameOperations.GetPlayer(target.Id) 
                ?? throw new InvalidOperationException($"Priest target player {target.Name} ({target.Id}) not found in game state.");
            
            var targetCardInHand = targetPlayerActual.Hand.GetHeldCard() 
                ?? throw new InvalidOperationException($"Priest target {target.Name} ({targetPlayerActual.Name}) has no card to reveal.");

            var revealedRank = GetPremiumRankFromCard(targetCardInHand);
            _logger.LogDebug("{ActingPlayerName} sees {TargetPlayerName}'s hand, which contains {RevealedCardName}", actingPlayer.Name, target.Name, revealedRank.Name);
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.PriestEffect, actingPlayer.Id, actingPlayer.Name, $"You used Priest to look at {target.Name}'s hand. They have: {revealedRank.Name} (Rank {revealedRank.Value}).", true) // isPrivate = true
            {
                PlayedCard = cardPlayed,
                TargetPlayerId = target.Id,
                TargetPlayerName = target.Name,
                RevealedPlayerCard = targetCardInHand
            });
        }

        private void Effect_Cardinal(IGameOperations gameOperations, Player actingPlayer, Player target1, Player target2, Card cardPlayed)
        {
            // Target existence and distinctness already validated by ExecuteCardEffect.
            // Now, ensure both players actually have cards to swap.
            var player1Actual = gameOperations.GetPlayer(target1.Id) 
                ?? throw new InvalidOperationException($"Cardinal target player {target1.Name} ({target1.Id}) not found in game state."); // Should not happen if ExecuteCardEffect is correct
            var player2Actual = gameOperations.GetPlayer(target2.Id) 
                ?? throw new InvalidOperationException($"Cardinal target player {target2.Name} ({target2.Id}) not found in game state."); // Should not happen

            var target1CardInHand = player1Actual.Hand.GetHeldCard() 
                ?? throw new InvalidOperationException($"Cardinal target {player1Actual.Name} has no card to swap.");
            var target2CardInHand = player2Actual.Hand.GetHeldCard() 
                ?? throw new InvalidOperationException($"Cardinal target {player2Actual.Name} has no card to swap.");

            gameOperations.SwapPlayerHands(target1.Id, target2.Id);

            _logger.LogDebug("{PlayerName} played Cardinal and made {Target1Name} and {Target2Name} trade hands.", actingPlayer.Name, target1.Name, target2.Name);
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.CardinalEffect, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Cardinal and made {target1.Name} and {target2.Name} trade hands.")
            {
                PlayedCard = cardPlayed,
                TargetPlayerId = target1.Id, // Primary target for log context
                TargetPlayerName = target1.Name,
                // Secondary target info is in the message for now.
                // Consider adding TargetPlayer2Id, TargetPlayer2Name to GameLogEntry if needed more broadly
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
                    _logger.LogDebug("Cardinal view by {PlayerName}: saw {TargetName}'s new hand: {CardName} (Rank {CardRank})", playerToRevealTo.Name, handOfPlayerSeen.Name, revealedPremiumRank?.Name ?? "Unknown", revealedCard.Rank);
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
                    _logger.LogDebug("Cardinal view by {PlayerName}: saw {TargetName}'s new hand: Empty/Unknown", playerToRevealTo.Name, handOfPlayerSeen.Name);
                    gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.CardinalEffect, playerToRevealTo.Id, playerToRevealTo.Name, $"As Cardinal player, you attempt to see {handOfPlayerSeen.Name}'s new hand, but it's empty/unknown.", true) // isPrivate = true
                    {
                        PlayedCard = cardPlayed,
                        TargetPlayerId = handOfPlayerSeen.Id,
                        TargetPlayerName = handOfPlayerSeen.Name
                    });
                }
            }
        }

        private void Effect_Baron(IGameOperations gameOperations, Player actingPlayer, Player target, Card cardPlayed)
        {
            var targetPlayerActual = gameOperations.GetPlayer(target.Id) 
                ?? throw new InvalidOperationException($"Baron target player {target.Name} ({target.Id}) not found in game state.");
            
            var actingPlayerCard = actingPlayer.Hand.GetHeldCard() 
                ?? throw new InvalidOperationException($"Baron failed: Acting player {actingPlayer.Name} has no card to compare.");
            var targetCard = targetPlayerActual.Hand.GetHeldCard() 
                ?? throw new InvalidOperationException($"Baron failed: Target player {target.Name} has no card to compare.");

            var actingPlayerRank = GetPremiumRankFromCard(actingPlayerCard);
            var targetRank = GetPremiumRankFromCard(targetCard);

            _logger.LogDebug("Baron comparison by {PlayerName} ({Card1Rank}) vs {TargetName} ({Card2Rank})", actingPlayer.Name, actingPlayerCard.Rank, target.Name, targetCard.Rank);

            // Private logs for players involved in comparison
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BaronCompare, actingPlayer.Id, actingPlayer.Name, $"You (Baron) are comparing your {actingPlayerRank?.Name ?? "Unknown"} (Rank {actingPlayerCard.Rank}) with {target.Name}'s {targetRank?.Name ?? "Unknown"} (Rank {targetCard.Rank}).", true) 
            {
                PlayedCard = cardPlayed,
                TargetPlayerId = target.Id, 
                TargetPlayerName = target.Name,
                ActingPlayerBaronCard = actingPlayerCard, 
                TargetPlayerBaronCard = targetCard 
            });
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BaronCompare, target.Id, target.Name, $"{actingPlayer.Name}'s Baron is comparing their {actingPlayerRank?.Name ?? "Unknown"} (Rank {actingPlayerCard.Rank}) with your {targetRank?.Name ?? "Unknown"} (Rank {targetCard.Rank}).", true) 
            {
                PlayedCard = cardPlayed,
                ActingPlayerId = actingPlayer.Id, 
                ActingPlayerName = actingPlayer.Name,
                ActingPlayerBaronCard = actingPlayerCard, 
                TargetPlayerBaronCard = targetCard 
            });

            if (actingPlayerCard.Rank > targetCard.Rank)
            {
                _logger.LogDebug("Baron result: {TargetName} eliminated by {ActingPlayerName}'s Baron.", target.Name, actingPlayer.Name);
                gameOperations.EliminatePlayer(target.Id, $"{actingPlayer.Name}'s Baron eliminated {target.Name} ({actingPlayerRank?.Name} > {targetRank?.Name}).", cardPlayed);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BaronCompare, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name}'s Baron eliminated {target.Name} ({actingPlayerRank?.Name} > {targetRank?.Name}).") 
                { 
                    PlayedCard = cardPlayed, 
                    BaronLoserPlayerId = target.Id,
                    RevealedCardOnElimination = targetCard, // The loser's card is revealed
                    ActingPlayerBaronCard = actingPlayerCard, 
                    TargetPlayerBaronCard = targetCard 
                });
            }
            else if (targetCard.Rank > actingPlayerCard.Rank)
            {
                _logger.LogDebug("Baron result: {ActingPlayerName} eliminated by {TargetName} in Baron comparison.", actingPlayer.Name, target.Name);
                gameOperations.EliminatePlayer(actingPlayer.Id, $"{target.Name}'s Baron comparison eliminated {actingPlayer.Name} ({targetRank?.Name} > {actingPlayerRank?.Name}).", cardPlayed);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BaronCompare, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{target.Name} eliminated {actingPlayer.Name} with Baron ({targetRank?.Name} > {actingPlayerRank?.Name}).") 
                { 
                    PlayedCard = cardPlayed, 
                    BaronLoserPlayerId = actingPlayer.Id,
                    RevealedCardOnElimination = actingPlayerCard, // The loser's card is revealed
                    ActingPlayerBaronCard = actingPlayerCard, 
                    TargetPlayerBaronCard = targetCard 
                });
            }
            else // Ranks are equal
            {
                _logger.LogDebug("Baron result: Ranks equal between {ActingPlayerName} and {TargetName}. No one eliminated.", actingPlayer.Name, target.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BaronCompare, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name} and {target.Name} compared cards with Baron, but ranks were equal. No one was eliminated.")
                {
                    PlayedCard = cardPlayed,
                    ActingPlayerBaronCard = actingPlayerCard, 
                    TargetPlayerBaronCard = targetCard 
                    // No BaronLoserPlayerId as it's a tie
                });
            }
        }

        private void Effect_Baroness(IGameOperations gameOperations, Player actingPlayer, Player target1, Player? target2, Card cardPlayed)
        {
            var target1CardInHand = gameOperations.GetPlayer(target1!.Id)?.Hand.GetHeldCard() 
                ?? throw new InvalidOperationException($"Baroness target {target1.Name} ({target1.Id}) has no card to reveal.");

            var target1Rank = GetPremiumRankFromCard(target1CardInHand);
            _logger.LogDebug("Baroness effect by {PlayerName}: viewed {TargetName1}'s hand: {CardName1} (Rank {CardRank1})", 
                actingPlayer.Name, target1!.Name, target1Rank.Name, target1CardInHand.Rank);
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BaronessEffect, actingPlayer.Id, actingPlayer.Name, $"You used Baroness to look at {target1!.Name}'s hand, revealing {target1Rank.Name} (Rank {target1CardInHand.Rank}).", true) // isPrivate = true
            {
                PlayedCard = cardPlayed,
                TargetPlayerId = target1.Id,
                TargetPlayerName = target1.Name,
                RevealedPlayerCard = target1CardInHand
            });

            // If there's a second target, reveal their hand too
            if (target2 != null)
            {
                var target2CardInHand = gameOperations.GetPlayer(target2.Id)?.Hand.GetHeldCard() 
                    ?? throw new InvalidOperationException($"Baroness target {target2.Name} ({target2.Id}) has no card to reveal.");

                var target2Rank = GetPremiumRankFromCard(target2CardInHand);
                _logger.LogDebug("Baroness effect by {PlayerName}: also viewed {TargetName2}'s hand: {CardName2} (Rank {CardRank2})", 
                    actingPlayer.Name, target2.Name, target2Rank.Name, target2CardInHand.Rank);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BaronessEffect, actingPlayer.Id, actingPlayer.Name, $"You used Baroness to also look at {target2.Name}'s hand, revealing {target2Rank.Name} (Rank {target2CardInHand.Rank}).", true) // isPrivate = true
                {
                    PlayedCard = cardPlayed,
                    TargetPlayerId = target2.Id,
                    TargetPlayerName = target2.Name,
                    RevealedPlayerCard = target2CardInHand
                });
            }
        }

        private void Effect_Handmaid(IGameOperations gameOperations, Player actingPlayer, Card cardPlayed)
        {
            actingPlayer.SetProtection(true);
            _logger.LogDebug("Handmaid effect: {PlayerName} is protected until their next turn.", actingPlayer.Name);
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.HandmaidProtection, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Handmaid and is protected until their next turn.")
            {
                PlayedCard = cardPlayed,
                TargetPlayerId = actingPlayer.Id, // Handmaid protects the acting player
                TargetPlayerName = actingPlayer.Name
            });
        }

        private void Effect_Sycophant(IGameOperations gameOperations, Player actingPlayer, Player target, Card cardPlayed)
        {
            // Target existence already validated by ExecuteCardEffect.
            // Mark the target player with the Sycophant's mark, storing the acting player's ID as the one who placed it.
            gameOperations.SetPlayerDeckStatus(target.Id, DeckId, _sycophantMarkKey, actingPlayer.Id.ToString());

            _logger.LogDebug("{PlayerName} played Sycophant, marking {TargetName}.", actingPlayer.Name, target.Name);
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.SycophantEffect, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Sycophant, marking {target.Name}. The next applicable targeted card play by any player must target {target.Name}.")
            {
                PlayedCard = cardPlayed,
                TargetPlayerId = target.Id,
                TargetPlayerName = target.Name
            });
        }

        private void Effect_Prince(IGameOperations gameOperations, Player actingPlayer, Player target, Card cardPlayed)
        {
            _logger.LogDebug("Executing Prince effect: {ActingPlayerName} targets {TargetPlayerName}", actingPlayer.Name, target.Name);

            var targetPlayerActual = gameOperations.GetPlayer(target.Id) 
                ?? throw new InvalidOperationException($"Prince target player {target.Name} ({target.Id}) not found in game state.");
            
            var discardedCard = targetPlayerActual.DiscardHand();

            if (discardedCard == null)
            {
                _logger.LogInformation("Prince target {TargetPlayerName} had no card to discard.", targetPlayerActual.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled,
                    actingPlayer.Id, actingPlayer.Name,
                    $"{actingPlayer.Name} uses the Prince on {targetPlayerActual.Name}, but they had no card.")
                { PlayedCard = cardPlayed,  FizzleReason = "Target had no card in hand." });
                return;
            }

            var discardedRank = GetPremiumRankFromCard(discardedCard);
            _logger.LogInformation("Prince forces {TargetPlayerName} to discard {DiscardedCardName}", targetPlayerActual.Name, discardedRank.Name);

            if (discardedRank.Name == PremiumCardRank.Princess.Name)
            {
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.PlayerEliminated,
                    targetPlayerActual.Id, targetPlayerActual.Name,
                    $"{targetPlayerActual.Name} is forced by {actingPlayer.Name}'s Prince to discard the Princess and is eliminated.")
                { PlayedCard = cardPlayed, RevealedCardOnElimination = discardedCard  });
                gameOperations.EliminatePlayer(targetPlayerActual.Id, "Discarded the Princess", cardPlayed);
            }
            else
            {
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.PrinceDiscard,
                    targetPlayerActual.Id, targetPlayerActual.Name,
                    $"{actingPlayer.Name}'s Prince forces {targetPlayerActual.Name} to discard their hand.")
                { PlayedCard = cardPlayed, RevealedCardOnElimination =  discardedCard  });

                var drawnCard = gameOperations.DrawCardForPlayer(targetPlayerActual.Id);
                if (drawnCard == null)
                {
                    _logger.LogInformation("{TargetPlayerName} discarded, but the deck is empty and they are eliminated.", targetPlayerActual.Name);
                    gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.PlayerEliminated,
                        targetPlayerActual.Id, targetPlayerActual.Name,
                        $"{targetPlayerActual.Name} discarded their card, but was eliminated because the deck is empty.")
                    { PlayedCard = cardPlayed, RevealedCardOnElimination =  discardedCard  });
                    gameOperations.EliminatePlayer(targetPlayerActual.Id, "Forced to draw from an empty deck", cardPlayed);
                }
            }
        }

        private void Effect_Count(IGameOperations gameOperations, Player actingPlayer, Card cardPlayed)
        {
            var playerActual = gameOperations.GetPlayer(actingPlayer.Id);
            var cardInHand = playerActual?.Hand.GetHeldCard(); // Should always exist as player just played a card and has one left

            if (cardInHand != null)
            {
                var premiumRankInHand = GetPremiumRankFromCard(cardInHand);
                _logger.LogDebug("Count effect: {PlayerName} adds current card value {CardValue} to their score pile.", 
                    actingPlayer.Name, premiumRankInHand.Value);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.CountEffect, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Count and added their current card's value ({premiumRankInHand.Value}) to their score pile.")
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
                // This should ideally not happen if player has a card after playing one.
                _logger.LogWarning("Count effect: {PlayerName} had no card in hand after playing Count. This is unexpected.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.EffectFizzled, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played Count, but an issue occurred accessing their hand.")
                {
                    PlayedCard = cardPlayed,
                    FizzleReason = "Player had no card in hand after playing Count."
                });
            }
        }

        private void Effect_King(IGameOperations gameOperations, Player actingPlayer, Player target, Card cardPlayed)
        {
            _ = gameOperations.GetPlayer(target.Id) ?? throw new InvalidOperationException($"King target player {target.Name} ({target.Id}) not found in game state.");
            
            gameOperations.SwapPlayerHands(actingPlayer.Id, target.Id);
            _logger.LogDebug("King effect: {ActingPlayerName} swapped hands with {TargetPlayerName}", actingPlayer.Name, target.Name);
            gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.KingTrade, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played King and traded hands with {target.Name}.")
            {
                PlayedCard = cardPlayed,
                TargetPlayerId = target.Id,
                TargetPlayerName = target.Name
            });
        }

        private void Effect_Constable(IGameOperations gameOperations, Player actingPlayer, Card cardPlayed)
        {
            _logger.LogDebug("Executing Constable effect for {PlayerName}. This card has a passive effect and does not generate a game log entry on its own.", actingPlayer.Name);
            // No game log entry is needed here. The effect is passive.
            // The awarding of a Jester token will be logged if/when the player is eliminated.
            gameOperations.SetPlayerDeckStatus(actingPlayer.Id, DeckId, PremiumCardRank.Constable.Name, "true");
        }

        private void Effect_Countess(IGameOperations gameOperations, Player actingPlayer, Card cardPlayed)
        {
            _logger.LogDebug("Executing Countess effect for {PlayerName}. No immediate effect, so no game log entry is generated.", actingPlayer.Name);
            // No game log entry is needed. The core game engine logs the card play,
            // and since there is no subsequent effect, there's nothing more to add to the player-facing log.
        }

        private void Effect_DowagerQueen(IGameOperations gameOperations, Player actingPlayer, Player target, Card cardPlayed)
        {
            var targetPlayerActual = gameOperations.GetPlayer(target.Id) 
                ?? throw new InvalidOperationException($"Dowager Queen target player {target.Name} ({target.Id}) not found in game state.");
            
            var actingPlayerCard = actingPlayer.Hand.GetHeldCard() 
                ?? throw new InvalidOperationException($"Dowager Queen failed: Acting player {actingPlayer.Name} has no card to compare.");
            var targetCard = targetPlayerActual.Hand.GetHeldCard() 
                ?? throw new InvalidOperationException($"Dowager Queen failed: Target player {target.Name} has no card to compare.");

            var actingPlayerRank = GetPremiumRankFromCard(actingPlayerCard);
            var targetRank = GetPremiumRankFromCard(targetCard);

            _logger.LogDebug("Dowager Queen by {PlayerName} (has {ActingCardRank}) vs {TargetName} (has {TargetCardRank}): {WinnerName} wins comparison.", 
                actingPlayer.Name, actingPlayerRank.Name, target.Name, targetRank.Name, 
                actingPlayerRank.Value > targetRank.Value ? actingPlayer.Name : (targetRank.Value > actingPlayerRank.Value ? target.Name : "Tie"));

            if (actingPlayerRank.Value > targetRank.Value)
            {
                _logger.LogDebug("Dowager Queen result: {ActingPlayerName} has the higher card.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.DowagerQueenCompare, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name}'s Dowager Queen revealed they have a higher card ({actingPlayerRank.Name}) than {target.Name} ({targetRank.Name}).")
                {
                    PlayedCard = cardPlayed,
                    WinnerPlayerId = actingPlayer.Id
                });
            }
            else if (targetRank.Value > actingPlayerRank.Value)
            {
                _logger.LogDebug("Dowager Queen result: {TargetName} has the higher card.", target.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.DowagerQueenCompare, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{target.Name} revealed they have a higher card ({targetRank.Name}) than {actingPlayer.Name} ({actingPlayerRank.Name}) in Dowager Queen comparison.")
                {
                    PlayedCard = cardPlayed,
                    WinnerPlayerId = target.Id
                });
            }
            else
            {
                _logger.LogDebug("Dowager Queen result: Tie between {ActingPlayerName} and {TargetName}.", actingPlayer.Name, target.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.DowagerQueenCompare, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name}'s Dowager Queen comparison with {target.Name} resulted in a tie ({actingPlayerRank.Name} vs {targetRank.Name}).")
                {
                    PlayedCard = cardPlayed
                });
            }
        }

        // Princess effect is handled in ExecuteCardEffect as it's about being discarded.

        private void Effect_Bishop(IGameOperations gameOperations, Player actingPlayer, Player target, PremiumCardRank guessedRank, Card cardPlayed)
        {
            var targetCardInHand = gameOperations.GetPlayer(target.Id)?.Hand.GetHeldCard() 
                ?? throw new InvalidOperationException($"Bishop target {target.Name} ({target.Id}) has no card to guess against.");

            var actualRank = GetPremiumRankFromCard(targetCardInHand);
            bool guessCorrect = actualRank.Value == guessedRank.Value;

            _logger.LogDebug("Bishop effect by {PlayerName} on {TargetName}: Guessed {GuessedRankName} ({GuessedRankValue}). Target has {ActualRankName} ({ActualRankValue}). Correct: {IsCorrect}", 
                actingPlayer.Name, target.Name, guessedRank.Name, guessedRank.Value, actualRank.Name, actualRank.Value, guessCorrect);

            // Private log for acting player about their guess
            gameOperations.AddLogEntry(new GameLogEntry(guessCorrect ? GameLogEventType.BishopGuessCorrect : GameLogEventType.BishopGuessIncorrect, actingPlayer.Id, actingPlayer.Name, $"You played Bishop and guessed {guessedRank.Name} for {target.Name}'s card. Your guess was {(guessCorrect ? "correct" : "incorrect")}.", true)
            {
                PlayedCard = cardPlayed,
                TargetPlayerId = target.Id,
                TargetPlayerName = target.Name,
                GuessedRank = guessedRank.Value,
                WasGuessCorrect = guessCorrect
            });

            // Public log about the outcome
            if (guessCorrect)
            {
                _logger.LogInformation("Bishop by {PlayerName} on {TargetName}: Correctly guessed {GuessedRankName}. {TargetName} discards and draws. {PlayerName} gains a token.", 
                    actingPlayer.Name, target.Name, guessedRank.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BishopGuessCorrect, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name}'s Bishop correctly guessed {target.Name}'s card ({guessedRank.Name}). {actingPlayer.Name} gains a token. {target.Name} discards and draws.")
                {
                    PlayedCard = cardPlayed,
                    GuessedRank = guessedRank.Value,
                    WasGuessCorrect = true,
                    RevealedCardOnElimination = targetCardInHand // Not elimination, but card is revealed
                });

                gameOperations.AwardAffectionToken(actingPlayer.Id, 1);
                gameOperations.GetPlayer(target.Id)?.DiscardHand(); // Discard their current hand
                var drawnCard = gameOperations.DrawCardForPlayer(target.Id); // Draw a new card
                if (drawnCard == null)
                {
                    gameOperations.EliminatePlayer(target.Id, "Forced to draw from an empty deck after Bishop guess.", cardPlayed);
                }
            }
            else // Guess incorrect
            {
                _logger.LogInformation("Bishop by {PlayerName} on {TargetName}: Incorrectly guessed {GuessedRankName}. Target had {ActualRankName}. No effect.", 
                    actingPlayer.Name, target.Name, guessedRank.Name, actualRank.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.BishopGuessIncorrect, actingPlayer.Id, actingPlayer.Name, target.Id, target.Name, $"{actingPlayer.Name}'s Bishop incorrectly guessed {target.Name}'s card (guessed {guessedRank.Name}, had {actualRank.Name}). No effect.")
                {
                    PlayedCard = cardPlayed,
                    GuessedRank = guessedRank.Value,
                    WasGuessCorrect = false,
                    GuessedPlayerActualCard = targetCardInHand
                });
            }
        }

        private void AssignJesterToken(IGameOperations gameOperations, Player target)
        {
            gameOperations.SetPlayerDeckStatus(target.Id,DeckId,_jesterTokenKey, "true");
        }

        private void Effect_Jester(IGameOperations gameOperations, Player actingPlayer, Card cardPlayed)
        {
            if (gameOperations.GetPlayerDeckStatus(actingPlayer.Id, DeckId, _jesterTokenKey) != "true")
            {
                AssignJesterToken(gameOperations, actingPlayer);
                _logger.LogDebug("{PlayerName} played Jester and gained a Jester token.", actingPlayer.Name);
                gameOperations.AddLogEntry(new GameLogEntry(GameLogEventType.JesterTokenAssigned, actingPlayer.Id, actingPlayer.Name, $"{actingPlayer.Name} played the Jester and gained a Jester Token!")
                {
                    PlayedCard = cardPlayed
                });
            }
            else
            {
                _logger.LogDebug("{PlayerName} played Jester, but already had a Jester token. No new token assigned.", actingPlayer.Name);
            }
        }

         private void Effect_Assassin(IGameOperations gameOperations, Player actingPlayer, Card cardPlayed) // Target parameter removed as Assassin has no targets when played
         {
             // As per CardEffects.md: "Does nothing when played."
             // The reactive part is handled in the effect of the card targeting the Assassin holder (e.g., Guard).
             _logger.LogDebug("Assassin played by {PlayerName}. Effect: Does nothing on its own.", actingPlayer.Name);
         }
    }
}