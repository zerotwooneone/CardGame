using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Domain.Game;
using CardGame.Domain.Tests.TestDoubles;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using CardGame.Domain.Game.Event;
using Microsoft.Extensions.Logging;
using CardGame.Decks.BaseGame;

namespace CardGame.Domain.Tests;

using TestedGame = CardGame.Domain.Game.Game;

[TestFixture]
public class GameTests
{
    // Shared RankDefinitions for the base game
    private static readonly RankDefinition GuardRank = new(Guid.NewGuid(), CardRank.Guard.Value);
    private static readonly RankDefinition PriestRank = new(Guid.NewGuid(), CardRank.Priest.Value);
    private static readonly RankDefinition BaronRank = new(Guid.NewGuid(), CardRank.Baron.Value);
    private static readonly RankDefinition HandmaidRank = new(Guid.NewGuid(), CardRank.Handmaid.Value);
    private static readonly RankDefinition PrinceRank = new(Guid.NewGuid(), CardRank.Prince.Value);
    private static readonly RankDefinition KingRank = new(Guid.NewGuid(), CardRank.King.Value);
    private static readonly RankDefinition CountessRank = new(Guid.NewGuid(), CardRank.Countess.Value);
    private static readonly RankDefinition PrincessRank = new(Guid.NewGuid(), CardRank.Princess.Value);

    private FakeLoggerFactory _fakeLoggerFactory;
    private NonShufflingRandomizer _nonShufflingRandomizer;
    private FakeDeckProvider _fakeDeckProvider;
    private List<PlayerInfo> _twoPlayerInfos;
    private List<Card> _defaultTestDeckCards;

    [SetUp]
    public void SetUp()
    {
        _fakeLoggerFactory = new FakeLoggerFactory();
        _nonShufflingRandomizer = new NonShufflingRandomizer();

        _defaultTestDeckCards = new List<Card>
        {
            new Card(GuardRank, "g1"), 
            new Card(GuardRank, "g2"),
            new Card(PriestRank, "p1"),
            new Card(PrincessRank, "princess1"),
            new Card(GuardRank, "g3"),
            new Card(BaronRank, "b1")
        };

        _fakeDeckProvider = new FakeDeckProvider(new List<Card>(_defaultTestDeckCards));

        _twoPlayerInfos = new List<PlayerInfo>
        {
            new PlayerInfo(Guid.NewGuid(), "Player 1"),
            new PlayerInfo(Guid.NewGuid(), "Player 2")
        };
    }
    
    [TearDown]
    public void TearDown()
    {
        _fakeLoggerFactory.Dispose();
    }

    private (TestedGame game, List<Player> players, IGameOperations operations) CreateTestGame(
        List<PlayerInfo>? playerInfos = null,
        int tokensToWin = 4,
        FakeDeckProvider? deckProvider = null,
        List<Card>? initialDeck = null)
    {
        var pInfos = playerInfos ?? _twoPlayerInfos;
        var providerToUse = deckProvider ?? _fakeDeckProvider;
        var cardsToUse = initialDeck ?? providerToUse.GetDeck().Cards.ToList();
        
        if (!cardsToUse.Any())
        {
            cardsToUse = new List<Card> { new Card(GuardRank, "fallback_g") };
        }

        var game = TestedGame.CreateNewGame(
            providerToUse.DeckId,
            pInfos,
            pInfos.First().Id,
            cardsToUse,
            _fakeLoggerFactory,
            providerToUse,
            tokensToWin,
            _nonShufflingRandomizer
        );
        return (game, game.Players, (IGameOperations)game);
    }

    [Test]
    public void CreateNewGame_WithValidPlayers_InitializesCorrectly()
    {
        // Arrange & Act
        var (game, players, _) = CreateTestGame();

        // Assert
        game.Should().NotBeNull();
        players.Count.Should().Be(2);
        game.GamePhase.Should().Be(GamePhase.RoundInProgress);
        game.RoundNumber.Should().Be(1);
        game.TokensNeededToWin.Should().Be(4);
        game.DomainEvents.OfType<GameCreated>().Should().HaveCount(1);
        game.DomainEvents.OfType<RoundStarted>().Should().HaveCount(1);
        
        // Check player hands - each should have 1 card initially, then first player draws 1 more.
        var firstPlayer = players.Single(p => p.Id == game.CurrentTurnPlayerId);
        var otherPlayer = players.Single(p => p.Id != game.CurrentTurnPlayerId);

        firstPlayer.Hand.Cards.Should().HaveCount(2);
        otherPlayer.Hand.Cards.Should().HaveCount(1);
    }

    [Test]
    public void PlayCard_FirstPlay_WithEmptyDeck_EndsRoundAndStartsNewRound()
    {
        // Arrange
        // Cards are defined in the order they will be in the initial list for FakeDeckProvider.
        // NonShufflingRandomizer reverses this list, so drawing happens from the 'end' of this defined list.
        var card_Priest_R2 = new Card(PriestRank, "sa_priest");    // Will be P1's 2nd card (turn draw)
        var card_Baron_R3 = new Card(BaronRank, "sa_baron");       // Will be P2's hand card
        var card_Handmaid_R4 = new Card(HandmaidRank, "sa_handmaid"); // Will be P1's 1st card
        var card_Prince_R5 = new Card(PrinceRank, "p1_prince");    // Will be set aside (3rd)
        var card_King_R6 = new Card(KingRank, "p2_king");          // Will be set aside (2nd)
        var card_Countess_R7 = new Card(CountessRank, "p1_countess"); // Will be set aside (1st)

        var initialDeckCards = new List<Card> 
        {
            // Order for FakeDeckProvider (draw order is reverse of this)
            card_Priest_R2, card_Baron_R3, card_Handmaid_R4, 
            card_Prince_R5, card_King_R6, card_Countess_R7
        };

        var testDeckProvider = new FakeDeckProvider(new List<Card>(initialDeckCards));
        testDeckProvider.CardEffectAction = (g, ap, c, tp, gr) => { /* No-op card effect */ };

        var (game, players, operations) = CreateTestGame(
            deckProvider: testDeckProvider, 
            tokensToWin: 2
        );

        var player1 = players.Single(p => p.Id == game.CurrentTurnPlayerId);
        var player2 = players.Single(p => p.Id != game.CurrentTurnPlayerId);

        // Actual hands after setup (2 players, 3 set aside, 1 each, P1 draws 1):
        // P1 Hand: [Handmaid (R4), Priest (R2)]
        // P2 Hand: [Baron (R3)]
        // Set Aside: [Countess (R7), King (R6), Prince (R5)]

        var cardToPlay = player1.Hand.Cards.First(c => c.Rank.Value == CardRank.Handmaid.Value); // P1 plays Handmaid (R4)
        var player1ExpectedRemainingCard = card_Priest_R2; // P1's remaining card is Priest (R2)
        var player2ExpectedCard = card_Baron_R3;          // P2's card is Baron (R3)

        // Act
        game.PlayCard(player1.Id, cardToPlay, null, null);

        // Assert
        // Get player states after PlayCard completes (which includes starting Round 2)
        var player1AfterPlay = game.Players.Single(p => p.Id == player1.Id); 
        var player2AfterPlay = game.Players.Single(p => p.Id == player2.Id);

        // Assert Tokens (cumulative state after Round 1)
        player1AfterPlay.TokensWon.Should().Be(0, "Player 1 lost Round 1.");
        player2AfterPlay.TokensWon.Should().Be(1, "Player 2 won Round 1.");

        // Game state after Round 1 ended and Round 2 started
        game.GamePhase.Should().Be(GamePhase.RoundInProgress, "A new round should have started.");
        game.RoundNumber.Should().Be(2, "It should be Round 2.");

        // Log and Domain Event assertions for Round 1 events
        game.LogEntries.Should().ContainSingle(le => 
            le.EventType == GameLogEventType.CardPlayed && 
            le.ActingPlayerId == player1.Id && 
            le.PlayedCard != null && le.PlayedCard.Equals(cardToPlay), "Player 1 playing Handmaid should be logged.");

        var roundEndedEvents = game.DomainEvents.OfType<RoundEnded>().ToList();
        roundEndedEvents.Should().HaveCount(1, "Only one RoundEnded event for Round 1.");
        var round1EndedEvent = roundEndedEvents.First();
        round1EndedEvent.WinnerPlayerId.Should().Be(player2.Id, $"Player 2 should win Round 1 (Baron {player2ExpectedCard.Rank.Value} > Priest {player1ExpectedRemainingCard.Rank.Value}).");

        // Assert PlayerSummaries from RoundEnded event for accurate end-of-round-1 hands
        var player1Summary = round1EndedEvent.PlayerSummaries.Single(s => s.PlayerId == player1.Id);
        player1Summary.CardsHeld.Should().HaveCount(1).And.Contain(player1ExpectedRemainingCard, "Player 1 summary should show Priest R2 at end of Round 1.");

        var player2Summary = round1EndedEvent.PlayerSummaries.Single(s => s.PlayerId == player2.Id);
        player2Summary.CardsHeld.Should().HaveCount(1).And.Contain(player2ExpectedCard, "Player 2 summary should show Baron R3 at end of Round 1.");

        game.DomainEvents.OfType<PlayerPlayedCard>().Should().ContainSingle(e => 
            e.PlayerId == player1.Id && 
            e.PlayedCard.Equals(cardToPlay) 
            , "PlayerPlayedCard event for Round 1.");

        // Assert new hands for Round 2 (current turn player has 2, other has 1)
        var currentTurnPlayer_R2 = game.Players.Single(p => p.Id == game.CurrentTurnPlayerId);
        var otherPlayer_R2 = game.Players.Single(p => p.Id != game.CurrentTurnPlayerId);

        currentTurnPlayer_R2.Hand.Cards.Should().HaveCount(2, "Current player in Round 2 should have 2 cards.");
        otherPlayer_R2.Hand.Cards.Should().HaveCount(1, "Other player in Round 2 should have 1 card.");
    }
}