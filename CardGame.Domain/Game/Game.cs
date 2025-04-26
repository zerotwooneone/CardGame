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
    public Deck Deck { get; private set; } = Deck.CreateShuffledDeck(); // Initialize deck
    public List<Card> DiscardPile { get; private set; } = new List<Card>(); // Now holds specific Card instances
    public Guid CurrentTurnPlayerId { get; private set; }
    public GamePhase GamePhase { get; private set; } = GamePhase.NotStarted;
    public int RoundNumber { get; private set; }
    public int TokensNeededToWin { get; private set; } = 4; // Example default
    public Card? SetAsideCard { get; private set; } // Changed from CardType? to Card?

    // --- Domain Event Handling ---
    private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    // --- End Domain Event Handling ---


    // Private constructor for persistence/mapping frameworks
    private Game(Guid id) { Id = id; }

    /// <summary>
    /// Creates a brand new game instance using provided player info (ID and Name).
    /// </summary>
    /// <param name="playerInfos">Information for the players joining the game.</param>
    /// <param name="tokensToWin">Number of tokens needed to win.</param>
    /// <returns>A new Game instance.</returns>
    public static Game CreateNewGame(IEnumerable<PlayerInfo> playerInfos, int tokensToWin = 4) // Changed parameter type
    {
        var gameId = Guid.NewGuid();
        var game = new Game(gameId) // Use private constructor
        {
            TokensNeededToWin = tokensToWin,
            GamePhase = GamePhase.NotStarted,
            Deck = Deck.CreateShuffledDeck() // Ensure new game gets a fresh deck
        };

        // Ensure playerInfos is materialized if needed for validation/event
        var playerInfoList = playerInfos?.ToList() ?? new List<PlayerInfo>();

        if (!playerInfoList.Any()) throw new ArgumentException("Player information cannot be empty.", nameof(playerInfos));

        foreach (var pInfo in playerInfoList)
        {
            // **ASSUMPTION:** Player needs a way to be created with a specific ID and Name.
            // Option A: Modify Player.Create
            // var player = Player.Create(pInfo.Id, pInfo.Name);
            // Option B: Use Player.Load (if appropriate semantics)
            var player = Player.Load(pInfo.Id, pInfo.Name, PlayerStatus.Active, Hand.Empty, new List<CardType>(), 0, false);

            game.Players.Add(player);
            // PlayerInfo list already created above
        }

        if (game.Players.Count < 2) throw new DomainException("Game requires at least 2 players.", 1000);
        if (game.Players.Count > 4) throw new DomainException("Game cannot have more than 4 players.", 1000); // Added upper bound check

        // Pass the original PlayerInfo list to the event
        game.AddDomainEvent(new GameCreated(gameId, playerInfoList, tokensToWin));
        return game;
    }
    
    /// <summary>
    /// Rehydrates a Game aggregate from its persisted state.
    /// Assumes constituent objects (Players, Deck, Cards) are already rehydrated.
    /// </summary>
    public static Game Load(
        Guid id,
        int roundNumber,
        GamePhase gamePhase,
        Guid currentTurnPlayerId,
        List<Player> players, 
        Deck deck,          
        Card? setAsideCard,  
        List<Card> discardPile, 
        int tokensToWin)
    {
        var game = new Game(id);

        // Assign loaded state
        game.RoundNumber = roundNumber;
        game.GamePhase = gamePhase;
        game.CurrentTurnPlayerId = currentTurnPlayerId;
        game.Players = players ?? new List<Player>();
        game.Deck = deck ?? Deck.CreateShuffledDeck(); // Or handle empty deck state appropriately
        game.SetAsideCard = setAsideCard;
        game.DiscardPile = discardPile ?? new List<Card>();
        game.TokensNeededToWin = tokensToWin;

        // No domain events are raised during loading
        return game;
    }    


    public void StartNewRound(IRandomizer? randomizer = null)
    {
        if (GamePhase == GamePhase.GameOver) throw new GameRuleException("Cannot start a new round, the game is over.");
        // Allow starting round if NotStarted or RoundOver
        if (GamePhase == GamePhase.RoundInProgress) throw new GameRuleException("Cannot start a new round while one is in progress.");

        RoundNumber++;
        Deck = Deck.CreateShuffledDeck(randomizer); // Creates Deck with Card instances
        DiscardPile.Clear();
        SetAsideCard = null; // Reset set aside card

        // Reset players for the new round
        foreach (var player in Players)
        {
            player.PrepareForNewRound(); // Resets status, hand, protection etc.
        }

        // Set aside card(s) - example: one card face down
        if (!Deck.IsEmpty)
        {
             // Draw returns (Card DrawnCard, Deck RemainingDeck)
             (Card drawnCard, Deck remainingDeck) = Deck.Draw();
             SetAsideCard = drawnCard; // Store the actual Card instance
             Deck = remainingDeck; // Update the deck state
        }
        // Optional: Set aside more cards face up depending on player count (add logic here)

        // Deal initial hands
        var playerIds = new List<Guid>();
        foreach (var player in Players)
        {
             if (!Deck.IsEmpty)
             {
                 (Card dealtCard, Deck remainingDeck) = Deck.Draw();
                 player.GiveCard(dealtCard); // Give the specific Card instance
                 Deck = remainingDeck; // Update deck state
             }
             playerIds.Add(player.Id);
        }

        // Set starting player (e.g., winner of last round, or random/fixed for first round)
        CurrentTurnPlayerId = Players.First().Id; // Simplified starting player logic
        GamePhase = GamePhase.RoundInProgress;

        // Raise events about the round start and the first turn starting
        // Assuming RoundStarted event now takes Card? instead of CardType?
        AddDomainEvent(new RoundStarted(Id, RoundNumber, playerIds, Deck.CardsRemaining, SetAsideCard?.Type)); // Send Type for event
        // Immediately start the first turn's drawing phase
        HandleTurnStartDrawing();
    }


    // Central method to handle playing a card (Refined Aggregate approach)
    // Changed cardToPlay parameter from CardType to Card
    public void PlayCard(Guid playerId, Card cardToPlayInstance, Guid? targetPlayerId, CardType? guessedCardType)
    {
        // Get the type from the instance for logic checks
        var cardType = cardToPlayInstance.Type;

        // --- 1. Validation ---
        // Pass the instance to validation, but validation mostly uses the type
        ValidatePlayCardAction(playerId, cardType, targetPlayerId, guessedCardType);
        var actingPlayer = GetPlayerById(playerId);
        Player? targetPlayer = targetPlayerId.HasValue ? GetPlayerById(targetPlayerId.Value) : null;

        // --- 2. Perform the Action (Remove card first) ---
        actingPlayer.PlayCard(cardToPlayInstance);

        // Add the specific played card instance to the central discard pile
        DiscardPile.Add(cardToPlayInstance);

        // Raise event AFTER card is confirmed played and state updated
        AddDomainEvent(new PlayerPlayedCard(Id, playerId, cardType, targetPlayerId, guessedCardType));

        // --- 3. Dispatch to Specific Card Logic ---
        switch (cardType)
        {
            case var _ when cardType == CardType.Guard:
                ExecuteGuardEffect(actingPlayer, targetPlayer, guessedCardType);
                break;
            case var _ when cardType == CardType.Priest:
                ExecutePriestEffect(actingPlayer, targetPlayer);
                break;
            case var _ when cardType == CardType.Baron:
                ExecuteBaronEffect(actingPlayer, targetPlayer);
                break;
            case var _ when cardType == CardType.Handmaid:
                ExecuteHandmaidEffect(actingPlayer);
                break;
            case var _ when cardType == CardType.Prince:
                Player princeTarget = targetPlayer ?? actingPlayer;
                ExecutePrinceEffect(princeTarget);
                break;
            case var _ when cardType == CardType.King:
                ExecuteKingEffect(actingPlayer, targetPlayer);
                break;
            case var _ when cardType == CardType.Countess:
                ExecuteCountessEffect(actingPlayer);
                break;
            case var _ when cardType == CardType.Princess:
                // Pass the specific instance in case EliminatePlayer needs it (though unlikely)
                ExecutePrincessEffect(actingPlayer, cardToPlayInstance);
                break;
            default:
                // Use cardType for the exception message
                throw new ArgumentOutOfRangeException(nameof(cardToPlayInstance), $"Unknown card type: {cardType.Name}");
        }

        // --- 4. Post-Effect State Checks & Turn Advancement ---
        bool roundEnded = CheckRoundEndCondition();
        if (!roundEnded && GamePhase == GamePhase.RoundInProgress)
        {
             AdvanceTurn();
        }
    }

    private void ValidatePlayCardAction(Guid playerId, CardType cardType, Guid? targetPlayerId, CardType? guessedCardType)
    {
        if (GamePhase != GamePhase.RoundInProgress)
             throw new InvalidMoveException("Cannot play cards when the round is not in progress.");
        if (CurrentTurnPlayerId != playerId)
            throw new InvalidMoveException($"It is not player {GetPlayerById(playerId).Name}'s turn.");

        var player = GetPlayerById(playerId);
        // Validate that the player's hand contains the specific card instance (or at least the type)
        // Checking by type is usually sufficient validation here, assuming input is trusted.
        if (!player.Hand.Contains(cardType))
            throw new InvalidMoveException($"Player {player.Name} does not have a {cardType.Name} card.");
        // More precise check (optional): if (!player.Hand.GetCards().Contains(cardToPlayInstance)) ...

        // Countess Rule Check (uses Type)
        if ((cardType == CardType.King || cardType == CardType.Prince) && player.Hand.Contains(CardType.Countess))
             throw new GameRuleException($"Player {player.Name} must play the Countess.");

        // Target Validation (operates on Player state)
        if (targetPlayerId.HasValue)
        {
            var targetPlayer = GetPlayerById(targetPlayerId.Value);
            if (targetPlayer.Status == PlayerStatus.Eliminated)
                 throw new InvalidMoveException($"Cannot target eliminated player {targetPlayer.Name}.");
            if (targetPlayer.IsProtected)
                 throw new InvalidMoveException($"Player {targetPlayer.Name} is protected by a Handmaid.");
            // Check if targeting self is allowed for this card type
            if (targetPlayerId.Value == playerId && !CanTargetSelf(cardType) &&
                (cardType == CardType.Guard || cardType == CardType.Priest || cardType == CardType.Baron || cardType == CardType.King))
                throw new InvalidMoveException($"Cannot target self with {cardType.Name}.");
        }
        else if ((cardType == CardType.Guard || cardType == CardType.Priest || cardType == CardType.Baron || cardType == CardType.King))
        {
             bool validTargetsExist = Players.Any(p => p.Id != playerId && p.Status == PlayerStatus.Active && !p.IsProtected);
             if (validTargetsExist)
             {
                 throw new InvalidMoveException($"{cardType.Name} requires a target player.");
             }
        }

        if (cardType == CardType.Guard && (targetPlayerId == null || guessedCardType == null || guessedCardType == CardType.Guard))
            throw new InvalidMoveException("Guard requires a valid target player and a non-Guard guess.");
    }

    private bool CanTargetSelf(CardType cardType) => cardType == CardType.Prince; 

    private void ExecuteGuardEffect(Player actingPlayer, Player? targetPlayer, CardType? guessedCardType)
    {
        if (targetPlayer == null || guessedCardType == null) return;
        bool correctGuess = targetPlayer.Hand.Contains(guessedCardType); // Checks type
        if (correctGuess)
        {
            EliminatePlayer(targetPlayer.Id, $"guessed correctly by {actingPlayer.Name} with a Guard", CardType.Guard);
        }
        AddDomainEvent(new GuardGuessResult(Id, actingPlayer.Id, targetPlayer.Id, guessedCardType, correctGuess));
    }

    private void ExecutePriestEffect(Player actingPlayer, Player? targetPlayer)
    {
        if (targetPlayer == null) return;
        // GetHeldCard() returns the specific Card instance
        var revealedCard = targetPlayer.Hand.GetHeldCard();
        if (revealedCard != null) {
            // Raise event with specific Card ID and Type
            AddDomainEvent(new PriestEffectUsed(
                Id,
                actingPlayer.Id,
                targetPlayer.Id,
                revealedCard.Id, // Pass the ID
                revealedCard.Type // Pass the Type
            ));
        }
    }

     private void ExecuteBaronEffect(Player actingPlayer, Player? targetPlayer)
    {
         if (targetPlayer == null) return;
        Card? actingPlayerCard = actingPlayer.Hand.GetHeldCard(); // Gets Card instance
        Card? targetPlayerCard = targetPlayer.Hand.GetHeldCard();
        if (actingPlayerCard == null || targetPlayerCard == null) return;

        Guid? loserId = null;
        if (actingPlayerCard.Rank > targetPlayerCard.Rank) // Compares Rank
        {
             loserId = targetPlayer.Id;
             EliminatePlayer(targetPlayer.Id, $"lost Baron comparison to {actingPlayer.Name}", CardType.Baron);
        }
        else if (targetPlayerCard.Rank > actingPlayerCard.Rank)
        {
             loserId = actingPlayer.Id;
             EliminatePlayer(actingPlayer.Id, $"lost Baron comparison to {targetPlayer.Name}", CardType.Baron);
        }
        AddDomainEvent(new BaronComparisonResult(Id, actingPlayer.Id, actingPlayerCard.Type, targetPlayer.Id, targetPlayerCard.Type, loserId)); // Uses Type
    }

     private void ExecuteHandmaidEffect(Player actingPlayer)
     {
         actingPlayer.SetProtection(true);
         AddDomainEvent(new HandmaidProtectionSet(Id, actingPlayer.Id));
     }

    private void ExecutePrinceEffect(Player targetPlayer)
    {
        if(targetPlayer.Hand.IsEmpty)
        {
             AddDomainEvent(new PrinceEffectFailed(Id, targetPlayer.Id, "Target hand empty"));
             return;
        }
        Card? discardedCard = targetPlayer.DiscardHand(Deck.IsEmpty); 
        if (discardedCard == null) return;

        AddDomainEvent(new PrinceEffectUsed(Id, CurrentTurnPlayerId, targetPlayer.Id, discardedCard.Type, discardedCard.Id)); 

        if (discardedCard.Type == CardType.Princess)
        {
            EliminatePlayer(targetPlayer.Id, "discarded the Princess", CardType.Prince);
        }
        else if (targetPlayer.Status == PlayerStatus.Active)
        {
             if (!Deck.IsEmpty)
             {
                 (Card newCard, Deck remainingDeck) = Deck.Draw();
                 Deck = remainingDeck;
                 targetPlayer.GiveCard(newCard); 
                 AddDomainEvent(new PlayerDrewCard(Id, targetPlayer.Id));
                 AddDomainEvent(new DeckChanged(Id, Deck.CardsRemaining));
             }
             else if (SetAsideCard != null)
             {
                  targetPlayer.GiveCard(SetAsideCard); // Gives Card instance
                  AddDomainEvent(new PlayerDrewCard(Id, targetPlayer.Id));
                  var usedCardType = SetAsideCard.Type;
                  SetAsideCard = null;
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
        // Pass the CardType to EliminatePlayer for consistency in event/reason
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
        GamePhase = GamePhase.RoundOver;
        Guid? winnerId = null;
        string reason;
        var finalHands = Players.ToDictionary(p => p.Id, p => p.Hand.GetHeldCard()?.Type); // Uses Type

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
                var highestRank = activePlayers.Max(p => p.Hand.GetHeldCard()?.Rank ?? -1); // Uses Rank
                var potentialWinners = activePlayers.Where(p => (p.Hand.GetHeldCard()?.Rank ?? -1) == highestRank).ToList();
                if (potentialWinners.Count == 1) winnerId = potentialWinners.Single().Id;
                else winnerId = potentialWinners.FirstOrDefault()?.Id; // Simplified tie-breaker
                if(potentialWinners.Count > 1) reason += " (Tie broken arbitrarily)";
            }
        }
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
        do // Find next active player
        {
            nextPlayerIndex = (nextPlayerIndex + 1) % Players.Count;
            nextPlayer = Players[nextPlayerIndex];
            if (nextPlayerIndex == currentPlayerIndex && nextPlayer.Status != PlayerStatus.Active) break; // All others eliminated
        } while (nextPlayer.Status == PlayerStatus.Eliminated);

        CurrentTurnPlayerId = nextPlayer.Id;
        HandleTurnStartDrawing(); // Draw card and raise TurnStarted event
    }

    private void HandleTurnStartDrawing()
    {
        var player = GetPlayerById(CurrentTurnPlayerId);
        if (player.Status != PlayerStatus.Active) return; // Eliminated players don't draw

        bool drewCard = false;
        if (!Deck.IsEmpty)
        {
            (Card card, Deck remainingDeck) = Deck.Draw();
            Deck = remainingDeck; // Deck might be empty now
            player.GiveCard(card);
            AddDomainEvent(new PlayerDrewCard(Id, player.Id));
            AddDomainEvent(new DeckChanged(Id, Deck.CardsRemaining));
            drewCard = true;
        }

        // Check round end condition AFTER the draw attempt, regardless of success.
        // This correctly handles the case where the draw emptied the deck.
        bool roundEnded = CheckRoundEndCondition();

        // Only raise TurnStarted if the round didn't just end
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