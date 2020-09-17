using System;

namespace CardGame.CommonModel.Bus
{
    public class CardPlayed: IEvent
    {
        public Guid CorrelationId { get; set; }
        public string ErrorMessage { get; set; }
    }
}