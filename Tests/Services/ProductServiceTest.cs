using Application.Request;
using Application.Services;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Tests.Services
{
    public class ProductServiceTest
    {
        private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
        private readonly IImageUploadService _imageUploadService = Substitute.For<IImageUploadService>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly ILogger<ProductService> _logger = Substitute.For<ILogger<ProductService>>();
        private readonly ProductService _service;

        public ProductServiceTest()
        {
            _service = new ProductService(_productRepository, _imageUploadService, _unitOfWork, _logger);
        }

        [Fact]
        public async Task CreateNewProduct_WhenRequestIsNull_ReturnsInvalidArgument()
        {
            var result = await _service.CreateNewProduct(null!);

            Assert.Equal("invalid_argument", result.Status);
            Assert.Equal("Parametros estao vazios ou nulos", result.Message);

            await _unitOfWork.Received(1).BeginTransactionAsync();
            await _unitOfWork.Received(0).CommitAsync();
        }

        [Fact]
        public async Task CreateNewProduct_WhenDuplicateSizes_ReturnsInvalidArgument()
        {
            var request = new ProductRequestDto
            {
                Name = "Camiseta",
                Variant = new List<ProductVariantRequest>
                {
                    new ProductVariantRequest { Size = 1, Stock = 10 },
                    new ProductVariantRequest { Size = 1, Stock = 5 }
                }
            };

            var result = await _service.CreateNewProduct(request);

            Assert.Equal("invalid_argument", result.Status);
            Assert.Equal("Tamanhos duplicados na requisicao", result.Message);
        }

        [Fact]
        public async Task CreateNewProduct_WhenImageUploadFails_ReturnsInvalidArgument()
        {
            var request = new ProductRequestDto
            {
                Name = "Camiseta",
                Variant = new List<ProductVariantRequest>
                {
                    new ProductVariantRequest { Size = 1, Stock = 10 }
                },
                Image = Substitute.For<IFormFile>()
            };

            _imageUploadService.UploadImageAsync(Arg.Any<IFormFile>()).Returns(string.Empty);

            var result = await _service.CreateNewProduct(request);

            Assert.Equal("invalid_argument", result.Status);
            Assert.Equal("Imagem obrigatoria", result.Message);
        }

        [Fact]
        public async Task CreateNewProduct_WhenSizeAlreadyExists_ReturnsInvalidArgument()
        {
            var request = new ProductRequestDto
            {
                Name = "Camiseta",
                Variant = new List<ProductVariantRequest>
                {
                    new ProductVariantRequest { Size = 1, Stock = 10 }
                },
                Image = Substitute.For<IFormFile>()
            };

            _imageUploadService.UploadImageAsync(Arg.Any<IFormFile>()).Returns("http://image");
            _productRepository.GetByNameAsync(request.Name).Returns(new List<Product>
            {
                new Product { Size = ProductSize.Pequeno }
            });

            var result = await _service.CreateNewProduct(request);

            Assert.Equal("invalid_argument", result.Status);
            Assert.Equal("Tamanho ja existe para este produto", result.Message);
        }

        [Fact]
        public async Task CreateNewProduct_WhenValid_ReturnsSuccess()
        {
            var request = new ProductRequestDto
            {
                Name = "Camiseta",
                CategoryId = 2,
                Price = "120",
                Gender = 1,
                Image = Substitute.For<IFormFile>(),
                Variant = new List<ProductVariantRequest>
                {
                    new ProductVariantRequest { Size = 1, Stock = 10 },
                    new ProductVariantRequest { Size = 2, Stock = 5 }
                }
            };

            _imageUploadService.UploadImageAsync(Arg.Any<IFormFile>()).Returns("http://image");
            _productRepository.GetByNameAsync(request.Name).Returns(new List<Product>());
            _productRepository.AddAsync(Arg.Any<Product>()).Returns(call => call.Arg<Product>());

            var result = await _service.CreateNewProduct(request);

            Assert.Equal("success", result.Status);
            Assert.Equal("Grade de produto criada com sucesso", result.Message);
            Assert.Equal(request.Name, result.Data?.Name);
            Assert.Equal(request.CategoryId, result.Data?.Category);

            await _productRepository.Received(2).AddAsync(Arg.Any<Product>());
            await _unitOfWork.Received(1).CommitAsync();
            await _unitOfWork.Received(1).CommitTransactionAsync();
        }

        [Fact]
        public async Task UpdateStatus_WhenProductNotFound_ReturnsNotFound()
        {
            _productRepository.GetByIdAsync(Arg.Any<Guid>()).ReturnsNull();

            var result = await _service.UpdateStatusAsync(Guid.NewGuid(), ProductStatus.Inactive);

            Assert.Equal("not_found", result.Status);
            Assert.Equal("Produto não encontrado", result.Message);
        }

        [Fact]
        public async Task UpdateStatus_WhenUpdateFails_ReturnsError()
        {
            var product = new Product { Id = Guid.NewGuid(), Name = "Camiseta" };

            _productRepository.GetByIdAsync(product.Id).Returns(product);
            _productRepository.UpdateAsync(Arg.Any<Product>()).ReturnsNull();

            var result = await _service.UpdateStatusAsync(product.Id, ProductStatus.Inactive);

            Assert.Equal("error", result.Status);
            Assert.Equal("Erro ao persistir a mudança de status no banco", result.Message);
        }

        [Fact]
        public async Task UpdateStatus_WhenValid_ReturnsSuccess()
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Camiseta",
                Status = ProductStatus.Available
            };

            _productRepository.GetByIdAsync(product.Id).Returns(product);
            _productRepository.UpdateAsync(Arg.Any<Product>()).Returns(product);

            var result = await _service.UpdateStatusAsync(product.Id, ProductStatus.Inactive);

            Assert.Equal("success", result.Status);
            Assert.Equal("Status atualizado com sucesso", result.Message);
            Assert.Equal("Inactive", result.Data?.ProductStatus);
        }
    }
}
