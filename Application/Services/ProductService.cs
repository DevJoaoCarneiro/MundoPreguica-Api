using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class ProductService : IProductServices
    {
        private readonly IProductRepository _productRepository;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepository, IImageUploadService imageUploadService, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _imageUploadService = imageUploadService;
            _logger = logger;
        }

        public async Task<ProductResponseDto> createNewProduct(ProductRequestDto productRequestDto)
        {
            try
            {
                if (productRequestDto == null)
                {
                    _logger.LogInformation("Parametros null ou vazio");
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
                    _logger.LogWarning("Falha no upload da imagem para o produto: {ProductName}", productRequestDto.Name);
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
                    Size = (ProductSize)productRequestDto.Size,
                    Status = ProductStatus.Available,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Salvando novo produto no banco de dados. ID: {ProductId}", newProduct.Id);
                var savedProduct = await _productRepository.AddAsync(newProduct);

                if (savedProduct == null)
                {
                    _logger.LogError("Erro ao persistir no banco.", newProduct.Id);
                    return new ProductResponseDto
                    {
                        Message = "Erro ao persistir no banco",
                        Status = "error",
                        Data = null
                    };
                }


                _logger.LogInformation("Produto {ProductName} criado com sucesso!", newProduct.Name);
                return new ProductResponseDto
                {
                    Message = "Product created successfully",
                    Status = "success",
                    Data = new Data
                    {
                        Id = newProduct.Id,
                        Name = newProduct.Name,
                        Category = newProduct.CategoryId,
                        Size = newProduct.Size.ToString(),
                        Price = newProduct.Price,
                        ImageUrL = newProduct.ImageUrL
                    }
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar o produto: {ProductName}", productRequestDto?.Name);

                return new ProductResponseDto
                {
                    Message = "Error while is creating a product.: " +ex.Message,
                    Status = "error",
                    Data = null
                };
            }
            

        }

        public async Task<FilterProductResponse> GetByFiltersAsync(ProductFilterRequest filter)
        {
            try
            {
                _logger.LogInformation("Iniciando busca filtrada de produtos");
                var products = await _productRepository.GetByFiltersAsync(
                    filter.Name,
                    filter.CategoryId,
                    filter.Status,
                    filter.Page,
                    filter.Size);

                var productList = products.Select(p => new DataResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category?.Name ?? "Sem Categoria",
                    Size = p.Size.ToString(),
                    Status = p.Status.ToString(),
                    Price = p.Price,
                    ImageUrL = p.ImageUrL
                }).ToList();

                _logger.LogInformation("Busca finalizada. Quantidade de produtos encontrados: {Count}", productList.Count);

                return new FilterProductResponse
                {
                    Message = productList.Any() ? "Produtos listados com sucesso" : "Nenhum produto encontrado",
                    Status = "success",
                    DataList = productList
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar consulta filtrada de produtos.");
                return new FilterProductResponse
                {
                    Message = "Erro ao processar consulta: " + ex.Message,
                    Status = "error",
                    DataList = null
                };
            }
        }
    }
}
