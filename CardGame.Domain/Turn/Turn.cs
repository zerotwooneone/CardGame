namespace CardGame.Domain.Turn;

public class Turn
{
    public uint Number { get; private set; }
    public Game Game { get; }
    public Round Round { get; private set; }
    public CurrentPlayer CurrentPlayer { get; private set; }

    public Turn(
        uint number, 
        Game game, 
        Round round, 
        CurrentPlayer currentPlayer)
    {
        Number = number;
        Game = game;
        Round = round;
        CurrentPlayer = currentPlayer;
    }

    public async Task Play(
        CardId playedCardId, 
        PlayParams playParams,
        IInspectNotificationService inspectNotificationService)
    {
        if (Game.Complete)
        {
            throw new Exception("game is complete");
        }

        if (Round.Complete)
        {
            throw new Exception("round is complete");
        }
        var playableCard=CurrentPlayer.GetHand().Single(c=>c.CardId == playedCardId);
        
        CurrentPlayer.Play(playableCard);
        var currentRemainingPlayer = Round.RemainingPlayers.Single(p => p.Id == CurrentPlayer.Id);

        var remainingCard = CurrentPlayer.GetHand().Single();
        var discardedRoundCard = currentRemainingPlayer.Hand.Id.Equals(playableCard.CardId) 
            ? currentRemainingPlayer.Hand
            : new RoundCard(playableCard);
        var handRoundCard = currentRemainingPlayer.Hand.Id.Equals(playableCard.CardId)
            ? new RoundCard(remainingCard)
            : currentRemainingPlayer.Hand;
        currentRemainingPlayer.ReplaceHand(discardedRoundCard, handRoundCard);
        if (playableCard.KickOutOfRoundOnDiscard)
        {
            Round.EliminatePlayer(currentRemainingPlayer);
        }
        if (playableCard.RequiresTargetPlayer)
        {
            var targetPlayer =GetTargetPlayer(playableCard, playParams);
            var otherRemainingPlayers = Round.RemainingPlayers
                .Where(p=>!p.Id.Equals(CurrentPlayer.Id))
                .ToArray();
            if (playableCard.TradeHands && !otherRemainingPlayers.All(p=>p.IsProtected))
            {
                if (targetPlayer.IsProtected)
                {
                    throw new Exception("cannot trade with someone protected. trade hands");
                }
                var targetHand = targetPlayer.Trade(currentRemainingPlayer.Hand);
                CurrentPlayer.Trade(targetHand.ToPlayableCard());
                currentRemainingPlayer.Trade(targetHand);
            }
            if (playableCard.DiscardAndDraw)
            {
                if (targetPlayer.IsProtected && !CurrentPlayer.Id.Equals(playParams.TargetPlayer))
                {
                    throw new Exception("cannot target someone protected. discard and draw");
                }
                if (otherRemainingPlayers.All(p => p.IsProtected) && !CurrentPlayer.Id.Equals(playParams.TargetPlayer))
                {
                    throw new Exception("must target self when all other players are protected");
                }

                if (!targetPlayer.IsProtected)
                {
                    var drawnForDiscard = Round.DrawForDiscard();
                    var discarded = targetPlayer.Hand;
                    targetPlayer.ReplaceHand(discarded,drawnForDiscard);

                    if(targetPlayer.Id == CurrentPlayer.Id)
                    {
                        CurrentPlayer.Discard(new Card{Id = discarded.Id, Value = discarded.Value});
                        CurrentPlayer.Draw(drawnForDiscard.ToPlayableCard());
                    }

                    if (Game.DiscardAndDrawKickEnabled && discarded.KickOutOfRoundOnDiscard)
                    {
                        Round.EliminatePlayer(targetPlayer);
                    }
                }
            }

            if (playableCard.Compare)
            {
                if (!otherRemainingPlayers.All(p => p.IsProtected))
                {
                    if (targetPlayer.IsProtected)
                    {
                        throw new Exception("cannot target someone protected. compare");
                    }
                    var targetCard = targetPlayer.Hand;
                    var playerCard = CurrentPlayer.GetHand().Single();
                    if(targetCard.Value != playerCard.Value)
                    {
                        var toBeRemoved = targetCard.Value.Value > playerCard.Value.Value 
                            ? currentRemainingPlayer 
                            : targetPlayer;
                        Round.EliminatePlayer(toBeRemoved);
                    }
                }
            }

            if (playableCard.Inspect)
            {
                if (targetPlayer.IsProtected)
                {
                    throw new Exception("cannot target someone protected. inspect");
                }
                var targetCard = new Card{Id = targetPlayer.Hand.Id, Value = targetPlayer.Hand.Value};
                await inspectNotificationService.Notify(targetCard).ConfigureAwait(false);
            }

            if (playableCard.Guess && !otherRemainingPlayers.All(p=>p.IsProtected))
            {
                if (targetPlayer.IsProtected)
                {
                    throw new Exception("guess cannot target someone protected.");
                }

                if (playParams.Guess == null)
                {
                    throw new Exception("guess is required");
                }
                var targetCard = targetPlayer.Hand;
                if (playParams.Guess == targetCard.Value)
                {
                    Round.EliminatePlayer(targetPlayer);
                }
            }
        }
        if(playableCard.Protect)
        {
            currentRemainingPlayer.Protect();
        }
    }

    public async Task NextTurn(IRoundFactory roundFactory, IShuffleService shuffleService)
    {
        if (Game.Complete)
        {
            throw new Exception("game is complete");
        }

        PlayerId nextPlayerId;
        if (Round.Complete)
        {
            var winner = Round.GetWinner();
            var roundWinner = Game.Players.First(p => p.Id == winner.Id);
            roundWinner.WinRound();
            if (Game.Complete)
            {
                return;
            }
            Round = await roundFactory.CreateFrom(
                Round.Number+1,
                roundWinner, 
                Game.Players, 
                Game.Deck,
                shuffleService).ConfigureAwait(false);
            nextPlayerId = winner.Id;
        }
        else
        {
            var remainingIdsInOrder =  Game.Players
                .Where(gp => Round.RemainingPlayers
                    .Select(p => p.Id).Contains(gp.Id))
                .Select(gp=>gp.Id)
                .ToArray();
            var currentIndex = Array.IndexOf(remainingIdsInOrder, CurrentPlayer.Id);
            
            var nextIndex = currentIndex == remainingIdsInOrder.Length-1
                ? 0
                : currentIndex+1;
            nextPlayerId = remainingIdsInOrder[nextIndex];
        }
        var drawn =Round.DrawForTurn();
        CurrentPlayer = Round.RemainingPlayers
            .Where(p => p.Id == nextPlayerId)
            .Select(p => p.ToCurrentPlayer(drawn))
            .Single();
        var currentRemainingPlayer = Round.RemainingPlayers.Single(p => p.Id == CurrentPlayer.Id);
        currentRemainingPlayer.StartTurn();
        Number++;
    }

    private RemainingPlayer GetTargetPlayer(PlayableCard playableCard, PlayParams playParams)
    {
        if (!playableCard.RequiresTargetPlayer)
        {
            throw new Exception("no target required");
        }
        if (playParams.TargetPlayer == null)
        {
            throw new Exception("missing target player");
        }

        var possibleTargets = playableCard.CanTargetSelf
            ? Round.RemainingPlayers
            : Round.RemainingPlayers.Where(r=>!r.Id.Equals(CurrentPlayer.Id)).ToArray();
        var selectedTarget = possibleTargets.Single(p => playParams.TargetPlayer.HasValue && p.Id == playParams.TargetPlayer.Value);

        var roundTarget = Round.RemainingPlayers.Single(p => p.Id == selectedTarget.Id);

        return roundTarget;
    }
}