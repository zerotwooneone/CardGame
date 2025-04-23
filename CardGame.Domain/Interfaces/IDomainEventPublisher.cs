namespace CardGame.Domain.Interfaces;

/// <summary>
/// Defines the contract for publishing domain events.
/// The Domain layer depends on this abstraction, while the
/// implementation resides in a higher layer (Application/Infrastructure).
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publishes the specified domain event.
    /// </summary>
    /// <typeparam name="TEvent">The type of the domain event.</typeparam>
    /// <param name="domainEvent">The domain event instance to publish.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent; // Ensure the event implements the marker interface
}