using CardGame.Domain.Game;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using CardGame.Domain.Game.GameException;
using CardGame.Domain.Game.Event; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

namespace CardGame.Domain.Providers
{
    public abstract class BaseDeckProvider : IDeckProvider
    {
        protected record CardQuantity(CardRank Rank, int Count);

        // Abstract properties to be implemented by derived classes
        public abstract Guid DeckId { get; }
        public abstract string DisplayName { get; }
        public abstract string Description { get; }
        protected abstract string ThemeName { get; }
        protected abstract string DeckBackAppearanceId { get; }

        /// <summary>
        /// Gets the list of card types and their quantities for this deck.
        /// </summary>
        protected abstract IEnumerable<CardQuantity> GetCardQuantities();

        /// <summary>
        /// Gets the appearance ID for a given card type, based on the theme.
        /// Derived classes can override for more complex appearance logic.
        /// </summary>
        protected virtual string GetCardAppearanceId(CardRank cardRank)
        {
            return $"assets/decks/{ThemeName}/{cardRank.Name.ToLowerInvariant()}.webp";
        }

        public DeckDefinition GetDeck()
        {
            var cards = new List<Card>();
            foreach (var quantity in GetCardQuantities())
            {
                var appearanceId = GetCardAppearanceId(quantity.Rank);
                for (int i = 0; i < quantity.Count; i++)
                {
                    cards.Add(new Card(appearanceId, quantity.Rank));
                }
            }
            return new DeckDefinition(cards.ToImmutableList(), DeckBackAppearanceId, this);
        }

        public virtual void ExecuteCardEffect(IGameOperations game, Player actingPlayer, Card card, Player? targetPlayer, CardRank? guessedCardType)
        {
            // General Countess Rule: If holding Countess AND (King or Prince), must play Countess.
            bool handHasCountess = actingPlayer.Hand.Cards.Any(c => c.Rank == CardRank.Countess);
            bool handHasKing = actingPlayer.Hand.Cards.Any(c => c.Rank == CardRank.King);
            bool handHasPrince = actingPlayer.Hand.Cards.Any(c => c.Rank == CardRank.Prince);

            if (handHasCountess && (handHasKing || handHasPrince))
            {
                if (card.Rank == CardRank.King || card.Rank == CardRank.Prince)
                {
                    throw new InvalidMoveException($"Cannot play {card.Rank.Name} when holding the Countess (and King/Prince). You must play the Countess.");
                }
            }

            // Target validation
            if (RequiresTargetPlayer(card.Rank))
            {
                if (targetPlayer == null)
                {
                    throw new InvalidMoveException($"{card.Rank.Name} requires a target player.");
                }
                if (!CanTargetSelf(card.Rank) && targetPlayer.Id == actingPlayer.Id)
                {
                    throw new InvalidMoveException($"{card.Rank.Name} cannot target self.");
                }
            }

            // Guess validation
            if (RequiresGuess(card.Rank))
            {
                if (guessedCardType == null)
                {
                    throw new InvalidMoveException($"{card.Rank.Name} requires a guessed card type.");
                }
                if (guessedCardType == CardRank.Guard)
                {
                    throw new InvalidMoveException("Cannot guess Guard with a Guard.");
                }
            }

            PerformCardEffect(game, actingPlayer, card, targetPlayer, guessedCardType);
        }

        /// <summary>
        /// Performs the actual effect of the card after validation has passed.
        /// Derived classes must implement this to define card behaviors.
        /// </summary>
        protected virtual void PerformCardEffect(IGameOperations game, Player actingPlayer, Card card, Player? targetPlayer, CardRank? guessedCardType)
        {
            switch (card.Rank.Value)
            {
                case 1: // CardType.Guard
                    ExecuteGuardEffect(game, actingPlayer, targetPlayer!, guessedCardType!, card);
                    break;
                case 2: // CardType.Priest
                    ExecutePriestEffect(game, actingPlayer, targetPlayer!, card);
                    break;
                case 3: // CardType.Baron
                    ExecuteBaronEffect(game, actingPlayer, targetPlayer!, card);
                    break;
                case 4: // CardType.Handmaid
                    ExecuteHandmaidEffect(game, actingPlayer, card); // Handmaid does not use targetPlayer or guessedCardType
                    break;
                case 5: // CardType.Prince
                    ExecutePrinceEffect(game, actingPlayer, targetPlayer!, card); // Prince targetPlayer is validated by RequiresTargetPlayer
                    break;
                case 6: // CardType.King
                    ExecuteKingEffect(game, actingPlayer, targetPlayer!, card);
                    break;
                case 7: // CardType.Countess
                    ExecuteCountessEffect(game, actingPlayer, card); // Countess does not use targetPlayer or guessedCardType
                    break;
                case 8: // CardType.Princess
                    ExecutePrincessEffect(game, actingPlayer, card); // Princess does not use targetPlayer or guessedCardType
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(card.Rank), $"Unknown card type value: {card.Rank.Value} (Name: {card.Rank.Name})");
            }
        }

        // Common rule implementations - can be overridden if a deck changes these fundamental rules
        public virtual bool RequiresTargetPlayer(CardRank cardRank) =>
            cardRank == CardRank.Guard || 
            cardRank == CardRank.Priest || 
            cardRank == CardRank.Baron || 
            cardRank == CardRank.Prince || 
            cardRank == CardRank.King;
    
        public virtual bool CanTargetSelf(CardRank cardRank) => 
            cardRank == CardRank.Prince; 

        public virtual bool RequiresGuess(CardRank cardRank) => 
            cardRank == CardRank.Guard;

        // Default Card Effect Implementations
        protected virtual void ExecuteGuardEffect(IGameOperations game, Player actingPlayer, Player targetPlayer, CardRank? guessedCardType, Card guardCard)
        {
            if (targetPlayer.IsProtected)
            {
                game.AddLogEntry(new GameLogEntry(
                    eventType: GameLogEventType.EffectFizzled,
                    actingPlayerId: actingPlayer.Id,
                    actingPlayerName: actingPlayer.Name,
                    message: $"The Guard's guess was blocked by the Handmaid's protection.",
                    isPrivate: false)
                {
                    FizzleReason = "Target is protected by Handmaid",
                    PlayedCard = guardCard
                });
                return;
            }

            var targetCard = targetPlayer.Hand.Cards.First();
            if (targetCard.Rank == guessedCardType)
            {
                game.AddLogEntry(new GameLogEntry(
                    eventType: GameLogEventType.GuardHit,
                    actingPlayerId: actingPlayer.Id,
                    actingPlayerName: actingPlayer.Name,
                    targetPlayerId: targetPlayer.Id,
                    targetPlayerName: targetPlayer.Name,
                    message: $"{actingPlayer.Name} correctly guessed {targetPlayer.Name}'s {targetCard.Rank.Name} with the Guard!")
                {
                    PlayedCard = guardCard,
                    GuessedRank = guessedCardType,
                    GuessedPlayerActualCard = targetCard,
                    WasGuessCorrect = true,
                    RevealedCardOnElimination = targetCard
                });
                
                game.EliminatePlayer(targetPlayer.Id, "were correctly guessed by the Guard", guardCard);
            }
            else
            {
                game.AddLogEntry(new GameLogEntry(
                    eventType: GameLogEventType.GuardMiss,
                    actingPlayerId: actingPlayer.Id,
                    actingPlayerName: actingPlayer.Name,
                    targetPlayerId: targetPlayer.Id,
                    targetPlayerName: targetPlayer.Name,
                    message: $"{actingPlayer.Name} guessed {guessedCardType?.Name ?? "nothing"} for {targetPlayer.Name}'s card, but was wrong.")
                {
                    PlayedCard = guardCard,
                    GuessedRank = guessedCardType,
                    WasGuessCorrect = false
                });
            }
        }
        
        protected virtual void ExecutePriestEffect(IGameOperations game, Player actingPlayer, Player targetPlayer, Card priestCard)
        {
            if (targetPlayer.IsProtected)
            {
                game.AddLogEntry(new GameLogEntry(
                    GameLogEventType.EffectFizzled,
                    actingPlayer.Id,
                    actingPlayer.Name,
                    $"{targetPlayer.Name} is protected by the Handmaid and cannot be targeted.")
                );
                return;
            }

            var targetCard = targetPlayer.Hand.Cards.First();
            
            game.AddLogEntry(new GameLogEntry(
                GameLogEventType.PriestEffect,
                actingPlayer.Id,
                actingPlayer.Name,
                $"{actingPlayer.Name} looked at {targetPlayer.Name}'s {targetCard.Rank.Name} with the Priest.")
            );
        }
        
        protected virtual void ExecuteBaronEffect(IGameOperations game, Player actingPlayer, Player targetPlayer, Card baronCard)
        {
            if (targetPlayer.IsProtected)
            {
                game.AddLogEntry(new GameLogEntry(
                    GameLogEventType.EffectFizzled,
                    actingPlayer.Id,
                    actingPlayer.Name,
                    $"{targetPlayer.Name} is protected by the Handmaid and cannot be targeted.")
                );
                return;
            }

            var actingPlayerCard = actingPlayer.Hand.Cards.First();
            var targetPlayerCard = targetPlayer.Hand.Cards.First();
            
            if (actingPlayerCard.Rank.Value > targetPlayerCard.Rank.Value)
            {
                game.AddLogEntry(new GameLogEntry(
                    GameLogEventType.BaronCompare,
                    actingPlayer.Id,
                    actingPlayer.Name,
                    $"{actingPlayer.Name} played Baron against {targetPlayer.Name} and won with {actingPlayerCard.Rank.Name} vs {targetPlayerCard.Rank.Name}")
                );
                
                game.EliminatePlayer(targetPlayer.Id, "lost a Baron comparison", baronCard);
            }
            else if (actingPlayerCard.Rank.Value < targetPlayerCard.Rank.Value)
            {
                game.AddLogEntry(new GameLogEntry(
                    GameLogEventType.BaronCompare,
                    actingPlayer.Id,
                    actingPlayer.Name,
                    $"{actingPlayer.Name} played Baron against {targetPlayer.Name} and lost with {actingPlayerCard.Rank.Name} vs {targetPlayerCard.Rank.Name}")
                );
                
                game.EliminatePlayer(actingPlayer.Id, "lost a Baron comparison", baronCard);
            }
            else
            {
                game.AddLogEntry(new GameLogEntry(
                    GameLogEventType.BaronCompare,
                    actingPlayer.Id,
                    actingPlayer.Name,
                    $"{actingPlayer.Name} played Baron against {targetPlayer.Name} but it was a tie with {actingPlayerCard.Rank.Name}")
                );
            }
        }
        
        protected virtual void ExecuteHandmaidEffect(IGameOperations game, Player actingPlayer, Card handmaidCard)
        {
            actingPlayer.SetProtection(true);
            
            game.AddLogEntry(new GameLogEntry(
                eventType: GameLogEventType.HandmaidProtection,
                actingPlayerId: actingPlayer.Id,
                actingPlayerName: actingPlayer.Name,
                message: $"{actingPlayer.Name} played the Handmaid and is protected until their next turn.")
            {
                PlayedCard = handmaidCard
            });
        }
        
        protected virtual void ExecutePrinceEffect(IGameOperations game, Player actingPlayer, Player targetPlayer, Card princeCard) // targetPlayer is non-null due to prior validation
        {
            Player target = targetPlayer; // Use targetPlayer directly as it's validated non-null if required
            
            if (target.IsProtected && target.Id != actingPlayer.Id) // Can't Prince a protected player unless it's self
            {
                game.AddLogEntry(new GameLogEntry(
                    eventType: GameLogEventType.EffectFizzled,
                    actingPlayerId: actingPlayer.Id,
                    actingPlayerName: actingPlayer.Name,
                    message: $"The Prince's effect was blocked by Handmaid protection on {target.Name}.",
                    isPrivate: false)
                {
                    FizzleReason = "Target is protected by Handmaid",
                    PlayedCard = princeCard,
                    TargetPlayerId = target.Id,
                    TargetPlayerName = target.Name
                });
                return;
            }

            var discardedCard = target.DiscardHand(true);
            
            if (discardedCard != null)
            {
                game.AddLogEntry(new GameLogEntry(
                    eventType: GameLogEventType.PrinceDiscard,
                    actingPlayerId: actingPlayer.Id,
                    actingPlayerName: actingPlayer.Name,
                    targetPlayerId: target.Id,
                    targetPlayerName: target.Name,
                    message: $"{actingPlayer.Name} used the Prince to make {target.Name} discard their {discardedCard.Rank.Name}.")
                {
                    PlayedCard = princeCard,
                    TargetDiscardedCard = discardedCard, // Changed from DiscardedCard to TargetDiscardedCard for clarity
                    IsPrivate = discardedCard.Rank == CardRank.Princess // Keep Princess discard private
                });
                
                if (discardedCard.Rank == CardRank.Princess)
                {
                    game.AddLogEntry(new GameLogEntry(
                        eventType: GameLogEventType.PlayerEliminated,
                        actingPlayerId: target.Id, // The player who discarded the Princess eliminates themselves
                        actingPlayerName: target.Name,
                        message: $"{target.Name} discarded the Princess and is eliminated!",
                        isPrivate: false)
                    {
                        PlayedCard = princeCard, // Card that caused the discard (Prince)
                        RevealedCardOnElimination = discardedCard // The Princess card itself
                    });
                    
                    game.EliminatePlayer(target.Id, "discarded the Princess", princeCard);
                    return; 
                }
            }
            
            if (target.Status != PlayerStatus.Eliminated)
            {
                var drawnCard = game.DrawCardForPlayer(target.Id);
                
                if (drawnCard != null)
                {
                    game.AddLogEntry(new GameLogEntry(
                        eventType: GameLogEventType.PlayerDrewCard, // Changed from CardDrawn
                        actingPlayerId: target.Id,
                        actingPlayerName: target.Name,
                        message: $"{target.Name} drew a new card after discarding due to the Prince.")
                    {
                        DrawnCard = drawnCard, // For private logs to the player
                        IsPrivate = true // Only the drawing player should know what they drew
                    });
                }
            }
        }
        
        protected virtual void ExecuteKingEffect(IGameOperations game, Player actingPlayer, Player targetPlayer, Card kingCard)
        {
            if (targetPlayer.IsProtected)
            {
                game.AddLogEntry(new GameLogEntry(
                    GameLogEventType.EffectFizzled,
                    actingPlayer.Id,
                    actingPlayer.Name,
                    $"{targetPlayer.Name} is protected by the Handmaid and cannot be targeted.")
                );
                return;
            }
            
            game.SwapPlayerHands(actingPlayer.Id, targetPlayer.Id);
            
            var actingPlayerCard = actingPlayer.Hand.Cards.FirstOrDefault();
            var targetPlayerCard = targetPlayer.Hand.Cards.FirstOrDefault();
            
            if (actingPlayerCard != null && targetPlayerCard != null)
            {
                game.AddLogEntry(new GameLogEntry(
                    eventType: GameLogEventType.KingTrade, // Changed from KingEffect
                    actingPlayerId: actingPlayer.Id,
                    actingPlayerName: actingPlayer.Name,
                    targetPlayerId: targetPlayer.Id,
                    targetPlayerName: targetPlayer.Name,
                    message: $"{actingPlayer.Name} played the King and swapped hands with {targetPlayer.Name}. {actingPlayer.Name} now has {actingPlayerCard.Rank.Name}, {targetPlayer.Name} now has {targetPlayerCard.Rank.Name}.")
                {
                    PlayedCard = kingCard // Added to log which card caused the trade
                });
            }
        }
        
        protected virtual void ExecuteCountessEffect(IGameOperations game, Player actingPlayer, Card countessCard)
        {
            game.AddLogEntry(new GameLogEntry(
                eventType: GameLogEventType.CountessDiscard,
                actingPlayerId: actingPlayer.Id,
                actingPlayerName: actingPlayer.Name,
                message: $"{actingPlayer.Name} played the Countess.")
            {
                PlayedCard = countessCard,
                DiscardedCard = countessCard
            });
        }
        
        protected virtual void ExecutePrincessEffect(IGameOperations game, Player actingPlayer, Card princessCard)
        {
            game.AddLogEntry(new GameLogEntry(
                eventType: GameLogEventType.PlayerEliminated,
                actingPlayerId: actingPlayer.Id,
                actingPlayerName: actingPlayer.Name,
                message: $"{actingPlayer.Name} played the Princess and is eliminated!",
                isPrivate: false)
            {
                PlayedCard = princessCard,
                RevealedCardOnElimination = princessCard
            });
            
            game.EliminatePlayer(actingPlayer.Id, "played the Princess", princessCard);
        }
    }
}
