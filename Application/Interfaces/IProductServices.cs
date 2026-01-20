using Application.Request;
using Application.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IProductServices
    {
        Task<ProductResponseDto> createNewProduct(ProductRequestDto productRequestDto);
    }
}
