using CardGame.Domain.Exceptions;

namespace CardGame.Domain.Game.GameException;

/// <summary>
/// Exception thrown when a player attempts an action (move) that is
/// invalid based on the current game state or the action's parameters.
/// Examples: Playing out of turn, targeting a protected player, invalid target/guess.
/// ErrorCode: 1001
/// </summary>
// Optional: Add Serializable attribute if needed
// [Serializable]
public class InvalidMoveException : DomainException // Inherit from base DomainException
{
    /// <summary>
    /// Defines the specific error code for InvalidMoveException.
    /// </summary>
    public const int InvalidMoveErrorCode = 1001; // Specific code >= 1000

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidMoveException"/> class.
    /// </summary>
    public InvalidMoveException()
        : base("The attempted move is invalid based on the current game state.", InvalidMoveErrorCode) // Pass code to base
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidMoveException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidMoveException(string message)
        : base(message, InvalidMoveErrorCode) // Pass message and code to base
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidMoveException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public InvalidMoveException(string message, Exception innerException)
        : base(message, innerException, InvalidMoveErrorCode) // Pass message, inner, and code to base
    {
    }

    // Optional: Constructor for serialization support
    /*
        protected InvalidMoveException(SerializationInfo info, StreamingContext context)
            : base(info, context) // Base constructor handles ErrorCode de-serialization if implemented
        {
        }
        */
}