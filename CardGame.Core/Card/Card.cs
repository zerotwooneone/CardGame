using System;

namespace CardGame.Core.Card
{
    public class Card : IPlayCard
    {
        public Guid Id { get; }
        public CardValue Value { get; }
        public string Name { get; }
        public string TypeName { get; }

        public Card(Guid id,
            CardValue value,
            string name,
            string typeName)
        {
            Id = id;
            Value = value;
            Name = name;
            TypeName = typeName;
        }
    }
}