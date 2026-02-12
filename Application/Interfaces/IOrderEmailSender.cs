using Application.Notifications;

namespace Application.Interfaces
{
    public interface IOrderEmailSender
    {
        Task SendNewOrderAsync(OrderCreatedEmailNotification notification, CancellationToken cancellationToken = default);
    }
}
