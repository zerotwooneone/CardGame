using System.Collections.Concurrent;
using CardGame.Domain.Game;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;

namespace CardGame.Infrastructure.Persistance;

/// <summary>
    /// In-memory implementation of the IGameRepository for testing and development.
    /// Initializes with one pre-loaded game state in progress.
    /// </summary>
    public class InMemoryGameRepository : IGameRepository
    {
        // Use ConcurrentDictionary for basic thread safety if accessed by multiple threads
        private static readonly ConcurrentDictionary<Guid, Game> _games = new ConcurrentDictionary<Guid, Game>();

        // A known Guid for the pre-loaded game for easy access in tests/dev
        public static readonly Guid PreLoadedGameId = new Guid("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE");

        // Static constructor to initialize the pre-loaded game state once
        static InMemoryGameRepository()
        {
            InitializePreLoadedGame();
        }

        private static void InitializePreLoadedGame()
        {
            try
            {
                // --- Define the state for the pre-loaded game ---
                int tokensToWin = 1; // Game ends after this round is won

                // Create the full initial card list (predictable order)
                var fullCardList = CreateStandardCardListForLoad(); // Use helper

                // Distribute cards based on non-shuffled order (top is last element)
                Card setAsideCard = fullCardList[15];   // Guard
                Card p1InitialCard = fullCardList[14]; // Guard
                Card p2InitialCard = fullCardList[13]; // Guard
                Card p1Turn1Draw = fullCardList[12];  // Guard
                // Remaining cards for deck (indices 0-11)
                var remainingCardsForDeckList = fullCardList.Take(12).ToList();

                // Create Player States
                var aliceId = Guid.NewGuid(); // Give players known IDs if helpful, or new Guids
                var bobId = Guid.NewGuid();

                var aliceHand = Hand.Load(new List<Card> { p1InitialCard, p1Turn1Draw });
                var alice = Player.Load(aliceId, "Alice", PlayerStatus.Active, aliceHand, new List<CardType>(), 0, false);

                var bobHand = Hand.Load(new List<Card> { p2InitialCard });
                var bob = Player.Load(bobId, "Bob", PlayerStatus.Active, bobHand, new List<CardType>(), 0, false);

                var players = new List<Player> { alice, bob };

                // Create Deck State
                var deck = Deck.Load(remainingCardsForDeckList); // Load remaining cards

                // Create Discard Pile State
                var discardPile = new List<Card>(); // Empty

                // Load the Game aggregate using the static factory
                var game = Game.Load(
                    id: PreLoadedGameId, // Use the known Guid
                    roundNumber: 1,
                    gamePhase: GamePhase.RoundInProgress,
                    currentTurnPlayerId: aliceId, // Alice's turn
                    players: players,
                    deck: deck,
                    setAsideCard: setAsideCard,
                    discardPile: discardPile,
                    tokensToWin: tokensToWin
                );

                // Store the pre-loaded game
                _games.TryAdd(game.Id, game);
            }
            catch (Exception ex)
            {
                // Handle or log initialization error appropriately
                Console.WriteLine($"Error initializing pre-loaded game: {ex.Message}");
                // Depending on requirements, might want to throw or ensure _games is empty
            }
        }

        /// <summary>
        /// Helper to create the card list for the pre-loaded game state.
        /// Ensures consistency with Deck's internal creation logic.
        /// </summary>
        private static List<Card> CreateStandardCardListForLoad()
        {
             // Copied from Deck.CreateStandardCardList - ensure this stays in sync
             // if the standard deck composition changes.
             return new List<Card>
            {
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


        // --- IGameRepository Implementation ---

        public async Task<Game?> GetByIdAsync(Guid gameId, CancellationToken cancellationToken = default)
        {
            // Simulate async operation
            await Task.Yield(); // Use Task.Yield or Task.Delay(1) for more realistic async simulation if desired

            _games.TryGetValue(gameId, out var game);
            // Return a copy to prevent unintended modifications to the stored object?
            // For in-memory, returning the direct reference is common but less safe.
            // Let's return the reference for simplicity. Be careful in tests/usage.
            return game;
        }

        public Task SaveAsync(Game game, CancellationToken cancellationToken = default)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));

            // Simulate async operation
            // await Task.Delay(1, cancellationToken); // Simulate minimal I/O delay

            // AddOrUpdate handles both new and existing games
            _games.AddOrUpdate(game.Id, game, (id, existingGame) => game);

            // Important: After saving, the caller should typically retrieve and publish
            // domain events from the game object. This repository doesn't do that.
            // game.ClearDomainEvents(); // Should be done AFTER events are published by caller

            return Task.CompletedTask;
        }
    }