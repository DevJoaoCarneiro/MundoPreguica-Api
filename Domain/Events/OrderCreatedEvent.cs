using Domain.Common;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Events
{
    public class OrderCreatedEvent : IDomainEvent
    {
        public Order Order { get; }
        public OrderCreatedEvent(Order order)
        {
            Order = order;
        }
    }
}
