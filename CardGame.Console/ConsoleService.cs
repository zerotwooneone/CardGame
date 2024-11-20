using CardGame.Application;
using CardGame.Domain.Turn;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ConsoleService:IHostedService
{
    private readonly ILogger<ConsoleService> _logger;
    private readonly IApplicationTurnService _applicationTurnService;
    public ConsoleService(
        ILogger<ConsoleService> logger,
        IApplicationTurnService applicationTurnService)
    {
        _logger = logger;
        _applicationTurnService = applicationTurnService;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"in console service");
        var turn = await _applicationTurnService.Play(1, 1, 2, new PlayParams{TargetPlayer = (PlayerId)2, Guess = (CardValue)2}).ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}