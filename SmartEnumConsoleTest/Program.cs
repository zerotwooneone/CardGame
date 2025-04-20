// In your SampleApp project

// Reference GeneratorAttributes project
// Reference EnumLikeGenerator project as Analyzer

using GeneratorAttributes;

namespace SmartEnumConsoleTest
{
    // 1. Define the partial class with the [EnumLike] attribute
    //    and private static fields marked with [GeneratedEnumValue]
    [EnumLike]
    public sealed partial class CardType // Must be partial!
    {
        // Define private static readonly fields holding the VALUE for each enum member.
        // Name must start with '_' and have [GeneratedEnumValue] attribute.
        // The generator will create public static readonly CardType fields (e.g., CardType.Guard).

        [GeneratedEnumValue]
        private static readonly int _guard = 1; // Generator creates CardType.Guard

        [GeneratedEnumValue]
        private static readonly int _priest = 2; // Generator creates CardType.Priest

        [GeneratedEnumValue]
        private static readonly int _baron = 3;

        [GeneratedEnumValue]
        private static readonly int _handmaid = 4;

        [GeneratedEnumValue]
        private static readonly int _prince = 5;

        [GeneratedEnumValue]
        private static readonly int _king = 6;

        [GeneratedEnumValue]
        private static readonly int _countess = 7;

        [GeneratedEnumValue]
        private static readonly int _princess = 8;

        // You can add other non-static or static members to this partial class manually
        public bool IsRoyalty() => this == King || this == Prince || this == Princess || this == Countess;
    }

    // 2. Use the generated members on the CardType class
    public class GameLogic
    {
        public void ProcessCard(CardType card) // Use the CardType class directly
        {
            Console.WriteLine($"Processing card: {card.Name}"); // Access generated Name property
            Console.WriteLine($"  Value: {card.Value}"); // Access generated Value property

            if (card == CardType.Guard) // Use generated static instance for comparison
            {
                Console.WriteLine("  It's the Guard!");
            }

            if (card.Value > 5) // Compare using the generated Value property
            {
                Console.WriteLine("  It's a high-rank card!");
            }

            if(card.IsRoyalty()) // Use manually added method
            {
                 Console.WriteLine("  It's royalty!");
            }
        }

        public void ListAllCards()
        {
            Console.WriteLine("\nAvailable Card Types:");
            // Use static List() method generated into CardType
            foreach (var cardType in CardType.List())
            {
                Console.WriteLine($"- {cardType.Name} (Value: {cardType.Value})");
            }
        }

        public CardType GetCardByValue(int value)
        {
            // Use static FromValue() method generated into CardType
            try
            {
                 return CardType.FromValue(value);
            }
            catch (ArgumentException ex)
            {
                 Console.WriteLine($"Error finding card: {ex.Message}");
                 throw;
            }
        }
    }

    // Main Program (Example) - Mostly unchanged
    class Program
    {
        static void Main(string[] args)
        {
            var logic = new GameLogic();

            logic.ProcessCard(CardType.Princess);
            logic.ProcessCard(CardType.Guard);
            logic.ProcessCard(CardType.King); // Test manual method

            Console.WriteLine();
            logic.ListAllCards();
            Console.WriteLine();

            try
            {
                var baron = logic.GetCardByValue(3);
                Console.WriteLine($"Found by value 3: {baron.Name}");
                var invalid = logic.GetCardByValue(99);
            }
            catch (Exception) { Console.WriteLine($"Caught exception as expected."); }

             var guard1 = CardType.Guard;
             var guard2 = CardType.FromValue(1);
             Console.WriteLine($"guard1 == guard2: {guard1 == guard2}"); // True
        }
    }
}
