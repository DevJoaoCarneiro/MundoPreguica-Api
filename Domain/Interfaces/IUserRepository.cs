using Domain.entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Repository
{
    public interface IUserRepository
    {
        Task<User> AddAsync(User user);

        Task<User> GetByEmailAsync(string email);

        Task<User> GetByIdAsync(Guid userId);

        Task<User> UpdateAsync(User user);

        Task<User?> GetByResetTokenAsync(string resetToken);
    }
}
