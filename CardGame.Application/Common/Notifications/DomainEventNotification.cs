using CardGame.Domain.Interfaces;
using MediatR;

namespace CardGame.Application.Common.Notifications;

/// <summary>
/// A wrapper class that implements MediatR's INotification interface
/// and holds an instance of a domain event (which implements IDomainEvent).
/// This decouples the domain events themselves from the MediatR library.
/// </summary>
/// <typeparam name="TDomainEvent">The type of the domain event being wrapped.</typeparam>
public class DomainEventNotification<TDomainEvent> : INotification where TDomainEvent : IDomainEvent
{
    /// <summary>
    /// Gets the domain event instance.
    /// </summary>
    public TDomainEvent DomainEvent { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventNotification{TDomainEvent}"/> class.
    /// </summary>
    /// <param name="domainEvent">The domain event to wrap.</param>
    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent ?? throw new ArgumentNullException(nameof(domainEvent));
    }
}