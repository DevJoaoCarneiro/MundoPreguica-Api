using Domain.entities;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<Product> AddAsync(Product product);

    }
}
