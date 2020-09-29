using System.Collections.Generic;
using System.Linq;

namespace CardGame.Application.DTO
{
    public class CardDto
    {
        public int CardStrength { get; set; }
        public int Variant { get; set; }

        public static IEnumerable<CardDto> Create(string hand)
        {
            if(string.IsNullOrWhiteSpace(hand)) return Enumerable.Empty<CardDto>();
            var card = hand.Split(";").Where(s => !string.IsNullOrWhiteSpace(s));
            return card.Select(s =>
            {
                var str = int.Parse(s.Substring(0, 1));
                var v = int.Parse(s.Substring(1, 1));
                return new CardDto
                {
                    CardStrength = str,
                    Variant = v
                };
            }).ToArray();
        }
    }
}