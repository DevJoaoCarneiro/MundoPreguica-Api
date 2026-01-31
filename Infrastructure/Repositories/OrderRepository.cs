using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order> AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            return order;
        }

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetAllPagedAsync(int page, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .AsNoTracking();

            var totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }
    }
}
