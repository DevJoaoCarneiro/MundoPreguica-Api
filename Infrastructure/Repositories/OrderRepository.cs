using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderRepository> _logger;
        public OrderRepository(AppDbContext context, ILogger<OrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Order> AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            return order;
        }

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetByFiltersAsync(
            string? phone,
            Domain.Entities.Enum.OrderStatus? status,
            DateTime? startDate,
            DateTime? endDate,
            int page,
            int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(phone))
            {
                query = query.Where(o => o.Client.clientPhone.Contains(phone));
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.OrderStatus == status.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endDate.Value);
            }

            var totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<Order?> GetByIdAsync(Guid orderId)
        {
            _logger.LogInformation("Consultando detalhes do pedido {OrderId} no banco de dados.", orderId);

            return await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
    }
}
