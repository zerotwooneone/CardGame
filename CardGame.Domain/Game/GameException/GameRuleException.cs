using CardGame.Domain.Exceptions;

namespace CardGame.Domain.Game.GameException;

/// <summary>
/// Exception thrown when an action violates a fundamental rule of the game
/// that isn't strictly related to the validity of a specific move's parameters or timing.
/// Examples: Must play Countess rule, trying to start a round when game is over.
/// ErrorCode: 1002
/// </summary>
// Optional: Add Serializable attribute if needed
// [Serializable]
public class GameRuleException : DomainException // Inherit from base DomainException
{
    /// <summary>
    /// Defines the specific error code for GameRuleException.
    /// </summary>
    public const int GameRuleErrorCode = 1002; // Specific code >= 1000

    /// <summary>
    /// Initializes a new instance of the <see cref="GameRuleException"/> class.
    /// </summary>
    public GameRuleException()
        : base("A fundamental game rule was violated.", GameRuleErrorCode) // Pass code to base
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameRuleException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public GameRuleException(string message)
        : base(message, GameRuleErrorCode) // Pass message and code to base
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameRuleException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public GameRuleException(string message, Exception innerException)
        : base(message, innerException, GameRuleErrorCode) // Pass message, inner, and code to base
    {
    }

    // Optional: Constructor for serialization support
    /*
        protected GameRuleException(SerializationInfo info, StreamingContext context)
            : base(info, context) // Base constructor handles ErrorCode de-serialization if implemented
        {
        }
        */
}