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

    // --- Domain Event Handling ---
    private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() { _domainEvents.Clear(); }
    private void AddDomainEvent(IDomainEvent domainEvent) { _domainEvents.Add(domainEvent); }
    // --- End Domain Event Handling ---

    public IReadOnlyList<GameLogEntry> LogEntries => _logEntries.AsReadOnly(); // Public accessor for log entries

    private Game(Guid id, IReadOnlyList<Card> initialDeckCardSet)
    {
        Id = id;
        _initialDeckCardSet = initialDeckCardSet ?? throw new ArgumentNullException(nameof(initialDeckCardSet));
        Deck = Deck.Load(Enumerable.Empty<Card>());
    }

    // --- Factory Methods ---
    public static Game CreateNewGame( IEnumerable<PlayerInfo> playerInfos, Guid creatorPlayerId, int tokensToWin = 4, IEnumerable<Card>? initialDeckCards = null)
    {
        var gameId = Guid.NewGuid();
        List<Card> cardSetToUse;
        var providedList = initialDeckCards?.ToList();
        if (providedList != null && providedList.Count == 16) { cardSetToUse = providedList; }
        else { cardSetToUse = CreateStandardCardList(); }

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

    public static Game Load( Guid id, int roundNumber, GamePhase gamePhase, Guid currentTurnPlayerId, List<Player> players, Deck deck, Card? setAsideCard, List<Card> publiclySetAsideCards, List<Card> discardPile, int tokensToWin, Guid? lastRoundWinnerId, List<Card> initialDeckCardSet)
    {
        var game = new Game(id, new ReadOnlyCollection<Card>(initialDeckCardSet ?? CreateStandardCardList()))
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

        AddDomainEvent(new RoundStarted(
            Id, RoundNumber, playerIds, Deck.CardsRemaining, SetAsideCard?.Type,
            PubliclySetAsideCards.Select(c => new PublicCardInfo(c.Id, c.Type)).ToList()
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

        // Check if the round ended due to the card play (elimination) or if deck was already empty.
        bool roundEndedImmediately = CheckRoundEndCondition();

        // If the round didn't end from the card play itself, advance the turn.
        if (!roundEndedImmediately && GamePhase == GamePhase.RoundInProgress)
        {
             AdvanceTurn();
        }
    }

    // --- Private Helper Methods ---

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

    private void ExecuteGuardEffect(Player actingPlayer, Player? targetPlayer, CardType? guessedCardType)
    {
        if (targetPlayer == null || guessedCardType == null) return;
        // Check protection BEFORE applying effect
        if (targetPlayer.IsProtected)
        {
            AddDomainEvent(new CardEffectFizzled(Id, actingPlayer.Id, CardType.Guard, targetPlayer.Id, "Target was protected"));
            return; // Effect fizzles
        }

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
        // Check protection BEFORE applying effect
        if (targetPlayer.IsProtected)
        {
             AddDomainEvent(new CardEffectFizzled(Id, actingPlayer.Id, CardType.Priest, targetPlayer.Id, "Target was protected"));
             return; // Effect fizzles
        }

        var revealedCard = targetPlayer.Hand.GetHeldCard();
        if (revealedCard != null)
        {
            AddDomainEvent(new PriestEffectUsed(Id, actingPlayer.Id, targetPlayer.Id, revealedCard.Id, revealedCard.Type));
        }
    }

    private void ExecuteBaronEffect(Player actingPlayer, Player? targetPlayer)
    {
        if (targetPlayer == null) return;
        // Check protection BEFORE applying effect
        if (targetPlayer.IsProtected)
        {
             AddDomainEvent(new CardEffectFizzled(Id, actingPlayer.Id, CardType.Baron, targetPlayer.Id, "Target was protected"));
             return; // Effect fizzles
        }

        Card? actingPlayerCard = actingPlayer.Hand.GetHeldCard();
        Card? targetPlayerCard = targetPlayer.Hand.GetHeldCard();
        if (actingPlayerCard == null || targetPlayerCard == null) return;

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
        if (targetPlayer.IsProtected)
        {
             AddDomainEvent(new CardEffectFizzled(Id, CurrentTurnPlayerId, CardType.Prince, targetPlayer.Id, "Target was protected"));
             return; // Effect fizzles
        }

        if (targetPlayer.Hand.IsEmpty)
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
                (Card c, Deck d) = Deck.Draw(); 
                Deck = d; 
                targetPlayer.GiveCard(c); 
                AddDomainEvent(new PlayerDrewCard(Id, targetPlayer.Id)); 
                AddDomainEvent(new DeckChanged(Id, Deck.CardsRemaining));
            }
            else if (SetAsideCard != null)
            {
                targetPlayer.GiveCard(SetAsideCard); 
                AddDomainEvent(new PlayerDrewCard(Id, targetPlayer.Id)); 
                var usedType = SetAsideCard.Type; SetAsideCard = null; 
                AddDomainEvent(new SetAsideCardUsed(Id, usedType));
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
        if (targetPlayer.IsProtected)
        {
             AddDomainEvent(new CardEffectFizzled(Id, actingPlayer.Id, CardType.King, targetPlayer.Id, "Target was protected"));
             return; // Effect fizzles
        }
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

        if (activePlayers.Count == 1)
        {
            winningPlayer = activePlayers.Single();
            winnerId = winningPlayer.Id;
            reason = "Last player standing";
        }
        else // Deck is empty
        {
            reason = "Deck empty, highest card wins";
            if (!activePlayers.Any()) { winnerId = null; }
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
                        int currentSum = potentialWinner.PlayedCards.Select(c=> c.Value).Sum();
                        if (currentSum > highestDiscardSum) { highestDiscardSum = currentSum; finalWinners.Clear(); finalWinners.Add(potentialWinner); }
                        else if (currentSum == highestDiscardSum) { finalWinners.Add(potentialWinner); }
                    }
                    if (finalWinners.Count == 1) { winningPlayer = finalWinners.Single(); reason += " (Tie broken by discard sum)"; }
                    else { reason += " (Tie in rank and discard sum)"; } // No single winner
                }
                winnerId = winningPlayer?.Id;
            }
        }

        LastRoundWinnerId = winnerId;

        // If there's a winner, award token *before* creating summaries for RoundEnded event
        if (winningPlayer != null)
        {
            AwardTokenToPlayer(winningPlayer); // This increments TokensWon and raises TokenAwarded
        }

        // Create detailed player summaries *after* token has been awarded
        var playerSummaries = Players.Select(p => new PlayerRoundEndSummary(
            PlayerId: p.Id,
            PlayerName: p.Name,
            FinalHeldCard: p.Status == PlayerStatus.Active ? p.Hand.GetHeldCard() : null,
            DiscardPileValues: p.PlayedCards.Select(c=> c.Value).ToList(),
            TokensWon: p.TokensWon // This now includes the token for the just-ended round
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
        AddDomainEvent(new TokenAwarded(Id, player.Id, player.TokensWon));
    }

    private void EndGame(Guid winnerId)
    {
        if (GamePhase == GamePhase.GameOver) return;
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

        if (!Deck.IsEmpty)
        {
            (Card card, Deck remainingDeck) = Deck.Draw();
            Deck = remainingDeck;
            player.GiveCard(card);
            AddDomainEvent(new PlayerDrewCard(Id, player.Id));
            AddDomainEvent(new DeckChanged(Id, Deck.CardsRemaining));
        }

        // Check round end condition AFTER the draw attempt.
        // This handles the case where the draw emptied the deck.
        // CheckRoundEndCondition() calls EndRound() if true.
        bool roundEnded = CheckRoundEndCondition();

        // Only raise TurnStarted if the round didn't just end
        if(!roundEnded && GamePhase == GamePhase.RoundInProgress)
        {
            AddDomainEvent(new TurnStarted(Id, CurrentTurnPlayerId, RoundNumber));
        }
    }

    private Player GetPlayerById(Guid playerId) { var player = Players.FirstOrDefault(p => p.Id == playerId); if (player == null) throw new ArgumentException($"Player with ID {playerId} not found in this game."); return player; }

    // New method to add log entries
    public void AddLogEntry(GameLogEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));
        _logEntries.Insert(0, entry); // Insert at the beginning to keep newest first
    }
}