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

        [SmartEnumProps("Guess another player's non-Guard card.", true, true)] // desc, target, guess
        Guard = 1, // Underlying value (Rank)

        [SmartEnumProps("Look at another player's hand.", true, false)] // desc, target, guess=false
        Priest = 2,

        [SmartEnumProps("Compare hands; lower rank is out.", true, false)]
        Baron = 3,

        [SmartEnumProps("Protection until your next turn.", false, false)]
        Handmaid = 4,

        [SmartEnumProps("Discard hand and draw.", true, false)] // Assuming target=true means targettable, even if self
        Prince = 5,

        [SmartEnumProps("Trade hands.", true, false)]
        King = 6,

        [SmartEnumProps("Must discard if holding King or Prince.", false, false)]
        Countess = 7,

        [SmartEnumProps("Lose if discarded.", false, false)]
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
