// In your SampleApp project (e.g., Console App or Class Library)

// Make sure this project references the SmartEnumGeneratorAttributes project.
// Also, ensure it references the Source Generator project itself as an Analyzer (see Build Config).
// Assume the SmartEnumBase namespace (containing the base class) is accessible.

using SmartEnumGeneratorAttributes;
// For the base class

namespace SmartEnumConsoleTest
{
    // 1. Define the source enum with attributes
    // Note: The 'Rank' property name matches the first ctor arg in SmartEnumProps
    //       The 'Description' property name matches the second ctor arg
    //       The 'RequiresTarget' property name matches the third ctor arg
    //       The 'RequiresGuess' property name matches the fourth ctor arg
    // THIS MAPPING IS FRAGILE in the current generator - relies on order and primitive naming.

    [GenerateSmartEnum(ValuePropertyName = "Rank", GeneratedClassName = "CardType")] // Override generated name
    internal enum CardTypeDefinition // Internal - just for definition
    {
        // Order of values in SmartEnumProps MUST match the expected constructor parameters
        // (after value and name). Generator currently infers types/names poorly.
        // Assumed constructor: CardType(int value, string name, string description, bool requiresTarget, bool requiresGuess)

        Guard = 1, 
        Priest = 2,
        Baron = 3,
        Handmaid = 4,
        Prince = 5,
        King = 6,
        Countess = 7,
        Princess = 8
    }

    // 2. Use the generated Smart Enum class (CardType)
    public class GameLogic
    {
        public void ProcessCard(CardType card) // Use the generated CardType class
        {
            Console.WriteLine($"Processing card: {card.Name}");
            Console.WriteLine($"  Rank: {card.Rank}"); // Access generated property 'Rank' (alias for Value)
            // Console.WriteLine($"  Description: {card.Description}"); // Access generated property
            // Console.WriteLine($"  Requires Target: {card.RequiresTarget}"); // Access generated property
            // Console.WriteLine($"  Requires Guess: {card.RequiresGuess}"); // Access generated property

            if (card == CardType.Guard) // Type-safe comparison
            {
                Console.WriteLine("  It's the Guard!");
            }

            if (card.Rank > 5)
            {
                Console.WriteLine("  It's a high-rank card!");
            }
        }

        public void ListAllCards()
        {
            Console.WriteLine("\nAvailable Card Types:");
            // Use static methods from the base class (or generated ones)
            foreach (var cardType in CardType.List())
            {
                Console.WriteLine($"- {cardType.Name} (Rank: {cardType.Rank})");
            }
        }

        public CardType GetCardByRank(int rank)
        {
            // Use static methods from the base class (or generated ones)
            if (CardType.TryFromValue(rank, out var cardType))
            {
                 return cardType!;
            }
            throw new ArgumentException("Invalid rank", nameof(rank));
        }
    }

    // Main Program (Example)
    class Program
    {
        static void Main(string[] args)
        {
            var logic = new GameLogic();

            logic.ProcessCard(CardType.Princess); // Use static instance
            logic.ProcessCard(CardType.Guard);

            Console.WriteLine();

            logic.ListAllCards();

            Console.WriteLine();

            try
            {
                var baron = logic.GetCardByRank(3);
                Console.WriteLine($"Found by rank 3: {baron.Name}");

                var invalid = logic.GetCardByRank(99); // This will throw
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
