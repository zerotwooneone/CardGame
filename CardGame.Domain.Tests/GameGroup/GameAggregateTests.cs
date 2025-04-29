using CardGame.Domain.Game;
using CardGame.Domain.Game.Event;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Tests.Helpers;
using CardGame.Domain.Tests.TestDoubles;
using CardGame.Domain.Types;
using Castle.Components.DictionaryAdapter;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Equivalency;

namespace CardGame.Domain.Tests.GameGroup;

[TestFixture] // NUnit attribute to mark a class containing tests
public class GameAggregateTests
{
    // --- Hardcoded Guids for Test Cards REMOVED - Now in TestDeckHelper ---

    // Helper to exclude common event metadata properties
    private static Func<EquivalencyOptions<T>, EquivalencyOptions<T>> ExcludeEventMetadata<T>() where T : IDomainEvent
    {
        return options => options
            .Excluding(ev => ev.EventId)
            .Excluding(ev => ev.OccurredOn)
            .Excluding(ev => ev.CorrelationId);
    }

    // --- CreateStandardTestCardList method REMOVED - Now in TestDeckHelper ---


    [Test]
    public void PlayCard_WhenPlayer1PlaysGuardSuccessfully_ShouldUpdateStateAndRaiseEvents()
    {
        // ARRANGE

        // 1. Define player info
        var aliceInfo = new PlayerInfo(Guid.NewGuid(), "Alice");
        var bobInfo = new PlayerInfo(Guid.NewGuid(), "Bob");
        var playerInfos = new List<PlayerInfo> {aliceInfo, bobInfo};
        var creatorId = aliceInfo.Id;

        // 2. Define the PREDICTABLE deck order using hardcoded IDs from helper
        var specificDeck = new List<Card>
        {
            new Card(TestDeckHelper.CardId_Pss, CardType.Princess),
            new Card(TestDeckHelper.CardId_C, CardType.Countess),
            new Card(TestDeckHelper.CardId_K, CardType.King), 
            new Card(TestDeckHelper.CardId_Pr1, CardType.Prince),
            new Card(TestDeckHelper.CardId_Pr2, CardType.Prince), 
            new Card(TestDeckHelper.CardId_H1, CardType.Handmaid),
            new Card(TestDeckHelper.CardId_H2, CardType.Handmaid),
            new Card(TestDeckHelper.CardId_G1, CardType.Guard), // P2 Draws this eventually (index 7)
            new Card(TestDeckHelper.CardId_B1, CardType.Baron), // P1 Draws this on turn 1 (index 8)
            new Card(TestDeckHelper.CardId_B2, CardType.Baron), // P2 Dealt this (index 9)
            new Card(TestDeckHelper.CardId_G3, CardType.Guard), // P1 Dealt this (index 10)
            new Card(TestDeckHelper.CardId_P1, CardType.Priest), // Public Set Aside 3 (index 11)
            new Card(TestDeckHelper.CardId_G4, CardType.Guard), // Public Set Aside 2 (index 12)
            new Card(TestDeckHelper.CardId_G5, CardType.Guard), // Public Set Aside 1 (index 13)
            new Card(TestDeckHelper.CardId_P2, CardType.Priest), // Private Set Aside (index 14)
            new Card(TestDeckHelper.CardId_G2, CardType.Guard) // Bottom card (index 15)
        };
        // Draw order prediction remains the same
        Card p1DealtCard = specificDeck[11]; // P1
        Card p2DealtCard = specificDeck[10]; // G3
        Card p1DrawnCard = specificDeck[9]; // B2

        // 3. Create the game using PlayerInfo, CreatorId, and the specific deck
        var game = Game.Game.CreateNewGame(playerInfos, creatorId, tokensToWin: 4, initialDeckCards: specificDeck);
        var player1 = game.Players.First(p => p.Id == aliceInfo.Id);
        var player2 = game.Players.First(p => p.Id == bobInfo.Id);

        // 4. Start the round (no randomizer needed)
        var deterministicRandomizer = new NonShufflingRandomizer();
        game.StartNewRound(deterministicRandomizer);

        // 5. Define action (predictable now)
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
            player1.Hand.GetHeldCard().Should().Be(cardToKeepInstance); // Kept B2
            player1.PlayedCards.Should().Contain(cardToPlayInstance.Type); // Played Priest
            game.DiscardPile.Should().Contain(cardToPlayInstance); // Contains P1
            game.DiscardPile.Last().Should().Be(cardToPlayInstance);
            player2.Status.Should().Be(PlayerStatus.Active);
            player2.Hand.Count.Should().Be(2, "Player 2 should have drawn a card");
            // P2 dealt G3, drew B1
            var player2HandCardTypes = player2.Hand.GetCards().Select(c => c.Type).ToList();
            player2HandCardTypes.Should().BeEquivalentTo(new[] {CardType.Guard, CardType.Baron});
            player2.Hand.GetCards().Should().Contain(p2DealtCard); // Still has G3
            player2.Hand.GetCards().Should().Contain(c => c.Id == TestDeckHelper.CardId_B1); // Drew B1

            var events = game.DomainEvents.ToList();
            events.Should().HaveCount(5);

            events.Should().ContainSingle(e => e is PlayerPlayedCard).Which.Should().BeEquivalentTo(
                new PlayerPlayedCard(game.Id, player1.Id, cardToPlayInstance.Type, targetPlayerId, null),
                ExcludeEventMetadata<PlayerPlayedCard>());

            events.Should().ContainSingle(e => e is PriestEffectUsed).Which.Should().BeEquivalentTo(
                new PriestEffectUsed(game.Id, player1.Id, targetPlayerId, p2DealtCard.Id,
                    p2DealtCard.Type), // Revealed G3
                ExcludeEventMetadata<PriestEffectUsed>());

            events.Should().ContainSingle(e => e is TurnStarted).Which.Should().BeEquivalentTo(
                new TurnStarted(game.Id, player2.Id, game.RoundNumber),
                ExcludeEventMetadata<TurnStarted>());

            events.Should().ContainSingle(e => e is PlayerDrewCard && ((PlayerDrewCard) e).PlayerId == player2.Id);
            events.Should().ContainSingle(e => e is DeckChanged);
            events.OfType<DeckChanged>().Single().CardsRemaining.Should()
                .Be(8); // 16 - 4 aside - 2 dealt - 1 P1 draw - 1 P2 draw = 8
        }
    }

    [Test] // Test for game end condition
    public void PlayCard_WhenPlayerWinsFinalRound_ShouldEndGameAndDeclareWinner()
    {
        // ARRANGE
        var aliceInfo = new PlayerInfo(Guid.NewGuid(), "Alice");
        var bobInfo = new PlayerInfo(Guid.NewGuid(), "Bob");
        var playerInfos = new List<PlayerInfo> {aliceInfo, bobInfo};
        var creatorId = aliceInfo.Id;
        var tokensNeededToWin = 1;
        var specificDeck = new List<Card>
        {
            /* ... deck definition from previous test ... */
            new Card(TestDeckHelper.CardId_Pss, CardType.Princess),
            new Card(TestDeckHelper.CardId_C, CardType.Countess),
            new Card(TestDeckHelper.CardId_K, CardType.King), new Card(TestDeckHelper.CardId_Pr1, CardType.Prince),
            new Card(TestDeckHelper.CardId_Pr2, CardType.Prince),
            new Card(TestDeckHelper.CardId_H1, CardType.Handmaid),
            new Card(TestDeckHelper.CardId_H2, CardType.Handmaid), new Card(TestDeckHelper.CardId_B1, CardType.Baron),
            new Card(TestDeckHelper.CardId_B2, CardType.Baron), // P2 Draws this eventually (index 8)
            new Card(TestDeckHelper.CardId_P1, CardType.Priest), // P1 Draws this on turn 1 (index 9)
            new Card(TestDeckHelper.CardId_P2, CardType.Priest), // P2 Dealt this (index 10)
            new Card(TestDeckHelper.CardId_G3, CardType.Guard), // P1 Dealt this (index 11)
            new Card(TestDeckHelper.CardId_G1, CardType.Guard), // Public Set Aside 3 (index 12)
            new Card(TestDeckHelper.CardId_G2, CardType.Guard), // Public Set Aside 2 (index 13)
            new Card(TestDeckHelper.CardId_G4, CardType.Guard), // Public Set Aside 1 (index 14)
            new Card(TestDeckHelper.CardId_G5, CardType.Guard) // Private Set Aside (index 15)
        };
        Card p1DealtCard = specificDeck[11]; // G3
        Card p2DealtCard = specificDeck[10]; // P2
        Card p1DrawnCard = specificDeck[9]; // P1
        var game = Game.Game.CreateNewGame(playerInfos, creatorId, tokensNeededToWin, specificDeck);
        var player1 = game.Players.First(p => p.Id == aliceInfo.Id);
        var player2 = game.Players.First(p => p.Id == bobInfo.Id);
        var nonShufflingRandomizer = new NonShufflingRandomizer();
        game.StartNewRound(nonShufflingRandomizer);
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

            // Use ContainSingle for each event type
            events.Should().ContainSingle(e => e is PlayerPlayedCard).Which.Should().BeEquivalentTo(
                new PlayerPlayedCard(game.Id, player1.Id, cardToPlayInstance.Type, targetPlayerId, guessedCardType),
                ExcludeEventMetadata<PlayerPlayedCard>());

            events.Should().ContainSingle(e => e is GuardGuessResult).Which.Should().BeEquivalentTo(
                new GuardGuessResult(game.Id, player1.Id, targetPlayerId, guessedCardType, true), // Correct guess
                ExcludeEventMetadata<GuardGuessResult>());

            events.Should().ContainSingle(e => e is PlayerEliminated).Which.Should().BeEquivalentTo(
                new PlayerEliminated(game.Id, player2.Id, $"guessed correctly by {player1.Name} with a Guard",
                    CardType.Guard),
                ExcludeEventMetadata<PlayerEliminated>());

            // Define expected final hands for RoundEnded assertion
            var expectedFinalHandsWin = new Dictionary<Guid, CardType?>
            {
                {player1.Id, cardToKeepInstance.Type}, // Alice kept Priest
                {player2.Id, CardType.Priest} // Bob was eliminated, hand is irrelevant/null
            };
            events.Should().ContainSingle(e => e is RoundEnded).Which.Should().BeEquivalentTo(
                new RoundEnded(game.Id, player1.Id, expectedFinalHandsWin, "Last player standing"),
                ExcludeEventMetadata<RoundEnded>());
            // Optionally verify winner ID separately if BeEquivalentTo on dictionary is complex
            // events.OfType<RoundEnded>().Single().WinnerPlayerId.Should().Be(player1.Id);

            events.Should().ContainSingle(e => e is TokenAwarded).Which.Should().BeEquivalentTo(
                new TokenAwarded(game.Id, player1.Id, 1),
                ExcludeEventMetadata<TokenAwarded>());

            events.Should().ContainSingle(e => e is GameEnded).Which.Should().BeEquivalentTo(
                new GameEnded(game.Id, player1.Id),
                ExcludeEventMetadata<GameEnded>());
        }
    }

    // --- Deck Empty Test (Uses Load) ---
    [Test]
    public void PlayCard_WhenPlayerPlaysAndNextPlayerCannotDraw_ShouldEndRoundAndDeclareWinnerByHighestRank()
    {
        // ARRANGE
        Guid gameId = Guid.NewGuid();
        int tokensToWin = 4;
        var p1Id = Guid.NewGuid();
        var p2Id = Guid.NewGuid();
        // Use hardcoded IDs from helper
        var p1Card_King = new Card(TestDeckHelper.CardId_K, CardType.King);
        var p1Card_Handmaid = new Card(TestDeckHelper.CardId_H1, CardType.Handmaid);
        var p2Card_Princess = new Card(TestDeckHelper.CardId_Pss, CardType.Princess);
        var aliceHand = Hand.Load(new List<Card> {p1Card_King, p1Card_Handmaid});
        var alice = Player.Load(p1Id, "Alice", PlayerStatus.Active, aliceHand, new List<CardType>(), 0, false);
        var bobHand = Hand.Load(new List<Card> {p2Card_Princess});
        var bob = Player.Load(p2Id, "Bob", PlayerStatus.Active, bobHand, new List<CardType>(), 0, false);
        var players = new List<Player> {alice, bob};
        var deck = Deck.Load(Enumerable.Empty<Card>()); // Empty Deck
        var discardPile = new List<Card>();
        var publiclySetAsideCards = new List<Card>();
        // Need the base card set for Game.Load, use helper
        var initialDeckCardSet = TestDeckHelper.CreateStandardTestCardList(); // Use helper

        var game = Game.Game.Load(gameId, 1, GamePhase.RoundInProgress, p1Id, players, deck, null, publiclySetAsideCards,
            discardPile, tokensToWin, null, initialDeckCardSet);
        var cardToPlayInstance = p1Card_Handmaid; // Alice plays Handmaid
        game.ClearDomainEvents();

        // ACT
        game.PlayCard(p1Id, cardToPlayInstance, null, null);

        // ASSERT
        using (new AssertionScope())
        {
            game.GamePhase.Should().Be(GamePhase.RoundOver);
            alice.Status.Should().Be(PlayerStatus.Active);
            alice.Hand.Count.Should().Be(1);
            alice.Hand.GetHeldCard()?.Type.Should().Be(CardType.King);
            bob.Status.Should().Be(PlayerStatus.Active);
            bob.Hand.Count.Should().Be(1);
            bob.Hand.GetHeldCard()?.Type.Should().Be(CardType.Princess);

            var events = game.DomainEvents.ToList();
            events.Should().HaveCount(4);

            // ... (event assertions remain the same, using ExcludeEventMetadata) ...
            events.Should().ContainSingle(e => e is PlayerPlayedCard).Which.Should().BeEquivalentTo(
                new PlayerPlayedCard(game.Id, p1Id, CardType.Handmaid, null, null),
                ExcludeEventMetadata<PlayerPlayedCard>());
            events.Should().ContainSingle(e => e is HandmaidProtectionSet).Which.Should()
                .BeEquivalentTo(new HandmaidProtectionSet(game.Id, p1Id),
                    ExcludeEventMetadata<HandmaidProtectionSet>());
            var expectedFinalHands = new Dictionary<Guid, CardType?> {{p1Id, CardType.King}, {p2Id, CardType.Princess}};
            events.Should().ContainSingle(e => e is RoundEnded).Which.Should().BeEquivalentTo(
                new RoundEnded(game.Id, p2Id, expectedFinalHands, "Deck empty, highest card wins"),
                ExcludeEventMetadata<RoundEnded>());
            events.OfType<RoundEnded>().Single().WinnerPlayerId.Should().Be(p2Id);
            events.Should().ContainSingle(e => e is TokenAwarded).Which.Should()
                .BeEquivalentTo(new TokenAwarded(game.Id, p2Id, 1), ExcludeEventMetadata<TokenAwarded>());
            events.Should().NotContain(e => e is GameEnded);
            events.Should().NotContain(e => e is TurnStarted && ((TurnStarted) e).PlayerId == p2Id);
            events.Should().NotContain(e => e is PlayerDrewCard && ((PlayerDrewCard) e).PlayerId == p2Id);
            events.Should().NotContain(e => e is DeckChanged);
        }
    }


    // Add more tests...
}