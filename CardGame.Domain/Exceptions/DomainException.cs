namespace CardGame.Domain.Exceptions;

/// <summary>
/// Base class for exceptions thrown by the Domain layer.
/// Includes a unique ErrorCode for classification.
/// </summary>
// Optional: Add Serializable attribute if needed
// [Serializable]
public class DomainException : Exception
{
    /// <summary>
    /// Gets the integer error code associated with this exception type.
    /// Minimum value should be 1000.
    /// </summary>
    public int ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class.
    /// Requires derived classes to provide an error code.
    /// </summary>
    protected DomainException(int errorCode) // Made protected - base shouldn't be thrown directly without code
        : base("A domain rule violation occurred.")
    {
        ValidateErrorCode(errorCode);
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class
    /// with a specified error message and error code.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="errorCode">The integer error code (>= 1000).</param>
    public DomainException(string message, int errorCode)
        : base(message)
    {
        ValidateErrorCode(errorCode);
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class
    /// with a specified error message, error code, and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="errorCode">The integer error code (>= 1000).</param>
    public DomainException(string message, Exception innerException, int errorCode)
        : base(message, innerException)
    {
        ValidateErrorCode(errorCode);
        ErrorCode = errorCode;
    }

    // Optional: Constructor for serialization support
    /*
    protected DomainException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        // De-serialize ErrorCode if needed
        // ErrorCode = info.GetInt32(nameof(ErrorCode));
    }

    // Optional: Override GetObjectData for serialization
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        // Serialize ErrorCode if needed
        // info.AddValue(nameof(ErrorCode), ErrorCode);
    }
    */

    /// <summary>
    /// Validates that the error code meets the minimum requirement.
    /// </summary>
    private static void ValidateErrorCode(int errorCode)
    {
        if (errorCode < 1000)
        {
            // Use ArgumentOutOfRangeException as this is an issue with the value passed to the constructor
            throw new ArgumentOutOfRangeException(nameof(errorCode), "ErrorCode must be 1000 or greater.");
        }
    }
}