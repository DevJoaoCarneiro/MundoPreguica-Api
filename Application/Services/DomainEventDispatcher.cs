using Application.Interfaces;
using Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services
{
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public DomainEventDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            foreach (var domainEvent in domainEvents)
            {
                await DispatchDomainEventAsync(domainEvent, cancellationToken);
            }
        }

        private async Task DispatchDomainEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            var domainEventType = domainEvent.GetType();
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEventType);

            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod("HandleAsync");
                if (method == null)
                {
                    continue;
                }

                var result = method.Invoke(handler, new object[] { domainEvent, cancellationToken });
                if (result is Task task)
                {
                    await task;
                }
            }
        }
    }
}
