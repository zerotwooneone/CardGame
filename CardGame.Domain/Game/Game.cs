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
    public Card? SetAsideCard { get; private set; } // The single card set aside face-down
    public List<Card> PubliclySetAsideCards { get; private set; } = new List<Card>(); // Cards set aside face-up (for 2 players)
    public Guid? LastRoundWinnerId { get; private set; }

    // --- Added field to store the definitive card set for the game's deck ---
    private readonly IReadOnlyList<Card> _initialDeckCardSet;

    // --- Domain Event Handling ---
    private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() { _domainEvents.Clear(); }
    private void AddDomainEvent(IDomainEvent domainEvent) { _domainEvents.Add(domainEvent); }
    // --- End Domain Event Handling ---

    // Private constructor for factory methods and persistence/mapping frameworks
    private Game(Guid id, IReadOnlyList<Card> initialDeckCardSet)
    {
        Id = id;
        _initialDeckCardSet = initialDeckCardSet ?? throw new ArgumentNullException(nameof(initialDeckCardSet));
        Deck = Deck.Load(Enumerable.Empty<Card>()); // Initialize with empty deck placeholder
    }

    // --- Factory Methods ---

    /// <summary>
    /// Creates a brand new game instance using provided player info (ID and Name).
    /// Optionally accepts a predefined list of cards for the initial deck.
    /// </summary>
    public static Game CreateNewGame(
        IEnumerable<PlayerInfo> playerInfos,
        Guid creatorPlayerId,
        int tokensToWin = 4,
        IEnumerable<Card>? initialDeckCards = null)
    {
        var gameId = Guid.NewGuid();
        List<Card> cardSetToUse;

        var providedList = initialDeckCards?.ToList();
        if (providedList != null && providedList.Count == 16) // Example: Validate count
        {
            cardSetToUse = providedList;
        }
        else
        {
            cardSetToUse = CreateStandardCardList(); // Use internal helper
        }

        var game = new Game(gameId, new ReadOnlyCollection<Card>(cardSetToUse))
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


    /// <summary>
    /// Rehydrates a Game aggregate from its persisted state.
    /// </summary>
    public static Game Load(
        Guid id,
        int roundNumber,
        GamePhase gamePhase,
        Guid currentTurnPlayerId,
        List<Player> players,
        Deck deck,
        Card? setAsideCard,
        List<Card> publiclySetAsideCards,
        List<Card> discardPile,
        int tokensToWin,
        Guid? lastRoundWinnerId,
        List<Card> initialDeckCardSet)
    {
        var game = new Game(id, new ReadOnlyCollection<Card>(initialDeckCardSet ?? CreateStandardCardList()))
        {
            RoundNumber = roundNumber,
            GamePhase = gamePhase,
            CurrentTurnPlayerId = currentTurnPlayerId,
            Players = players ?? new List<Player>(),
            Deck = deck, // Assign the loaded current deck state
            SetAsideCard = setAsideCard,
            PubliclySetAsideCards = publiclySetAsideCards ?? new List<Card>(),
            DiscardPile = discardPile ?? new List<Card>(),
            TokensNeededToWin = tokensToWin,
            LastRoundWinnerId = lastRoundWinnerId
        };
        return game;
    }

    // --- Public Methods (Game Actions) ---

    /// <summary>
    /// Starts a new round of the game using the game's initial card set.
    /// </summary>
    public void StartNewRound(IRandomizer? randomizer = null)
    {
        if (GamePhase == GamePhase.GameOver) throw new GameRuleException("Cannot start a new round, the game is over.");
        if (GamePhase == GamePhase.RoundInProgress) throw new GameRuleException("Cannot start a new round while one is in progress.");

        RoundNumber++;
        Deck = Deck.CreateShuffled(_initialDeckCardSet, randomizer); // Use the stored set
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

        if (CheckRoundEndCondition()) return; // Check if dealing emptied deck

        // Determine Starting Player
        Guid startingPlayerId;
        if (RoundNumber == 1 || LastRoundWinnerId == null || !Players.Any(p => p.Id == LastRoundWinnerId)) { startingPlayerId = Players.First().Id; }
        else { startingPlayerId = LastRoundWinnerId.Value; }
        CurrentTurnPlayerId = startingPlayerId;

        GamePhase = GamePhase.RoundInProgress;

        // Raise event including public set aside cards info (ID and Type)
        AddDomainEvent(new RoundStarted(
            Id, RoundNumber, playerIds, Deck.CardsRemaining, SetAsideCard?.Type,
            PubliclySetAsideCards.Select(c => new PublicCardInfo(c.Id, c.Type)).ToList()
        ));
        HandleTurnStartDrawing(); // Start the first turn's draw phase
    }

    /// <summary>
    /// Handles a player playing a card.
    /// </summary>
    public void PlayCard(Guid playerId, Card cardToPlayInstance, Guid? targetPlayerId, CardType? guessedCardType)
    {
        var cardType = cardToPlayInstance.Type;
        ValidatePlayCardAction(playerId, cardToPlayInstance, targetPlayerId, guessedCardType);
        var actingPlayer = GetPlayerById(playerId);
        Player? targetPlayer = targetPlayerId.HasValue ? GetPlayerById(targetPlayerId.Value) : null;

        actingPlayer.PlayCard(cardToPlayInstance);
        DiscardPile.Add(cardToPlayInstance);

        AddDomainEvent(new PlayerPlayedCard(Id, playerId, cardType, targetPlayerId, guessedCardType));

        // Dispatch to specific card logic
        switch (cardType)
        {
            case var _ when cardType == CardType.Guard: ExecuteGuardEffect(actingPlayer, targetPlayer, guessedCardType); break;
            case var _ when cardType == CardType.Priest: ExecutePriestEffect(actingPlayer, targetPlayer); break;
            case var _ when cardType == CardType.Baron: ExecuteBaronEffect(actingPlayer, targetPlayer); break;
            case var _ when cardType == CardType.Handmaid: ExecuteHandmaidEffect(actingPlayer); break;
            case var _ when cardType == CardType.Prince: ExecutePrinceEffect(targetPlayer ?? actingPlayer); break;
            case var _ when cardType == CardType.King: ExecuteKingEffect(actingPlayer, targetPlayer); break;
            case var _ when cardType == CardType.Countess: ExecuteCountessEffect(actingPlayer); break;
            case var _ when cardType == CardType.Princess: ExecutePrincessEffect(actingPlayer, cardToPlayInstance); break;
            default: throw new ArgumentOutOfRangeException(nameof(cardToPlayInstance), $"Unknown card type: {cardType.Name}");
        }

        bool roundEnded = CheckRoundEndCondition();
        if (!roundEnded && GamePhase == GamePhase.RoundInProgress)
        {
             AdvanceTurn();
        }
    }

    // --- Private Helper Methods ---

    // Moved CreateStandardCardList here from Deck
    private static List<Card> CreateStandardCardList()
    {
        return new List<Card> {
            new Card(Guid.NewGuid(), CardType.Princess), new Card(Guid.NewGuid(), CardType.Countess),
            new Card(Guid.NewGuid(), CardType.King), new Card(Guid.NewGuid(), CardType.Prince),
            new Card(Guid.NewGuid(), CardType.Prince), new Card(Guid.NewGuid(), CardType.Handmaid),
            new Card(Guid.NewGuid(), CardType.Handmaid), new Card(Guid.NewGuid(), CardType.Baron),
            new Card(Guid.NewGuid(), CardType.Baron), new Card(Guid.NewGuid(), CardType.Priest),
            new Card(Guid.NewGuid(), CardType.Priest), new Card(Guid.NewGuid(), CardType.Guard),
            new Card(Guid.NewGuid(), CardType.Guard), new Card(Guid.NewGuid(), CardType.Guard),
            new Card(Guid.NewGuid(), CardType.Guard), new Card(Guid.NewGuid(), CardType.Guard),
        };
    }

    private void ValidatePlayCardAction(Guid playerId, Card cardToPlayInstance, Guid? targetPlayerId, CardType? guessedCardType)
    {
        var cardType = cardToPlayInstance.Type;
        if (GamePhase != GamePhase.RoundInProgress) throw new InvalidMoveException("Cannot play cards when the round is not in progress.");
        if (CurrentTurnPlayerId != playerId) throw new InvalidMoveException($"It is not player {GetPlayerById(playerId).Name}'s turn.");
        var player = GetPlayerById(playerId);
        if (!player.Hand.GetCards().Any(c => c.Id == cardToPlayInstance.Id)) throw new InvalidMoveException($"Player {player.Name} does not hold the specified card instance (ID: {cardToPlayInstance.Id}).");
        if ((cardType == CardType.King || cardType == CardType.Prince) && player.Hand.Contains(CardType.Countess)) throw new GameRuleException($"Player {player.Name} must play the Countess.");
        bool requiresTarget = cardType == CardType.Guard || cardType == CardType.Priest || cardType == CardType.Baron || cardType == CardType.King || cardType == CardType.Prince;
        if (targetPlayerId.HasValue)
        {
            var targetPlayer = GetPlayerById(targetPlayerId.Value);
            if (targetPlayer.Status == PlayerStatus.Eliminated) throw new InvalidMoveException($"Cannot target eliminated player {targetPlayer.Name}.");
            if (targetPlayer.IsProtected) throw new InvalidMoveException($"Player {targetPlayer.Name} is protected by a Handmaid.");
            if (targetPlayerId.Value == playerId && !CanTargetSelf(cardType) && (cardType == CardType.Guard || cardType == CardType.Priest || cardType == CardType.Baron || cardType == CardType.King)) throw new InvalidMoveException($"Cannot target self with {cardType.Name}.");
        }
        else if ((cardType == CardType.Guard || cardType == CardType.Priest || cardType == CardType.Baron || cardType == CardType.King))
        {
             bool validTargetsExist = Players.Any(p => p.Id != playerId && p.Status == PlayerStatus.Active && !p.IsProtected);
             if (validTargetsExist) throw new InvalidMoveException($"{cardType.Name} requires a target player.");
        }
        if (cardType == CardType.Guard && (targetPlayerId == null || guessedCardType == null || guessedCardType == CardType.Guard)) throw new InvalidMoveException("Guard requires a valid target player and a non-Guard guess.");
    }

    private bool CanTargetSelf(CardType cardType) => cardType == CardType.Prince;

    private void ExecuteGuardEffect(Player actingPlayer, Player? targetPlayer, CardType? guessedCardType)
    {
        if (targetPlayer == null || guessedCardType == null) return;
        bool correctGuess = targetPlayer.Hand.Contains(guessedCardType);
        if (correctGuess)
        {
            EliminatePlayer(targetPlayer.Id, $"guessed correctly by {actingPlayer.Name} with a Guard", CardType.Guard);
        }
        AddDomainEvent(new GuardGuessResult(Id, actingPlayer.Id, targetPlayer.Id, guessedCardType, correctGuess));
    }

    private void ExecutePriestEffect(Player actingPlayer, Player? targetPlayer)
    {
        if (targetPlayer == null) return;
        var revealedCard = targetPlayer.Hand.GetHeldCard();
        if (revealedCard != null)
        {
            AddDomainEvent(new PriestEffectUsed(Id, actingPlayer.Id, targetPlayer.Id, revealedCard.Id, revealedCard.Type));
        }
    }

    private void ExecuteBaronEffect(Player actingPlayer, Player? targetPlayer)
    {
        if (targetPlayer == null) return;
        Card? actingPlayerCard = actingPlayer.Hand.GetHeldCard();
        Card? targetPlayerCard = targetPlayer.Hand.GetHeldCard();
        if (actingPlayerCard == null || targetPlayerCard == null) return; // Cannot compare if hands are empty/invalid

        Guid? loserId = null;
        if (actingPlayerCard.Rank > targetPlayerCard.Rank)
        {
            loserId = targetPlayer.Id;
            EliminatePlayer(targetPlayer.Id, $"lost Baron comparison to {actingPlayer.Name}", CardType.Baron);
        }
        else if (targetPlayerCard.Rank > actingPlayerCard.Rank)
        {
            loserId = actingPlayer.Id;
            EliminatePlayer(actingPlayer.Id, $"lost Baron comparison to {targetPlayer.Name}", CardType.Baron);
        }
        AddDomainEvent(new BaronComparisonResult(Id, actingPlayer.Id, actingPlayerCard.Type, targetPlayer.Id, targetPlayerCard.Type, loserId));
    }

    private void ExecuteHandmaidEffect(Player actingPlayer)
    {
        actingPlayer.SetProtection(true);
        AddDomainEvent(new HandmaidProtectionSet(Id, actingPlayer.Id));
    }

    private void ExecutePrinceEffect(Player targetPlayer)
    {
        if (targetPlayer.Hand.IsEmpty)
        {
            AddDomainEvent(new PrinceEffectFailed(Id, targetPlayer.Id, "Target hand empty"));
            return;
        }
        Card? discardedCard = targetPlayer.DiscardHand(Deck.IsEmpty);
        if (discardedCard == null) return; // Should not happen if hand wasn't empty

        AddDomainEvent(new PrinceEffectUsed(Id, CurrentTurnPlayerId, targetPlayer.Id, discardedCard.Type, discardedCard.Id)); // Event includes discarded card type

        if (discardedCard.Type == CardType.Princess)
        {
            EliminatePlayer(targetPlayer.Id, "discarded the Princess", CardType.Prince);
        }
        else if (targetPlayer.Status == PlayerStatus.Active) // Don't draw if eliminated
        {
            if (!Deck.IsEmpty)
            {
                (Card newCard, Deck remainingDeck) = Deck.Draw();
                Deck = remainingDeck;
                targetPlayer.GiveCard(newCard);
                AddDomainEvent(new PlayerDrewCard(Id, targetPlayer.Id));
                AddDomainEvent(new DeckChanged(Id, Deck.CardsRemaining));
            }
            else if (SetAsideCard != null) // Draw set aside card if deck empty
            {
                targetPlayer.GiveCard(SetAsideCard);
                AddDomainEvent(new PlayerDrewCard(Id, targetPlayer.Id));
                var usedCardType = SetAsideCard.Type;
                SetAsideCard = null; // Set aside card is now used
                AddDomainEvent(new SetAsideCardUsed(Id, usedCardType));
            }
            else
            {
                EliminatePlayer(targetPlayer.Id, "had no card to draw after Prince effect", CardType.Prince);
            }
        }
    }

    private void ExecuteKingEffect(Player actingPlayer, Player? targetPlayer)
    {
        if (targetPlayer == null) return;
        actingPlayer.SwapHandWith(targetPlayer);
        AddDomainEvent(new KingEffectUsed(Id, actingPlayer.Id, targetPlayer.Id));
    }

    private void ExecuteCountessEffect(Player actingPlayer)
    {
        AddDomainEvent(new PlayerPlayedCountess(Id, actingPlayer.Id));
    }

    private void ExecutePrincessEffect(Player actingPlayer, Card princessCardInstance)
    {
        EliminatePlayer(actingPlayer.Id, "discarded the Princess", princessCardInstance.Type);
    }

    private void EliminatePlayer(Guid playerId, string reason, CardType? cardResponsible)
    {
        var player = GetPlayerById(playerId);
        if (player.Status == PlayerStatus.Active)
        {
            player.Eliminate();
            AddDomainEvent(new PlayerEliminated(Id, playerId, reason, cardResponsible));
        }
    }

    private bool CheckRoundEndCondition()
    {
        if (GamePhase != GamePhase.RoundInProgress) return false;
        var activePlayers = Players.Where(p => p.Status == PlayerStatus.Active).ToList();
        if (activePlayers.Count <= 1 || Deck.IsEmpty)
        {
            EndRound(activePlayers);
            return true;
        }
        return false;
    }

    private void EndRound(List<Player> activePlayers)
    {
        if (GamePhase != GamePhase.RoundInProgress) return; // Prevent multiple calls

        GamePhase = GamePhase.RoundOver;
        Guid? winnerId = null;
        string reason;
        var finalHands = Players.ToDictionary(p => p.Id, p => p.Hand.GetHeldCard()?.Type);

        if (activePlayers.Count == 1)
        {
            winnerId = activePlayers.Single().Id;
            reason = "Last player standing";
        }
        else // Deck is empty
        {
            reason = "Deck empty, highest card wins";
            if (!activePlayers.Any()) { /* Draw */ }
            else
            {
                var highestRank = activePlayers.Max(p => p.Hand.GetHeldCard()?.Rank ?? -1);
                var potentialWinners = activePlayers.Where(p => (p.Hand.GetHeldCard()?.Rank ?? -1) == highestRank).ToList();
                if (potentialWinners.Count == 1) winnerId = potentialWinners.Single().Id;
                else winnerId = potentialWinners.FirstOrDefault()?.Id; // Simplified tie-breaker
                if(potentialWinners.Count > 1) reason += " (Tie broken arbitrarily)";
            }
        }

        LastRoundWinnerId = winnerId; // Store winner for next round start

        AddDomainEvent(new RoundEnded(Id, winnerId, finalHands, reason));
        if (winnerId.HasValue) AwardToken(winnerId.Value);
    }

    private void AwardToken(Guid playerId)
    {
        var player = GetPlayerById(playerId);
        player.AddToken();
        AddDomainEvent(new TokenAwarded(Id, playerId, player.TokensWon));
        if (player.TokensWon >= TokensNeededToWin) EndGame(playerId);
    }

    private void EndGame(Guid winnerId)
    {
        GamePhase = GamePhase.GameOver;
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
            if (++loopCheck > Players.Count * 2) break; // Safety break
        } while (nextPlayer.Status == PlayerStatus.Eliminated);

        var activePlayers = Players.Count(p => p.Status == PlayerStatus.Active);
        if(activePlayers <= 1 && loopCheck <= Players.Count * 2) // Check if only one active player remains
        {
             if (!CheckRoundEndCondition()) { EndRound(Players.Where(p => p.Status == PlayerStatus.Active).ToList()); }
             return;
        }

        CurrentTurnPlayerId = nextPlayer.Id;
        HandleTurnStartDrawing();
    }

    private void HandleTurnStartDrawing()
    {
        var player = GetPlayerById(CurrentTurnPlayerId);
        if (player.Status != PlayerStatus.Active) return;
        bool drewCard = false;
        if (!Deck.IsEmpty)
        {
            (Card card, Deck remainingDeck) = Deck.Draw();
            Deck = remainingDeck;
            player.GiveCard(card);
            AddDomainEvent(new PlayerDrewCard(Id, player.Id));
            AddDomainEvent(new DeckChanged(Id, Deck.CardsRemaining));
            drewCard = true;
        }
        bool roundEnded = CheckRoundEndCondition(); // Check AFTER draw attempt
        if(!roundEnded && GamePhase == GamePhase.RoundInProgress)
        {
            AddDomainEvent(new TurnStarted(Id, CurrentTurnPlayerId, RoundNumber));
        }
    }

    private Player GetPlayerById(Guid playerId)
    {
        var player = Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) throw new ArgumentException($"Player with ID {playerId} not found in this game.");
        return player;
    }

}