using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReceivedExtensions;
using NSubstitute.ReturnsExtensions;
using System.Xml.Linq;

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

        [Fact]
        public async Task Should_Return_All_Category_When_No_Error()
        {
            var expectedList = new List<Category>
            {
                new Category
                {
                    Id = 1,
                    Name = "Pijamas",
                    CreatedAt = DateTime.UtcNow
                }, 
            };

            _categorRepository.GetAllCategoryNames().Returns(expectedList);

            var result = await _service.GetAllCategoriesAsync();


            Assert.Equal("Categorias listadas com sucesso", result.Message);
            Assert.Equal("success", result.Status);

            var category = result.Categories.First();

            Assert.Equal(1, category.Id);
            Assert.Equal("Pijamas", category.Name);

            Assert.Equal(expectedList[0].Name, category.Name);
            Assert.Equal(expectedList[0].Id, category.Id);

            _categorRepository.Received(1).GetAllCategoryNames();
        }

        [Fact]
        public async Task When_The_Table_Category_Is_Empty_Or_Result_Is_Null()
        {
            _categorRepository.GetAllCategoryNames().ReturnsNull();

            var result = await _service.GetAllCategoriesAsync();

            Assert.Equal("Nenhuma categoria encontrada", result.Message);
            Assert.Equal("not_found", result.Status);
            Assert.Empty(result.Categories);

            _categorRepository.Received(1).GetAllCategoryNames();

        }

        [Fact]
        public async Task Should_Return_Catch_When_Error()
        {
            _categorRepository.GetAllCategoryNames().ThrowsAsync(new Exception("Error simulated"));

            var result = await _service.GetAllCategoriesAsync();

            Assert.Equal("error", result.Status);
            Assert.Empty(result.Categories);

            _categorRepository.Received(1).GetAllCategoryNames();
        }

        [Fact]
        public async Task Should_Delete_A_Category_When_No_Error()
        {
            var idCategory = 1;

            var expetectedCategory = new Category
            {
                Id = idCategory,
            };

            _categorRepository.GetByIdAsync(idCategory).Returns(expetectedCategory);
            _categorRepository.CategoryHasProductsAsync(idCategory).Returns(false);

            _categorRepository.Delete(Arg.Any<Category>());

            var result = await _service.DeleteCategoryAsync(idCategory);

            Assert.Equal("Categoria removida com sucesso", result.Message);
            Assert.Equal("success", result.Status);

            _categorRepository.Received(1).GetByIdAsync(idCategory);
            await _categorRepository.Received(1).CategoryHasProductsAsync(idCategory);

            _categorRepository.Received(1).Delete(Arg.Any<Category>());

        }

        [Fact]
        public async Task Should_Return_Conflict_When_Category_Has_Products()
        {
            var idCategory = 1;

            var expetectedCategory = new Category
            {
                Id = idCategory,
            };

            _categorRepository.GetByIdAsync(idCategory).Returns(expetectedCategory);
            _categorRepository.CategoryHasProductsAsync(idCategory).Returns(true);

            var result = await _service.DeleteCategoryAsync(idCategory);

            Assert.Equal("Categoria possui produtos cadastrados", result.Message);
            Assert.Equal("conflict", result.Status);

            _categorRepository.Received(1).GetByIdAsync(idCategory);
            await _categorRepository.Received(1).CategoryHasProductsAsync(idCategory);
            _categorRepository.Received(0).Delete(Arg.Any<Category>());
        }

        [Fact]
        public async Task Should_Return_Not_Found_When_Id_Is_Null()
        {
            var idCategory = 1;

            _categorRepository.GetByIdAsync(idCategory).ReturnsNull();

            var result = await _service.DeleteCategoryAsync(idCategory);

            Assert.Equal("Categoria não encontrada", result.Message);
            Assert.Equal("not_found", result.Status);

            _categorRepository.Received(1).GetByIdAsync(idCategory);
        }
    }
}
