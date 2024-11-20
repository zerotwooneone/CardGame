namespace CardGame.Domain.Turn;

public interface IInspectNotificationService
{
    Task Notify(Card card);
}