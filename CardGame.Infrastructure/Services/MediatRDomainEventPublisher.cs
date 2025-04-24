using CardGame.Application.Common.Notifications;
using CardGame.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Infrastructure.Services;

/// <summary>
/// Implements IDomainEventPublisher using MediatR.
/// Dynamically wraps domain events in the correct DomainEventNotification<T>
/// before publishing via MediatR.
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
    /// Publishes a domain event by dynamically wrapping it in the appropriate
    /// DomainEventNotification<TEvent> and sending it via MediatR.
    /// </summary>
    // Method signature changed: no longer generic, accepts IDomainEvent
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent));
        }

        _logger.LogDebug("Publishing domain event: {DomainEventType} - {EventId}", domainEvent.GetType().Name, domainEvent.EventId);

        // 1. Get the actual runtime type of the domain event
        Type domainEventType = domainEvent.GetType();

        // 2. Construct the generic DomainEventNotification<> type using the actual event type
        Type wrapperType = typeof(DomainEventNotification<>).MakeGenericType(domainEventType);

        // 3. Create an instance of the wrapper, passing the domain event to its constructor
        // Activator.CreateInstance returns object?, so we need a cast.
        // Ensure DomainEventNotification has a public constructor accepting the domain event.
        object? notificationWrapper = Activator.CreateInstance(wrapperType, domainEvent);

        if (notificationWrapper == null)
        {
            // This should ideally not happen if the wrapper and event types are valid
            _logger.LogError("Could not create DomainEventNotification wrapper for event type {DomainEventType}", domainEventType.Name);
            return Task.CompletedTask; // Or throw?
        }

        // 4. Publish the wrapper object via MediatR.
        // MediatR's Publish method accepts 'object' or 'INotification'.
        // It will internally dispatch based on the actual runtime type of notificationWrapper.
        return _mediator.Publish(notificationWrapper, cancellationToken);
    }
}