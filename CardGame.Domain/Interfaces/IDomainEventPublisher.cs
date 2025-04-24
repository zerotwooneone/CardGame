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
    /// The implementation is responsible for dispatching to appropriate handlers.
    /// </summary>
    /// <param name="domainEvent">The domain event instance to publish.</param> // Parameter is now just IDomainEvent
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}