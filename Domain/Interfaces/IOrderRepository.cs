using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> AddAsync(Order product);

        Task<(IEnumerable<Order> Orders, int TotalCount)> GetAllPagedAsync(int page, int pageSize);

        Task<Order?> GetByIdAsync(Guid orderId);
    }
}
