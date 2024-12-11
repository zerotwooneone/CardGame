using CardGame.Application;
using CardGame.Domain.Turn;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ConsoleService:IHostedService
{
    private readonly ILogger<ConsoleService> _logger;
    private readonly IApplicationTurnService _applicationTurnService;
    private readonly ITurnRepository _turnRepository;

    public ConsoleService(
        ILogger<ConsoleService> logger,
        IApplicationTurnService applicationTurnService,
        ITurnRepository turnRepository)
    {
        _logger = logger;
        _applicationTurnService = applicationTurnService;
        _turnRepository = turnRepository;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"starting game");
        var turn = await _turnRepository.GetCurrentTurn((GameId)1).ConfigureAwait(false);
        
        Print(turn ?? throw new Exception("no turn"));

        try
        {
            Print( await _applicationTurnService.Play(1, 1, 4, new PlayParams{TargetPlayer = (PlayerId)2, Guess = (CardValue)2}).ConfigureAwait(false));
            Print( await _applicationTurnService.Play(1, 2, 14, new PlayParams{TargetPlayer = (PlayerId)1}).ConfigureAwait(false));
            Print( await _applicationTurnService.Play(1, 3, 10, new PlayParams{}).ConfigureAwait(false));
            Print( await _applicationTurnService.Play(1, 4, 2, new PlayParams{TargetPlayer = (PlayerId)1, Guess = (CardValue)2}).ConfigureAwait(false));
        }
        catch (Exception e)
        {
            _logger.LogError(e,"play failed");
        }
        
    }

    private void Print(Turn turn)
    {
        _logger.LogInformation("Round:{RoundNumber} Turn:{TurnNumber} RemainingPlayers:{RemainingPlayers} ", turn.Round.Number, turn.Number, string.Join(",", turn.Round.RemainingPlayers.Select(p=>$"{p.Id}:{string.Join(",",p.Hand.Value) }" )));
        _logger.LogInformation("Player:{CurrentPlayerId} Hand:{Hand} Discard:{Discard} ", turn.CurrentPlayer.Id, string.Join(",", turn.CurrentPlayer.GetHand().Select(c=>$"id:{c.CardId} value:{c.Value}")), string.Join(",", (turn.Round.RemainingPlayers.Single(p=>p.Id == turn.CurrentPlayer.Id)).DiscardPile.Select(c=>c.Id)));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}