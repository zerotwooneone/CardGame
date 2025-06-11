using System.Collections.ObjectModel;
using CardGame.Domain.Common;
using CardGame.Domain.Exceptions;
using CardGame.Domain.Game.Event;
using CardGame.Domain.Game.GameException;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using Microsoft.Extensions.Logging;

namespace CardGame.Domain.Game;

public class Game : IGameOperations // Aggregate Root
{
    public Guid Id { get; private set; }
    public List<Player> Players { get; private set; } = new List<Player>();
    public Deck Deck { get; private set; } // Initialized in StartNewRound or Load
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
    private readonly IRandomizer _gameRandomizer;
    private readonly ILogger<Game> _logger; // ADDED
    private readonly ILoggerFactory _loggerFactory; // ADDED
    private readonly IDeckProvider _deckProvider; // ADDED
    private bool _isSettingUpRound = false;

    // --- Domain Event Handling ---
    private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() { _domainEvents.Clear(); }
    private void AddDomainEvent(IDomainEvent domainEvent) { _domainEvents.Add(domainEvent); }
    // --- End Domain Event Handling ---

    public IReadOnlyList<GameLogEntry> LogEntries => _logEntries.AsReadOnly(); // Public accessor for log entries

    #region IGameOperations Implementation

    // Explicit interface implementation to avoid cluttering the public API
    IGameStateInfo IGameOperations.GetGameState() => new GameStateInfo(this);

    void IGameOperations.AddLogEntry(GameLogEntry entry) => AddLogEntry(entry);

    private class GameStateInfo : IGameStateInfo
    {
        private readonly Game _game;

        public GameStateInfo(Game game)
        {
            _game = game;
        }

        public int CardsRemaining => _game.Deck.CardsRemaining;
        public int RoundNumber => _game.RoundNumber;
        public Guid CurrentTurnPlayerId => _game.CurrentTurnPlayerId;
        public IReadOnlyList<Player> Players => _game.Players.AsReadOnly();
    }

    Player? IGameOperations.GetPlayer(Guid playerId)
    {
        return GetPlayerById(playerId); // GetPlayerById will throw if not found
    }
    

    void IGameOperations.GiveCardToPlayer(Guid playerId, Card card)
    {
        var player = GetPlayerById(playerId); // GetPlayerById will throw if not found
        if (player.Status == PlayerStatus.Eliminated)
            throw new InvalidOperationException("Cannot give cards to an eliminated player");

        player.GiveCard(card);
    }

    Card? IGameOperations.DrawCardForPlayer(Guid playerId)
    {
        if (Deck.CardsRemaining == 0)
            return null;

        var (card, remainingDeck) = Deck.Draw();
        Deck = remainingDeck;

        var player = GetPlayerById(playerId); // GetPlayerById will throw if not found
        // if (player != null && player.Status != PlayerStatus.Eliminated) // No longer needed here
        // {
        player.GiveCard(card);
        AddDomainEvent(new PlayerDrewCard(Id, playerId));
        // }
        return card;
    }

    void IGameOperations.SwapPlayerHands(Guid player1Id, Guid player2Id)
    {
        var player1 = GetPlayerById(player1Id); // GetPlayerById will throw if not found
        var player2 = GetPlayerById(player2Id); // GetPlayerById will throw if not found

        // if (player1 == null || player2 == null) // No longer needed here
        //     throw new DomainException("One or both players not found for hand swap.", 1002);

        if (player1.Status == PlayerStatus.Eliminated || player2.Status == PlayerStatus.Eliminated)
            throw new InvalidOperationException("Cannot swap hands with an eliminated player");

        // Use the Player.SwapHandWith method to handle the swap
        player1.SwapHandWith(player2);

        // Log the hand swap
        AddLogEntry(new GameLogEntry(
            GameLogEventType.KingTrade,
            player1.Id,
            player1.Name,
            $"{player1.Name} swapped hands with {player2.Name} using the King")
        );
    }

    #endregion

    private Game(Guid id, Guid deckDefinitionId, IReadOnlyList<Card> initialDeckCardSet, IRandomizer? randomizer, ILogger<Game> logger, ILoggerFactory loggerFactory, IDeckProvider deckProvider)
    {
        Id = id;
        _deckDefinitionId = deckDefinitionId; // Assign deckDefinitionId
        _initialDeckCardSet = initialDeckCardSet ?? throw new ArgumentNullException(nameof(initialDeckCardSet));
        if (!_initialDeckCardSet.Any()) throw new ArgumentException("Initial deck card set cannot be empty.", nameof(initialDeckCardSet));
        Deck = Deck.Load(Enumerable.Empty<Card>());
        _gameRandomizer = randomizer ?? new DefaultRandomizer();
        _logger = logger; // ADDED
        _loggerFactory = loggerFactory; // ADDED
        _deckProvider = deckProvider ?? throw new ArgumentNullException(nameof(deckProvider)); // ADDED
    }

    // --- Factory Methods ---
    public static Game CreateNewGame(
        Guid deckDefinitionId,
        IEnumerable<PlayerInfo> playerInfos,
        Guid creatorPlayerId,
        IEnumerable<Card> initialDeckCards,
        ILoggerFactory loggerFactory,
        IDeckProvider deckProvider,
        int tokensToWin = 4,
        IRandomizer? randomizer = null)
    {
        var gameId = Guid.NewGuid();
        var cardSetToUse = initialDeckCards?.ToList() ?? throw new ArgumentNullException(nameof(initialDeckCards));
        if (!cardSetToUse.Any()) throw new ArgumentException("Initial deck cards cannot be empty.", nameof(initialDeckCards));

        var gameLogger = loggerFactory.CreateLogger<Game>();
        var game = new Game(
                gameId,
                deckDefinitionId,
                new ReadOnlyCollection<Card>(cardSetToUse),
                randomizer,
                gameLogger,
                loggerFactory,
                deckProvider)
        {
            TokensNeededToWin = tokensToWin,
            GamePhase = GamePhase.NotStarted,
            LastRoundWinnerId = null
        };

        var playerInfoList = playerInfos?.ToList() ?? new List<PlayerInfo>();
        if (!playerInfoList.Any()) throw new ArgumentException("Player information cannot be empty.", nameof(playerInfos));

        // Check for duplicate player IDs
        var duplicatePlayerIds = playerInfoList
            .GroupBy(p => p.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicatePlayerIds.Any())
        {
            throw new DomainException("Duplicate player IDs are not allowed.", 1001);
        }

        bool creatorFound = false;
        foreach (var pInfo in playerInfoList)
        {
            // Assuming Player.Load now takes List<int> for known cards (already refactored as per memory)
            var player = Player.Load(pInfo.Id, pInfo.Name, PlayerStatus.Active, Hand.Empty, new List<Card>(), 0, false, loggerFactory.CreateLogger<Player>());
            game.Players.Add(player);
            if (pInfo.Id == creatorPlayerId) creatorFound = true;
        }

        if (!creatorFound) throw new DomainException("Creator ID must be included in the player list.", 1000);
        if (game.Players.Count < 2 || game.Players.Count > 4) throw new DomainException($"Game must have between 2 and 4 players (found {game.Players.Count}).", 1000);

        game.AddDomainEvent(new GameCreated(gameId, playerInfoList, tokensToWin, creatorPlayerId));
        game.StartNewRound();
        return game;
    }

    public static Game Load(Guid id, Guid deckDefinitionId, int roundNumber, GamePhase gamePhase, Guid currentTurnPlayerId, List<Player> players, Deck deck, Card? setAsideCard, List<Card> publiclySetAsideCards, int tokensToWin, Guid? lastRoundWinnerId, List<Card> initialDeckCardSet, ILoggerFactory loggerFactory, IDeckProvider deckProvider)
    {
        if (initialDeckCardSet == null) throw new ArgumentNullException(nameof(initialDeckCardSet));
        if (!initialDeckCardSet.Any()) throw new ArgumentException("Initial deck card set cannot be empty for loading.", nameof(initialDeckCardSet));

        var gameLogger = loggerFactory.CreateLogger<Game>();
        var game = new Game(
            id,
            deckDefinitionId,
            new ReadOnlyCollection<Card>(initialDeckCardSet),
            null,
            gameLogger,
            loggerFactory,
            deckProvider)
        {
            RoundNumber = roundNumber,
            GamePhase = gamePhase,
            CurrentTurnPlayerId = currentTurnPlayerId,
            Players = players ?? new List<Player>(),
            Deck = deck,
            SetAsideCard = setAsideCard,
            PubliclySetAsideCards = publiclySetAsideCards ?? new List<Card>(),
            TokensNeededToWin = tokensToWin,
            LastRoundWinnerId = lastRoundWinnerId
        };
        return game;
    }

    private void StartNewRound()
    {
        _logger.LogDebug("[Game {GameId}] StartNewRound: Entry. Current _isSettingUpRound: {IsSettingUpValue}, GamePhase: {CurrentPhase}", Id, _isSettingUpRound, GamePhase);
        _isSettingUpRound = true; // Set flag at the beginning
        try
        {
            if (GamePhase == GamePhase.GameOver)
                throw new InvalidOperationException("Cannot start a new round, game is over.");
            if (GamePhase == GamePhase.RoundInProgress)
                throw new InvalidOperationException("Cannot start a new round while one is in progress.");

            RoundNumber++;
            _logger.LogInformation("Starting Round {RoundNumber} for Game {GameId}", RoundNumber, Id);
            SetAsideCard = null;
            PubliclySetAsideCards.Clear();

            // Use the _initialDeckCardSet provided when the game was created/loaded.
            var deckCards = new List<Card>(_initialDeckCardSet);
            Deck = Deck.CreateShuffled(deckCards, _gameRandomizer);

            // Reset players for the new round
            foreach (var player in Players)
            {
                player.StartNewRound();
            }

            // Set aside one card if 2 players, three if more (Love Letter rules for some editions/variants)
            // Standard rules: 2 players: 3 cards face up. >2 players: 1 card face down.
            if (Players.Count == 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (Deck.CardsRemaining > 0)
                    {
                        var (publiclySetAside, newDeck) = Deck.Draw();
                        Deck = newDeck;
                        PubliclySetAsideCards.Add(publiclySetAside);
                        AddLogEntry(new GameLogEntry(
                            GameLogEventType.CardSetAsidePublicly,
                            Id,
                            "System",
                            $"Rank {publiclySetAside.Rank.Value} Rank Id:{publiclySetAside.Rank.Id} ({publiclySetAside.AppearanceId}) set aside publicly.")
                        { PlayedCard = publiclySetAside });
                    }
                }
            }
            else if (Players.Count > 2)
            {
                if (Deck.CardsRemaining > 0)
                {
                    var (setAside, newDeck) = Deck.Draw();
                    Deck = newDeck;
                    SetAsideCard = setAside;
                }
            }

            // Deal one card to each player
            foreach (var player in Players)
            {
                if (Deck.CardsRemaining > 0)
                {
                    var (card, newDeck) = Deck.Draw();
                    Deck = newDeck;
                    player.GiveCard(card); // Assuming Player.GiveCard adds to hand
                    AddLogEntry(new GameLogEntry(GameLogEventType.PlayerDrewCard, player.Id, player.Name, $"{player.Name} drew a card.", true) { DrawnCard = card });
                    AddDomainEvent(new PlayerDrewCard(Id, player.Id));
                    AddDomainEvent(new DeckChanged(Id, Deck.CardsRemaining)); // Inform about deck change
                }
                else
                {
                    throw new GameRuleException($"Not enough cards to deal initial hand to player {player.Name} in Game {Id}, Round {RoundNumber}.");
                }
            }

            CurrentTurnPlayerId = LastRoundWinnerId ?? Players.First().Id;
            GetPlayerById(CurrentTurnPlayerId).IsPlayersTurn = true;

            GamePhase = GamePhase.RoundInProgress;
            _logger.LogDebug("[Game {GameId}] StartNewRound: Set GamePhase to RoundInProgress. Current _isSettingUpRound: {IsSettingUpValue}", Id, _isSettingUpRound);

            AddDomainEvent(new RoundStarted(
                Id,
                RoundNumber,
                Players.Select(p => p.Id).ToList(),
                Deck.CardsRemaining,
                SetAsideCard,
                PubliclySetAsideCards.Select(c => new PublicCardInfo(c.AppearanceId, c)).ToList(),
                _deckDefinitionId
            ));
            AddLogEntry(new GameLogEntry(GameLogEventType.RoundStart, Id, "System", $"Round {RoundNumber} started. {GetPlayerById(CurrentTurnPlayerId).Name} begins."));

            _logger.LogDebug("[Game {GameId}] StartNewRound: Calling DrawInitialTurnCard. _isSettingUpRound: {IsSettingUpValue}", Id, _isSettingUpRound);
            DrawInitialTurnCard(CurrentTurnPlayerId);
            _logger.LogDebug("[Game {GameId}] StartNewRound: Returned from DrawInitialTurnCard. GamePhase: {CurrentPhase}, _isSettingUpRound: {IsSettingUpValue}", Id, GamePhase, _isSettingUpRound);
        }
        finally
        {
            _logger.LogDebug("[Game {GameId}] StartNewRound: Finally block. Current _isSettingUpRound: {IsSettingUpValue}. Setting to false", Id, _isSettingUpRound);
            _isSettingUpRound = false;
            _logger.LogDebug("[Game {GameId}] StartNewRound: Finally block. _isSettingUpRound after setting to false: {IsSettingUpValue}", Id, _isSettingUpRound);
        }
        _logger.LogDebug("[Game {GameId}] StartNewRound: Exit. GamePhase: {CurrentPhase}, _isSettingUpRound: {IsSettingUpValue}", Id, GamePhase, _isSettingUpRound);
    }

    // MODIFIED: guessedCardType to guessedRankValue, removed IDeckProvider parameter (use field _deckProvider)
    public void PlayCard(Guid playerId, Card cardToPlay, Guid? targetPlayerId, int? guessedRankValue)
    {
        // 1. SETUP & INITIAL VALIDATION
        var player = GetPlayerById(playerId); // GetPlayerById will throw if not found
        Player? targetPlayer = targetPlayerId.HasValue ? GetPlayerById(targetPlayerId.Value) : null;

        ValidatePlayCard(player, cardToPlay, targetPlayerId, guessedRankValue); // MODIFIED: Pass guessedRankValue

        // Pre-effect validation: Ensure target player (if any) is in a valid state to be targeted.
        // This check could also be part of ValidatePlayCard or handled by the deckProvider if card-specific.
        if (targetPlayer != null && targetPlayer.Status == PlayerStatus.Active && !targetPlayer.Hand.Cards.Any())
        {
            throw new GameRuleException($"[Game {Id}] Target player {targetPlayer.Name} (ID: {targetPlayer.Id}) is active but has no cards in hand. This is an invalid state to be targeted by most card effects.");
        }

        var cardToPlayInstance = player.Hand.Cards.First(c => c== cardToPlay); 

        // 2. PLAYER ACTION - PLAY CARD & BASIC STATE UPDATE
        player.PlayCard(cardToPlayInstance); 

        // 3. LOG THE ACTION
        var logEntry = new GameLogEntry(
            GameLogEventType.CardPlayed,
            playerId,
            player.Name,
            // MODIFIED: Use RankValue and _deckProvider for name, and guessedRankValue
            $"{player.Name} played {cardToPlayInstance} " +
            (targetPlayer != null ? $"targeting {targetPlayer.Name}" : "") +
            (guessedRankValue))
        {
            PlayedCard = cardToPlayInstance,
            TargetPlayerId = targetPlayerId,
            TargetPlayerName = targetPlayer?.Name,
            GuessedRank = guessedRankValue,
        };
        AddLogEntry(logEntry);
        // MODIFIED: PlayerPlayedCard event to use RankValue and AppearanceId from cardToPlayInstance, and guessedRankValue
        AddDomainEvent(new PlayerPlayedCard(Id, playerId, cardToPlayInstance,  targetPlayerId, guessedRankValue));

        // 4. EXECUTE CARD EFFECT
        // This method is responsible for the card's primary logic, including any direct eliminations 
        // or state changes it causes (e.g., updating player status, forcing draws/discards).
        // It should also handle its own specific logging for the effect's outcome.
        _deckProvider.ExecuteCardEffect(this, player, cardToPlayInstance, targetPlayer, guessedRankValue); 

        // 5. POST-EFFECT STATE CHECKS & GENERAL ELIMINATIONS
        // Check if the acting player was eliminated or ran out of cards when the deck is empty.
        CheckAndHandleHandlessPlayerWithEmptyDeck(player, cardToPlayInstance);

        // Check if the target player (if any and still active) was eliminated or ran out of cards when the deck is empty.
        if (targetPlayer != null && targetPlayer.Status == PlayerStatus.Active) // Only check if still active after card effect
        {
            CheckAndHandleHandlessPlayerWithEmptyDeck(targetPlayer, cardToPlayInstance);
        }

        // 6. ROUND & GAME PROGRESSION
        // CheckRoundEndCondition will internally call EndRound if conditions are met.
        // EndRound will then call StartNewRound if the game is not over.
        bool roundEnded = CheckRoundEndCondition(isCheckingAfterPlayerAction: true);

        if (!roundEnded && GamePhase == GamePhase.RoundInProgress)
        {
            // AdvanceTurn should correctly skip any players eliminated during this turn.
            AdvanceTurn();
        }
    }

    // MODIFIED: guessedCardType to guessedRankValue, removed IDeckProvider parameter
    private void ValidatePlayCard(Player player, Card cardToPlayInstance, Guid? targetPlayerId, int? guessedRankValue)
    {
        if (GamePhase != GamePhase.RoundInProgress)
            throw new InvalidMoveException("Game is not in progress.");
        if (player.Id != CurrentTurnPlayerId)
            throw new InvalidMoveException($"It is not player {player.Name}'s turn.");
        if (player.Status == PlayerStatus.Eliminated)
            throw new InvalidMoveException($"Player {player.Name} is eliminated and cannot play cards.");
        // MODIFIED: c.Rank to c.RankValue
        if (player.Hand.Cards.All(c => c != cardToPlayInstance))
            throw new InvalidMoveException($"Player {player.Name} does not hold the specified card instance (ID: {cardToPlayInstance.AppearanceId}).");

        // Card-specific validation is now handled by ExecuteCardEffect in the IDeckProvider implementation.
        // Player? targetPlayerObject = targetPlayerId.HasValue ? GetPlayerById(targetPlayerId.Value) : null;
        // if (!deckProvider.CanPlayCard(player, cardToPlayInstance, targetPlayerObject, guessedRankValue))
        // {
        //     throw new InvalidMoveException("Invalid card play according to deck rules.");
        // }
    }

    // --- Private Helper Methods ---
    private Player GetPlayerById(Guid playerId)
    {
        var player = Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) throw new DomainException($"Player with ID '{playerId}' not found in game {Id}.", 1002); // Corrected to DomainException with error code
        return player;
    }

    private void CheckAndHandleHandlessPlayerWithEmptyDeck(Player playerToCheck, Card? cardPlayedOrResponsible)
    {
        if (playerToCheck.Status == PlayerStatus.Active && !playerToCheck.Hand.Cards.Any() && Deck.CardsRemaining == 0)
        {
            _logger.LogInformation($"[Game {Id}] Player {playerToCheck.Name} (ID: {playerToCheck.Id}) is active, handless, and deck empty. Attempting elimination.");
            // MODIFIED: Call EliminatePlayer with RankValue and AppearanceId
            EliminatePlayer(playerToCheck.Id, "became handless with an empty deck", cardPlayedOrResponsible);
            _logger.LogWarning($"[Game {Id}] DIAGNOSTIC: After EliminatePlayer call in C&HHPlayerWED for Player {playerToCheck.Id}, Status is: {playerToCheck.Status}");
        }
    }

    private void AddLogEntry(GameLogEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));
        _logEntries.Insert(0, entry); // Insert at the beginning to keep newest first
    }

    // MODIFIED: Signature changed to use rank value and appearance ID
    public void EliminatePlayer(Guid playerId, string reason, Card? cardResponsible = null)
    {
        var player = GetPlayerById(playerId);
        if (player.Status == PlayerStatus.Active)
        {
            player.Eliminate();
            // MODIFIED: GameLogEntry to use RevealedRankValueOnElimination and RevealedAppearanceIdOnElimination
            AddLogEntry(new GameLogEntry(GameLogEventType.PlayerEliminated, playerId, player.Name, $"{player.Name} was eliminated: {reason}") );
            // MODIFIED: PlayerEliminated event to use cardResponsibleRankValue and cardResponsibleAppearanceId
            AddDomainEvent(new PlayerEliminated(Id, playerId, reason, cardResponsible));
        }
    }

    private void DrawInitialTurnCard(Guid playerId)
    {
        var player = GetPlayerById(playerId);
        _logger.LogDebug("[Game {GameId}] DrawInitialTurnCard for Player {PlayerId}. Entry. Current _isSettingUpRound: {IsSettingUpValue}, GamePhase: {CurrentPhase}", Id, playerId, _isSettingUpRound, GamePhase);

        if (!player.CanDrawCard())
        {
            throw new GameRuleException($"Player {player.Name} (ID: {playerId}) in game {Id} cannot draw initial turn card. Hand state may be invalid or player status prevents drawing.");
        }

        if (Deck.CardsRemaining == 0)
        {
            _logger.LogWarning("[Game {GameId}] Deck is empty, cannot draw initial turn card for Player {PlayerId}", Id, playerId);
            // ADDED: Check if player is now handless with an empty deck
            CheckAndHandleHandlessPlayerWithEmptyDeck(player, null);
        }
        else
        {
            var (drawnCard, newDeck) = Deck.Draw();
            Deck = newDeck;
            player.GiveCard(drawnCard);
            AddLogEntry(new GameLogEntry(GameLogEventType.PlayerDrewCard, player.Id, player.Name, $"{player.Name} drew their starting second card for the turn.", true) { DrawnCard = drawnCard });
            AddDomainEvent(new PlayerDrewCard(Id, player.Id));
            AddDomainEvent(new DeckChanged(Id, Deck.CardsRemaining));
            _logger.LogDebug("[Game {GameId}] Player {PlayerId} drew initial card. Deck remaining: {DeckCount}", Id, playerId, Deck.CardsRemaining);
        }

        _logger.LogDebug("[Game {GameId}] DrawInitialTurnCard: Calling CheckRoundEndCondition. _isSettingUpRound: {IsSettingUpValue}", Id, _isSettingUpRound);
        bool roundEnded = CheckRoundEndCondition(false);
        _logger.LogDebug("[Game {GameId}] DrawInitialTurnCard: Returned from CheckRoundEndCondition. roundEnded: {EndedValue}, GamePhase: {CurrentPhase}, _isSettingUpRound: {IsSettingUpValue}", Id, roundEnded, GamePhase, _isSettingUpRound);
    }

    private bool CheckRoundEndCondition(bool isCheckingAfterPlayerAction)
    {
        _logger.LogDebug("[Game {GameId}] CheckRoundEndCondition: Entry. isCheckingAfterPlayerAction: {IsAfterAction}, _isSettingUpRound: {IsSettingUpValue}, GamePhase: {CurrentPhase}, DeckRemaining: {DeckCount}",
            Id, isCheckingAfterPlayerAction, _isSettingUpRound, GamePhase, Deck.CardsRemaining);

        // DIAGNOSTIC LOGGING
        _logger.LogWarning("[Game {Id}] DIAGNOSTIC: In CheckRoundEndCondition, iterating player statuses BEFORE Where clause:", Id);
        foreach (var p_diag in Players)
        {
            _logger.LogWarning($"[Game {Id}] DIAGNOSTIC: Player {p_diag.Id} - Status: {p_diag.Status}");
        }

        if (_isSettingUpRound)
        {
            _logger.LogDebug("[Game {GameId}] CheckRoundEndCondition: _isSettingUpRound is TRUE. Returning false", Id);
            return false; // Do not end round during setup phase
        }

        if (GamePhase != GamePhase.RoundInProgress)
        {
            _logger.LogDebug("[Game {GameId}] CheckRoundEndCondition: GamePhase is not RoundInProgress ({CurrentPhase}). Returning false", Id, GamePhase);
            return false;
        }

        var activePlayers = new List<Player>();
        foreach (var p_filter in Players)
        {
            if (p_filter.Status == PlayerStatus.Active)
            {
                activePlayers.Add(p_filter);
            }
        }
        _logger.LogInformation($"[Game {Id}] CheckRoundEndCondition: Manual filter found {activePlayers.Count} active players (Deck: {Deck.CardsRemaining}).");

        bool roundEndConditionMet = activePlayers.Count <= 1;
        if (isCheckingAfterPlayerAction && Deck.CardsRemaining == 0)
        {
            roundEndConditionMet = true;
        }

        if (roundEndConditionMet)
        {
            _logger.LogInformation("[Game {GameId}] CheckRoundEndCondition: Condition met (ActivePlayers: {ACount}, Deck: {DCount}, IsAfterAction: {IsAfterAction}). Calling EndRound",
                Id, activePlayers.Count, Deck.CardsRemaining, isCheckingAfterPlayerAction);
            EndRound(activePlayers);
            _logger.LogDebug("[Game {GameId}] CheckRoundEndCondition: Returned from EndRound. GamePhase: {CurrentPhase}. Returning true", Id, GamePhase);
            return true;
        }
        _logger.LogDebug("[Game {GameId}] CheckRoundEndCondition: Condition NOT met. Returning false", Id);
        return false;
    }

    private void EndRound(List<Player> activePlayers)
    {
        _logger.LogDebug("[Game {GameId}] EndRound: Entry. Current GamePhase: {CurrentPhase}, _isSettingUpRound: {IsSettingUpValue}", Id, GamePhase, _isSettingUpRound);
        if (GamePhase == GamePhase.RoundOver || GamePhase == GamePhase.GameOver)
        {
            throw new InvalidMoveException($"[Game {Id}] EndRound cannot be called when the game phase is already {GamePhase}.");
        }

        Player? roundWinner = null;
        Card? winningCard = null;
        string roundEndReason;

        if (activePlayers.Count == 1)
        {
            roundWinner = activePlayers.Single();
            winningCard = roundWinner.Hand.GetHeldCard();
            roundEndReason = $"Player {roundWinner.Name} is the last one active.";
            _logger.LogDebug("[Game {GameId}] Round {RoundNumber} ended. Winner: {PlayerName} (last active)", Id, RoundNumber, roundWinner.Name);
        }
        else if (activePlayers.Count > 1 && Deck.CardsRemaining == 0)
        {
            roundEndReason = "Deck is empty. Comparing hands.";
            _logger.LogDebug("[Game {GameId}] Round {RoundNumber} ended. Deck empty, comparing hands of {ActivePlayerCount} players", Id, RoundNumber, activePlayers.Count);

            int highestRank = -1;
            List<Player> potentialWinners = new List<Player>();

            foreach (var player in activePlayers)
            {
                if (player.Hand.Count == 0) // Player is active, deck is empty, but they have no card.
                {
                    throw new GameRuleException($"[Game {Id}] Player {player.Name} (ID: {player.Id}) is active and deck is empty, but has no card in hand to compare. This indicates an inconsistent state or rule violation.");
                }
                else if (player.Hand.Count > 1) // Player has too many cards for round-end comparison
                {
                    throw new GameRuleException($"[Game {Id}] Player {player.Name} (ID: {player.Id}) is active and deck is empty, but has {player.Hand.Count} cards in hand. Should have exactly one. This indicates an inconsistent state or rule violation.");
                }
                else // Player has exactly one card
                {
                    var playerHeldCard = player.Hand.Cards.Single(); // Safe to get the single card
                    // MODIFIED: Use RankValue and _deckProvider for name
                    _logger.LogTrace("[Game {GameId}] Player {PlayerName} holds {PlayerHeldValue})", Id, player.Name, playerHeldCard.Rank.Value);
                    if (playerHeldCard.Rank.Value > highestRank)
                    {
                        highestRank = playerHeldCard.Rank.Value;
                        potentialWinners.Clear();
                        potentialWinners.Add(player);
                    }
                    else if (playerHeldCard.Rank.Value == highestRank)
                    {
                        potentialWinners.Add(player);
                    }
                }
            }

            if (potentialWinners.Count == 1)
            {
                roundWinner = potentialWinners.Single();
                winningCard = roundWinner.Hand.GetHeldCard();
                // MODIFIED: Use RankValue and _deckProvider for name
                _logger.LogDebug("[Game {GameId}] Round {RoundNumber} winner by highest card: {PlayerName} with {CardValue})", Id, RoundNumber, roundWinner.Name, winningCard?.Rank.Value);
            }
            else if (potentialWinners.Count > 1)
            {
                _logger.LogDebug("[Game {GameId}] Round {RoundNumber} ended in a tie for highest card. No token awarded this round", Id, RoundNumber);
            }
            else
            {
                _logger.LogDebug("[Game {GameId}] Round {RoundNumber} ended. No players had cards to compare or some other edge case. No token awarded", Id, RoundNumber);
            }
        }
        else
        {
            throw new InvalidMoveException($"[Game {Id}] EndRound was called under unexpected conditions. ActivePlayers: {activePlayers.Count}, DeckRemaining: {Deck.CardsRemaining}. The game state does not permit ending the round.");
        }

        LastRoundWinnerId = roundWinner?.Id;

        // Create GameLogEntry
        var logEntry = new GameLogEntry(GameLogEventType.RoundEnd, roundEndReason)
        {
            WinnerPlayerId = roundWinner?.Id,
            WinnerPlayerName = roundWinner?.Name,
            RoundPlayerSummaries = Players.Select(p =>
                new GameLogEntry.GameLogPlayerRoundSummary(p.Id, p.Name)
                {
                    CardsHeld = p.Hand.GetCards().ToList(),
                    Score = p.TokensWon,
                    WasActive = p.Status == PlayerStatus.Active
                }
            ).ToList()
        };
        AddLogEntry(logEntry);

        // Determine tokens awarded this round for the summary
        int tokensAwardedToWinnerThisRound = 0;
        if (roundWinner != null)
        {
            // Assuming 1 token is standard for a round win. This could be a constant.
            tokensAwardedToWinnerThisRound = 1;
        }

        // MODIFIED: PlayerRoundEndSummary instantiation to include IsWinner and TokensAwarded for the current round
        var playerSummariesForEvent = Players.Select(p =>
            new PlayerRoundEndSummary(
                p.Id,
                p.Name,
                p.Hand.GetCards().ToList(), 
                p.Id == roundWinner?.Id, // IsWinner
                (p.Id == roundWinner?.Id) ? tokensAwardedToWinnerThisRound : 0 // TokensAwarded this round
            )
        ).ToList();

        if (roundWinner != null)
        {
            roundWinner.AddToken(); // Player's total tokens are updated here
            _logger.LogDebug("[Game {GameId}] Player {PlayerName} awarded a token. Total tokens: {Tokens}", Id, roundWinner.Name, roundWinner.TokensWon);

            // Safely get winning card name for the event
            string winningCardNameForEvent = winningCard?.Rank.Value.ToString() ?? "";
            
            // MODIFIED: Use safely determined winningCardNameForEvent
            AddDomainEvent(new RoundEnded(Id, roundWinner.Id, winningCardNameForEvent, playerSummariesForEvent));

            if (roundWinner.TokensWon >= TokensNeededToWin)
            {
                GamePhase = GamePhase.GameOver;
                _logger.LogInformation("[Game {GameId}] Game Over! Winner: {PlayerName} reached {TokensWon} tokens (needed {TokensNeeded})", Id, roundWinner.Name, roundWinner.TokensWon, TokensNeededToWin);
                AddDomainEvent(new GameEnded(Id, roundWinner.Id));
            }
            else
            {
                GamePhase = GamePhase.RoundOver;
            }
        }
        else
        {
            GamePhase = GamePhase.RoundOver;
            // playerSummariesForEvent is already correctly populated for the tie/no-winner case
            AddDomainEvent(new RoundEnded(Id, null, "Tie or no winner determined this round.", playerSummariesForEvent));
        }

        if (GamePhase == GamePhase.RoundOver)
        {
            StartNewRound();
        }
    }

    private void AdvanceTurn()
    {
        _logger.LogDebug("[Game {GameId}] AdvanceTurn: Entry. CurrentTurnPlayerId: {PlayerId}, GamePhase: {CurrentPhase}", Id, CurrentTurnPlayerId, GamePhase);
        if (GamePhase != GamePhase.RoundInProgress)
        {
            throw new InvalidMoveException($"Cannot advance turn in game {Id}: GamePhase is {GamePhase}, but must be RoundInProgress.");
        }

        var currentPlayer = GetPlayerById(CurrentTurnPlayerId);
        currentPlayer.SetProtection(false); // Current player loses protection at end of their turn
        currentPlayer.IsPlayersTurn = false;
        _logger.LogDebug("[Game {GameId}] AdvanceTurn: Player {PlayerId} turn ended. Protection removed", Id, currentPlayer.Id);

        int currentPlayerIndex = Players.FindIndex(p => p.Id == CurrentTurnPlayerId);
        Player nextPlayer;
        int attempts = 0;
        do
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % Players.Count;
            nextPlayer = Players[currentPlayerIndex];
            attempts++;
            if (attempts > Players.Count * 2) // Safety break for infinite loops
            {
                throw new GameRuleException($"Failed to determine next active player in game {Id} after {attempts} attempts. Game state is inconsistent. CurrentTurnPlayerId: {CurrentTurnPlayerId}");
            }
        } while (nextPlayer.Status == PlayerStatus.Eliminated);

        CurrentTurnPlayerId = nextPlayer.Id;
        nextPlayer.IsPlayersTurn = true;
        _logger.LogInformation("[Game {GameId}] AdvanceTurn: Next player is {PlayerName} ({PlayerId})", Id, nextPlayer.Name, nextPlayer.Id);

        // New player draws their turn card
        _logger.LogDebug("[Game {GameId}] AdvanceTurn: Calling DrawInitialTurnCard for new player {PlayerId}. _isSettingUpRound: {IsSettingUpValue}", Id, CurrentTurnPlayerId, _isSettingUpRound);
        DrawInitialTurnCard(CurrentTurnPlayerId);
        _logger.LogDebug("[Game {GameId}] AdvanceTurn: Returned from DrawInitialTurnCard for new player. GamePhase: {CurrentPhase}", Id, GamePhase);

        // This check is important if DrawInitialTurnCard itself could end the round (e.g. deck empty)
        // However, CheckRoundEndCondition is already called at the end of DrawInitialTurnCard.
        // If DrawInitialTurnCard didn't end the round, but something else needs to be checked before turn truly starts, do it here.

        // Log turn start event after card is drawn and player is ready
        AddLogEntry(new GameLogEntry(GameLogEventType.TurnStart, CurrentTurnPlayerId, nextPlayer.Name, $"{nextPlayer.Name}'s turn started."));
        AddDomainEvent(new TurnStarted(Id, CurrentTurnPlayerId, RoundNumber));
        _logger.LogDebug("[Game {GameId}] AdvanceTurn: Exit. CurrentTurnPlayerId: {PlayerId}", Id, CurrentTurnPlayerId);
    }
}