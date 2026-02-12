using Domain.Common;
using Domain.Entities.Enum;
using Domain.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Order : Entity
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderType TypeOrder { get; set; }
        public decimal TotalValue { get; set; }

        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;
        public List<OrderItem> Items { get; set; } = new();

        public void MarkAsCreated()
        {
            AddDomainEvent(new OrderCreatedEvent(this));
        }
    }
}
