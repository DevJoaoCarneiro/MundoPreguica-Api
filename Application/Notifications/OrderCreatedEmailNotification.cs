using Domain.Entities.Enum;

namespace Application.Notifications
{
    public record OrderCreatedEmailNotification(
        Guid OrderId,
        DateTime OrderDate,
        OrderType OrderType,
        decimal TotalValue,
        string ClientName,
        string ClientPhone,
        IReadOnlyCollection<OrderCreatedEmailItemNotification> Items);

    public record OrderCreatedEmailItemNotification(
        string ProductName,
        string Size,
        int Quantity,
        decimal UnitPrice);
}
