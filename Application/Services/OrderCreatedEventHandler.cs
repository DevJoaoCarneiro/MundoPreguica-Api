using Application.Interfaces;
using Application.Notifications;
using Domain.Events;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class OrderCreatedEventHandler : IDomainEventHandler<OrderCreatedEvent>
    {
        private readonly IOrderCreatedEmailQueue _queue;
        private readonly ILogger<OrderCreatedEventHandler> _logger;

        public OrderCreatedEventHandler(IOrderCreatedEmailQueue queue, ILogger<OrderCreatedEventHandler> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        public async Task HandleAsync(OrderCreatedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            if (domainEvent?.Order == null)
            {
                return;
            }


            var order = domainEvent.Order;
            var notification = new OrderCreatedEmailNotification(
                order.Id,
                order.OrderDate,
                order.TypeOrder,
                order.TotalValue,
                order.Client?.clientName ?? string.Empty,
                order.Client?.clientPhone ?? string.Empty,
                order.Items
                    .Select(item => new OrderCreatedEmailItemNotification(
                        item.Product?.Name ?? "Produto sem nome",
                        item.Product?.Size.ToString() ?? "",
                        item.Quantity,
                        item.UnitPrice))
                    .ToList());

            await _queue.QueueAsync(notification, cancellationToken);

            _logger.LogInformation("Pedido {OrderId} enfileirado para envio de e-mail.", order.Id);
        }
    }
}
