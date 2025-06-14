using CardGame.Domain.Game; // For Game, Player, Card, Deck, Hand
using CardGame.Domain.Game.Event; // For Domain Events like PlayerEliminatedEvent
using CardGame.Domain.Interfaces; // For IGameOperations, IDeckProvider
using CardGame.Domain.Tests.TestDoubles;
using CardGame.Decks.BaseGame; // For CardRank
using FluentAssertions;
using CardGame.Domain.Types; // Explicitly for PlayerInfo, PlayerStatus, GamePhase

namespace CardGame.Domain.Tests;

using TestedGame = CardGame.Domain.Game.Game;

[TestFixture]
public class GameTests
{
    private static FakeLoggerFactory _fakeLoggerFactory = null!;
    private static NonShufflingRandomizer _nonShufflingRandomizer = null!;
    private static FakeDeckProvider _defaultDeckProvider = null!;
    private static List<PlayerInfo> _twoPlayerInfos = null!;
    private static List<PlayerInfo> _threePlayerInfos = null!;
    private static List<PlayerInfo> _fourPlayerInfos = null!;

    // Canonical Rank Definitions for tests
    private static readonly RankDefinition _guardRankDef = new(new Guid("10000000-0000-0000-0000-000000000001"), CardRank.Guard.Value);
    private static readonly RankDefinition _priestRankDef = new(new Guid("10000000-0000-0000-0000-000000000002"), CardRank.Priest.Value);
    private static readonly RankDefinition _baronRankDef = new(new Guid("10000000-0000-0000-0000-000000000003"), CardRank.Baron.Value);
    private static readonly RankDefinition _handmaidRankDef = new(new Guid("10000000-0000-0000-0000-000000000004"), CardRank.Handmaid.Value);
    private static readonly RankDefinition _princeRankDef = new(new Guid("10000000-0000-0000-0000-000000000005"), CardRank.Prince.Value);
    private static readonly RankDefinition _kingRankDef = new(new Guid("10000000-0000-0000-0000-000000000006"), CardRank.King.Value);
    private static readonly RankDefinition _countessRankDef = new(new Guid("10000000-0000-0000-0000-000000000007"), CardRank.Countess.Value);
    private static readonly RankDefinition _princessRankDef = new(new Guid("10000000-0000-0000-0000-000000000008"), CardRank.Princess.Value);

    // List of all unique rank definitions used in tests
    private static readonly List<RankDefinition> _allTestRankDefinitions = new List<RankDefinition>
    {
        _guardRankDef, _priestRankDef, _baronRankDef, _handmaidRankDef,
        _princeRankDef, _kingRankDef, _countessRankDef, _princessRankDef
    };

    // Canonical Card instances for tests
    private static readonly Card _g1 = new(_guardRankDef, "g1");
    private static readonly Card _g2 = new(_guardRankDef, "g2");
    private static readonly Card _p1 = new(_priestRankDef, "p1");
    private static readonly Card _p2 = new(_priestRankDef, "p2");
    private static readonly Card _b1 = new(_baronRankDef, "b1");
    private static readonly Card _b2 = new(_baronRankDef, "b2");
    private static readonly Card _h1 = new(_handmaidRankDef, "h1");
    private static readonly Card _h2 = new(_handmaidRankDef, "h2");
    private static readonly Card _prince1 = new(_princeRankDef, "prince1");
    private static readonly Card _prince2 = new(_princeRankDef, "prince2");
    private static readonly Card _k = new(_kingRankDef, "k");
    private static readonly Card _c = new(_countessRankDef, "c");
    private static readonly Card _princess = new(_princessRankDef, "princess");

    private static List<Card> _defaultTestDeckCards = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _fakeLoggerFactory = new FakeLoggerFactory();
        _nonShufflingRandomizer = new NonShufflingRandomizer();
        
        _twoPlayerInfos = new List<PlayerInfo> { new PlayerInfo(new Guid("00000000-0000-0000-0000-FFFFEE000001"), "Player1"), new PlayerInfo(new Guid("00000000-0000-0000-0000-FFFFEE000002"), "Player2") };
        _threePlayerInfos = new List<PlayerInfo> { _twoPlayerInfos[0], _twoPlayerInfos[1], new PlayerInfo(new Guid("00000000-0000-0000-0000-FFFFEE000003"), "Player3") };
        _fourPlayerInfos = new List<PlayerInfo> { _threePlayerInfos[0], _threePlayerInfos[1], _threePlayerInfos[2], new PlayerInfo(new Guid("00000000-0000-0000-0000-FFFFEE000004"), "Player4") };

        _defaultTestDeckCards = new List<Card> { _g1, _g2, _p1, _p2, _b1, _b2, _h1, _h2, _prince1, _prince2, _k, _c, _princess };
        // Default provider for tests that don't need custom card effects or specific deck setups beyond the default.
        _defaultDeckProvider = new FakeDeckProvider(_defaultTestDeckCards, _allTestRankDefinitions, _fakeLoggerFactory);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _fakeLoggerFactory?.Dispose();
    }

    private (TestedGame game, List<Player> players, IGameOperations operations) CreateTestGame(
        List<PlayerInfo>? playerInfos = null,
        int tokensToWin = 4,
        List<Card>? deckCards = null, 
        Action<IGameOperations, Player, Card, Player?, int?>? cardEffectAction = null) 
    {
        var pInfos = playerInfos ?? _twoPlayerInfos;
        var cardsForDeck = deckCards ?? _defaultTestDeckCards;

        if (!cardsForDeck.Any())
        {
            // Ensure there's at least one card for the game to start if a custom empty list was somehow passed.
            cardsForDeck = new List<Card> { new Card(_guardRankDef, "fallback_g1_crt_game") }; 
        }

        // Instantiate FakeDeckProvider with all necessary parameters, including the cardEffectAction
        var currentTestDeckProvider = new FakeDeckProvider(
            new List<Card>(cardsForDeck), // The specific cards for this game's deck
            _allTestRankDefinitions,      // All possible rank definitions this provider knows
            _fakeLoggerFactory,           // Logger factory
            cardEffectAction              // The card effect action for this test
        );

        var game = TestedGame.CreateNewGame(
            currentTestDeckProvider.DeckId, // Game uses DeckId from provider
            pInfos,
            pInfos.First().Id, // Creator ID
            cardsForDeck,      // Initial cards for the game's first deck instance
            _fakeLoggerFactory,
            currentTestDeckProvider, // The IDeckProvider instance
            tokensToWin,
            _nonShufflingRandomizer
        );
        return (game, game.Players, (IGameOperations)game);
    }

    [Test]
    public void CreateNewGame_WithValidPlayers_InitializesCorrectly()
    {
        var (game, players, _) = CreateTestGame();

        game.Should().NotBeNull();
        players.Count.Should().Be(2);
        game.GamePhase.Should().Be(GamePhase.RoundInProgress);
        game.RoundNumber.Should().Be(1);
        game.TokensNeededToWin.Should().Be(4);
        game.DomainEvents.Should().ContainSingle(e => e is GameCreated);
        game.DomainEvents.Should().ContainSingle(e => e is RoundStarted);
        
        var firstPlayer = players.Single(p => p.Id == game.CurrentTurnPlayerId);
        var otherPlayer = players.Single(p => p.Id != game.CurrentTurnPlayerId);

        firstPlayer.Hand.Cards.Should().HaveCount(2);
        otherPlayer.Hand.Cards.Should().HaveCount(1);
    }

    [Test]
    public void PlayCard_PlayerNotInTurn_ThrowsException()
    {
        // Arrange
        var p1InitialCardToPlay = new Card(_guardRankDef, "g1_nit_hand");     // P1 should play this Guard first
        var p1CardToPlayOutOfTurn = new Card(_priestRankDef, "p1_nit_oops"); // P1 should attempt to play this Priest out of turn
        var p2InitialCardInHand = new Card(_baronRankDef, "b2_nit_hand");

        var saCard1_TopOfDeck = new Card(_kingRankDef, "sa1_nit_deck_top");       // Will be drawn 1st (Public Set Aside 1)
        var saCard2_MidDeck = new Card(_countessRankDef, "sa2_nit_deck_mid");   // Will be drawn 2nd (Public Set Aside 2)
        var saCard3_LowDeck = new Card(_princessRankDef, "sa3_nit_deck_low");   // Will be drawn 3rd (Public Set Aside 3)

        // Deck setup for 2 players (1 private set-aside, 1 public set-aside):
        // Stack (top to bottom) -> Draw order:
        // 1. saCard3_LowDeck (Princess)    - Privately Set Aside
        // 2. saCard1_TopOfDeck (King)      - Publicly Set Aside
        // 3. p1InitialCardToPlay (Guard)   - P1 Initial Hand
        // 4. p2InitialCardInHand (Baron)   - P2 Initial Hand
        // 5. p1CardToPlayOutOfTurn (Priest)- P1 Draws on Turn
        // 6. saCard2_MidDeck (Countess)    - Remains in deck (bottom)
        //
        // List for ImmutableStack.CreateRange (last element is top of stack):
        var deckForNotInTurn = new List<Card>
        {
            saCard2_MidDeck,        // Bottom of stack
            p1CardToPlayOutOfTurn, 
            p2InitialCardInHand,    
            p1InitialCardToPlay,    
            saCard1_TopOfDeck,      
            saCard3_LowDeck         // Top of stack
        };

        var (game, players, operations) = CreateTestGame(deckCards: deckForNotInTurn, playerInfos: _twoPlayerInfos);
        var player1 = players.Single(p => p.Id == _twoPlayerInfos[0].Id);
        var player2 = players.Single(p => p.Id == _twoPlayerInfos[1].Id);

        // P1's turn: P1 should have p1InitialCardToPlay and p1CardToPlayOutOfTurn
        // P1 plays their initial card (Guard)
        // The card P1 was dealt as their first card is p1InitialCardToPlay
        var cardForP1ToPlayFirst = player1.Hand.Cards.First();
        game.PlayCard(player1.Id, cardForP1ToPlayFirst, player2.Id, _baronRankDef.Value); // P1 plays Guard, guesses Baron on P2

        // Re-fetch player1 to get the updated hand state after playing a card
        player1 = game.Players.Single(p => p.Id == _twoPlayerInfos[0].Id);

        // P2's turn (simulated by advancing turn if necessary, though for this test, P1 just tries to play again)
        // The AdvanceTurn() in PlayCard should handle making it P2's turn.

        // Act & Assert: P1 tries to play their other card (Priest) when it's not their turn.
        // The card P1 drew for their turn is p1CardToPlayOutOfTurn
        var cardForP1ToPlayOutOfTurnAttempt = player1.Hand.Cards.First();
        Action act = () => game.PlayCard(player2.Id, cardForP1ToPlayOutOfTurnAttempt, player1.Id, null);
        act.Should().Throw<Exception>("because Player 2 is not the current turn player.");
    }

    [Test]
    public void PlayCard_PlayerPlaysCardNotInHand_ThrowsException()
    {
        // Arrange
        var (game, players, operations) = CreateTestGame();
        var player1 = players.Single(p => p.Id == _twoPlayerInfos[0].Id);
        var player2 = players.Single(p => p.Id == _twoPlayerInfos[1].Id);
        var cardNotInHand = new Card(_princessRankDef, "fake_princess"); // A card not in P1's hand

        // Act & Assert
        Action action = () => game.PlayCard(player1.Id, cardNotInHand, player2.Id, null);
        action.Should().Throw<Exception>("because Player 1 does not have the specified card in hand.");
    }

    [Test]
    public void PlayCard_DeckEmptiesOnNextPlayerTurnDraw_RoundContinuesUntilPlay()
    {
        // Arrange
        // Deck: P2Draw (last card), P1Play, P2Initial, P1Initial, SA1, SA2, SA3
        var p2DrawCard_LastInDeck = new Card(_princeRankDef, "p2_draw_last");
        var p1PlayCard = new Card(_guardRankDef, "p1_play_g");
        var p2InitialCard = new Card(_baronRankDef, "p2_init_b");
        var p1InitialCard = new Card(_priestRankDef, "p1_init_p");
        
        var initialDeck = new List<Card>
        {
            p2DrawCard_LastInDeck, // This will be drawn by P2 when their turn starts
            p1PlayCard,          // This will be drawn by P1 for their first turn
            p2InitialCard,       // P2's initial hand card
            p1InitialCard,       // P1's initial hand card
            new Card(_kingRankDef, "sa1_ed"),
            new Card(_countessRankDef, "sa2_ed"),
            new Card(_princessRankDef, "sa3_ed")
        };

        var (game, players, operations) = CreateTestGame(deckCards: initialDeck, tokensToWin: 2);
        var player1 = players.Single(p => p.Id == _twoPlayerInfos[0].Id);
        var player2 = players.Single(p => p.Id == _twoPlayerInfos[1].Id);

        // P1's hand: [p1InitialCard, p1PlayCard]. P1 plays p1PlayCard.
        var cardToPlayP1 = player1.Hand.Cards.Single(c => c.Equals(p1PlayCard));
        game.PlayCard(player1.Id, cardToPlayP1, player2.Id, null);

        // Now it's P2's turn. Deck has 1 card (p2DrawCard_LastInDeck). P2 will draw it.
        // Act: No explicit action needed as P2 drawing happens when their turn begins, which is implicitly handled by PlayCard advancing turn.
        // We need to check P2's hand *after* their turn has effectively started (which means after P1's PlayCard completes and turn advances)
        
        // Assert
        var player2AfterP1Play = game.Players.Single(p => p.Id == _twoPlayerInfos[1].Id);
        player2AfterP1Play.Hand.Cards.Should().HaveCount(2, "Player 2 should have drawn the last card and now have 2 cards.");
        player2AfterP1Play.Hand.Cards.Should().Contain(p2DrawCard_LastInDeck);
        player2AfterP1Play.Hand.Cards.Should().Contain(p2InitialCard);
        
        game.Deck.CardsRemaining.Should().Be(0, "Deck should be empty after P2 draws.");
        game.GamePhase.Should().Be(GamePhase.RoundInProgress, "Round should continue as P2 can still play.");
        game.CurrentTurnPlayerId.Should().Be(player2.Id, "It should be Player 2's turn.");
    }

    [Test]
    public void CreateNewGame_WithThreePlayers_InitializesCorrectly()
    {
        // Arrange
        var threePlayerInfos = new List<PlayerInfo>(_twoPlayerInfos)
        {
            new PlayerInfo(new Guid("00000000-0000-0000-0000-FFFFEE000003"), "Player 3")
        };
        var (game, players, operations) = CreateTestGame(playerInfos: threePlayerInfos, deckCards: _defaultTestDeckCards.ToList());

        // Assert
        game.Players.Should().HaveCount(3);
        game.GamePhase.Should().Be(GamePhase.RoundInProgress);
        game.RoundNumber.Should().Be(1);
        game.PubliclySetAsideCards.Should().BeEmpty("For 3+ players, no cards are publicly set aside.");
        game.SetAsideCard.Should().NotBeNull("For 3+ players, one card should be set aside privately.");

        var player1 = players.Single(p => p.Id == _twoPlayerInfos[0].Id);
        var player2 = players.Single(p => p.Id == _twoPlayerInfos[1].Id);
        var player3 = players.Single(p => p.Id == new Guid("00000000-0000-0000-0000-FFFFEE000003"));

        // Each player gets 1 card initially, current player draws 1 more
        player1.Hand.Cards.Should().HaveCount(2, "Player 1 is current turn player and should have 2 cards");
        player2.Hand.Cards.Should().HaveCount(1);
        player3.Hand.Cards.Should().HaveCount(1);

        var totalCardsInHands = players.Sum(p => p.Hand.Cards.Count);
        var expectedDeckRemaining = _defaultTestDeckCards.Count - totalCardsInHands - (game.SetAsideCard != null ? 1 : 0) - game.PubliclySetAsideCards.Count;
        game.Deck.CardsRemaining.Should().Be(expectedDeckRemaining);
    }

    [Test]
    public void CreateNewGame_WithFourPlayers_InitializesCorrectly()
    {
        // Arrange
        var fourPlayerInfos = new List<PlayerInfo>(_twoPlayerInfos)
        {
            new PlayerInfo(new Guid("00000000-0000-0000-0000-FFFFEE000003"), "Player 3"),
            new PlayerInfo(new Guid("00000000-0000-0000-0000-FFFFEE000004"), "Player 4")
        };
        var (game, players, operations) = CreateTestGame(playerInfos: fourPlayerInfos, deckCards: _defaultTestDeckCards.ToList());

        // Assert
        game.Players.Should().HaveCount(4);
        game.GamePhase.Should().Be(GamePhase.RoundInProgress);
        game.RoundNumber.Should().Be(1);
        game.PubliclySetAsideCards.Should().BeEmpty("For 4 players, no cards are publicly set aside.");
        game.SetAsideCard.Should().NotBeNull("For 4 players, one card should be set aside privately.");

        var player1 = players.Single(p => p.Id == _twoPlayerInfos[0].Id);
        var player2 = players.Single(p => p.Id == _twoPlayerInfos[1].Id);
        var player3 = players.Single(p => p.Id == new Guid("00000000-0000-0000-0000-FFFFEE000003"));
        var player4 = players.Single(p => p.Id == new Guid("00000000-0000-0000-0000-FFFFEE000004"));

        // Each player gets 1 card initially, current player draws 1 more
        player1.Hand.Cards.Should().HaveCount(2, "Player 1 is current turn player and should have 2 cards");
        player2.Hand.Cards.Should().HaveCount(1);
        player3.Hand.Cards.Should().HaveCount(1);
        player4.Hand.Cards.Should().HaveCount(1);

        var totalCardsInHands = players.Sum(p => p.Hand.Cards.Count);
        var expectedDeckRemaining = _defaultTestDeckCards.Count - totalCardsInHands - (game.SetAsideCard != null ? 1 : 0) - game.PubliclySetAsideCards.Count;
        game.Deck.CardsRemaining.Should().Be(expectedDeckRemaining);
    }

    [Test]
    public void PlayCard_LeadsToLastPlayerStanding_EndsRoundAndAwardsToken()
    {
        // Arrange
        var player1Info = _twoPlayerInfos[0];
        var player2Info = _twoPlayerInfos[1];
        var pInfos = new List<PlayerInfo> { player1Info, player2Info };

        // Deck: P1 draws Guard, P2 draws Priest. P1 will play Guard to eliminate P2.
        // Order for ImmutableStack: last item is top. So P1 draws G1, P2 draws P1.
        // Initial deal: P1 gets CardA (and one set aside). P2 gets CardB. P1 draws CardC.
        // P1 Hand: [CardA, CardC], P2 Hand: [CardB]
        // To make P1 draw G1 (to play) and P2 have P1 (to be eliminated with no effect):
        // SetAside: X (irrelevant for this test, e.g. _princess)
        // P2 Hand (1st card dealt to P2): _p1 (Priest)
        // P1 Hand (1st card dealt to P1): _b1 (Baron, irrelevant, will be kept)
        // P1 Draw (card P1 draws on their turn): _g1 (Guard, to be played)
        // Remaining deck (bottom up): ..., _princess (set aside), _p1 (to P2), _b1 (to P1), _g1 (P1 draws)
        var deckCards = new List<Card> { _h1, _h2, _g1, _p1, _b1, _princess }; // P1 draws _g1, P2 has _p1, P1 has _b1
        
        var (game, players, operations) = CreateTestGame(
            playerInfos: pInfos,
            tokensToWin: 1, // Game ends after this round
            deckCards: deckCards,
            cardEffectAction: (gameOps, actingPlayer, cardPlayed, targetPlayer, guessedRank) =>
            {
                // Ensure the targetPlayer (player2) is eliminated.
                if (targetPlayer != null && cardPlayed.Rank.Value == CardRank.Guard.Value)
                {
                    // The gameId for EliminatePlayer should come from the game instance itself (gameOps)
                    gameOps.EliminatePlayer(targetPlayer.Id, "Guard effect in test");
                }
            }
        );

        var player1 = game.Players.Single(p => p.Id == player1Info.Id);
        var player2 = game.Players.Single(p => p.Id == player2Info.Id);

        // Act
        // Player1 plays Guard (_g1), targeting Player2.
        // Player1's hand before play should be [_b1, _g1]. They play _g1.
        var cardToPlay = player1.Hand.Cards.Single(c => c.AppearanceId == _g1.AppearanceId); 
        game.PlayCard(player1.Id, cardToPlay, player2.Id, null); // No guess needed for Guard effect here

        // Assert
        var updatedPlayer1 = game.Players.Single(p => p.Id == player1Info.Id);
        var updatedPlayer2 = game.Players.Single(p => p.Id == player2Info.Id);

        updatedPlayer2.Status.Should().Be(PlayerStatus.Eliminated);
        game.DomainEvents.OfType<PlayerEliminated>().Should().ContainSingle(pe => pe.PlayerId == player2.Id);
        game.GamePhase.Should().Be(GamePhase.GameOver);
        game.DomainEvents.Should().ContainSingle(e => e is RoundEnded);
        updatedPlayer1.TokensWon.Should().Be(1);
        game.DomainEvents.Should().ContainSingle(e => e is GameEnded); // Since tokensToWin is 1
        game.LastRoundWinnerId.Should().Be(player1.Id);
    }

    [Test]
    public void PlayCard_AwardsFinalToken_EndsGameAndDeclaresWinner()
    {
        // Arrange
        var player1Info = _twoPlayerInfos[0];
        var player2Info = _twoPlayerInfos[1];
        var pInfos = new List<PlayerInfo> { player1Info, player2Info };

        // Deck is reversed due to ImmutableStack in Deck creation.
        // Goal: P1 hand: {_b1, _g1}, P2 hand: {_p1}, Set-aside: _h1. P1 plays _g1.
        // Effective draw order (from stack top): _h1 (set-aside), _b1 (P1), _p1 (P2), _g1 (P1 draws).
        // Input list for this (reversed): _princess, _h2 (deck), _g1, _p1, _b1, _h1
        var deckCards = new List<Card> { _princess, _h2, _g1, _p1, _b1, _h1 };

        var (game, players, operations) = CreateTestGame(
            playerInfos: pInfos,
            tokensToWin: 1, // Player needs 1 token to win.
            deckCards: deckCards,
            cardEffectAction: (gameOps, actingPlayer, cardPlayed, targetPlayer, guessedRank) =>
            {
                if (targetPlayer != null && cardPlayed.Rank.Value == CardRank.Guard.Value)
                {
                    gameOps.EliminatePlayer(targetPlayer.Id, "Guard effect for final token");
                }
            }
        );

        var player1 = game.Players.Single(p => p.Id == player1Info.Id);
        var player2 = game.Players.Single(p => p.Id == player2Info.Id);
        // Player1 starts with 0 tokens.

        // Act
        // Player1 plays Guard (_g1), targeting Player2.
        var cardToPlay = player1.Hand.Cards.Single(c => c.AppearanceId == _g1.AppearanceId);
        game.PlayCard(player1.Id, cardToPlay, player2.Id, null);

        // Assert
        var updatedPlayer1 = game.Players.Single(p => p.Id == player1Info.Id);
        var updatedPlayer2 = game.Players.Single(p => p.Id == player2Info.Id);

        updatedPlayer2.Status.Should().Be(PlayerStatus.Eliminated);
        updatedPlayer1.TokensWon.Should().Be(1); // Player1 gets the token
        game.GamePhase.Should().Be(GamePhase.GameOver); // Game should be over
        game.DomainEvents.OfType<GameEnded>().Should().ContainSingle(ge => ge.WinnerPlayerId == player1.Id);
        game.LastRoundWinnerId.Should().Be(player1.Id);
    }

    [Test]
    public void PlayerElimination_ReducesActivePlayerCount_AndCanEndRound()
    {
        var p1Info = _threePlayerInfos[0];
        var p2Info = _threePlayerInfos[1];
        var p3Info = _threePlayerInfos[2];

        var initialDeck = new List<Card> { _g1, _g2, _p1, _p2, _b1, _b2, _h1 };
        initialDeck.Reverse(); // Reverse in place

        Action<IGameOperations, Player, Card, Player?, int?> cardEffectAction = (gameOps, actingPlayer, playedCard, targetPlayer, chosenRank) =>
        {
            if (targetPlayer != null) gameOps.EliminatePlayer(targetPlayer.Id, "TestEliminationIn3P");
        };

        var (game, players, operations) = CreateTestGame(
            playerInfos: new List<PlayerInfo> { p1Info, p2Info, p3Info },
            tokensToWin: 2,
            deckCards: initialDeck,
            cardEffectAction: cardEffectAction
        );

        var player1 = game.Players.Single(p => p.Id == p1Info.Id);
        var player2 = game.Players.Single(p => p.Id == p2Info.Id);
        var player3 = game.Players.Single(p => p.Id == p3Info.Id);

        // Assume player1 is current, plays a card that eliminates player2
        // For simplicity, we'll directly call EliminatePlayer via the cardEffectAction by playing any card targeting player2
        var cardP1Plays = player1.Hand.Cards.First();
        game.PlayCard(player1.Id, cardP1Plays, player2.Id, null); // Target player2, effect will eliminate them

        var player2AfterPlay = game.Players.Single(p => p.Id == p2Info.Id);
        player2AfterPlay.Status.Should().Be(PlayerStatus.Eliminated);
        game.Players.Count(p => p.Status == PlayerStatus.Active).Should().Be(2);

        // Act: The final play that ends the round.
        // The turn has advanced. Find the current player and the last remaining target.
        var currentPlayer = game.Players.Single(p => p.Id == game.CurrentTurnPlayerId);
        var finalTarget = game.Players.Single(p => p.Status == PlayerStatus.Active && p.Id != currentPlayer.Id);
        var cardToPlay = currentPlayer.Hand.Cards.First();

        game.ClearDomainEvents(); // Clear events from the first elimination to isolate this action.
        game.PlayCard(currentPlayer.Id, cardToPlay, finalTarget.Id, null);

        // Assert: Check events for the round outcome and that a new round has begun.
        var roundEndedEvent = game.DomainEvents.OfType<RoundEnded>().Single();
        roundEndedEvent.WinnerPlayerId.Should().Be(currentPlayer.Id, "the last player standing should win the round");

        var playerEliminatedEvent = game.DomainEvents.OfType<PlayerEliminated>().Single();
        playerEliminatedEvent.PlayerId.Should().Be(finalTarget.Id, "the targeted player should be eliminated");

        // Assert the new game state
        game.GamePhase.Should().Be(GamePhase.RoundInProgress, "a new round should start automatically");
        game.RoundNumber.Should().Be(2);
        game.Players.Count(p => p.Status == PlayerStatus.Active).Should().Be(3, "all players should be active for the new round");

        var winnerData = game.Players.Single(p => p.Id == currentPlayer.Id);
        winnerData.TokensWon.Should().Be(1);
    }

    [Test]
    public void EliminatePlayer_RemovesPlayerFromActivePlayers()
    {
        var player1Info = _twoPlayerInfos[0];
        var player2Info = _twoPlayerInfos[1];
        var pInfos = new List<PlayerInfo> { player1Info, player2Info };

        var (game, players, operations) = CreateTestGame(playerInfos: pInfos);

        var playerToEliminate = players.Single(p => p.Id == player2Info.Id);
        var survivingPlayer = players.Single(p => p.Id == _twoPlayerInfos[0].Id);

        // Act
        Action act = () => game.EliminatePlayer(playerToEliminate.Id, "Test Elimination");
        act.Should().NotThrow();

        // Assert
        playerToEliminate.Status.Should().Be(PlayerStatus.Eliminated);
    }
}