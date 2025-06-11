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
    // Shared RankDefinitions for the base game using fixed Guids
    private static readonly RankDefinition GuardRank = new(new Guid("00000000-0000-0000-0000-000000000001"), CardRank.Guard.Value);
    private static readonly RankDefinition PriestRank = new(new Guid("00000000-0000-0000-0000-000000000002"), CardRank.Priest.Value);
    private static readonly RankDefinition BaronRank = new(new Guid("00000000-0000-0000-0000-000000000003"), CardRank.Baron.Value);
    private static readonly RankDefinition HandmaidRank = new(new Guid("00000000-0000-0000-0000-000000000004"), CardRank.Handmaid.Value);
    private static readonly RankDefinition PrinceRank = new(new Guid("00000000-0000-0000-0000-000000000005"), CardRank.Prince.Value);
    private static readonly RankDefinition KingRank = new(new Guid("00000000-0000-0000-0000-000000000006"), CardRank.King.Value);
    private static readonly RankDefinition CountessRank = new(new Guid("00000000-0000-0000-0000-000000000007"), CardRank.Countess.Value);
    private static readonly RankDefinition PrincessRank = new(new Guid("00000000-0000-0000-0000-000000000008"), CardRank.Princess.Value);

    private FakeLoggerFactory _fakeLoggerFactory;
    private NonShufflingRandomizer _nonShufflingRandomizer;
    private List<PlayerInfo> _twoPlayerInfos;
    private List<Card> _defaultTestDeckCards;

    // Fixed Player IDs
    private static readonly Guid Player1Id = new Guid("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Player2Id = new Guid("22222222-2222-2222-2222-222222222222");

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

        _twoPlayerInfos = new List<PlayerInfo>
        {
            new PlayerInfo(Player1Id, "Player 1"),
            new PlayerInfo(Player2Id, "Player 2")
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
        List<Card>? deckCards = null, 
        Action<IGameOperations, Player, Card, Player?, int?>? cardEffectAction = null) 
    {
        var pInfos = playerInfos ?? _twoPlayerInfos;
        var cardsForDeck = deckCards ?? _defaultTestDeckCards;

        if (!cardsForDeck.Any())
        {
            cardsForDeck = new List<Card> { new Card(GuardRank, "fallback_g1_crt_game") };
        }

        var currentTestDeckProvider = new FakeDeckProvider(new List<Card>(cardsForDeck));
        if (cardEffectAction != null)
        {
            currentTestDeckProvider.CardEffectAction = cardEffectAction;
        }

        var game = TestedGame.CreateNewGame(
            currentTestDeckProvider.DeckId, 
            pInfos,
            pInfos.First().Id, 
            cardsForDeck,      
            _fakeLoggerFactory,
            currentTestDeckProvider, 
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
        game.DomainEvents.OfType<GameCreated>().Should().HaveCount(1);
        game.DomainEvents.OfType<RoundStarted>().Should().HaveCount(1);
        
        var firstPlayer = players.Single(p => p.Id == game.CurrentTurnPlayerId);
        var otherPlayer = players.Single(p => p.Id != game.CurrentTurnPlayerId);

        firstPlayer.Hand.Cards.Should().HaveCount(2);
        otherPlayer.Hand.Cards.Should().HaveCount(1);
    }

    [Test]
    public void PlayCard_FirstPlay_WithEmptyDeck_EndsRoundAndStartsNewRound()
    {
        var card_Priest_R2_TurnDraw = new Card(PriestRank, "sa_priest");
        var card_Baron_R3_P2Hand = new Card(BaronRank, "sa_baron");
        var card_Handmaid_R4_P1Hand = new Card(HandmaidRank, "sa_handmaid");
        var card_Prince_R5_SetAside3 = new Card(PrinceRank, "p1_prince");
        var card_King_R6_SetAside2 = new Card(KingRank, "p2_king");
        var card_Countess_R7_SetAside1 = new Card(CountessRank, "p1_countess");

        var initialDeckCards = new List<Card> 
        {
            card_Priest_R2_TurnDraw, card_Baron_R3_P2Hand, card_Handmaid_R4_P1Hand, 
            card_Prince_R5_SetAside3, card_King_R6_SetAside2, card_Countess_R7_SetAside1
        };

        Action<IGameOperations, Player, Card, Player?, int?> noOpCardEffect = (g, ap, c, tp, gr) => { /* No-op */ };

        var (game, players, operations) = CreateTestGame(
            deckCards: initialDeckCards, 
            tokensToWin: 2,
            cardEffectAction: noOpCardEffect
        );

        var player1 = players.Single(p => p.Id == game.CurrentTurnPlayerId); 
        var player2 = players.Single(p => p.Id != game.CurrentTurnPlayerId); 

        var cardToPlay = player1.Hand.Cards.First(c => c.Rank.Value == CardRank.Handmaid.Value); 
        var player1ExpectedRemainingCard = card_Priest_R2_TurnDraw; 
        var player2ExpectedCard = card_Baron_R3_P2Hand;

        game.PlayCard(player1.Id, cardToPlay, null, null);

        var player1AfterPlay = game.Players.Single(p => p.Id == player1.Id); 
        var player2AfterPlay = game.Players.Single(p => p.Id == player2.Id);

        player1AfterPlay.TokensWon.Should().Be(0, "Player 1 lost Round 1.");
        player2AfterPlay.TokensWon.Should().Be(1, "Player 2 won Round 1.");

        game.GamePhase.Should().Be(GamePhase.RoundInProgress, "A new round should have started.");
        game.RoundNumber.Should().Be(2, "It should be Round 2.");

        game.LogEntries.Should().ContainSingle(le => 
            le.EventType == GameLogEventType.CardPlayed && 
            le.ActingPlayerId == player1.Id && 
            le.PlayedCard != null && le.PlayedCard.Equals(cardToPlay), "Player 1 playing Handmaid should be logged.");

        var roundEndedEvents = game.DomainEvents.OfType<RoundEnded>().ToList();
        roundEndedEvents.Should().HaveCount(1, "Only one RoundEnded event for Round 1.");
        var round1EndedEvent = roundEndedEvents.First();
        round1EndedEvent.WinnerPlayerId.Should().Be(player2.Id, $"Player 2 should win Round 1 (Baron {player2ExpectedCard.Rank.Value} > Priest {player1ExpectedRemainingCard.Rank.Value}).");

        var player1Summary = round1EndedEvent.PlayerSummaries.Single(s => s.PlayerId == player1.Id);
        player1Summary.CardsHeld.Should().HaveCount(1).And.Contain(player1ExpectedRemainingCard, "Player 1 summary should show Priest at end of Round 1.");

        var player2Summary = round1EndedEvent.PlayerSummaries.Single(s => s.PlayerId == player2.Id);
        player2Summary.CardsHeld.Should().HaveCount(1).And.Contain(player2ExpectedCard, "Player 2 summary should show Baron at end of Round 1.");

        game.DomainEvents.OfType<PlayerPlayedCard>().Should().ContainSingle(e => 
            e.PlayerId == player1.Id && 
            e.PlayedCard.Equals(cardToPlay) 
            , "PlayerPlayedCard event for Round 1.");

        var currentTurnPlayer_R2 = game.Players.Single(p => p.Id == game.CurrentTurnPlayerId);
        var otherPlayer_R2 = game.Players.Single(p => p.Id != game.CurrentTurnPlayerId);

        currentTurnPlayer_R2.Hand.Cards.Should().HaveCount(2, "Current turn player in Round 2 should have 2 cards.");
        otherPlayer_R2.Hand.Cards.Should().HaveCount(1, "Other player in Round 2 should have 1 card.");
    }
}