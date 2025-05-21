using System.Collections.ObjectModel;
using CardGame.Domain.Common;
using CardGame.Domain.Exceptions;
using CardGame.Domain.Game.Event;
using CardGame.Domain.Game.GameException;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;

namespace CardGame.Domain.Game;

public class Game // Aggregate Root
{
    public Guid Id { get; private set; }
    public List<Player> Players { get; private set; } = new List<Player>();
    public Deck Deck { get; private set; } // Initialized in StartNewRound or Load
    public List<Card> DiscardPile { get; private set; } = new List<Card>();
    public Guid CurrentTurnPlayerId { get; private set; }
    public GamePhase GamePhase { get; private set; } = GamePhase.NotStarted;
    public int RoundNumber { get; private set; } = 0;
    public int TokensNeededToWin { get; private set; } = 4;
    public Card? SetAsideCard { get; private set; }
    public List<Card> PubliclySetAsideCards { get; private set; } = new List<Card>();
    public Guid? LastRoundWinnerId { get; private set; }
    private readonly List<GameLogEntry> _logEntries = new(); // New log entry list

    private readonly IReadOnlyList<Card> _initialDeckCardSet;
    private readonly Guid _deckDefinitionId; // Added field
    public Guid DeckDefinitionId => _deckDefinitionId; // ADDED Public getter

    // --- Domain Event Handling ---
    private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() { _domainEvents.Clear(); }
    private void AddDomainEvent(IDomainEvent domainEvent) { _domainEvents.Add(domainEvent); }
    // --- End Domain Event Handling ---

    public IReadOnlyList<GameLogEntry> LogEntries => _logEntries.AsReadOnly(); // Public accessor for log entries

    private Game(Guid id, Guid deckDefinitionId, IReadOnlyList<Card> initialDeckCardSet) // Added deckDefinitionId parameter
    {
        Id = id;
        _deckDefinitionId = deckDefinitionId; // Assign deckDefinitionId
        _initialDeckCardSet = initialDeckCardSet ?? throw new ArgumentNullException(nameof(initialDeckCardSet));
        if (!_initialDeckCardSet.Any()) throw new ArgumentException("Initial deck card set cannot be empty.", nameof(initialDeckCardSet));
        Deck = Deck.Load(Enumerable.Empty<Card>());
    }

    // --- Factory Methods ---
    public static Game CreateNewGame(Guid deckDefinitionId, IEnumerable<PlayerInfo> playerInfos, Guid creatorPlayerId, IEnumerable<Card> initialDeckCards, int tokensToWin = 4) // Added deckDefinitionId parameter
    {
        var gameId = Guid.NewGuid();
        var cardSetToUse = initialDeckCards?.ToList() ?? throw new ArgumentNullException(nameof(initialDeckCards));
        if (!cardSetToUse.Any()) throw new ArgumentException("Initial deck cards cannot be empty.", nameof(initialDeckCards));

        var game = new Game(gameId, deckDefinitionId, new ReadOnlyCollection<Card>(cardSetToUse)) // Pass deckDefinitionId
        {
            TokensNeededToWin = tokensToWin,
            GamePhase = GamePhase.NotStarted,
            LastRoundWinnerId = null
        };

        var playerInfoList = playerInfos?.ToList() ?? new List<PlayerInfo>();
        if (!playerInfoList.Any()) throw new ArgumentException("Player information cannot be empty.", nameof(playerInfos));

        bool creatorFound = false;
        foreach (var pInfo in playerInfoList)
        {
            var player = Player.Load(pInfo.Id, pInfo.Name, PlayerStatus.Active, Hand.Empty, new List<CardType>(), 0, false);
            game.Players.Add(player);
            if (pInfo.Id == creatorPlayerId) creatorFound = true;
        }

        if (!creatorFound) throw new DomainException("Creator ID must be included in the player list.", 1000);
        if (game.Players.Count < 2 || game.Players.Count > 4) throw new DomainException($"Game must have between 2 and 4 players (found {game.Players.Count}).", 1000);

        game.AddDomainEvent(new GameCreated(gameId, playerInfoList, tokensToWin, creatorPlayerId));
        return game;
    }

    public static Game Load(Guid id, Guid deckDefinitionId, int roundNumber, GamePhase gamePhase, Guid currentTurnPlayerId, List<Player> players, Deck deck, Card? setAsideCard, List<Card> publiclySetAsideCards, List<Card> discardPile, int tokensToWin, Guid? lastRoundWinnerId, List<Card> initialDeckCardSet) // Added deckDefinitionId parameter
    {
        if (initialDeckCardSet == null) throw new ArgumentNullException(nameof(initialDeckCardSet));
        if (!initialDeckCardSet.Any()) throw new ArgumentException("Initial deck card set cannot be empty for loading.", nameof(initialDeckCardSet));

        var game = new Game(id, deckDefinitionId, new ReadOnlyCollection<Card>(initialDeckCardSet)) // Pass deckDefinitionId
        {
            RoundNumber = roundNumber,
            GamePhase = gamePhase,
            CurrentTurnPlayerId = currentTurnPlayerId,
            Players = players ?? new List<Player>(),
            Deck = deck,
            SetAsideCard = setAsideCard,
            PubliclySetAsideCards = publiclySetAsideCards ?? new List<Card>(),
            DiscardPile = discardPile ?? new List<Card>(),
            TokensNeededToWin = tokensToWin,
            LastRoundWinnerId = lastRoundWinnerId
        };
        return game;
    }

    // --- Public Methods (Game Actions) ---
    public void StartNewRound(IRandomizer? randomizer = null)
    {
        if (GamePhase == GamePhase.GameOver) throw new GameRuleException("Cannot start a new round, the game is over.");
        if (GamePhase == GamePhase.RoundInProgress) throw new GameRuleException("Cannot start a new round while one is in progress.");

        RoundNumber++;
        Deck = Deck.CreateShuffled(_initialDeckCardSet, randomizer);
        DiscardPile.Clear();
        SetAsideCard = null;
        PubliclySetAsideCards.Clear();

        foreach (var player in Players) { player.PrepareForNewRound(); }

        // Set Aside Cards Logic
        int playerCount = Players.Count;
        if (playerCount >= 2 && !Deck.IsEmpty)
        {
            (Card drawnDown, Deck afterDown) = Deck.Draw();
            SetAsideCard = drawnDown; Deck = afterDown;
            if (playerCount == 2)
            {
                for (int i = 0; i < 3 && !Deck.IsEmpty; i++)
                {
                    (Card drawnUp, Deck afterUp) = Deck.Draw();
                    PubliclySetAsideCards.Add(drawnUp); Deck = afterUp;
                }
            }
        }

        // Deal initial hands
        var playerIds = new List<Guid>();
        foreach (var player in Players)
        {
             if (!Deck.IsEmpty)
             {
                 (Card dealtCard, Deck remainingDeck) = Deck.Draw();
                 player.GiveCard(dealtCard); Deck = remainingDeck;
             }
             playerIds.Add(player.Id);
        }

        if (CheckRoundEndCondition()) return;

        // Determine Starting Player
        Guid startingPlayerId;
        if (RoundNumber == 1 || LastRoundWinnerId == null || !Players.Any(p => p.Id == LastRoundWinnerId)) { startingPlayerId = Players.First().Id; }
        else { startingPlayerId = LastRoundWinnerId.Value; }
        CurrentTurnPlayerId = startingPlayerId;

        GamePhase = GamePhase.RoundInProgress;

        string startingPlayerName = GetPlayerById(startingPlayerId).Name;
        string roundStartMessage = $"Round {RoundNumber} has started. {startingPlayerName} will go first.";
        if (SetAsideCard != null) roundStartMessage += $" A card was set aside face down.";
        if (PubliclySetAsideCards.Any()) roundStartMessage += $" {PubliclySetAsideCards.Count} cards were set aside face up.";
        
        AddGameLogEntry(new GameLogEntry(
            GameLogEventType.RoundStart,
            null, // No specific acting player for round start
            "Game", // No specific acting player name, use "Game"
            roundStartMessage
        ));

        AddDomainEvent(new RoundStarted(
            Id, RoundNumber, playerIds, Deck.CardsRemaining, SetAsideCard?.Type,
            PubliclySetAsideCards.Select(c => new PublicCardInfo(c.AppearanceId, c.Type)).ToList(),
            _deckDefinitionId // Use stored _deckDefinitionId
        ));
        HandleTurnStartDrawing();
    }

    public void PlayCard(Guid playerId, Card cardToPlayInstance, Guid? targetPlayerId, CardType? guessedCardType)
    {
        var cardType = cardToPlayInstance.Type;
        ValidatePlayCardAction(playerId, cardToPlayInstance, targetPlayerId, guessedCardType);
        var actingPlayer = GetPlayerById(playerId);
        Player? targetPlayer = targetPlayerId.HasValue ? GetPlayerById(targetPlayerId.Value) : null;

        actingPlayer.PlayCard(cardToPlayInstance);
        DiscardPile.Add(cardToPlayInstance);

        string actingPlayerName = actingPlayer.Name;
        string? targetPlayerName = targetPlayer?.Name;
        string playedCardStringName = cardType.ToString();
        string message;

        message = $"{actingPlayerName} played {playedCardStringName}.";

        var logEntry = new GameLogEntry(
            GameLogEventType.CardPlayed,
            playerId,
            actingPlayerName,
            message,
            false // isPrivate
        )
        {
            PlayedCard = cardToPlayInstance
        };
        AddGameLogEntry(logEntry);

        AddDomainEvent(new PlayerPlayedCard(Id, playerId, cardToPlayInstance, targetPlayerId, guessedCardType));

        // Dispatch to specific card logic
        switch (cardType)
        {
            case var _ when cardType == CardType.Guard: ExecuteGuardEffect(actingPlayer, targetPlayer, guessedCardType, cardToPlayInstance); break;
            case var _ when cardType == CardType.Priest: ExecutePriestEffect(actingPlayer, targetPlayer, cardToPlayInstance); break;
            case var _ when cardType == CardType.Baron: ExecuteBaronEffect(actingPlayer, targetPlayer, cardToPlayInstance); break;
            case var _ when cardType == CardType.Handmaid: ExecuteHandmaidEffect(actingPlayer, cardToPlayInstance); break;
            case var _ when cardType == CardType.Prince: ExecutePrinceEffect(actingPlayer, targetPlayer ?? actingPlayer, cardToPlayInstance); break;
            case var _ when cardType == CardType.King: ExecuteKingEffect(actingPlayer, targetPlayer, cardToPlayInstance); break;
            case var _ when cardType == CardType.Countess: ExecuteCountessEffect(actingPlayer); break;
            case var _ when cardType == CardType.Princess: ExecutePrincessEffect(actingPlayer, cardToPlayInstance); break;
            default: throw new ArgumentOutOfRangeException(nameof(cardToPlayInstance), $"Unknown card type: {cardType.Name}");
        }

        // Check if the round ended due to the card play (elimination) or if deck was already empty.
        bool roundEndedImmediately = CheckRoundEndCondition();

        // If the round didn't end from the card play itself, advance the turn.
        if (!roundEndedImmediately && GamePhase == GamePhase.RoundInProgress)
        {
             AdvanceTurn();
        }
    }

    // --- Private Helper Methods ---

    private void ValidatePlayCardAction(Guid playerId, Card cardToPlayInstance, Guid? targetPlayerId, CardType? guessedCardType)
    {
        var cardType = cardToPlayInstance.Type;
        if (GamePhase != GamePhase.RoundInProgress) throw new InvalidMoveException("Cannot play cards when the round is not in progress.");
        if (CurrentTurnPlayerId != playerId) throw new InvalidMoveException($"It is not player {GetPlayerById(playerId).Name}'s turn.");
        var player = GetPlayerById(playerId);
        if (!player.Hand.GetCards().Any(c => c.AppearanceId == cardToPlayInstance.AppearanceId)) throw new InvalidMoveException($"Player {player.Name} does not hold the specified card instance (ID: {cardToPlayInstance.AppearanceId}).");
        if ((cardType == CardType.King || cardType == CardType.Prince) && player.Hand.Contains(CardType.Countess)) throw new GameRuleException($"Player {player.Name} must play the Countess.");

        if (targetPlayerId.HasValue)
        {
            var targetPlayer = GetPlayerById(targetPlayerId.Value); // Check existence
            if (targetPlayer.Status == PlayerStatus.Eliminated) throw new InvalidMoveException($"Cannot target eliminated player {targetPlayer.Name}.");
            if (targetPlayerId.Value == playerId && !CanTargetSelf(cardType) && (cardType == CardType.Guard || cardType == CardType.Priest || cardType == CardType.Baron || cardType == CardType.King)) throw new InvalidMoveException($"Cannot target self with {cardType.Name}.");
        }
        else if ((cardType == CardType.Guard || cardType == CardType.Priest || cardType == CardType.Baron || cardType == CardType.King)) // Cards requiring a target (unless no valid targets exist)
        {
             bool validTargetsExist = Players.Any(p => p.Id != playerId && p.Status == PlayerStatus.Active && !p.IsProtected);
             bool onlyProtectedTargetsExist = !validTargetsExist && Players.Any(p => p.Id != playerId && p.Status == PlayerStatus.Active && p.IsProtected);
             if (validTargetsExist || onlyProtectedTargetsExist) // If any target exists (protected or not)
             {
                  throw new InvalidMoveException($"{cardType.Name} requires a target player.");
             }
             // If no active opponents exist at all, the play is allowed but will have no effect (no exception)
        }

        if (cardType == CardType.Guard && (targetPlayerId == null || guessedCardType == null || guessedCardType == CardType.Guard)) throw new InvalidMoveException("Guard requires a valid target player and a non-Guard guess.");
    }

    private bool CanTargetSelf(CardType cardType) => cardType == CardType.Prince;

    private void ExecuteGuardEffect(Player actingPlayer, Player? targetPlayer, CardType? guessedCardType, Card guardCardInstance)
    {
        if (targetPlayer == null || guessedCardType == null) 
        {
            throw new GameRuleException("Guard requires a valid target player and a non-Guard guess.");
        }

        string actingPlayerName = actingPlayer.Name;
        string targetPlayerName = targetPlayer.Name;

        // Check protection BEFORE applying effect
        if (targetPlayer.IsProtected)
        {
            string fizzleMessage = $"{actingPlayerName}'s Guard against {targetPlayerName} had no effect because {targetPlayerName} is protected by a Handmaid.";
            AddGameLogEntry(new GameLogEntry(
                GameLogEventType.EffectFizzled,
                actingPlayer.Id,
                actingPlayerName,
                targetPlayer.Id,
                targetPlayerName,
                fizzleMessage
            ) 
            {
                PlayedCard = guardCardInstance, // The Guard card that was played
                FizzleReason = "Target was protected"
            });
            AddDomainEvent(new CardEffectFizzled(Id, actingPlayer.Id, CardType.Guard, targetPlayer.Id, "Target was protected"));
            return; // Effect fizzles
        }

        bool correctGuess = targetPlayer.Hand.Contains(guessedCardType); // .Value as guessedCardType is CardType?
        if (correctGuess)
        {
            string hitMessage = $"{actingPlayerName} correctly guessed that {targetPlayerName} held a {guessedCardType.Value.ToString()} with their Guard! {targetPlayerName} is eliminated.";
            Card? revealedTargetCard = targetPlayer.Hand.GetHeldCard(); // Get for logging
            AddGameLogEntry(new GameLogEntry(
                GameLogEventType.GuardHit, 
                actingPlayer.Id,
                actingPlayerName,
                targetPlayer.Id,
                targetPlayerName,
                hitMessage
            )
            {
                PlayedCard = guardCardInstance,
                GuessedRank = guessedCardType, // The rank that was guessed. Ensure GuessedRank in GameLogEntry is CardType not int.
                GuessedPlayerActualCard = revealedTargetCard, // The actual card the target player held
                WasGuessCorrect = true
            });
            string eliminationReason = $"guessed correctly by {actingPlayerName} with a Guard";
            EliminatePlayer(targetPlayer.Id, eliminationReason, guardCardInstance); // Use guardCardInstance
        }
        else // Incorrect Guess
        {
            string missMessage = $"{actingPlayerName} guessed that {targetPlayerName} held a {guessedCardType.Value.ToString()}, but was incorrect.";
            AddGameLogEntry(new GameLogEntry(
                GameLogEventType.GuardMiss, 
                actingPlayer.Id,
                actingPlayerName,
                targetPlayer.Id,
                targetPlayerName,
                missMessage
            )
            {
                PlayedCard = guardCardInstance, // The Guard card that was played
                GuessedRank = guessedCardType, // The rank that was guessed
                // GuessedPlayerActualCard remains null on a miss for public logs
                WasGuessCorrect = false
            });
        }
        AddDomainEvent(new GuardGuessResult(Id, actingPlayer.Id, targetPlayer.Id, guessedCardType, correctGuess));
    }

    private void ExecutePriestEffect(Player actingPlayer, Player? targetPlayer, Card priestCardInstance) 
    {
        if (targetPlayer == null)
        {
            throw new GameRuleException("Priest requires a valid target player.");
        }

        string actingPlayerName = actingPlayer.Name;
        string targetPlayerName = targetPlayer.Name;

        if (targetPlayer.IsProtected)
        {
            string fizzleMessage = $"{actingPlayerName}'s Priest targeting {targetPlayerName} had no effect because {targetPlayerName} is protected by a Handmaid.";
            AddGameLogEntry(new GameLogEntry(
                GameLogEventType.EffectFizzled,
                actingPlayer.Id,
                actingPlayerName,
                targetPlayer.Id,
                targetPlayerName,
                fizzleMessage
            ) 
            {
                PlayedCard = priestCardInstance, // The Priest card that was played
                FizzleReason = "Target was protected"
            });
            AddDomainEvent(new CardEffectFizzled(Id, actingPlayer.Id, CardType.Priest, targetPlayer.Id, "Target was protected"));
            return; // Effect fizzles
        }

        Card? revealedCard = targetPlayer.Hand.GetHeldCard();
        string? revealedCardStringName = revealedCard?.Type.ToString();

        // Public log about the action (without revealing the card)
        string publicLogMessage = $"{actingPlayerName} used Priest on {targetPlayerName} to look at their hand.";
        AddGameLogEntry(new GameLogEntry(
            GameLogEventType.PriestEffect, // General event type for Priest action
            actingPlayer.Id,
            actingPlayerName,
            targetPlayer.Id,
            targetPlayerName,
            publicLogMessage
        )
        {
            PlayedCard = priestCardInstance, // The Priest card that was played
            RevealedPlayerCard = revealedCard // The card revealed by the Priest
        });

        // Private log for the acting player
        string privateLogMessage = $"You used Priest on {targetPlayerName} and saw their {revealedCardStringName}.";
        AddGameLogEntry(new GameLogEntry(
            GameLogEventType.PriestEffect,
            actingPlayer.Id,
            actingPlayerName, 
            targetPlayer.Id,
            targetPlayerName, 
            privateLogMessage,
            true // isPrivate set to true
        ) 
        {
            PlayedCard = priestCardInstance, // The Priest card that was played
            RevealedPlayerCard = revealedCard // The card revealed by the Priest
        });

        // Domain event to notify the specific player (handler will use this)
        if (revealedCard != null)
        {
            AddDomainEvent(new PriestEffectUsed(Id, actingPlayer.Id, targetPlayer.Id, revealedCard.AppearanceId, revealedCard.Type));
        }
    }

    private void ExecuteBaronEffect(Player actingPlayer, Player? targetPlayer, Card baronCardInstance) // Added baronCardInstance
    {
        if (targetPlayer == null)
        {
            throw new GameRuleException("Baron requires a valid target player.");
        }

        string actingPlayerName = actingPlayer.Name;
        string targetPlayerName = targetPlayer.Name;

        // Check protection BEFORE applying effect
        if (targetPlayer.IsProtected)
        {
            string fizzleMessage = $"{actingPlayerName}'s Baron against {targetPlayerName} had no effect because {targetPlayerName} is protected by a Handmaid.";
            AddGameLogEntry(new GameLogEntry(
                GameLogEventType.EffectFizzled,
                actingPlayer.Id,
                actingPlayerName,
                targetPlayer.Id,
                targetPlayerName,
                fizzleMessage
            ) 
            {
                PlayedCard = baronCardInstance, // The Baron card that was played
                FizzleReason = "Target was protected"
            });
            AddDomainEvent(new CardEffectFizzled(Id, actingPlayer.Id, CardType.Baron, targetPlayer.Id, "Target was protected"));
            return; // Effect fizzles
        }

        Card? actingPlayerCard = actingPlayer.Hand.GetHeldCard();
        Card? targetPlayerCard = targetPlayer.Hand.GetHeldCard();

        if (actingPlayerCard == null || targetPlayerCard == null) 
        {
            throw new GameRuleException("Baron requires both players to have a card in their hand."); 
        }

        Player? eliminatedPlayer = null;
        string outcomeMessage;

        if (actingPlayerCard.Type.Value > targetPlayerCard.Type.Value)
        {
            eliminatedPlayer = targetPlayer;
            EliminatePlayer(targetPlayer.Id, "lost Baron comparison to {actingPlayerName} (their {targetPlayerCard.Type.ToString()} vs {actingPlayerCard.Type.ToString()})", baronCardInstance); // Use baronCardInstance
            outcomeMessage = $"{actingPlayerName} (holding {actingPlayerCard.Type.ToString()}) won the Baron comparison against {targetPlayerName} (holding {targetPlayerCard.Type.ToString()}). {targetPlayerName} is eliminated.";
        }
        else if (targetPlayerCard.Type.Value > actingPlayerCard.Type.Value)
        {
            eliminatedPlayer = actingPlayer;
            EliminatePlayer(actingPlayer.Id, "lost Baron comparison to {targetPlayerName} (their {actingPlayerCard.Type.ToString()} vs {targetPlayerCard.Type.ToString()})", baronCardInstance); // Use baronCardInstance
            outcomeMessage = $"{targetPlayerName} (holding {targetPlayerCard.Type.ToString()}) won the Baron comparison against {actingPlayerName} (holding {actingPlayerCard.Type.ToString()}). {actingPlayerName} is eliminated.";
        }
        else // Draw
        {
            outcomeMessage = $"{actingPlayerName} (holding {actingPlayerCard.Type.ToString()}) and {targetPlayerName} (holding {targetPlayerCard.Type.ToString()}) tied in the Baron comparison. No one is eliminated.";
        }

        AddGameLogEntry(new GameLogEntry(
            GameLogEventType.BaronCompare,
            actingPlayer.Id, 
            actingPlayerName,
            targetPlayer.Id,
            targetPlayerName,
            outcomeMessage
        ) 
        {
            PlayedCard = baronCardInstance, // The Baron card that was played
            ActingPlayerBaronCard = actingPlayerCard, // Card acting player compared
            TargetPlayerBaronCard = targetPlayerCard, // Card target player compared
            BaronLoserPlayerId = eliminatedPlayer?.Id // Log the ID of the player who lost the Baron comparison
        });

        AddDomainEvent(new BaronComparisonResult(
            Id,
            actingPlayer.Id,
            actingPlayerCard.Type,
            targetPlayer.Id,
            targetPlayerCard.Type,
            eliminatedPlayer?.Id
        ));
    }

    private void ExecuteKingEffect(Player actingPlayer, Player? targetPlayer, Card kingCardInstance) 
    {
        if (targetPlayer == null)
        {
            throw new GameRuleException("King requires a valid target player.");
        }

        string actingPlayerName = actingPlayer.Name;
        string targetPlayerName = targetPlayer.Name;

        if (targetPlayer.IsProtected)
        {
            string fizzleMessage = $"{actingPlayerName}'s King targeting {targetPlayerName} had no effect because {targetPlayerName} is protected by a Handmaid.";
            AddGameLogEntry(new GameLogEntry(
                GameLogEventType.EffectFizzled,
                actingPlayer.Id,
                actingPlayerName,
                targetPlayer.Id,
                targetPlayerName,
                fizzleMessage
            ) 
            {
                PlayedCard = kingCardInstance, // The King card that was played
                FizzleReason = "Target was protected"
            });
            AddDomainEvent(new CardEffectFizzled(Id, actingPlayer.Id, CardType.King, targetPlayer.Id, "Target was protected"));
            return;
        }

        Card? actingPlayerCard = actingPlayer.Hand.GetHeldCard();
        Card? targetPlayerCard = targetPlayer.Hand.GetHeldCard();

        if (actingPlayerCard == null || targetPlayerCard == null)
        {
            throw new GameRuleException("King requires both players to have a card in their hand.");
        }

        actingPlayer.Hand.Remove(actingPlayerCard);
        targetPlayer.Hand.Remove(targetPlayerCard);
        actingPlayer.Hand.Add(targetPlayerCard);
        targetPlayer.Hand.Add(actingPlayerCard);

        string successMessage = $"{actingPlayerName} used King and swapped hands with {targetPlayerName}.";
        AddGameLogEntry(new GameLogEntry(
            GameLogEventType.KingTrade,
            actingPlayer.Id,
            actingPlayerName,
            targetPlayer.Id,
            targetPlayerName,
            successMessage
        ) 
        {
            PlayedCard = kingCardInstance, 
            // OriginalCardInTrade = actingPlayerCard, // Removed, not a property on GameLogEntry
            RevealedTradedCard = targetPlayerCard    // Card acting player received (originally target's)
        });

        AddDomainEvent(new KingEffectUsed(Id, actingPlayer.Id, targetPlayer.Id));
    }

    private void ExecuteHandmaidEffect(Player actingPlayer, Card handmaidCardInstance)
    {
        actingPlayer.SetProtection(true);

        string message = $"{actingPlayer.Name} played Handmaid and is protected until their next turn.";
        AddGameLogEntry(new GameLogEntry(
            GameLogEventType.HandmaidProtection, // Corrected EventType
            actingPlayer.Id,
            actingPlayer.Name,
            message
        ) 
        {
            PlayedCard = handmaidCardInstance, // The Handmaid card that was played
        });

        AddDomainEvent(new HandmaidProtectionSet(Id, actingPlayer.Id));
    }

    private void ExecutePrinceEffect(Player actingPlayer, Player targetPlayer, Card princeCardInstance) // Added princeCardInstance
    {
        string actingPlayerName = actingPlayer.Name;
        string targetPlayerName = targetPlayer.Name;

        if (targetPlayer.IsProtected)
        {
            string fizzleMessage = $"{actingPlayerName}'s Prince targeting {targetPlayerName} had no effect because {targetPlayerName} is protected by a Handmaid.";
            AddGameLogEntry(new GameLogEntry(
                GameLogEventType.EffectFizzled,
                actingPlayer.Id,
                actingPlayerName,
                targetPlayer.Id,
                targetPlayerName,
                fizzleMessage
            ) 
            {
                PlayedCard = princeCardInstance, // The Prince card that was played
                FizzleReason = "Target was protected"
            });
            AddDomainEvent(new CardEffectFizzled(Id, actingPlayer.Id, CardType.Prince, targetPlayer.Id, "Target was protected"));
            return;
        }

        if (targetPlayer.Hand.IsEmpty)
        {
            throw new GameRuleException("Prince requires the target player's hand to not be empty."); 
        }

        Card? discardedCard = targetPlayer.DiscardHand(Deck.IsEmpty);
        if (discardedCard == null)
        {
             return;
        }

        string discardLogMessage = $"{actingPlayerName} used Prince on {targetPlayerName}. {targetPlayerName} discarded their {discardedCard.Type.ToString()}.";
        AddGameLogEntry(new GameLogEntry(
            GameLogEventType.PrinceDiscard, // Corrected EventType
            actingPlayer.Id,
            actingPlayerName,
            targetPlayer.Id,
            targetPlayerName,
            discardLogMessage
        ) 
        {
            PlayedCard = princeCardInstance, // The Prince card that was played
            TargetDiscardedCard = discardedCard // The card the target player discarded
        });

        AddDomainEvent(new PrinceEffectUsed(Id, actingPlayer.Id, targetPlayer.Id, discardedCard.Type, discardedCard.AppearanceId));

        if (discardedCard.Type == CardType.Princess)
        {
            EliminatePlayer(targetPlayer.Id, "discarding the Princess to a Prince", discardedCard); // Princess is responsible for its own elimination
        }
        else if (targetPlayer.Status == PlayerStatus.Active)
        {
            Card? newCardDrawn = null;
            if (!Deck.IsEmpty)
            {
                (Card c, Deck d) = Deck.Draw(); 
                Deck = d; 
                targetPlayer.GiveCard(c);
                newCardDrawn = c;
                AddDomainEvent(new PlayerDrewCard(Id, targetPlayer.Id)); 
                AddDomainEvent(new DeckChanged(Id, Deck.CardsRemaining));
            }
            else if (SetAsideCard != null)
            {
                targetPlayer.GiveCard(SetAsideCard);
                newCardDrawn = SetAsideCard;
                AddDomainEvent(new PlayerDrewCard(Id, targetPlayer.Id)); 
                var usedType = SetAsideCard.Type; SetAsideCard = null; 
                AddDomainEvent(new SetAsideCardUsed(Id, usedType));
            }

            if (newCardDrawn != null)
            {
                string drawMessage = $"{targetPlayerName} drew a new card after being targeted by Prince.";
                // This log can be private to the target player if the new card shouldn't be public knowledge
                // Or public if the act of drawing is public, but card itself isn't revealed here.
                // For now, making it a general public log about the draw action.
                AddGameLogEntry(new GameLogEntry(
                    GameLogEventType.PrincePlayerDrawsNewCard, // New specific event type
                    actingPlayer.Id,    // Prince player
                    actingPlayerName,
                    targetPlayer.Id,    // Player who drew
                    targetPlayerName,
                    drawMessage,
                    false // isPrivate - false for now, can be adjusted
                )
                {
                    PlayedCard = princeCardInstance, // The Prince that caused this
                    TargetNewCardAfterPrince = newCardDrawn // The new card drawn by the target
                });
            }
        }
    }

    private void ExecuteCountessEffect(Player actingPlayer)
    {
        AddDomainEvent(new PlayerPlayedCountess(Id, actingPlayer.Id));
    }

    private void ExecutePrincessEffect(Player actingPlayer, Card princessCardInstance)
    {
        EliminatePlayer(actingPlayer.Id, "discarded the Princess", princessCardInstance);
    }

    private void EliminatePlayer(Guid playerId, string reason, Card? cardResponsible = null)
    {
        var player = GetPlayerById(playerId);
        if (player.Status == PlayerStatus.Active)
        {
            player.Eliminate();
            string playerName = player.Name;
            string logMessage = $"{playerName} was eliminated because they {reason}.";
            AddGameLogEntry(new GameLogEntry(
                GameLogEventType.PlayerEliminated,
                player.Id,      // The player who was eliminated (as ActingPlayerId for this log type)
                playerName,     // (as ActingPlayerName for this log type)
                logMessage
            )
            {
                RevealedCardOnElimination = cardResponsible
            });

            // Instantiate the domain event separately
            var playerEliminatedEvent = new PlayerEliminated(
                Id,                         // GameId
                player.Id,                  // PlayerId
                reason,                     // Reason for elimination
                cardResponsible?.Type
            );
            AddDomainEvent(playerEliminatedEvent);
        }
    }

   private bool CheckRoundEndCondition()
    {
        // Check only if the round is currently marked as in progress
        if (GamePhase != GamePhase.RoundInProgress) return false; // Already ended or not started

        var activePlayers = Players.Where(p => p.Status == PlayerStatus.Active).ToList();
        if (activePlayers.Count <= 1 || Deck.IsEmpty)
        {
            EndRound(activePlayers);
            return true; // Indicate round has ended
        }
        return false; // Round continues
    }

    private void EndRound(List<Player> activePlayers)
    {
        if (GamePhase != GamePhase.RoundInProgress) return; // Prevent multiple calls

        GamePhase = GamePhase.RoundOver; // Mark round as over first
        Guid? winnerId = null;
        string reason;
        Player? winningPlayer = null;
        string roundEndMessage;

        if (activePlayers.Count == 1)
        {
            winningPlayer = activePlayers.Single();
            winnerId = winningPlayer.Id;
            reason = "Last player standing";
            roundEndMessage = $"Round {RoundNumber} ended. {winningPlayer.Name} wins as the last player standing.";
        }
        else // Deck is empty
        {
            reason = "Deck empty, highest card wins";
            if (!activePlayers.Any()) 
            {
                winnerId = null; 
                roundEndMessage = $"Round {RoundNumber} ended. No winner as deck is empty and no active players remain.";
            }
            else
            {
                var highestRank = activePlayers.Max(p => p.Hand.GetHeldCard()?.Rank ?? -1);
                var potentialWinners = activePlayers.Where(p => (p.Hand.GetHeldCard()?.Rank ?? -1) == highestRank).ToList();

                if (potentialWinners.Count == 1) { winningPlayer = potentialWinners.Single(); }
                else // Tie in rank, compare discard piles
                {
                    int highestDiscardSum = -1;
                    List<Player> finalWinners = new List<Player>();
                    foreach (var potentialWinner in potentialWinners)
                    {
                        int currentSum = potentialWinner.PlayedCards.Select(cardType => cardType.Value).Sum();
                        if (currentSum > highestDiscardSum) { highestDiscardSum = currentSum; finalWinners.Clear(); finalWinners.Add(potentialWinner); }
                        else if (currentSum == highestDiscardSum) { finalWinners.Add(potentialWinner); }
                    }
                    if (finalWinners.Count == 1) { winningPlayer = finalWinners.Single(); reason += " (Tie broken by discard sum)"; }
                    else { reason += " (Tie in rank and discard sum)"; } // No single winner
                }
                winnerId = winningPlayer?.Id;
                if (winningPlayer != null)
                {
                    roundEndMessage = $"Round {RoundNumber} ended. {winningPlayer.Name} wins: {reason}.";
                }
                else
                {
                    roundEndMessage = $"Round {RoundNumber} ended. Tie: {reason}.";
                }
            }
        }

        LastRoundWinnerId = winnerId;
        
        AddGameLogEntry(new GameLogEntry(
            GameLogEventType.RoundEnd,
            winningPlayer?.Id, // Winner is the 'acting' player in this context
            winningPlayer?.Name ?? string.Empty,
            roundEndMessage
        )
        {
            RoundEndReason = reason
        });

        // If there's a winner, award token *before* creating summaries for RoundEnded event
        if (winningPlayer != null)
        {
            AwardTokenToPlayer(winningPlayer); // This increments TokensWon and raises TokenAwarded
        }

        // Create detailed player summaries *after* token has been awarded
        var playerSummaries = Players.Select(p => new PlayerRoundEndSummary(
            p.Id,
            p.Name,
            p.Status == PlayerStatus.Active && !p.Hand.IsEmpty ? p.Hand.Cards.ToList() : new List<Card>(), // Correctly provide List<Card>
            p.PlayedCards.Select(cardType => cardType.Value).ToList(), // Corrected: Use PlayedCards and cardType.Value
            p.TokensWon
        )).ToList();

        AddDomainEvent(new RoundEnded(Id, winnerId, reason, playerSummaries));

        // Check for game end or start next round
        if (winningPlayer != null && winningPlayer.TokensWon >= TokensNeededToWin)
        {
            EndGame(winningPlayer.Id);
        }
        else if (GamePhase != GamePhase.GameOver) // If game not over (either by win or no winner)
        {
             StartNewRound(); // Automatically start the next round
        }
    }
    
    /// <summary>
    /// Awards a token to the player and raises the TokenAwarded event.
    /// Game end check is now handled after this call in EndRound.
    /// </summary>
    private void AwardTokenToPlayer(Player player)
    {
        player.AddToken(); // This increments player.TokensWon
        string tokenMessage = $"{player.Name} was awarded a token of affection. They now have {player.TokensWon} token(s).";
        AddGameLogEntry(new GameLogEntry(
            GameLogEventType.TokenAwarded,
            player.Id,
            player.Name,
            tokenMessage
        )
        {
            TokensHeld = player.TokensWon
        });
        AddDomainEvent(new TokenAwarded(Id, player.Id, player.TokensWon));
    }

    private void EndGame(Guid winnerId)
    {
        if (GamePhase == GamePhase.GameOver) return;
        GamePhase = GamePhase.GameOver;
        string endMessage = $"Game ended. {GetPlayerById(winnerId).Name} wins with {TokensNeededToWin} token(s).";
        AddGameLogEntry(new GameLogEntry(
            GameLogEventType.GameEnd,
            winnerId,
            GetPlayerById(winnerId).Name,
            endMessage
        ));
        AddDomainEvent(new GameEnded(Id, winnerId));
    }

    private void AdvanceTurn()
    {
        if (GamePhase != GamePhase.RoundInProgress) return;
        GetPlayerById(CurrentTurnPlayerId).SetProtection(false);
        int currentPlayerIndex = Players.FindIndex(p => p.Id == CurrentTurnPlayerId);
        int nextPlayerIndex = currentPlayerIndex;
        Player nextPlayer;
        int loopCheck = 0;
        do
        {
            nextPlayerIndex = (nextPlayerIndex + 1) % Players.Count;
            nextPlayer = Players[nextPlayerIndex];
            if (++loopCheck > Players.Count * 2) {
                 if (!CheckRoundEndCondition()) EndRound(new List<Player>());
                 return;
            }
        } while (nextPlayer.Status == PlayerStatus.Eliminated);

        // Check if only one player remains active after skipping
        // This check might be redundant if CheckRoundEndCondition is robust, but safe to keep
        var activePlayers = Players.Count(p => p.Status == PlayerStatus.Active);
        if(activePlayers <= 1)
        {
             if (CheckRoundEndCondition()) return; // Let Check call EndRound
             EndRound(Players.Where(p => p.Status == PlayerStatus.Active).ToList()); // Force EndRound if Check missed it
             return;
        }

        CurrentTurnPlayerId = nextPlayer.Id;
        HandleTurnStartDrawing(); // This method now checks for round end after drawing
    }

    private void HandleTurnStartDrawing()
    {
        var player = GetPlayerById(CurrentTurnPlayerId);
        if (player.Status != PlayerStatus.Active) return;

        Card? drawnCard = null; // Keep track of the card drawn this turn
        if (!Deck.IsEmpty)
        {
            (Card card, Deck remainingDeck) = Deck.Draw();
            Deck = remainingDeck;
            player.GiveCard(card);
            drawnCard = card; // Store the drawn card

            string drawMessage = $"{player.Name} drew a card.";
            AddGameLogEntry(new GameLogEntry(
                GameLogEventType.PlayerDrewCard, 
                player.Id, 
                player.Name, 
                drawMessage,
                true // This log is private to the player who drew
            )
            {
                DrawnCard = drawnCard // Corrected property name from CardDrawn
            });

            AddDomainEvent(new PlayerDrewCard(Id, player.Id)); // Corrected PlayerDrewCard domain event constructor call

            AddDomainEvent(new DeckChanged(Id, Deck.CardsRemaining));
        }

        // Check round end condition AFTER the draw attempt.
        // This handles the case where the draw emptied the deck or player drew a game-ending card (if such logic existed).
        // CheckRoundEndCondition() calls EndRound() if true.
        bool roundEnded = CheckRoundEndCondition();

        // Only raise TurnStarted event and log if the round didn't just end
        if(!roundEnded && GamePhase == GamePhase.RoundInProgress)
        {
            string turnStartMessage = $"{player.Name}'s turn has started.";
            AddGameLogEntry(new GameLogEntry(
                GameLogEventType.TurnStart,
                player.Id,
                player.Name,
                turnStartMessage
            ));
            AddDomainEvent(new TurnStarted(Id, CurrentTurnPlayerId, RoundNumber)); // Domain event for system use
        }
    }

    private Player GetPlayerById(Guid playerId) { var player = Players.FirstOrDefault(p => p.Id == playerId); if (player == null) throw new ArgumentException($"Player with ID {playerId} not found in this game."); return player; }

    // New method to add log entries
    public void AddLogEntry(GameLogEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));
        _logEntries.Insert(0, entry); // Insert at the beginning to keep newest first
    }

    private void AddGameLogEntry(GameLogEntry logEntry)
    {
        _logEntries.Insert(0, logEntry); // Keep newest first
    }
}