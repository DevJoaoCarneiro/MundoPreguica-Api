using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace Application.Services
{
    public class ProductService : IProductServices
    {
        private readonly IProductRepository _productRepository;
        private readonly IImageUploadService _imageUploadService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepository, IImageUploadService imageUploadService, IUnitOfWork unitOfWork, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _imageUploadService = imageUploadService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ProductResponseDto> CreateNewProduct(ProductRequestDto productRequestDto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (productRequestDto == null || productRequestDto.Variant == null || !productRequestDto.Variant.Any())
                {
                    _logger.LogInformation("Parametros null ou vazio");
                    return new ProductResponseDto
                    {
                        Message = "Parametros estao vazios ou nulos",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                if (!TryNormalizePriceField(productRequestDto.Price, true, out var normalizedPrice))
                {
                    _logger.LogWarning("Preço inválido informado para criação do produto: {Price}", productRequestDto.Price);
                    return new ProductResponseDto
                    {
                        Message = "Preço inválido",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                var requiredPrice = normalizedPrice ?? 0m;

                decimal? normalizedOldPrice = null;
                if (productRequestDto.IsPromotion)
                {
                    if (!TryNormalizePriceField(productRequestDto.OldPrice, false, out var parsedOldPrice))
                    {
                        _logger.LogWarning("Preço antigo inválido informado para criação do produto: {OldPrice}", productRequestDto.OldPrice);
                        return new ProductResponseDto
                        {
                            Message = "Preço antigo inválido",
                            Status = "invalid_argument",
                            Data = null
                        };
                    }

                    normalizedOldPrice = parsedOldPrice;
                }

                var duplicateSizes = productRequestDto.Variant
                    .GroupBy(v => v.Size)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateSizes.Any())
                {
                    _logger.LogWarning("Tamanhos duplicados na requisição: {Sizes}", string.Join(",", duplicateSizes));
                    return new ProductResponseDto
                    {
                        Message = "Tamanhos duplicados na requisicao",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                var imageUrl = await _imageUploadService.UploadImageAsync(productRequestDto.Image);

                if (string.IsNullOrEmpty(imageUrl))
                {
                    _logger.LogWarning("Falha no upload da imagem para o produto: {ProductName}", productRequestDto.Name);
                    return new ProductResponseDto
                    {
                        Message = "Imagem obrigatoria",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                var existingVariants = (await _productRepository.GetByNameAsync(productRequestDto.Name)).ToList();
                var existingSizes = existingVariants.Select(v => (int)v.Size).ToHashSet();
                var requestedSizes = productRequestDto.Variant.Select(v => v.Size).ToList();

                if (requestedSizes.Any(s => existingSizes.Contains(s)))
                {
                    _logger.LogWarning("Tentativa de criar tamanhos já existentes para o produto: {ProductName}", productRequestDto.Name);
                    return new ProductResponseDto
                    {
                        Message = "Tamanho ja existe para este produto",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                var createdProducts = new List<Product>();

                foreach (var variant in productRequestDto.Variant)
                {
                    var newProduct = new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = productRequestDto.Name,
                        CategoryId = productRequestDto.CategoryId,
                        Price = requiredPrice,
                        IsPromotion = productRequestDto.IsPromotion,
                        OldPrice = productRequestDto.IsPromotion ? normalizedOldPrice : null,
                        ImageUrL = imageUrl,
                        Size = (ProductSize)variant.Size,
                        Stock = variant.Stock,
                        Gender = productRequestDto.Gender,
                        Status = ProductStatus.Available,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _productRepository.AddAsync(newProduct);
                    createdProducts.Add(newProduct);
                }
                await _unitOfWork.CommitAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Grade do produto {ProductName} criada com {Count} variações.", productRequestDto.Name, createdProducts.Count);

                return new ProductResponseDto
                {
                    Message = "Grade de produto criada com sucesso",
                    Status = "success",
                    Data = new Data
                    {
                        Id = createdProducts.First().Id,
                        Name = productRequestDto.Name,
                        Gender = productRequestDto.Gender,
                        Price = requiredPrice,
                        IsPromotion = productRequestDto.IsPromotion,
                        OldPrice = productRequestDto.IsPromotion ? normalizedOldPrice : null,
                        Category = productRequestDto.CategoryId,
                        ImageUrL = imageUrl,
                    }
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Erro ao criar grade de produtos.");
                return new ProductResponseDto { Message = "Erro interno: " + ex.Message, Status = "error" };
            }


        }

        public async Task<FilterProductResponse> GetByFiltersAsync(ProductFilterRequest filter)
        {
            try
            {
                int currentPage = filter.Page > 0 ? filter.Page : 1;
                int pageSize = filter.PageSize > 0 ? filter.PageSize : 10;

                _logger.LogInformation("Buscando produtos filtrados e agrupando por nome.");

                var (products, totalItems) = await _productRepository.GetByFiltersAsync(
                        filter.Name,
                        filter.CategoryId,
                        filter.gender,
                    filter.IsPromotion,
                        filter.Status,
                        filter.Size,
                        currentPage,
                        pageSize);

                var groupedList = products
                    .Select(g => new DataResponse
                    {
                        Id = g.First().Id,
                        Name = g.Key,
                        Category = g.First().Category?.Name ?? "Sem Categoria",
                        Gender = g.First().Gender,
                        Price = g.First().Price,
                        IsPromotion = g.First().IsPromotion,
                        OldPrice = g.First().OldPrice,
                        ImageUrL = g.First().ImageUrL,
                        Status = GetDisplayName(g.First().Status),
                        Variants = g.Select(v => new VariantInfo
                        {
                            Id = v.Id,
                            Size = GetDisplayName(v.Size),
                            Stock = v.Stock
                        }).OrderBy(v => v.Size).ToList()
                    }).ToList();

                _logger.LogInformation("Busca finalizada. Modelos encontrados: {Count}", groupedList.Count);

                return new FilterProductResponse
                {
                    Message = groupedList.Any() ? "Produtos listados com sucesso" : "Nenhum produto encontrado",
                    Status = "success",
                    TotalItems = totalItems,
                    CurrentPage = currentPage,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                    DataList = groupedList
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na listagem agrupada de produtos.");
                return new FilterProductResponse
                {
                    Message = "Erro interno: " + ex.Message,
                    Status = "error"
                };
            }
        }


        public async Task<ProductResponseDto> GetByIdAsync(Guid productId)
        {
            try
            {
                _logger.LogInformation("Iniciando busca detalhada do modelo para o produto ID: {ProductId}", productId);

                var productVariant = await _productRepository.GetByIdAsync(productId);

                if (productVariant == null)
                {
                    _logger.LogInformation("Produto ID {ProductId} não encontrado.", productId);
                    return new ProductResponseDto
                    {
                        Message = "Produto não encontrado",
                        Status = "not_found",
                        Data = null
                    };
                }

                var allVariants = await _productRepository.GetByNameAsync(productVariant.Name);

                _logger.LogInformation("Modelo encontrado: {ProductName}. Variações identificadas: {Count}",
                    productVariant.Name, allVariants.Count());

                return new ProductResponseDto
                {
                    Message = "Produto encontrado com sucesso",
                    Status = "success",
                    Data = new Data
                    {
                        Id = productVariant.Id,
                        Name = productVariant.Name,
                        Category = productVariant.CategoryId,
                        Gender = productVariant.Gender,
                        Price = productVariant.Price,
                        IsPromotion = productVariant.IsPromotion,
                        OldPrice = productVariant.OldPrice,
                        ImageUrL = productVariant.ImageUrL,

                        Variants = allVariants
                            .OrderBy(v => (int)v.Size)
                            .Select(v => new VariantInfoResponse
                            {
                                Id = v.Id,
                                Size = GetDisplayName(v.Size),
                                Stock = v.Stock
                            })
                            .ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar detalhes do produto {ProductId}.", productId);
                return new ProductResponseDto
                {
                    Message = "Erro interno: " + ex.Message,
                    Status = "error",
                    Data = null
                };
            }
        }


        public async Task<ProductResponseDto> updateProductById(Guid productId, ProductRequestUpdateDto productRequestDto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Iniciando atualização do produto ID: {ProductId}", productId);

                var existingProduct = await _productRepository.GetByIdAsync(productId);

                if (existingProduct == null)
                {
                    _logger.LogWarning("Produto ID: {ProductId} não encontrado para edição.", productId);
                    return new ProductResponseDto
                    {
                        Message = "Produto não encontrado",
                        Status = "not_found",
                        Data = null
                    };
                }

                var allVariants = (await _productRepository.GetByNameAsync(existingProduct.Name)).ToList();

                var newName = string.IsNullOrWhiteSpace(productRequestDto.Name) ? existingProduct.Name : productRequestDto.Name;
                var newGender = productRequestDto.Gender > 0 ? productRequestDto.Gender : existingProduct.Gender;
                var newCategoryId = productRequestDto.CategoryId > 0 ? productRequestDto.CategoryId : existingProduct.CategoryId;
                var newIsPromotion = productRequestDto.IsPromotion ?? existingProduct.IsPromotion;
                var newOldPrice = existingProduct.OldPrice;
                if (productRequestDto.OldPrice != null)
                {
                    if (!TryNormalizePriceField(productRequestDto.OldPrice, false, out var parsedOldPrice))
                    {
                        _logger.LogWarning("Preço antigo inválido informado para atualização do produto {ProductId}: {OldPrice}", productId, productRequestDto.OldPrice);
                        return new ProductResponseDto
                        {
                            Message = "Preço antigo inválido",
                            Status = "invalid_argument",
                            Data = null
                        };
                    }

                    newOldPrice = parsedOldPrice;
                }
                if (!newIsPromotion)
                {
                    newOldPrice = null;
                }
                var newPrice = existingProduct.Price;
                if (!TryNormalizePriceField(productRequestDto.Price, false, out var normalizedPrice))
                {
                    _logger.LogWarning("Preço inválido informado para atualização do produto {ProductId}: {Price}", productId, productRequestDto.Price);
                    return new ProductResponseDto
                    {
                        Message = "Preço inválido",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                if (normalizedPrice.HasValue)
                {
                    newPrice = normalizedPrice.Value;
                }

                string imageUrl = existingProduct.ImageUrL;
                if (productRequestDto.Image != null)
                {
                    var uploadedUrl = await _imageUploadService.UploadImageAsync(productRequestDto.Image);
                    if (!string.IsNullOrEmpty(uploadedUrl))
                    {
                        imageUrl = uploadedUrl;
                    }
                }

                foreach (var p in allVariants)
                {
                    p.Name = newName;
                    p.CategoryId = newCategoryId;
                    p.Gender = newGender;
                    p.Price = newPrice;
                    p.IsPromotion = newIsPromotion;
                    p.OldPrice = newOldPrice;
                    p.ImageUrL = imageUrl;
                    p.UpdatedAt = DateTime.UtcNow;
                    await _productRepository.UpdateAsync(p);
                }

                await _unitOfWork.CommitAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ProductResponseDto
                {
                    Message = "Grade atualizada com sucesso",
                    Status = "success",
                    Data = new Data
                    {
                        Id = existingProduct.Id,
                        Name = allVariants.First().Name,
                        Price = allVariants.First().Price,
                        IsPromotion = allVariants.First().IsPromotion,
                        OldPrice = allVariants.First().OldPrice,
                        Category = allVariants.First().CategoryId,
                        Gender = allVariants.First().Gender,
                        ImageUrL = imageUrl,
                        Variants = allVariants.Select(v => new VariantInfoResponse
                        {
                            Id = v.Id,
                            Size = GetDisplayName(v.Size),
                            Stock = v.Stock
                        }).OrderBy(x => x.Size).ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Erro inesperado ao editar o produto: {ProductId}", productId);
                return new ProductResponseDto
                {
                    Message = "Erro ao atualizar o produto: " + ex.Message,
                    Status = "error",
                    Data = null
                };
            }
        }

        public async Task<ProductStatusResponseDto> UpdateStatusAsync(Guid productId, ProductStatus newStatus)
        {
            try
            {
                _logger.LogInformation("Iniciando alteração de status para o produto: {ProductId}", productId);

                var existingProduct = await _productRepository.GetByIdAsync(productId);

                if (existingProduct == null)
                {
                    _logger.LogWarning("Produto {ProductId} não encontrado para alteração de status.", productId);
                    return new ProductStatusResponseDto
                    {
                        Message = "Produto não encontrado",
                        Status = "not_found",
                        Data = null
                    };
                }

                var allVariants = (await _productRepository.GetByNameAsync(existingProduct.Name)).ToList();

                foreach (var variant in allVariants)
                {
                    variant.Status = newStatus;
                    variant.UpdatedAt = DateTime.UtcNow;

                    var updatedVariant = await _productRepository.UpdateAsync(variant);
                    if (updatedVariant == null)
                    {
                        return new ProductStatusResponseDto
                        {
                            Message = "Erro ao persistir a mudança de status no banco",
                            Status = "error",
                            Data = null
                        };
                    }
                }

                _logger.LogInformation("Status do produto {ProductId} alterado para {Status} com sucesso.", productId, newStatus);

                return new ProductStatusResponseDto
                {
                    Message = "Status atualizado com sucesso",
                    Status = "success",
                    Data = new DataStatus
                    {
                        Id = existingProduct.Id,
                        Name = existingProduct.Name,
                        ProductStatus = GetDisplayName(newStatus),
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao atualizar status do produto: {ProductId}", productId);
                return new ProductStatusResponseDto
                {
                    Message = "Erro ao atualizar status: " + ex.Message,
                    Status = "error",
                    Data = null
                };
            }


        }
        private static string GetDisplayName(Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .FirstOrDefault()?
                            .GetCustomAttribute<DisplayAttribute>()?
                            .GetName() ?? enumValue.ToString();
        }

        private static bool TryNormalizePrice(string rawPrice, out decimal price)
        {
            price = 0m;

            if (string.IsNullOrWhiteSpace(rawPrice))
            {
                return false;
            }

            var normalized = rawPrice.Trim().Replace(",", ".");

            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out price))
            {
                return false;
            }

            return price > 0;
        }

        private static bool TryNormalizePriceField(string? rawPrice, bool isRequired, out decimal? normalizedPrice)
        {
            normalizedPrice = null;

            if (string.IsNullOrWhiteSpace(rawPrice))
            {
                return !isRequired;
            }

            if (!TryNormalizePrice(rawPrice, out var parsedPrice))
            {
                return false;
            }

            normalizedPrice = parsedPrice;
            return true;
        }
    }
}
