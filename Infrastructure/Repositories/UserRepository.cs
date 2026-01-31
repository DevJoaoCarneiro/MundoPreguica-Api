using Domain.entities;
using Domain.Repository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<User> AddAsync(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch(Exception)
            {
                return null;
            }
            
        }


        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetByIdAsync(Guid userId)
        {
            return await _context.Users.FindAsync(userId);
        }


        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetByResetTokenAsync(string resetToken)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.PasswordHash == resetToken);

        }
    }
}
