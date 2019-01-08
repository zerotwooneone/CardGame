using System;
using CardGame.Core.Round;

namespace CardGame.Core.Card
{
    public class Card
    {
        public readonly Guid Id;
        public readonly string Name;
        public readonly Action<PlayContext> OnDiscard;
        public readonly Action<DrawContext> OnDraw;
        public readonly string TypeName;
        public readonly CardValue Value;

        public Card(Guid id,
            CardValue value,
            string name,
            string typeName,
            Action<DrawContext> specialOnDraw = null,
            Action<PlayContext> specialOnDiscard = null)
        {
            Id = id;
            Value = value;
            Name = name;
            TypeName = typeName;
            OnDraw = GetOnDraw(Value, specialOnDraw);
            OnDiscard = GetOnDiscard(Value, specialOnDiscard);
        }

        public Action<DrawContext> GetOnDraw(CardValue value, Action<DrawContext> specialOnDraw)
        {
            return drawContext =>
            {
                specialOnDraw?.Invoke(drawContext);
                switch (value)
                {
                    case CardValue.Countess:
                    default:
                        drawContext.MarkUnplayableCards();
                        break;
                }
            };
        }
        
        public Action<PlayContext> GetOnDiscard(CardValue value,
            Action<PlayContext> specialOnDiscard)
        {
            return (playContext) =>
            {
                specialOnDiscard?.Invoke(playContext);
                switch (value)
                {
                    case CardValue.Princess:
                        playContext.EliminateCurrentPlayer();
                        break;
                    case CardValue.King:
                        if (playContext.TargetPlayerId == null) break;
                        playContext.TradeHands(playContext.TargetPlayerId.Value);
                        break;
                    case CardValue.Prince:
                        if (playContext.TargetPlayerId == null) break;
                        playContext.DiscardAndDraw(playContext.TargetPlayerId.Value);
                        break;
                    case CardValue.Handmaid:
                        playContext.AddCurrentPlayerProtection();
                        break;
                    case CardValue.Baron:
                        if (playContext.TargetPlayerId == null) break;
                        playContext.CompareHands(playContext.TargetPlayerId.Value);
                        break;
                    case CardValue.Priest:
                        if (playContext.TargetPlayerId == null) break;
                        playContext.RevealHand(playContext.TargetPlayerId.Value);
                        break;
                    case CardValue.Guard:
                        if (playContext.TargetPlayerId == null) break;
                        if(playContext.GuessedCardvalue == null) break;
                        playContext.GuessAndCheck(playContext.TargetPlayerId.Value, playContext.GuessedCardvalue.Value);
                        break;
                    case CardValue.Countess:
                    default:
                        break;
                }
            };
        }
    }
}