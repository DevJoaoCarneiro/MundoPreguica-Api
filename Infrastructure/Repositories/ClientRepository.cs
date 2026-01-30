using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly AppDbContext _context;

        public ClientRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Client> AddAsync(Client client)
        {
            await _context.Client.AddAsync(client);
            return client;
        }

        public async Task<Client?> GetByPhoneAsync(string phone)
        {
            return await _context.Client
                .FirstOrDefaultAsync(c => c.clientPhone == phone);
        }
    }
}
