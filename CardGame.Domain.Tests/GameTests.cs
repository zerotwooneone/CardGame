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
            new Card(new RankDefinition(Guid.NewGuid(), CardRank.Guard.Value), "g1"), 
            new Card(new RankDefinition(Guid.NewGuid(), CardRank.Guard.Value), "g2"),
            new Card(new RankDefinition(Guid.NewGuid(), CardRank.Priest.Value), "p1"),
            new Card(new RankDefinition(Guid.NewGuid(), CardRank.Princess.Value), "princess1"),
            new Card(new RankDefinition(Guid.NewGuid(), CardRank.Guard.Value), "g3"),
            new Card(new RankDefinition(Guid.NewGuid(), CardRank.Baron.Value), "b1")
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
            cardsToUse = new List<Card> { new Card(new RankDefinition(Guid.NewGuid(), CardRank.Guard.Value), "fallback_g") };
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
}