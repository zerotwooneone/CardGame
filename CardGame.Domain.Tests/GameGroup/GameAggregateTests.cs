using CardGame.Domain.Game;
using CardGame.Domain.Game.Event;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Tests.Helpers;
using CardGame.Domain.Tests.TestDoubles;
using CardGame.Domain.Types;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Equivalency;
using Microsoft.Extensions.Logging;

namespace CardGame.Domain.Tests.GameGroup;

[TestFixture] // NUnit attribute to mark a class containing tests
public class GameAggregateTests
{
    // Helper to exclude common event metadata properties
    private static Func<EquivalencyOptions<T>, EquivalencyOptions<T>> ExcludeEventMetadata<T>() where T : IDomainEvent
    {
        return options => options
            .Excluding(ev => ev.EventId)
            .Excluding(ev => ev.OccurredOn)
            .Excluding(ev => ev.CorrelationId);
    }

    // Helper to create the standard card list for tests needing default deck
    private static List<Card> CreateStandardTestCardList()
    {
        return new List<Card>
        {
            new Card(CardType.Princess.Name, CardType.Princess),
            new Card(CardType.Countess.Name, CardType.Countess),
            new Card(CardType.King.Name, CardType.King), 
            new Card(CardType.Prince.Name, CardType.Prince),
            new Card(CardType.Prince.Name, CardType.Prince), 
            new Card(CardType.Handmaid.Name, CardType.Handmaid),
            new Card(CardType.Handmaid.Name, CardType.Handmaid), 
            new Card(CardType.Baron.Name, CardType.Baron),
            new Card(CardType.Baron.Name, CardType.Baron), 
            new Card(CardType.Priest.Name, CardType.Priest),
            new Card(CardType.Priest.Name, CardType.Priest), 
            new Card(CardType.Guard.Name, CardType.Guard),
            new Card(CardType.Guard.Name, CardType.Guard), 
            new Card(CardType.Guard.Name, CardType.Guard),
            new Card(CardType.Guard.Name, CardType.Guard), 
            new Card(CardType.Guard.Name, CardType.Guard),
        };
    }


    [Test]
    public void PlayCard_WhenPlayer1PlaysGuardSuccessfully_ShouldUpdateStateAndRaiseEvents()
    {
        // ARRANGE
        var aliceInfo = new PlayerInfo(Guid.NewGuid(), "Alice");
        var bobInfo = new PlayerInfo(Guid.NewGuid(), "Bob");
        var playerInfos = new List<PlayerInfo> {aliceInfo, bobInfo};
        var creatorId = aliceInfo.Id;
        var specificDeck = new List<Card>
        {
            /* ... deck definition ... */
            new Card(CardType.Princess.Name, CardType.Princess),
            new Card(CardType.Countess.Name, CardType.Countess),
            new Card(CardType.King.Name, CardType.King), 
            new Card(CardType.Prince.Name, CardType.Prince),
            new Card(CardType.Prince.Name, CardType.Prince), 
            new Card(CardType.Handmaid.Name, CardType.Handmaid),
            new Card(CardType.Handmaid.Name, CardType.Handmaid),
            new Card(CardType.Guard.Name, CardType.Guard), // P2 Draws
            new Card(CardType.Baron.Name, CardType.Baron), // P1 Draws
            new Card(CardType.Baron.Name, CardType.Baron), // P2 Dealt
            new Card(CardType.Guard.Name, CardType.Guard), // P1 Dealt
            new Card(CardType.Priest.Name, CardType.Priest), // Public Set Aside 3
            new Card(CardType.Guard.Name, CardType.Guard), // Public Set Aside 2
            new Card(CardType.Guard.Name, CardType.Guard), // Public Set Aside 1
            new Card(CardType.Priest.Name, CardType.Priest), // Private Set Aside
            new Card(CardType.Guard.Name, CardType.Guard) // Bottom card
        };
        Card p1DealtCard = specificDeck[11]; // P1
        Card p2DealtCard = specificDeck[10]; // G3
        Card p1DrawnCard = specificDeck[9]; // B2
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var game = Game.Game.CreateNewGame(Guid.NewGuid(), playerInfos, creatorId, specificDeck, tokensToWin: 4, randomizer: new NonShufflingRandomizer(), loggerFactory);
        var player1 = game.Players.First(p => p.Id == aliceInfo.Id);
        var player2 = game.Players.First(p => p.Id == bobInfo.Id);
        game.StartNewRound();
        var cardToPlayInstance = p1DealtCard; // P1 plays P1
        var cardToKeepInstance = p1DrawnCard; // P1 keeps B2
        var targetPlayerId = player2.Id;
        game.ClearDomainEvents();

        // ACT
        game.PlayCard(player1.Id, cardToPlayInstance, targetPlayerId, null); // Play Priest

        // ASSERT
        using (new AssertionScope())
        {
            game.CurrentTurnPlayerId.Should().Be(player2.Id);
            player1.Hand.Count.Should().Be(1);
            player1.Hand.GetHeldCard().Should().Be(cardToKeepInstance);
            player1.PlayedCards.Should().Contain(cardToPlayInstance.Rank);
            game.DiscardPile.Should().Contain(cardToPlayInstance);
            game.DiscardPile.Last().Should().Be(cardToPlayInstance);
            player2.Status.Should().Be(PlayerStatus.Active);
            player2.Hand.Count.Should().Be(2);
            var player2HandCardTypes = player2.Hand.GetCards().Select(c => c.Rank).ToList();
            player2HandCardTypes.Should().BeEquivalentTo(new[] {CardType.Guard, CardType.Baron});
            player2.Hand.GetCards().Should().Contain(p2DealtCard);
            player2.Hand.GetCards().Should().Contain(c => c.AppearanceId == CardType.Baron.Name);

            var events = game.DomainEvents.ToList();
            events.Should().HaveCount(5);

            events.Should().ContainSingle(e => e is PlayerPlayedCard).Which.Should().BeEquivalentTo(
                new PlayerPlayedCard(game.Id, player1.Id, cardToPlayInstance, targetPlayerId, null),
                ExcludeEventMetadata<PlayerPlayedCard>());

            events.Should().ContainSingle(e => e is PriestEffectUsed).Which.Should().BeEquivalentTo(
                new PriestEffectUsed(game.Id, player1.Id, targetPlayerId, p2DealtCard.AppearanceId, p2DealtCard.Rank),
                ExcludeEventMetadata<PriestEffectUsed>());

            events.Should().ContainSingle(e => e is TurnStarted).Which.Should().BeEquivalentTo(
                new TurnStarted(game.Id, player2.Id, game.RoundNumber),
                ExcludeEventMetadata<TurnStarted>());

            events.Should().ContainSingle(e => e is PlayerDrewCard && ((PlayerDrewCard) e).PlayerId == player2.Id);
            events.Should().ContainSingle(e => e is DeckChanged);
            events.OfType<DeckChanged>().Single().CardsRemaining.Should().Be(8);
        }
    }

        [Test]
        public void PlayCard_WhenPlayerWinsFinalRound_ShouldEndGameAndDeclareWinner()
        {
            // ARRANGE
            var aliceInfo = new PlayerInfo(Guid.NewGuid(), "Alice");
            var bobInfo = new PlayerInfo(Guid.NewGuid(), "Bob");
            var playerInfos = new List<PlayerInfo> { aliceInfo, bobInfo };
            var creatorId = aliceInfo.Id;
            var tokensNeededToWin = 1;
            var specificDeck = new List<Card> { /* ... deck definition ... */
                new Card(CardType.Princess.Name, CardType.Princess), 
                new Card(CardType.Countess.Name, CardType.Countess),
                new Card(CardType.King.Name, CardType.King), 
                new Card(CardType.Prince.Name, CardType.Prince), 
                new Card(CardType.Prince.Name, CardType.Prince),
                new Card(CardType.Handmaid.Name, CardType.Handmaid), 
                new Card(CardType.Handmaid.Name, CardType.Handmaid), 
                new Card(CardType.Baron.Name, CardType.Baron),
                new Card(CardType.Baron.Name, CardType.Baron),   // P2 Draws
                new Card(CardType.Priest.Name, CardType.Priest),  // P1 Draws
                new Card(CardType.Priest.Name, CardType.Priest),  // P2 Dealt
                new Card(CardType.Guard.Name, CardType.Guard),   // P1 Dealt
                new Card(CardType.Guard.Name, CardType.Guard),   // Public Set Aside 3
                new Card(CardType.Guard.Name, CardType.Guard),   // Public Set Aside 2
                new Card(CardType.Guard.Name, CardType.Guard),   // Public Set Aside 1
                new Card(CardType.Guard.Name, CardType.Guard)    // Private Set Aside
            };
            Card p1DealtCard = specificDeck[11]; // G3
            Card p2DealtCard = specificDeck[10]; // P2 (Priest)
            Card p1DrawnCard = specificDeck[9];  // P1 (Priest)
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var game = Game.Game.CreateNewGame(Guid.NewGuid(), playerInfos, creatorId, specificDeck, tokensNeededToWin, randomizer: new NonShufflingRandomizer(), loggerFactory);
            var player1 = game.Players.First(p => p.Id == aliceInfo.Id);
            var player2 = game.Players.First(p => p.Id == bobInfo.Id);
            game.StartNewRound();
            var cardToPlayInstance = p1DealtCard; // Alice plays G3
            var cardToKeepInstance = p1DrawnCard; // Alice keeps P1
            var targetPlayerId = bobInfo.Id;
            var guessedCardType = CardType.Priest; // Correctly guess Bob's P2
            game.ClearDomainEvents();

            // ACT
            game.PlayCard(player1.Id, cardToPlayInstance, targetPlayerId, guessedCardType);

            // ASSERT
            using (new AssertionScope())
            {
                game.GamePhase.Should().Be(GamePhase.GameOver);
                player2.Status.Should().Be(PlayerStatus.Eliminated);
                player1.Status.Should().Be(PlayerStatus.Active);
                player1.TokensWon.Should().Be(1);

                var events = game.DomainEvents.ToList();
                events.Should().HaveCount(6);

                events.OfType<PlayerPlayedCard>().Should().ContainSingle().Which.Should().BeEquivalentTo(
                    new PlayerPlayedCard(game.Id, player1.Id, cardToPlayInstance, targetPlayerId, guessedCardType),
                    ExcludeEventMetadata<PlayerPlayedCard>());
                events.OfType<GuardGuessResult>().Should().ContainSingle().Which.Should().BeEquivalentTo(
                    new GuardGuessResult(game.Id, player1.Id, targetPlayerId, guessedCardType, true),
                    ExcludeEventMetadata<GuardGuessResult>());
                events.OfType<PlayerEliminated>().Should().ContainSingle().Which.Should().BeEquivalentTo(
                    new PlayerEliminated(game.Id, player2.Id, $"guessed correctly by {player1.Name} with a Guard", CardType.Guard),
                    ExcludeEventMetadata<PlayerEliminated>());

                // Construct expected PlayerRoundEndSummary list
                var expectedPlayerSummaries = new List<PlayerRoundEndSummary>
                {
                    new PlayerRoundEndSummary(player1.Id, player1.Name, new List<Card> { cardToKeepInstance }, new List<int> { cardToPlayInstance.Rank.Value }, 1),
                    new PlayerRoundEndSummary(player2.Id, player2.Name, new List<Card>(), new List<int>(), 0) // Bob eliminated, no card, empty discard
                };
                events.OfType<RoundEnded>().Should().ContainSingle().Which.Should().BeEquivalentTo(
                    new RoundEnded(game.Id, player1.Id, "Last player standing", expectedPlayerSummaries),
                    ExcludeEventMetadata<RoundEnded>());

                events.OfType<TokenAwarded>().Should().ContainSingle().Which.Should().BeEquivalentTo(
                    new TokenAwarded(game.Id, player1.Id, 1),
                    ExcludeEventMetadata<TokenAwarded>());
                events.OfType<GameEnded>().Should().ContainSingle().Which.Should().BeEquivalentTo(
                    new GameEnded(game.Id, player1.Id),
                    ExcludeEventMetadata<GameEnded>());
            }
        }

          [Test]
        public void PlayCard_WhenPlayerPlaysAndNextPlayerCannotDraw_ShouldStartNextRound()
        {
            // ARRANGE
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Guid gameId = Guid.NewGuid();
            int tokensToWin = 4;
            var p1Id = Guid.NewGuid();
            var p2Id = Guid.NewGuid();
            var p1Card_King = new Card(CardType.King.Name, CardType.King);
            var p1Card_Handmaid = new Card(CardType.Handmaid.Name, CardType.Handmaid);
            var p2Card_Princess = new Card(CardType.Princess.Name, CardType.Princess);
            var aliceHand = Hand.Load(new List<Card> { p1Card_King, p1Card_Handmaid });
            var aliceLogger = loggerFactory.CreateLogger<Player>();
            var alice = Player.Load(p1Id, "Alice", PlayerStatus.Active, aliceHand, new List<CardType>(), 0, false, aliceLogger);
            var bobHand = Hand.Load(new List<Card> { p2Card_Princess });
            var bobLogger = loggerFactory.CreateLogger<Player>();
            var bob = Player.Load(p2Id, "Bob", PlayerStatus.Active, bobHand, new List<CardType>(), 0, false, bobLogger);
            var players = new List<Player> { alice, bob };
            var emptyDeck = Deck.Load(Enumerable.Empty<Card>());
            var discardPile = new List<Card>();
            var publiclySetAsideCards = new List<Card>();
            var initialDeckCardSet = TestDeckHelper.CreateStandardTestCardList();

            var game = Game.Game.Load(gameId, Guid.NewGuid(), 1, GamePhase.RoundInProgress, p1Id, players, emptyDeck, null, publiclySetAsideCards, discardPile, tokensToWin, null, initialDeckCardSet, loggerFactory);
            var cardToPlayInstance = p1Card_Handmaid;
            game.ClearDomainEvents();

            // ACT
            game.PlayCard(p1Id, cardToPlayInstance, null, null);

            // ASSERT
            using (new AssertionScope())
            {
                game.GamePhase.Should().Be(GamePhase.RoundInProgress);
                game.RoundNumber.Should().Be(2);
                alice.Status.Should().Be(PlayerStatus.Active);
                bob.Status.Should().Be(PlayerStatus.Active);
                bob.TokensWon.Should().Be(1);
                game.LastRoundWinnerId.Should().Be(p2Id);
                game.CurrentTurnPlayerId.Should().Be(p2Id); // Bob starts new round

                // Assertions for new round state (hands, deck)
                // Since the internal StartNewRound uses DefaultRandomizer, we can't predict exact cards.
                // We can only check counts and that cards exist.
                bob.Hand.Count.Should().Be(2, "Bob should have 2 cards in the new round after drawing.");
                bob.Hand.GetCards().Should().NotBeEmpty();
                bob.Hand.GetCards().All(c => c != null).Should().BeTrue();

                alice.Hand.Count.Should().Be(1, "Alice should have 1 card in the new round.");
                alice.Hand.GetHeldCard().Should().NotBeNull();

                // Deck has 16 cards, 4 set aside for 2 players, 2 dealt to players, 1 drawn by current player (Bob)
                game.Deck.CardsRemaining.Should().Be(16 - 4 - 2 - 1, "Deck count for new round is incorrect.");

                var events = game.DomainEvents.ToList();
                events.Should().HaveCount(8);

                events.OfType<PlayerPlayedCard>().Should().ContainSingle().Which.Should().BeEquivalentTo(
                    new PlayerPlayedCard(game.Id, p1Id, cardToPlayInstance, null, null),
                    ExcludeEventMetadata<PlayerPlayedCard>());

                events.OfType<HandmaidProtectionSet>().Should().ContainSingle().Which.Should().BeEquivalentTo(
                    new HandmaidProtectionSet(game.Id, p1Id),
                    ExcludeEventMetadata<HandmaidProtectionSet>());

                var expectedPlayerSummariesRound1 = new List<PlayerRoundEndSummary>
                {
                    new PlayerRoundEndSummary(p1Id, "Alice", new List<Card> { p1Card_King }, new List<int> { p1Card_Handmaid.Rank.Value }, 0),
                    new PlayerRoundEndSummary(p2Id, "Bob", new List<Card> { p2Card_Princess }, new List<int>(), 1)
                };
                events.OfType<RoundEnded>().Should().ContainSingle().Which.Should().BeEquivalentTo(
                    new RoundEnded(game.Id, p2Id, "Deck empty, highest card wins", expectedPlayerSummariesRound1),
                    ExcludeEventMetadata<RoundEnded>());

                events.OfType<TokenAwarded>().Should().ContainSingle().Which.Should().BeEquivalentTo(
                    new TokenAwarded(game.Id, p2Id, 1),
                    ExcludeEventMetadata<TokenAwarded>());

                events.OfType<RoundStarted>().Should().ContainSingle().Which.RoundNumber.Should().Be(2);
                events.OfType<TurnStarted>().Should().ContainSingle().Which.PlayerId.Should().Be(p2Id);
                events.Should().ContainSingle(e => e is PlayerDrewCard && ((PlayerDrewCard) e).PlayerId == p2Id);
                events.Should().ContainSingle(e => e is DeckChanged);

                events.Should().NotContain(e => e is GameEnded);
            }
        }


    // Add more tests...
}