using CardGame.Domain.Exceptions;

namespace CardGame.Domain.Game.GameException;

/// <summary>
/// Exception thrown when attempting to remove a card type from a hand where it is not present.
/// ErrorCode: 1005
/// </summary>
// Optional: Add Serializable attribute if needed
// [Serializable]
public class CardNotFoundInHandException : DomainException // Inherit from base DomainException
{
    /// <summary>
    /// Defines the specific error code for CardNotFoundInHandException.
    /// </summary>
    public const int CardNotFoundErrorCode = 1005; // Specific code >= 1000

    /// <summary>
    /// Gets the name of the card type that was not found.
    /// </summary>
    public string CardTypeName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CardNotFoundInHandException"/> class.
    /// </summary>
    /// <param name="cardTypeName">The name of the card type that could not be found.</param>
    public CardNotFoundInHandException(string cardTypeName)
        : base($"Cannot remove card type '{cardTypeName}', not found in hand.", CardNotFoundErrorCode) // Pass message and code to base
    {
        CardTypeName = cardTypeName ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CardNotFoundInHandException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="cardTypeName">The name of the card type that could not be found.</param>
    public CardNotFoundInHandException(string message, string cardTypeName)
        : base(message, CardNotFoundErrorCode) // Pass message and code to base
    {
        CardTypeName = cardTypeName ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CardNotFoundInHandException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    /// <param name="cardTypeName">The name of the card type that could not be found.</param>
    public CardNotFoundInHandException(string message, Exception innerException, string cardTypeName)
        : base(message, innerException, CardNotFoundErrorCode) // Pass message, inner, and code to base
    {
        CardTypeName = cardTypeName ?? string.Empty;
    }

    // Optional: Constructor for serialization support
    /*
        protected CardNotFoundInHandException(SerializationInfo info, StreamingContext context)
            : base(info, context) // Base constructor handles ErrorCode de-serialization if implemented
        {
            // De-serialize CardTypeName if needed
            // CardTypeName = info.GetString(nameof(CardTypeName));
        }

        // Optional: Override GetObjectData for serialization
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            // Serialize CardTypeName if needed
            // info.AddValue(nameof(CardTypeName), CardTypeName);
        }
        */
}