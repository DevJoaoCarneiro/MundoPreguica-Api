using Application.Request;
using Application.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IProductServices
    {
        Task<ProductResponseDto> createNewProduct(ProductRequestDto productRequestDto);

        Task<FilterProductResponse> GetByFiltersAsync(ProductFilterRequest filter);
    }
}
