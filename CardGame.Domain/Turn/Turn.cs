namespace CardGame.Domain.Turn;

public class Turn
{
    public uint Number { get; }
    public Game Game { get; }
    public Round Round { get; private set; }
    public CurrentPlayer CurrentPlayer { get; }

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
        PlayableCard playableCard, 
        IForcedDiscardEffectRepository effectRepository, 
        PlayParams playParams,
        IInspectNotificationService inspectNotificationService,
        IRoundFactory roundFactory,
        IShuffleService shuffleService)
    {
        if (Game.Complete)
        {
            throw new Exception("game is complete");
        }

        if (Round.Complete)
        {
            throw new Exception("round is complete");
        }
        var card = CurrentPlayer.GetHand().Single(c=> c.Id == playableCard.CardId);
        CurrentPlayer.Play(playableCard, card);
        var currentRemainingPlayer = Round.RemainingPlayers.Single(p => p.Id == CurrentPlayer.Id);
        currentRemainingPlayer.Discard(card, CurrentPlayer.GetHand().Single());
        if (playableCard.KickOutOfRoundOnDiscard)
        {
            Round.RemovePlayer(CurrentPlayer.Id);
        }
        if (playableCard.RequiresTargetPlayer)
        {
            var (target,roundTarget) =GetTargetPlayer(playableCard, playParams);
            var otherRemainingPlayers = Round.RemainingPlayers
                .Where(p=>!p.Id.Equals(CurrentPlayer.Id))
                .ToArray();
            if (playableCard.TradeHands && !otherRemainingPlayers.All(p=>p.IsProtected))
            {
                if (target.IsProtected)
                {
                    throw new Exception("cannot trade with someone protected. trade hands");
                }
                var playerHand = CurrentPlayer.GetHand().Single();
                var targetHand = target.Trade(playerHand);
                CurrentPlayer.Trade(targetHand);
                currentRemainingPlayer.Trade(targetHand);
            }
            if (playableCard.DiscardAndDraw)
            {
                if (target.IsProtected && !CurrentPlayer.Id.Equals(playParams.TargetPlayer))
                {
                    throw new Exception("cannot target someone protected. discard and draw");
                }
                if (otherRemainingPlayers.All(p => p.IsProtected) && !CurrentPlayer.Id.Equals(playParams.TargetPlayer))
                {
                    throw new Exception("must target self when all other players are protected");
                }

                if (!target.IsProtected)
                {
                    var drawnForDiscard = Round.DrawForDiscard();
                    var discarded =target.DiscardAndDraw(drawnForDiscard);

                    if(target.Id == CurrentPlayer.Id)
                    {
                        roundTarget.Discard(discarded, drawnForDiscard);
                    }
                    
                    var discardEffect = await effectRepository.Get(discarded.Value).ConfigureAwait(false);
                    if (discardEffect.DiscardAndDrawKickEnabled && discardEffect.KickOutOfRoundOnDiscard)
                    {
                        Round.RemovePlayer(target.Id);
                        roundTarget.RemoveFromRound(drawnForDiscard);
                    }
                }
            }

            if (playableCard.Compare)
            {
                if (!otherRemainingPlayers.All(p => p.IsProtected))
                {
                    if (target.IsProtected)
                    {
                        throw new Exception("cannot target someone protected. compare");
                    }
                    var targetCard = roundTarget.Hand;
                    var playerCard = CurrentPlayer.GetHand().Single();
                    if(targetCard.Value != playerCard.Value)
                    {
                        var toBeRemoved = targetCard.Value > playerCard.Value ? CurrentPlayer : target;
                        Round.RemovePlayer(toBeRemoved.Id);
                        roundTarget.RemoveFromRound(roundTarget.Hand);
                    }
                }
            }

            if (playableCard.Inspect)
            {
                if (target.IsProtected)
                {
                    throw new Exception("cannot target someone protected. inspect");
                }
                var targetCard = roundTarget.Hand;
                await inspectNotificationService.Notify(targetCard).ConfigureAwait(false);
            }

            if (playableCard.Guess && !otherRemainingPlayers.All(p=>p.IsProtected))
            {
                if (target.IsProtected)
                {
                    throw new Exception("cannot target someone protected. guess");
                }

                if (playParams.Guess == null)
                {
                    throw new Exception("guess is required");
                }
                var targetCard = roundTarget.Hand;
                if (playParams.Guess == targetCard.Value)
                {
                    Round.RemovePlayer(target.Id);
                    roundTarget.RemoveFromRound(targetCard);
                }
            }
        }
        if(playableCard.Protect)
        {
            CurrentPlayer.Protect();
        }

        if (Round.Complete)
        {
            var winner = Round.GetWinner();
            var roundWinner = Game.Players.First(p => p.Id == winner.Id);
            roundWinner.WinRound();
            if (Game.Complete)
            {
                return;
            }
            Round = roundFactory.CreateFrom(
                Round.Number+1,
                roundWinner, 
                Game.Players, 
                Game.Deck,
                shuffleService);
            NextPlayerId = winner.Id;
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
            NextPlayerId = remainingIdsInOrder[nextIndex];
        }
        var drawn =Round.DrawForTurn();
        var nextPlayer = Round.RemainingPlayers
            .Where(p => p.Id == NextPlayerId)
            .Select(p => p.ToCurrentPlayer(drawn))
            .Single();
        nextPlayer.StartTurn();
    }

    public PlayerId? NextPlayerId { get; private set; } = null;

    private (ITargetablePlayer, RemainingPlayer) GetTargetPlayer(PlayableCard playableCard, PlayParams playParams)
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
            : Round.RemainingPlayers.Prepend((ITargetablePlayer)CurrentPlayer);
        var selectedTarget = possibleTargets.First(p => playParams.TargetPlayer.HasValue && p.Id == playParams.TargetPlayer.Value);

        var roundTarget = Round.RemainingPlayers.Single(p => p.Id == selectedTarget.Id);

        return (selectedTarget, roundTarget);
    }
}