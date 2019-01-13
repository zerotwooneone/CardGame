using System;

namespace CardGame.Core.Card
{
    public class Card
    {
        public readonly Guid Id;
        public readonly string Name;
        public readonly string TypeName;
        public readonly CardValue Value;

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