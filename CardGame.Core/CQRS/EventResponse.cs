namespace CardGame.Core.CQRS
{
    public class EventResponse
    {
        public bool? Success { get; set; }
        public string CreatedId { get; set; }
    }
}