using System;

namespace CardGame.CommonModel.Bus
{
    public class CardPlayed: IEvent
    {
        public Guid CorrelationId { get; set; }
        public string ErrorMessage { get; set; }
        public Guid GameId { get; set; }
        public Guid PlayerId { get; set; }
        public int CardStrength { get; set; }
        public int CardVarient { get; set; }
        public Guid? TargetId { get; set; }
        public int? GuessValue { get; set; }
    }
}