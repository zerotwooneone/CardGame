using CardGame.Domain.Turn;
using Microsoft.Extensions.Logging;

public class LoggerNotificationService : IInspectNotificationService
{
    private readonly ILogger<LoggerNotificationService> _logger;

    public LoggerNotificationService(ILogger<LoggerNotificationService> logger)
    {
        _logger = logger;
    }
    public async Task Notify(Card card)
    {
        _logger.LogInformation("Inspected card id:{CardId} value:{CardValue}", card.Id, card.Value);
    }
}