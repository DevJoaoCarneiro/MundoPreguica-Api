using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class ProductService : IProductServices
    {
        private readonly IProductRepository _productRepository;
        private readonly IImageUploadService _imageUploadService;

        public ProductService(IProductRepository productRepository, IImageUploadService imageUploadService)
        {
            _productRepository = productRepository;
            _imageUploadService = imageUploadService;
        }

        public async Task<ProductResponseDto> createNewProduct(ProductRequestDto productRequestDto)
        {
            try
            {
                if (productRequestDto == null)
                {
                    return new ProductResponseDto
                    {
                        Message = "Parameters is empty or null",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                var imageUrl = await _imageUploadService.UploadImageAsync(productRequestDto.Image);

                if (imageUrl == null)
                {
                    return new ProductResponseDto
                    {
                        Message = "Image is required",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                
                var newProduct = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = productRequestDto.Name,
                    CategoryId = productRequestDto.CategoryId,
                    Price = productRequestDto.Price,
                    ImageUrL = imageUrl,
                    Status = ProductStatus.Available,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _productRepository.AddAsync(newProduct);

                return new ProductResponseDto
                {
                    Message = "Product created successfully",
                    Status = "success",
                    Data = new Data
                    {
                        Id = newProduct.Id,
                        Name = newProduct.Name,
                        Category = newProduct.CategoryId,
                        Price = newProduct.Price,
                        ImageUrL = newProduct.ImageUrL
                    }
                };

            }
            catch (Exception ex)
            {
                return new ProductResponseDto
                {
                    Message = "Error while is creating a product.: " +ex.Message,
                    Status = "success",
                    Data = null
                };
            }
            

        }
    }
}
