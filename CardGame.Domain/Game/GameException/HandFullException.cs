using CardGame.Domain.Exceptions;

namespace CardGame.Domain.Game.GameException;

/// <summary>
    /// Exception thrown when attempting to add a card to a hand that is already full (e.g., already has 2 cards).
    /// ErrorCode: 1004
    /// </summary>
    // Optional: Add Serializable attribute if needed
    // [Serializable]
    public class HandFullException : DomainException // Inherit from base DomainException
    {
        /// <summary>
        /// Defines the specific error code for HandFullException.
        /// </summary>
        public const int HandFullErrorCode = 1004; // Specific code >= 1000

        /// <summary>
        /// Initializes a new instance of the <see cref="HandFullException"/> class
        /// with a default error message.
        /// </summary>
        public HandFullException()
            : base("Cannot add card, hand is full (max 2 cards).", HandFullErrorCode) // Pass default message and code to base
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandFullException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public HandFullException(string message)
            : base(message, HandFullErrorCode) // Pass message and code to base
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandFullException"/> class
        /// with a specified error message and a reference to the inner exception
        /// that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public HandFullException(string message, Exception innerException)
            : base(message, innerException, HandFullErrorCode) // Pass message, inner, and code to base
        {
        }

        // Optional: Constructor for serialization support
        /*
        protected HandFullException(SerializationInfo info, StreamingContext context)
            : base(info, context) // Base constructor handles ErrorCode de-serialization if implemented
        {
        }
        */
    }