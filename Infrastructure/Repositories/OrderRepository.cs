using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
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
    }
}
