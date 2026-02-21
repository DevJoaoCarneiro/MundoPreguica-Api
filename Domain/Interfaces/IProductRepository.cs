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

        Task<(IEnumerable<IGrouping<string, Product>> Products, int TotalCount)> GetByFiltersAsync(
            string? name,
            int? categoryId,
            int? gender,
            bool? isPromotion,
            ProductStatus? status,
            ProductSize? size,
            int page,
            int pageSize);

        Task<Product> GetByIdAsync(Guid productId);

        Task<Product>UpdateAsync(Product product);

        Task<IEnumerable<Product>> GetByNameAsync(string name);


    }
}
