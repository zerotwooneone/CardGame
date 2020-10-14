using System;
using CardGame.CommonModel.Bus;
using CardGame.Domain.Abstractions.Card;

namespace CardGame.Domain.Abstractions.Game
{
    public class PlayRequest: IRequest
    {
        public Guid GameId { get; set; }
        public Guid PlayerId { get; set; } 
        public CardStrength CardStrength { get; set; }
        public int CardVarient { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid? TargetId { get; set; }
        public CardStrength? GuessValue { get; set; }
        public Guid EventId { get; set; }
    }
}