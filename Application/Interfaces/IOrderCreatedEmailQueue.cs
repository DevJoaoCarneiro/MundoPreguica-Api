using Application.Notifications;

namespace Application.Interfaces
{
    public interface IOrderCreatedEmailQueue
    {
        Task QueueAsync(OrderCreatedEmailNotification notification, CancellationToken cancellationToken = default);
        Task<OrderCreatedEmailNotification> DequeueAsync(CancellationToken cancellationToken);
    }
}
