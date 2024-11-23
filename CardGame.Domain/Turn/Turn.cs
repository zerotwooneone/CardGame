﻿namespace CardGame.Domain.Turn;

public class Turn
{
    public uint Number { get; private set; }
    public Game Game { get; }
    public Round Round { get; private set; }
    public Player Player=> Round.CurrentPlayer;
    
    public Turn(uint number, Game game, Round round)
    {
        Number = number;
        Game = game;
        Round = round;
    }

    public async Task Play(
        CardEffect cardEffect, 
        ICardEffectRepository cardEffectRepository, 
        PlayParams playParams,
        IInspectNotificationService inspectNotificationService,
        IRoundFactory roundFactory)
    {
        if (Game.Complete)
        {
            throw new Exception("game is complete");
        }

        if (Round.Complete)
        {
            throw new Exception("round is complete");
        }
        var card = Player.GetHand().First(c=> c.Id == cardEffect.Card);
        Player.Discard(cardEffect, card);
        Round.Play(cardEffect, Player);
        if (cardEffect.RequiresTargetPlayer)
        {
            var target =Round.GetTargetPlayer(cardEffect, playParams);
            var otherRemainingPlayers = Round.RemainingPlayers
                .Where(p=>!p.Equals(Player))
                .ToArray();
            if (cardEffect.TradeHands && !otherRemainingPlayers.All(p=>p.IsProtected))
            {
                if (target.IsProtected)
                {
                    throw new Exception("cannot trade with someone protected. trade hands");
                }
                var playerHand = Player.GetHand().Single();
                var targetHand = target.Trade(playerHand);
                Player.Trade(targetHand);
            }
            if (cardEffect.DiscardAndDraw)
            {
                if (target.IsProtected && !Player.Id.Equals(playParams.TargetPlayer))
                {
                    throw new Exception("cannot target someone protected. discard and draw");
                }
                if (otherRemainingPlayers.All(p => p.IsProtected) && !Player.Id.Equals(playParams.TargetPlayer))
                {
                    throw new Exception("must target self when all other players are protected");
                }

                if (!target.IsProtected)
                {
                    var drawnForDiscard = Round.DrawForDiscard();
                    var discarded =target.DiscardAndDraw(drawnForDiscard);
                    var discardEffect = await cardEffectRepository.Get(Game.Id, discarded.Id, playParams).ConfigureAwait(false);
                    Round.DiscardAndDraw(discardEffect, target);
                    target.RemoveFromRound();
                }
            }

            if (cardEffect.Compare)
            {
                if (!otherRemainingPlayers.All(p => p.IsProtected))
                {
                    if (target.IsProtected)
                    {
                        throw new Exception("cannot target someone protected. compare");
                    }
                    var targetCard = target.GetHand().Single();
                    var playerCard = Player.GetHand().Single();
                    if(targetCard.Value != playerCard.Value)
                    {
                        var toBeRemoved = targetCard.Value > playerCard.Value ? Player : target;
                        Round.RemovePlayer(toBeRemoved);
                        target.RemoveFromRound();
                    }
                }
            }

            if (cardEffect.Inspect)
            {
                if (target.IsProtected)
                {
                    throw new Exception("cannot target someone protected. inspect");
                }
                var targetCard = target.GetHand().Single();
                await inspectNotificationService.Notify(targetCard).ConfigureAwait(false);
            }

            if (cardEffect.Guess && !otherRemainingPlayers.All(p=>p.IsProtected))
            {
                if (target.IsProtected)
                {
                    throw new Exception("cannot target someone protected. guess");
                }

                if (playParams.Guess == null)
                {
                    throw new Exception("guess is required");
                }
                var targetCard = target.GetHand().Single();
                if (playParams.Guess == targetCard.Value)
                {
                    Round.RemovePlayer(target);
                    target.RemoveFromRound();
                }
            }
        }
        if(cardEffect.Protect)
        {
            Player.Protect();
        }

        if (Round.Complete)
        {
            var winner = Round.GetWinner();
            winner.WinRound();
            if (Game.Complete)
            {
                return;
            }
            Round = roundFactory.CreateFrom(Round.Number+1,winner, Game.Players, Game.Deck);
        }
        else
        {
            Round.NextPlayer();
        }
        var drawn =Round.DrawForTurn();
        Round.CurrentPlayer.Draw(drawn);
        Number++;
    }
}