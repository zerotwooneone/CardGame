using System.Collections.ObjectModel;
using CardGame.Domain;
using CardGame.Domain.Game;
using CardGame.Domain.Game.GameException;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Providers;
using CardGame.Domain.Types;

namespace CardGame.Decks.BaseGame;

/// <summary>
/// Provides the default Love Letter deck configuration.
/// </summary>
public class DefaultDeckProvider : BaseDeckProvider
{
    public override Guid DeckId => new("00000000-0000-0000-0000-000000000001");

    public override string DisplayName => "Standard Love Letter";

    public override string Description =>
        "The original Love Letter deck with 16 cards and 8 different character types.";

    protected override string ThemeName => "default";

    protected override string DeckBackAppearanceId => "assets/decks/default/back.webp";
    public override IReadOnlyDictionary<int, IEnumerable<RankDefinition>> RankDefinitions => Rank_Definitions;

    public static readonly IReadOnlyDictionary<int, IEnumerable<RankDefinition>>
        Rank_Definitions = DefineNumericRanks();

    private static IReadOnlyDictionary<int, IEnumerable<RankDefinition>> DefineNumericRanks()
    {
        var definitions = new Dictionary<int, IEnumerable<RankDefinition>>
        {
            [CardRank.Guard.Value] = new List<RankDefinition>
            {
                new(
                    Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    Value: CardRank.Guard.Value
                )
            },
            [CardRank.Priest.Value] = new List<RankDefinition>
            {
                new(
                    Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    Value: CardRank.Priest.Value
                )
            },
            [CardRank.Baron.Value] = new List<RankDefinition>
            {
                new(
                    Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    Value: CardRank.Baron.Value
                )
            },
            [CardRank.Handmaid.Value] = new List<RankDefinition>
            {
                new(
                    Guid.Parse("00000000-0000-0000-0000-000000000004"),
                    Value: CardRank.Handmaid.Value
                )
            },
            [CardRank.Prince.Value] = new List<RankDefinition>
            {
                new(
                    Guid.Parse("00000000-0000-0000-0000-000000000005"),
                    Value: CardRank.Prince.Value
                )
            },
            [CardRank.King.Value] = new List<RankDefinition>
            {
                new(
                    Guid.Parse("00000000-0000-0000-0000-000000000006"),
                    Value: CardRank.King.Value
                )
            },
            [CardRank.Countess.Value] = new List<RankDefinition>
            {
                new(
                    Guid.Parse("00000000-0000-0000-0000-000000000007"),
                    Value: CardRank.Countess.Value
                )
            },
            [CardRank.Princess.Value] = new List<RankDefinition>
            {
                new(
                    Guid.Parse("00000000-0000-0000-0000-000000000008"),
                    Value: CardRank.Princess.Value
                )
            }
        };
        return new ReadOnlyDictionary<int, IEnumerable<RankDefinition>>(definitions);
    }

    protected override IEnumerable<CardQuantity> GetCardQuantities()
    {
        return new List<CardQuantity>
        {
            new(Rank_Definitions[CardRank.Guard.Value].First(), 5),
            new(Rank_Definitions[CardRank.Priest.Value].First(), 2),
            new(Rank_Definitions[CardRank.Baron.Value].First(), 2),
            new(Rank_Definitions[CardRank.Handmaid.Value].First(), 2),
            new(Rank_Definitions[CardRank.Prince.Value].First(), 2),
            new(Rank_Definitions[CardRank.King.Value].First(), 1),
            new(Rank_Definitions[CardRank.Countess.Value].First(), 1),
            new(Rank_Definitions[CardRank.Princess.Value].First(), 1)
        };
    }

    /// <inheritdoc />
    protected override string GetCardAppearanceId(RankDefinition rank, int index)
    {
        var cardRank = CardRank.FromValue(rank.Value);
        return $"assets/decks/default/{cardRank.Name.ToLowerInvariant()}.webp";
    }

    public bool RequiresTargetPlayer(CardRank cardRank) =>
        cardRank == CardRank.Guard ||
        cardRank == CardRank.Priest ||
        cardRank == CardRank.Baron ||
        cardRank == CardRank.Prince ||
        cardRank == CardRank.King;

    public bool CanTargetSelf(CardRank cardRank) =>
        cardRank == CardRank.Prince;

    public bool RequiresGuess(CardRank cardRank) =>
        cardRank == CardRank.Guard;

    protected void ExecuteGuardEffect(IGameOperations game, Player actingPlayer, Player targetPlayer,
        CardRank? guessedCard, Card guardCard)
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
        var targetCardRank = CardRank.FromValue(targetCard.Rank.Value);
        if (targetCardRank == guessedCard)
        {
            game.AddLogEntry(new GameLogEntry(
                eventType: GameLogEventType.GuardHit,
                actingPlayerId: actingPlayer.Id,
                actingPlayerName: actingPlayer.Name,
                targetPlayerId: targetPlayer.Id,
                targetPlayerName: targetPlayer.Name,
                message:
                $"{actingPlayer.Name} correctly guessed {targetPlayer.Name}'s {targetCard.Rank.Value} with the Guard!")
            {
                PlayedCard = guardCard,
                GuessedRank = guessedCard.Value,
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
                message:
                $"{actingPlayer.Name} guessed {guessedCard?.Value.ToString() ?? "nothing"} for {targetPlayer.Name}'s card, but was wrong.")
            {
                PlayedCard = guardCard,
                GuessedRank = guessedCard?.Value,
                WasGuessCorrect = false
            });
        }
    }

    protected void ExecutePriestEffect(IGameOperations game, Player actingPlayer, Player targetPlayer)
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
            $"{actingPlayer.Name} looked at {targetPlayer.Name}'s {targetCard.Rank.Value} with the Priest.")
        );
    }

    protected void ExecuteBaronEffect(IGameOperations game, Player actingPlayer, Player targetPlayer,
        Card baronCard)
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
                $"{actingPlayer.Name} played Baron against {targetPlayer.Name} and won with {actingPlayerCard.Rank.Value} vs {targetPlayerCard.Rank.Value}")
            );

            game.EliminatePlayer(targetPlayer.Id, "lost a Baron comparison", baronCard);
        }
        else if (actingPlayerCard.Rank.Value < targetPlayerCard.Rank.Value)
        {
            game.AddLogEntry(new GameLogEntry(
                GameLogEventType.BaronCompare,
                actingPlayer.Id,
                actingPlayer.Name,
                $"{actingPlayer.Name} played Baron against {targetPlayer.Name} and lost with {actingPlayerCard.Rank.Value} vs {targetPlayerCard.Rank.Value}")
            );

            game.EliminatePlayer(actingPlayer.Id, "lost a Baron comparison", baronCard);
        }
        else
        {
            game.AddLogEntry(new GameLogEntry(
                GameLogEventType.BaronCompare,
                actingPlayer.Id,
                actingPlayer.Name,
                $"{actingPlayer.Name} played Baron against {targetPlayer.Name} but it was a tie with {actingPlayerCard.Rank.Value}")
            );
        }
    }

    protected void ExecuteHandmaidEffect(IGameOperations game, Player actingPlayer, Card handmaidCard)
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

    protected void ExecutePrinceEffect(IGameOperations game, Player actingPlayer, Player targetPlayer,
        Card princeCard)
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

        var discardedCard = target.DiscardHand();

        if (discardedCard != null)
        {
            game.AddLogEntry(new GameLogEntry(
                eventType: GameLogEventType.PrinceDiscard,
                actingPlayerId: actingPlayer.Id,
                actingPlayerName: actingPlayer.Name,
                targetPlayerId: target.Id,
                targetPlayerName: target.Name,
                message:
                $"{actingPlayer.Name} used the Prince to make {target.Name} discard their {discardedCard.Rank.Value}.")
            {
                PlayedCard = princeCard,
                TargetDiscardedCard = discardedCard, // Changed from DiscardedCard to TargetDiscardedCard for clarity
                IsPrivate = discardedCard.Rank.Value == CardRank.Princess.Value // Keep Princess discard private
            });

            if (discardedCard.Rank.Value == CardRank.Princess.Value)
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

    protected void ExecuteKingEffect(IGameOperations game, Player actingPlayer, Player targetPlayer,
        Card kingCard)
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
                message:
                $"{actingPlayer.Name} played the King and swapped hands with {targetPlayer.Name}. {actingPlayer.Name} now has {actingPlayerCard.Rank.Value}, {targetPlayer.Name} now has {targetPlayerCard.Rank.Value}.")
            {
                PlayedCard = kingCard // Added to log which card caused the trade
            });
        }
    }

    protected  void ExecuteCountessEffect(IGameOperations game, Player actingPlayer, Card countessCard)
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

    protected void ExecutePrincessEffect(IGameOperations game, Player actingPlayer, Card princessCard)
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

    protected override void ValidateCardEffect(
        Player actingPlayer,
        Card card,
        Player? targetPlayer,
        int? guessedRankValue)
    {
        var cardRank = CardRank.FromValue(card.Rank.Value);
        // General Countess Rule: If holding Countess AND (King or Prince), must play Countess.
        bool handHasCountess = actingPlayer.Hand.Cards.Any(c => c.Rank.Value == CardRank.Countess.Value);
        bool handHasKing = actingPlayer.Hand.Cards.Any(c => c.Rank.Value == CardRank.King.Value);
        bool handHasPrince = actingPlayer.Hand.Cards.Any(c => c.Rank.Value == CardRank.Prince.Value);

        if (handHasCountess && (handHasKing || handHasPrince))
        {
            if (cardRank == CardRank.King || cardRank == CardRank.Prince)
            {
                throw new InvalidMoveException(
                    $"Cannot play {cardRank} when holding the Countess (and King/Prince). You must play the Countess.");
            }
        }

        // Target validation
        if (RequiresTargetPlayer(cardRank))
        {
            if (targetPlayer == null)
            {
                throw new InvalidMoveException($"{cardRank} requires a target player.");
            }

            if (!CanTargetSelf(cardRank) && targetPlayer.Id == actingPlayer.Id)
            {
                throw new InvalidMoveException($"{cardRank} cannot target self.");
            }
        }

        // Guess validation
        if (RequiresGuess(cardRank))
        {
            if (guessedRankValue == null)
            {
                throw new InvalidMoveException($"{cardRank} requires a guessed card type.");
            }

            if (guessedRankValue == CardRank.Guard.Value)
            {
                throw new InvalidMoveException("Cannot guess Guard with a Guard.");
            }
        }
    }

    protected override void PerformCardEffect(IGameOperations game, Player actingPlayer, Card card,
        Player? targetPlayer, int? guessedCardRankValue)
    {
        switch (card.Rank.Value)
        {
            case 1: // CardType.Guard
                var guessedCardRank = CardRank.FromValue(guessedCardRankValue!.Value);
                ExecuteGuardEffect(game, actingPlayer,
                    targetPlayer ?? throw new ArgumentNullException(nameof(targetPlayer)), guessedCardRank, card);
                break;
            case 2: // CardType.Priest
                ExecutePriestEffect(game, actingPlayer,
                    targetPlayer ?? throw new ArgumentNullException(nameof(targetPlayer)));
                break;
            case 3: // CardType.Baron
                ExecuteBaronEffect(game, actingPlayer, targetPlayer!, card);
                break;
            case 4: // CardType.Handmaid
                ExecuteHandmaidEffect(game, actingPlayer,
                    card); // Handmaid does not use targetPlayer or guessedCardType
                break;
            case 5: // CardType.Prince
                ExecutePrinceEffect(game, actingPlayer,
                    targetPlayer ?? throw new ArgumentNullException(nameof(targetPlayer)),
                    card); // Prince targetPlayer is validated by RequiresTargetPlayer
                break;
            case 6: // CardType.King
                ExecuteKingEffect(game, actingPlayer,
                    targetPlayer ?? throw new ArgumentNullException(nameof(targetPlayer)), card);
                break;
            case 7: // CardType.Countess
                ExecuteCountessEffect(game, actingPlayer,
                    card); // Countess does not use targetPlayer or guessedCardType
                break;
            case 8: // CardType.Princess
                ExecutePrincessEffect(game, actingPlayer,
                    card); // Princess does not use targetPlayer or guessedCardType
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(card),
                    $"Unknown card type value: {card.Rank.Value} (Id: {card.Rank.Id})");
        }
    }
}


