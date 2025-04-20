using CardGame.Domain.Exceptions;

namespace CardGame.Domain.Game.GameException;

/// <summary>
    /// Exception thrown when an attempt is made to draw a card from an empty deck.
    /// ErrorCode: 1003
    /// </summary>
    // Optional: Add Serializable attribute if needed
    // [Serializable]
    public class EmptyDeckException : DomainException // Inherit from base DomainException
    {
        /// <summary>
        /// Defines the specific error code for EmptyDeckException.
        /// </summary>
        public const int EmptyDeckErrorCode = 1003; // Specific code >= 1000

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyDeckException"/> class
        /// with a default error message.
        /// </summary>
        public EmptyDeckException()
            : base("Cannot draw card from an empty deck.", EmptyDeckErrorCode) // Pass default message and code to base
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyDeckException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public EmptyDeckException(string message)
            : base(message, EmptyDeckErrorCode) // Pass message and code to base
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyDeckException"/> class
        /// with a specified error message and a reference to the inner exception
        /// that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public EmptyDeckException(string message, Exception innerException)
            : base(message, innerException, EmptyDeckErrorCode) // Pass message, inner, and code to base
        {
        }

        // Optional: Constructor for serialization support
        /*
        protected EmptyDeckException(SerializationInfo info, StreamingContext context)
            : base(info, context) // Base constructor handles ErrorCode de-serialization if implemented
        {
        }
        */
    }