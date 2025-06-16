using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CardGame.Domain;

namespace CardGame.Decks.Premium
{
    /// <summary>
    /// Defines the card ranks for the Love Letter Premium expansion deck.
    /// Each rank is a unique instance with its own Id, Name, and Value,
    /// allowing multiple distinct ranks to share the same numerical value.
    /// </summary>
    public sealed class PremiumCardRank
    {
        public Guid Id { get; }
        public string Name { get; }
        public int Value { get; }
        public int QuantityInDeck { get; } 
        public string Description { get; } 
        public bool RequiresTarget { get; } 
        public bool CanTargetSelf { get; } 

        // Private constructor to ensure instances are only created via static fields
        private PremiumCardRank(Guid id, string name, int value, int quantityInDeck, string description, bool requiresTarget, bool canTargetSelf)
        {
            Id = id;
            Name = name;
            Value = value;
            QuantityInDeck = quantityInDeck; 
            Description = description;       
            RequiresTarget = requiresTarget;   
            CanTargetSelf = canTargetSelf;     
        }

        // Define unique Guids and instances for each card rank
        // Order matters for FromValue(): primary/original cards for a value should be listed first.
        public static readonly PremiumCardRank Jester        = new(new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"), "Jester", 0, 1, "When played, you may give another player the \"Jester token\". If the player holding the Jester token at the end of the round wins that round, the player who gave them the Jester token also wins an Affection Token.", true, false);
        public static readonly PremiumCardRank Assassin      = new(new Guid("f8c9c3b0-8c1a-4b8c-9e0a-3a0b1d6c9e1f"), "Assassin", 0, 1, "Choose another player and name a card rank. If that player has that card, they are eliminated. If you are wrong, you are eliminated. (Note: CardEffects.md describes a reaction. This is an active play effect as implemented in PremiumDeckProvider).", true, false);

        public static readonly PremiumCardRank Guard         = new(new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), "Guard", 1, 8, "Choose another player and name a card rank (other than Guard). If that player has a card of that rank in their hand, they are eliminated from the round.", true, false);
        
        public static readonly PremiumCardRank Priest        = new(new Guid("72f3b478-a70a-4089-9086-96a507f0dd8b"), "Priest", 2, 2, "Look at another player's hand.", true, false);
        public static readonly PremiumCardRank Cardinal      = new(new Guid("1c9e3b7a-2d8f-4c6e-b0a1-9d5c8f7b2a1d"), "Cardinal", 2, 2, "Choose any two players (these can include yourself). The chosen players trade the cards currently in their hands. If you were one of the two players who traded, you may then look at the hand of the other player involved in the trade.", true, true);
        
        public static readonly PremiumCardRank Baron         = new(new Guid("4b8c9e0a-3a0b-1d6c-9e1f-f8c9c3b08c1a"), "Baron", 3, 2, "Choose another player. You and that player secretly compare hands. The player with the lower rank card is out of the round.", true, false);
        public static readonly PremiumCardRank Baroness      = new(new Guid("d4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f90"), "Baroness", 3, 2, "You may privately look at the hand(s) of one or two other players.", true, false);
        
        public static readonly PremiumCardRank Handmaid      = new(new Guid("9e1f0f8f-ad5b-d9cb-469f-a16570867728"), "Handmaid", 4, 2, "Until your next turn, ignore all effects from other players' cards (you cannot be targeted).", false, true);
        public static readonly PremiumCardRank Sycophant     = new(new Guid("b2c3d4e5-f6a7-b8c9-0d1e-2f3a4b5c6d7e"), "Sycophant", 4, 2, "Choose another player. The next card played by any player that requires targeting an opponent must target the player you chose with the Sycophant (if they are a valid target for that card's effect).", true, false);
        
        public static readonly PremiumCardRank Prince        = new(new Guid("ad5bd9cb-469f-a165-7086-7728950e0f8f"), "Prince", 5, 2, "Choose any player (including yourself). That player discards the card in their hand and draws a new card. If the discarded card is the Princess, that player is eliminated.", true, true);
        public static readonly PremiumCardRank Count         = new(new Guid("c3d4e5f6-a7b8-c90d-1e2f-3a4b5c6d7e8f"), "Count", 5, 2, "If you have played or discarded the Count this round and you are still in the round when it ends, add +1 to the strength of the card in your hand for the purpose of end-of-round comparison.", false, false); // No direct target on play
        
        public static readonly PremiumCardRank King          = new(new Guid("a1657086-7728-950e-0f8f-ad5bd9cb469f"), "King", 6, 1, "Trade the card in your hand with the card held by another player of your choice.", true, false);
        public static readonly PremiumCardRank Constable     = new(new Guid("950e0f8f-ad5b-d9cb-469f-a16570867728"), "Constable", 6, 1, "If you have played or discarded the Constable this round and you are eliminated from the round before it ends, you win an Affection Token.", false, false); // No direct target on play
        
        public static readonly PremiumCardRank Countess      = new(new Guid("0d1e2f3a-4b5c-6d7e-8f90-a1b2c3d4e5f6"), "Countess", 7, 1, "If you have this card in your hand along with either the King (Rank 6) or the Prince (Rank 5), you must discard the Countess. Playing or discarding her otherwise has no effect.", false, false); // No direct target on play
        public static readonly PremiumCardRank DowagerQueen  = new(new Guid("1e2f3a4b-5c6d-7e8f-90a1-b2c3d4e5f6a7"), "Dowager Queen", 7, 1, "Choose another player. You and that player privately compare the cards in your hands. The player whose card has a higher strength (rank) is eliminated from the round. (Ties result in no elimination, assumed).", true, false);
        
        public static readonly PremiumCardRank Princess      = new(new Guid("d9cb469f-a165-7086-7728-950e0f8fad5b"), "Princess", 8, 1, "If you discard this card for any reason (even by a Prince's effect), you are eliminated from the round.", false, false); // No direct target on play
        public static readonly PremiumCardRank Bishop        = new(new Guid("e0a1b2c3-d4e5-f6a7-b8c9-0d1e2f3a4b5c"), "Bishop", 9, 1, "Choose another player and declare a card rank (number). If the chosen player's hand contains a card of that rank: you immediately win an Affection Token (the round does not end), and the chosen player must discard their hand and draw a new card. Special: If holding at round end, Bishop beats all but Princess.", true, false);

        private static readonly List<PremiumCardRank> _allRanks = new List<PremiumCardRank>();

        static PremiumCardRank()
        {
            var fields = typeof(PremiumCardRank).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(f => f.IsInitOnly && f.FieldType == typeof(PremiumCardRank));
            foreach (var field in fields)
            {
                if (field.GetValue(null) is PremiumCardRank rankInstance)
                {
                    _allRanks.Add(rankInstance);
                }
            }
        }

        /// <summary>
        /// Gets the primary PremiumCardRank from its numerical value. 
        /// If multiple ranks share the same value (e.g., Priest and Cardinal are both 2),
        /// this method returns the one declared first among them (conventionally, the original base game card rank).
        /// For guessing or identifying a specific non-primary rank, use FromName(string) or FromId(Guid).
        /// </summary>
        public static PremiumCardRank? FromValue(int value) => _allRanks.FirstOrDefault(r => r.Value == value);

        /// <summary>
        /// Gets a PremiumCardRank from its name (case-insensitive).
        /// </summary>
        public static PremiumCardRank? FromName(string name) => _allRanks.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Gets a PremiumCardRank from its unique Guid Id.
        /// </summary>
        public static PremiumCardRank? FromId(Guid id) => _allRanks.FirstOrDefault(r => r.Id == id);

        /// <summary>
        /// Gets a list of all defined PremiumCardRank instances.
        /// </summary>
        public static IEnumerable<PremiumCardRank> All() => _allRanks.AsReadOnly();
    }
}
