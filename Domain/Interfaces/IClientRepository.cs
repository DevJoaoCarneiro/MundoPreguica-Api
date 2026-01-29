using Domain.entities;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IClientRepository
    {
        Task<Client> GetByPhoneAsync(string phone);

        Task<Client> AddAsync(Client user);
    }
}
