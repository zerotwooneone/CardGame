using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using CardGame.Domain.Game; 
using GameClass = CardGame.Domain.Game.Game; 
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using CardGame.Domain.Common; 
using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Domain.Game.GameException; 

namespace CardGame.Domain.Tests
{
    [TestFixture]
    public class GameTests
    { 
        private Mock<ILoggerFactory> _mockLoggerFactory;
        private Mock<ILogger<GameClass>> _mockGameLogger; 
        private Mock<ILogger<Player>> _mockPlayerLogger;   
        private Mock<IRandomizer> _mockRandomizer;
        private Mock<IDeckProvider> _mockDeckProvider; 
        private List<Card> _standardDeck;

        private static readonly Guid _testDeckDefinitionId = Guid.NewGuid();

        private static readonly Card _guard = new Card("guard-default", CardType.Guard);
        private static readonly Card _priest = new Card("priest-default", CardType.Priest);
        private static readonly Card _baron = new Card("baron-default", CardType.Baron);
        private static readonly Card _handmaid = new Card("handmaid-default", CardType.Handmaid);
        private static readonly Card _king = new Card("king-default", CardType.King);
        private static readonly Card _countess = new Card("countess-default", CardType.Countess);

        [SetUp]
        public void Setup()
        {
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockGameLogger = new Mock<ILogger<GameClass>>(); 
            _mockPlayerLogger = new Mock<ILogger<Player>>();   
            _mockRandomizer = new Mock<IRandomizer>();
            _mockDeckProvider = new Mock<IDeckProvider>(); 

            _mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object); 
            _mockLoggerFactory.Setup(x => x.CreateLogger(typeof(GameClass).FullName!)).Returns(_mockGameLogger.Object);
            _mockLoggerFactory.Setup(x => x.CreateLogger(typeof(Player).FullName!)).Returns(_mockPlayerLogger.Object);
            
            _standardDeck = new List<Card>
            {
                _guard, _guard, _guard, _guard, _guard, 
                _priest, _priest,                       
                _baron, _baron,                         
                _handmaid, _handmaid,                   
                _king,                                  
                _countess, 
                new Card("extra1-default", CardType.Guard), 
                new Card("extra2-default", CardType.Priest),
                new Card("extra3-default", CardType.Baron)
            }; 
        }

        private GameClass CreateGameForTest(List<PlayerInfo> playerInfos, Guid creatorId, List<Card>? initialDeck = null, int tokensToWin = 4)
        {
            var deckForGame = initialDeck ?? _standardDeck.ToList();

            var game = GameClass.CreateNewGame(
                _testDeckDefinitionId, 
                playerInfos,
                creatorId, 
                deckForGame, 
                _mockLoggerFactory.Object,
                tokensToWin,
                _mockRandomizer.Object
            );
            return game;
        }

        // Helper to ensure a player has *a* card, primarily through game's dealing/drawing mechanics.
        private Card EnsurePlayerHasAnyCard(GameClass game, Player player)
        {
            if (!player.Hand.Cards.Any()) // If hand is initially empty
            {
                // Attempt to draw if deck has cards.
                // This is for scenarios where a player might start with zero cards
                // and the test needs them to have one to proceed, outside normal turn draw.
                if (game.Deck.CardsRemaining > 0)
                {
                    var drawnCard = ((IGameOperations)game).DrawCardForPlayer(player.Id);
                    if (drawnCard != null) 
                    {
                        // Card was successfully drawn and added to hand.
                        return drawnCard; 
                    }
                }
                // If hand is still empty after attempting a draw (or if no draw was possible), throw.
                throw new InvalidOperationException($"Test setup error: Player {player.Name} has no cards and could not obtain one via drawing.");
            }
            // If hand was not empty initially (or became non-empty after a successful draw, though current logic returns directly)
            return player.Hand.Cards.First(); 
        }


        [Test]
        public void StartNewRound_For2Players_DealsCorrectCardsAndSetsAside()
        {
            var p1Info = new PlayerInfo(Guid.NewGuid(), "Player 1");
            var p2Info = new PlayerInfo(Guid.NewGuid(), "Player 2");
            GameClass game = CreateGameForTest(new List<PlayerInfo> { p1Info, p2Info }, p1Info.Id, initialDeck: new List<Card>(_standardDeck)); 

            Assert.That(game.GamePhase, Is.EqualTo(GamePhase.RoundInProgress));
            Assert.That(game.Players.Count, Is.EqualTo(2));
            foreach (var player in game.Players)
            {
                Assert.That(player.Hand.Count, Is.GreaterThanOrEqualTo(1));
                Assert.That(player.Status, Is.EqualTo(PlayerStatus.Active));
            }
            Assert.That(game.Deck.CardsRemaining, Is.EqualTo(_standardDeck.Count - 2 - 3 - 1)); 
            Assert.That(game.SetAsideCard, Is.Null); 
            Assert.That(game.PubliclySetAsideCards.Count, Is.EqualTo(3)); 
            Assert.That(game.Players.SingleOrDefault(p => p.IsPlayersTurn), Is.Not.Null);
        }

        [Test]
        public void StartNewRound_For3Players_DealsCorrectCards()
        {
            var p1Info = new PlayerInfo(Guid.NewGuid(), "Player 1");
            GameClass game = CreateGameForTest(new List<PlayerInfo> { p1Info, new PlayerInfo(Guid.NewGuid(), "Player 2"), new PlayerInfo(Guid.NewGuid(), "Player 3") }, p1Info.Id, initialDeck: new List<Card>(_standardDeck));

            Assert.That(game.GamePhase, Is.EqualTo(GamePhase.RoundInProgress));
            Assert.That(game.Players.Count, Is.EqualTo(3));
            foreach (var player in game.Players)
            {
                Assert.That(player.Hand.Count, Is.GreaterThanOrEqualTo(1));
                Assert.That(player.Status, Is.EqualTo(PlayerStatus.Active));
            }
            Assert.That(game.Deck.CardsRemaining, Is.EqualTo(_standardDeck.Count - 3 - 1 - 1)); 
            Assert.That(game.SetAsideCard, Is.Not.Null);
            Assert.That(game.PubliclySetAsideCards.Count, Is.EqualTo(0)); 
            Assert.That(game.Players.SingleOrDefault(p => p.IsPlayersTurn), Is.Not.Null);
        }

        [Test]
        public void StartNewRound_For4Players_DealsCorrectCards()
        {
            var p1Info = new PlayerInfo(Guid.NewGuid(), "Player 1");
            GameClass game = CreateGameForTest(new List<PlayerInfo> { p1Info, new PlayerInfo(Guid.NewGuid(), "Player 2"), new PlayerInfo(Guid.NewGuid(), "Player 3"), new PlayerInfo(Guid.NewGuid(), "Player 4") }, p1Info.Id, initialDeck: new List<Card>(_standardDeck));

            Assert.That(game.GamePhase, Is.EqualTo(GamePhase.RoundInProgress));
            Assert.That(game.Players.Count, Is.EqualTo(4));
            foreach (var player in game.Players)
            {
                Assert.That(player.Hand.Count, Is.GreaterThanOrEqualTo(1));
                Assert.That(player.Status, Is.EqualTo(PlayerStatus.Active));
            }
            Assert.That(game.Deck.CardsRemaining, Is.EqualTo(_standardDeck.Count - 4 - 1 - 1)); 
            Assert.That(game.SetAsideCard, Is.Not.Null);
            Assert.That(game.PubliclySetAsideCards.Count, Is.EqualTo(0)); 
            Assert.That(game.Players.SingleOrDefault(p => p.IsPlayersTurn), Is.Not.Null);
        }

        [Test]
        public void PlayCard_AdvancesTurnToNextActivePlayer()
        {
            var p1Info = new PlayerInfo(Guid.NewGuid(), "Player 1");
            var p2Info = new PlayerInfo(Guid.NewGuid(), "Player 2");
            GameClass game = CreateGameForTest(new List<PlayerInfo> { p1Info, p2Info, new PlayerInfo(Guid.NewGuid(), "Player 3") }, p1Info.Id);
            var p1 = game.Players.Single(p => p.Id == p1Info.Id);
            var p2 = game.Players.Single(p => p.Id == p2Info.Id);
            
            Assert.That(game.CurrentTurnPlayerId, Is.EqualTo(p1.Id));

            var cardToPlayP1 = EnsurePlayerHasAnyCard(game, p1);
            _mockDeckProvider.Setup(dp => dp.ExecuteCardEffect(It.IsAny<IGameOperations>(), p1, cardToPlayP1, null, null));

            game.PlayCard(p1.Id, cardToPlayP1, null, null, _mockDeckProvider.Object); 
            
            Assert.That(game.CurrentTurnPlayerId, Is.EqualTo(p2.Id));
            _mockDeckProvider.Verify(dp => dp.ExecuteCardEffect(It.IsAny<IGameOperations>(), p1, cardToPlayP1, null, null), Times.Once);
        }

        [Test]
        public void PlayCard_AdvancesTurn_SkipsEliminatedPlayer()
        {
            var p1Info = new PlayerInfo(Guid.NewGuid(), "Player 1");
            var p2Info = new PlayerInfo(Guid.NewGuid(), "Player 2");
            var p3Info = new PlayerInfo(Guid.NewGuid(), "Player 3");
            GameClass game = CreateGameForTest(new List<PlayerInfo> { p1Info, p2Info, p3Info }, p1Info.Id);
            var p1 = game.Players.Single(p => p.Id == p1Info.Id);
            var p2 = game.Players.Single(p => p.Id == p2Info.Id);
            var p3 = game.Players.Single(p => p.Id == p3Info.Id);

            Assert.That(game.CurrentTurnPlayerId, Is.EqualTo(p1.Id));
            
            var cardToPlayP1 = EnsurePlayerHasAnyCard(game, p1);
            _mockDeckProvider.Setup(dp => dp.ExecuteCardEffect(It.IsAny<IGameOperations>(), p1, cardToPlayP1, null, null))
                .Callback((IGameOperations gameOps, Player actor, Card card, Player? target, CardType? guess) => 
                {
                    gameOps.EliminatePlayer(p2.Id, "Eliminated by P1's card effect for test");
                });

            game.PlayCard(p1.Id, cardToPlayP1, null, null, _mockDeckProvider.Object); 
            
            Assert.That(p2.Status, Is.EqualTo(PlayerStatus.Eliminated));
            Assert.That(game.CurrentTurnPlayerId, Is.EqualTo(p3.Id), "Turn should skip P2 and go to P3.");
        }

        [Test]
        public void PlayCard_WhenOnePlayerRemainsActive_EndsRoundAndAwardsToken()
        {
            var p1Info = new PlayerInfo(Guid.NewGuid(), "Player 1");
            var p2Info = new PlayerInfo(Guid.NewGuid(), "Player 2");
            GameClass game = CreateGameForTest(new List<PlayerInfo> { p1Info, p2Info }, p1Info.Id);
            var p1 = game.Players.Single(p => p.Id == p1Info.Id);
            var p2 = game.Players.Single(p => p.Id == p2Info.Id);

            Assert.That(game.CurrentTurnPlayerId, Is.EqualTo(p1.Id));
            
            var cardToPlayP1 = EnsurePlayerHasAnyCard(game, p1);
            _mockDeckProvider.Setup(dp => dp.ExecuteCardEffect(It.IsAny<IGameOperations>(), p1, cardToPlayP1, p2, null))
                .Callback((IGameOperations gameOps, Player actor, Card card, Player? target, CardType? guess) => 
                {
                    if (target != null) gameOps.EliminatePlayer(target.Id, "Eliminated by P1's card effect for test");
                });

            game.PlayCard(p1.Id, cardToPlayP1, p2.Id, null, _mockDeckProvider.Object); 
            
            Assert.That(p1.TokensWon, Is.EqualTo(1));
            Assert.That(game.LastRoundWinnerId, Is.EqualTo(p1.Id));
        }

        [Test]
        public void PlayCard_WhenDeckIsEmptyAndTurnCompletes_EndsRoundAndDeterminesWinner()
        {
            var smallDeck = new List<Card> { _guard, _priest, _baron, _handmaid, _king, _countess }; 
            var p1Info = new PlayerInfo(Guid.NewGuid(), "Player 1");
            var p2Info = new PlayerInfo(Guid.NewGuid(), "Player 2");
            GameClass game = CreateGameForTest(new List<PlayerInfo> { p1Info, p2Info }, p1Info.Id, initialDeck: smallDeck);
            // StartNewRound will use all 6 cards for 2 players (3 set aside, 1 to p1, 1 to p2, p1 draws initial turn card)
            // Deck will be empty.
            // game.StartNewRound(); // REMOVED: CreateGameForTest already starts a round.

            var p1 = game.Players.Single(p => p.Id == p1Info.Id);
            var p2 = game.Players.Single(p => p.Id == p2Info.Id);

            Assert.That(game.Deck.CardsRemaining, Is.EqualTo(0), "Deck should be empty after initial setup for this test.");
            // With the fix in Game.cs, the round should NOT end prematurely during setup.
            Assert.That(game.GamePhase, Is.EqualTo(GamePhase.RoundInProgress), "Game should still be in RoundInProgress after setup, even with an empty deck.");

            var cardToPlayP1 = p1.Hand.Cards.FirstOrDefault(); 
            Assert.That(cardToPlayP1, Is.Not.Null, "P1 should have a card to play.");

            _mockDeckProvider.Setup(dp => dp.ExecuteCardEffect(It.IsAny<IGameOperations>(), p1, cardToPlayP1, null, null));
            // Now, when P1 plays, the round should end because the deck is empty and it's a valid play completion.
            game.PlayCard(p1.Id, cardToPlayP1, null, null, _mockDeckProvider.Object);
            
            Assert.That(game.LastRoundWinnerId, Is.Not.Null, "Winner should be determined when round ends by deck empty.");
        }


        [Test]
        public void GameEnds_WhenAPlayerReachesTokensToWin()
        {
            int tokensToWin = 1; 
            var p1Info = new PlayerInfo(Guid.NewGuid(), "Player 1");
            var p2Info = new PlayerInfo(Guid.NewGuid(), "Player 2");
            GameClass game = CreateGameForTest(new List<PlayerInfo> { p1Info, p2Info }, p1Info.Id, tokensToWin: tokensToWin);
            var p1 = game.Players.Single(p => p.Id == p1Info.Id);
            var p2 = game.Players.Single(p => p.Id == p2Info.Id);

            var cardToPlayP1Round1 = EnsurePlayerHasAnyCard(game, p1);
            _mockDeckProvider.Setup(dp => dp.ExecuteCardEffect(It.IsAny<IGameOperations>(), p1, cardToPlayP1Round1, p2, null))
                .Callback((IGameOperations gameOps, Player actor, Card card, Player? target, CardType? guess) => 
                {
                    if (target != null) gameOps.EliminatePlayer(target.Id, "Eliminated by P1 for round 1 win");
                });
            
            game.PlayCard(p1.Id, cardToPlayP1Round1, p2.Id, null, _mockDeckProvider.Object); 
            
            Assert.That(p1.TokensWon, Is.EqualTo(tokensToWin));
            Assert.That(game.GamePhase, Is.EqualTo(GamePhase.GameOver));
            Assert.That(game.LastRoundWinnerId, Is.EqualTo(p1.Id));
        }

        [Test]
        public void Player_CannotPlayCard_IfNotTheirTurn()
        {
            var p1Info = new PlayerInfo(Guid.NewGuid(), "Player 1");
            var p2Info = new PlayerInfo(Guid.NewGuid(), "Player 2");
            GameClass game = CreateGameForTest(new List<PlayerInfo> { p1Info, p2Info }, p1Info.Id);
            var p1 = game.Players.Single(p => p.Id == p1Info.Id);
            var p2 = game.Players.Single(p => p.Id == p2Info.Id); 

            var cardInHandP2 = EnsurePlayerHasAnyCard(game, p2); 

            Assert.Throws<InvalidMoveException>(() => 
                game.PlayCard(p2.Id, cardInHandP2, null, null, _mockDeckProvider.Object)
            );
        }

        [Test]
        public void Player_CannotPlayCard_NotInHand()
        {
            var p1Info = new PlayerInfo(Guid.NewGuid(), "Player 1");
            GameClass game = CreateGameForTest(new List<PlayerInfo> { p1Info, new PlayerInfo(Guid.NewGuid(), "Player 2") }, p1Info.Id);
            var p1 = game.Players.Single(p => p.Id == p1Info.Id); 
            var cardNotInHand = new Card("other-card-appearance", CardType.Baron); 
            
            Assert.Throws<InvalidMoveException>(() => 
                game.PlayCard(p1.Id, cardNotInHand, null, null, _mockDeckProvider.Object)
            );
        }
        
        // TODO: Add test for PlayCard when player is eliminated.
        // TODO: Add test for PlayCard when game is not in RoundInProgress phase.

    }
}
