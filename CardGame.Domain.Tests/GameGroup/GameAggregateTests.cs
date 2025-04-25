using CardGame.Domain.Game;
using CardGame.Domain.Game.Event;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Tests.TestDoubles;
using CardGame.Domain.Types;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Equivalency;

namespace CardGame.Domain.Tests.GameGroup;

    [TestFixture] 
    public class GameAggregateTests
    {
        // No SetUp needed for this specific test

        [Test] // NUnit attribute marks this method as a test case (replaced [Fact])
        public void PlayCard_WhenPlayer1PlaysGuardSuccessfully_ShouldUpdateStateAndRaiseEvents()
        {
            // ARRANGE

            // 1. Create the game with two players
            var playerNames = new List<PlayerInfo> { new PlayerInfo(Guid.NewGuid(), "Alice"), new PlayerInfo(Guid.NewGuid(), "Bob") }; 
            var game = Game.Game.CreateNewGame(playerNames);
            var player1 = game.Players.First(p => p.Name == "Alice");
            var player2 = game.Players.First(p => p.Name == "Bob");

            var deterministicRandomizer = new NonShufflingRandomizer();
            game.StartNewRound(randomizer: deterministicRandomizer);

            // Expected Types: Guard, Guard (based on previous prediction logic)
            var player1HandCards = player1.Hand.GetCards().ToList();
            player1HandCards.Should().HaveCount(2);
            player1HandCards.Should().AllSatisfy(c => c.Type.Should().Be(CardType.Guard));

            var cardToPlayInstance = player1HandCards[0];
            var cardToKeepInstance = player1HandCards[1];

            var targetPlayerId = player2.Id;
            var guessedCardType = CardType.Princess;

            // 5. Clear events raised during setup
            game.ClearDomainEvents();

            // ACT

            // Player 1 plays the specific Guard instance they were dealt first
            game.PlayCard(player1.Id, cardToPlayInstance, targetPlayerId, guessedCardType);


            // ASSERT

            // 1. Turn should advance to Player 2
            game.CurrentTurnPlayerId.Should().Be(player2.Id);

            // 2. Player 1's hand should contain only the card they kept
            player1.Hand.Count.Should().Be(1);
            player1.Hand.GetHeldCard().Should().Be(cardToKeepInstance); 

            // 3. Player 1's played cards should include the type played
            player1.PlayedCards.Should().Contain(cardToPlayInstance.Type);

            // 4. Discard pile should contain the specific card instance played
            game.DiscardPile.Should().Contain(cardToPlayInstance);
            game.DiscardPile.Last().Should().Be(cardToPlayInstance);

            // 5. Player 2 should still be active
            player2.Status.Should().Be(PlayerStatus.Active);
            // P2 was dealt Guard(2), drew Guard(0). Should have 1 card after P1 turn ends, P2 turn starts+draws.
            // Let's check the type and count after P2's draw (which happens in AdvanceTurn called by PlayCard)
            player2.Hand.Count.Should().Be(2); // P2 drew at start of their turn
            player2.Hand.GetHeldCard()?.Type.Should().Be(CardType.Guard); // Check the type


            // 6. Correct domain events should have been raised by the PlayCard action
            var events = game.DomainEvents.ToList();

            // Event 1: PlayerPlayedCard
            events.Should().ContainSingle(e => e is PlayerPlayedCard)
                  .Which.Should().BeEquivalentTo(new PlayerPlayedCard(
                      game.Id,
                      player1.Id,
                      cardToPlayInstance.Type, // Event contains the Type played
                      targetPlayerId,
                      guessedCardType
                  ), options => options.Excluding(e => e.EventId).Excluding(e => e.OccurredOn).Excluding(e => e.CorrelationId));

            // Event 2: GuardGuessResult (Guess was Princess, target has Guard -> incorrect)
            events.Should().ContainSingle(e => e is GuardGuessResult)
                  .Which.Should().BeEquivalentTo(new GuardGuessResult(
                      game.Id,
                      player1.Id,
                      targetPlayerId,
                      guessedCardType, // Princess
                      false // Guess was incorrect
                  ), options => options.Excluding(e => e.EventId).Excluding(e => e.OccurredOn).Excluding(e => e.CorrelationId));


            // Event 3: TurnStarted (for Player 2)
             events.Should().ContainSingle(e => e is TurnStarted)
                  .Which.Should().BeEquivalentTo(new TurnStarted(
                      game.Id,
                      player2.Id, // Player 2's turn started
                      game.RoundNumber
                  ), options => options.Excluding(e => e.EventId).Excluding(e => e.OccurredOn).Excluding(e => e.CorrelationId));

             // Event 4: PlayerDrewCard (for Player 2)
             events.Should().ContainSingle(e => e is PlayerDrewCard && ((PlayerDrewCard)e).PlayerId == player2.Id);

             // Event 5: DeckChanged (for Player 2's draw)
             events.Should().ContainSingle(e => e is DeckChanged);
             // Deck started with 16. SetAside=1, DealP1=1, DealP2=1, P1Draw=1, P1Play=0, P2Draw=1 -> 16-5 = 11 left
             events.OfType<DeckChanged>().Single().CardsRemaining.Should().Be(11);


             // Check total number of events raised by PlayCard action
             events.Should().HaveCount(5); // PlayerPlayedCard, GuardGuessResult, TurnStarted, PlayerDrewCard, DeckChanged
        }
        
        [Test] // New test for game end condition
        public void PlayCard_WhenPlayerWinsFinalRound_ShouldEndGameAndDeclareWinner()
        {
            // ARRANGE

            // 1. Define Game Parameters
            Guid gameId = Guid.NewGuid();
            int tokensToWin = 1; // Game ends after this round is won

            // 2. Create the full initial card list (predictable order)
            // We need specific instances to distribute correctly.
            var fullCardList = new List<Card> {
                new Card(Guid.NewGuid(), CardType.Princess), 
                new Card(Guid.NewGuid(), CardType.Countess),
                new Card(Guid.NewGuid(), CardType.King), 
                new Card(Guid.NewGuid(), CardType.Prince),
                new Card(Guid.NewGuid(), CardType.Prince), 
                new Card(Guid.NewGuid(), CardType.Handmaid),
                new Card(Guid.NewGuid(), CardType.Handmaid), 
                new Card(Guid.NewGuid(), CardType.Baron),
                new Card(Guid.NewGuid(), CardType.Baron), 
                new Card(Guid.NewGuid(), CardType.Priest),
                new Card(Guid.NewGuid(), CardType.Priest), 
                new Card(Guid.NewGuid(), CardType.Guard),
                new Card(Guid.NewGuid(), CardType.Guard), 
                new Card(Guid.NewGuid(), CardType.Guard),
                new Card(Guid.NewGuid(), CardType.Guard), 
                new Card(Guid.NewGuid(), CardType.Guard),
            };
            // Because Deck uses ImmutableStack.CreateRange, the effective draw order is reversed.
            // Top card to be drawn first is the *last* one in the list above.
            Card setAsideCard = fullCardList[15];   // Guard (last G)
            Card p1InitialCard = fullCardList[14]; // Guard (4th G)
            Card p2InitialCard = fullCardList[0]; // Guard (3rd G)
            Card p1Turn1Draw = fullCardList[12];  // Guard (2nd G)
            // Card p2Turn1Draw = fullCardList[11]; // Guard (1st G) - will be drawn after P1 plays

            // 3. Create Player States
            var aliceId = Guid.NewGuid();
            var bobId = Guid.NewGuid();

            // Player 1 (Alice) State: Active, 0 tokens, Hand{Guard(14), Guard(12)}
            var aliceHand = Hand.Load(new List<Card> { p1InitialCard, p1Turn1Draw });
            var alice = Player.Load(
                id: aliceId,
                name: "Alice",
                status: PlayerStatus.Active,
                hand: aliceHand,
                playedCards: new List<CardType>(), // No cards played yet
                tokensWon: 0, // Starts with 0 tokens
                isProtected: false
            );

            // Player 2 (Bob) State: Active, 0 tokens, Hand{Guard(13)}
            var bobHand = Hand.Load(new List<Card> { p2InitialCard });
            var bob = Player.Load(
                id: bobId,
                name: "Bob",
                status: PlayerStatus.Active,
                hand: bobHand,
                playedCards: new List<CardType>(),
                tokensWon: 0,
                isProtected: false
            );

            var players = new List<Player> { alice, bob };

            // 4. Create Deck State (Remaining cards after deal + P1 draw)
            // Cards remaining: Princess(0)...Guard(11)
            // Order for Deck.Load needs to be reverse draw order (top card last)
            var remainingCardsForDeck = fullCardList.Take(12).ToList(); // Indices 0 through 11
            var deck = Deck.Load(remainingCardsForDeck);

            // 5. Create Discard Pile State
            var discardPile = new List<Card>(); // Empty at start of P1's turn

            // 6. Load the Game aggregate
            var game = Game.Game.Load(
                id: gameId,
                roundNumber: 1,
                gamePhase: GamePhase.RoundInProgress,
                currentTurnPlayerId: aliceId, // Alice's turn
                players: players,
                deck: deck,
                setAsideCard: setAsideCard,
                discardPile: discardPile,
                tokensToWin: tokensToWin
            );

            // Get one of Alice's actual Guard cards from her hand
            Card cardToPlay = alice.Hand.GetCards().First(c => c.Type == CardType.Guard);

            var guessedCardType = CardType.Princess;
            game.PlayCard(aliceId, cardToPlay, bobId, guessedCardType); 

            // ASSERT

            // 1. Game Phase should be GameOver
            game.GamePhase.Should().Be(GamePhase.GameOver);

            // 2. Player 2 should be eliminated (from the Guard guess)
            bob.Status.Should().Be(PlayerStatus.Eliminated);

            // 3. Player 1 should have won 1 token
            alice.Status.Should().Be(PlayerStatus.Active); // Still active when round ended
            alice.TokensWon.Should().Be(1);

            // 4. Correct domain events should have been raised by the PlayCard action sequence
            var events = game.DomainEvents.ToList();

            // Check sequence and details (order matters here)
            events.Should().HaveCount(6, "Expected 6 events: PlayCard, GuardResult, Eliminate, RoundEnd, TokenAward, GameEnd");

            // Event 1: PlayerPlayedCard
            events[0].Should().BeOfType<PlayerPlayedCard>().Which.Should().BeEquivalentTo(
                new PlayerPlayedCard(game.Id, alice.Id, CardType.Guard, bobId, guessedCardType),
                options => options.ExcludingMissingMembers()
                    .Excluding(ev=>ev.EventId)
                    .Excluding(ev=>ev.OccurredOn)); // Exclude EventId, OccurredOn etc.

            // Event 3: PlayerEliminated (Player 2 eliminated by Guard)
            events[1].Should().BeOfType<PlayerEliminated>().Which.Should().BeEquivalentTo(
                new PlayerEliminated(game.Id, bobId, $"guessed correctly by {alice.Name} with a Guard", CardType.Guard),
                options => options.ExcludingMissingMembers()
                    .Excluding(ev=>ev.EventId)
                    .Excluding(ev=>ev.OccurredOn));
            
            // Event 2: GuardGuessResult (Correct guess)
            events[2].Should().BeOfType<GuardGuessResult>().Which.Should().BeEquivalentTo(
                new GuardGuessResult(game.Id, aliceId, bobId, guessedCardType, true), // WasCorrect = true
                options => options.ExcludingMissingMembers()
                    .Excluding(ev=>ev.EventId)
                    .Excluding(ev=>ev.OccurredOn));

            // Event 4: RoundEnded (Player 1 wins - last standing)
            var roundEndedEvent = events[3].Should().BeOfType<RoundEnded>().Subject;
            roundEndedEvent.WinnerPlayerId.Should().Be(aliceId);
            roundEndedEvent.Reason.Should().Be("Last player standing");
            // Could check FinalHands if needed

            // Event 5: TokenAwarded (Player 1 gets 1 token)
            events[4].Should().BeOfType<TokenAwarded>().Which.Should().BeEquivalentTo(
                new TokenAwarded(game.Id, aliceId, 1), // NewTokenCount = 1
                options => options.ExcludingMissingMembers()
                    .Excluding(ev=>ev.EventId)
                    .Excluding(ev=>ev.OccurredOn));

            // Event 6: GameEnded (Player 1 wins the game)
            events[5].Should().BeOfType<GameEnded>().Which.Should().BeEquivalentTo(
                new GameEnded(game.Id, aliceId), // WinnerPlayerId = player1.Id
                options => options.ExcludingMissingMembers()
                    .Excluding(ev=>ev.EventId)
                    .Excluding(ev=>ev.OccurredOn));

        }

        [Test]
        public void PlayCard_WhenPlayerPlaysAndNextPlayerCannotDraw_ShouldEndRoundAndDeclareWinnerByHighestRank()
        {
            // ARRANGE
            Guid gameId = Guid.NewGuid();
            int tokensToWin = 4;
            var p1Id = Guid.NewGuid();
            var p2Id = Guid.NewGuid();
            var p1Card_King = new Card(Guid.NewGuid(), CardType.King);
            var p1Card_Handmaid = new Card(Guid.NewGuid(), CardType.Handmaid);
            var p2Card_Princess = new Card(Guid.NewGuid(), CardType.Princess);
            var aliceHand = Hand.Load(new List<Card> {p1Card_King, p1Card_Handmaid});
            var alice = Player.Load(p1Id, "Alice", PlayerStatus.Active, aliceHand, new List<CardType>(), 0, false);
            var bobHand = Hand.Load(new List<Card> {p2Card_Princess});
            var bob = Player.Load(p2Id, "Bob", PlayerStatus.Active, bobHand, new List<CardType>(), 0, false);
            var players = new List<Player> {alice, bob};
            var deck = Deck.Load(Enumerable.Empty<Card>());
            var discardPile = new List<Card>();
            var game = Game.Game.Load(gameId, 1, GamePhase.RoundInProgress, p1Id, players, deck, null, discardPile,
                tokensToWin);
            var cardToPlayInstance = p1Card_Handmaid;
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

                events.Should().ContainSingle(e => e is PlayerPlayedCard).Which.Should().BeEquivalentTo(
                    new PlayerPlayedCard(game.Id, p1Id, CardType.Handmaid, null, null),
                    ExcludeEventMetadata<PlayerPlayedCard>());

                events.Should().ContainSingle(e => e is HandmaidProtectionSet).Which.Should().BeEquivalentTo(
                    new HandmaidProtectionSet(game.Id, p1Id),
                    ExcludeEventMetadata<HandmaidProtectionSet>());

                var expectedFinalHands = new Dictionary<Guid, CardType?>
                    {{p1Id, CardType.King}, {p2Id, CardType.Princess}};
                events.Should().ContainSingle(e => e is RoundEnded).Which.Should().BeEquivalentTo(
                    new RoundEnded(game.Id, p2Id, expectedFinalHands, "Deck empty, highest card wins"),
                    ExcludeEventMetadata<RoundEnded>());
                events.OfType<RoundEnded>().Single().WinnerPlayerId.Should().Be(p2Id); // Verify winner separately

                events.Should().ContainSingle(e => e is TokenAwarded).Which.Should().BeEquivalentTo(
                    new TokenAwarded(game.Id, p2Id, 1),
                    ExcludeEventMetadata<TokenAwarded>());

                events.Should().NotContain(e => e is GameEnded);
                events.Should().NotContain(e => e is TurnStarted && ((TurnStarted) e).PlayerId == p2Id);
                events.Should().NotContain(e => e is PlayerDrewCard && ((PlayerDrewCard) e).PlayerId == p2Id);
                events.Should().NotContain(e => e is DeckChanged);
            }
        }
        
        private static Func<EquivalencyOptions<T>, EquivalencyOptions<T>> ExcludeEventMetadata<T>() where T : IDomainEvent
        {
            return options => options
                .Excluding(ev => ev.EventId)
                .Excluding(ev => ev.OccurredOn)
                .Excluding(ev => ev.CorrelationId);
        }


    }