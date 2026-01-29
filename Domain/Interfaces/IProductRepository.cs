using Domain.entities;
using Domain.Entities;
using Domain.Entities.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<Product> AddAsync(Product product);

        Task<IEnumerable<Product>> GetByFiltersAsync(
            string? name,
            int? categoryId,
            ProductStatus? status,
            int page,
            ProductSize? size);

        Task<Product> GetByIdAsync(Guid productId);

        Task<Product>UpdateAsync(Product product);


    }
}
