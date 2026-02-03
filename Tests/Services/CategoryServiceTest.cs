using Application.Services;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReceivedExtensions;

namespace Tests.Services
{
    public class CategoryServiceTest
    {
        private readonly ILogger<CategoryService> _logger = Substitute.For<ILogger<CategoryService>>();
        private readonly ICategoryRepository _categorRepository = Substitute.For<ICategoryRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly CategoryService _service;
        public CategoryServiceTest()
        {
            _service = new CategoryService(
                _logger,
                _categorRepository,
                _unitOfWork
            );
        }

        [Fact]
        public async Task Should_Create_Category_Corretly_When_No_Error()
        {
            var name = "Eletronicos";

            _categorRepository.CategoryExistsAsync(name).Returns(false);

            var result = await _service.CreateCategoryAsync(name);


            Assert.Equal("Categoria criada com sucesso", result.Message);
            Assert.Equal("success", result.Status);

            await _categorRepository.Received(1).CategoryExistsAsync(Arg.Any<string>());
            await _unitOfWork.Received(1).BeginTransactionAsync();
            await _unitOfWork.Received(1).CommitAsync();
            await _unitOfWork.Received(1).CommitTransactionAsync();
            await _unitOfWork.Received(0).RollbackTransactionAsync();

        }

        [Fact]
        public async Task Should_Return_Invalid_Argument_When_Parameters_Is_Empty()
        {
            var emptyName = "";

            var result = await _service.CreateCategoryAsync(emptyName);

            Assert.Equal("Nome da categoria invalido", result.Message);
            Assert.Equal("invalid_argument", result.Status);
        }

        [Fact]
        public async Task Should_Return_Conflits_When_The_Category_Exists()
        {
            var name = "Eletronicos";

            _categorRepository.CategoryExistsAsync(name).Returns(true);

            var result = await _service.CreateCategoryAsync(name);

            Assert.Equal("Categoria já existe", result.Message);
            Assert.Equal("conflict", result.Status);
        }

        [Fact]
        public async Task Should_Return_Bad_Request_When_Error()
        {
            var name = "Eletronicos";

            _categorRepository.CategoryExistsAsync(name).ThrowsAsync(new Exception("Error simulated"));

            var result = await _service.CreateCategoryAsync(name);

            Assert.Equal("error", result.Status);
            await _categorRepository.Received(1).CategoryExistsAsync(Arg.Any<string>());
        }
    }
}
