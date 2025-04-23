using CardGame.Application.Common.Notifications;
using CardGame.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Infrastructure.Services;

/// <summary>
/// Implements IDomainEventPublisher using MediatR.
/// Wraps domain events in DomainEventNotification before publishing.
/// </summary>
public class MediatRDomainEventPublisher : IDomainEventPublisher
{
    private readonly IMediator _mediator;
    private readonly ILogger<MediatRDomainEventPublisher> _logger; // Optional logger

    public MediatRDomainEventPublisher(IMediator mediator, ILogger<MediatRDomainEventPublisher> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Handle logger injection
    }

    /// <summary>
    /// Publishes a domain event by wrapping it in a DomainEventNotification
    /// and sending it via MediatR.
    /// </summary>
    public Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent));
        }

        _logger.LogDebug("Publishing domain event: {DomainEventType} - {EventId}", domainEvent.GetType().Name,
            domainEvent.EventId);

        // Create the generic wrapper notification instance
        var notificationWrapper = new DomainEventNotification<TEvent>(domainEvent);

        // Publish the wrapper via MediatR
        // MediatR handlers will subscribe to DomainEventNotification<TEvent>
        return _mediator.Publish(notificationWrapper, cancellationToken);
    }
}