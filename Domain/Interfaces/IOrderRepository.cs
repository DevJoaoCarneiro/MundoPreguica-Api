using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> AddAsync(Order product);

        Task<(IEnumerable<Order> Orders, int TotalCount)> GetByFiltersAsync(
            string? phone,
            Domain.Entities.Enum.OrderStatus? status,
            Domain.Entities.Enum.OrderType? orderType,
            DateTime? startDate,
            DateTime? endDate,
            int page,
            int pageSize);

        Task<Order?> GetByIdAsync(Guid orderId);

        Task UpdateAsync(Order order);
    }
}
