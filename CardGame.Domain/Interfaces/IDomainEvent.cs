namespace CardGame.Domain.Interfaces;

/// <summary>
/// Marker interface to identify domain events.
/// Domain events represent significant occurrences within the domain model.
/// Includes standard properties for identification, correlation, and timestamping.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this specific event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the timestamp indicating when the event occurred.
    /// </summary>
    DateTimeOffset OccurredOn { get; }

    /// <summary>
    /// Gets an optional identifier used to correlate related events,
    /// often originating from the same initial command or request.
    /// </summary>
    Guid? CorrelationId { get; } // Nullable Guid
}