using System;
using CardGame.Domain.Abstractions.Card;

namespace CardGame.Application.DTO
{
    public class PlayRequest
    {
        public Guid PlayerId { get; set; } 
        public CardStrength CardStrength { get; set; }
        public int CardVariant { get; set; }
        public Guid? TargetId { get; set; }
        public CardStrength? GuessValue { get; set; }
    }
}