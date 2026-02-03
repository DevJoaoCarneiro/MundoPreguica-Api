using Application.Interfaces;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ILogger<CategoryService> _logger;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(ILogger<CategoryService> logger, ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(string name)
        {
            _unitOfWork.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    _logger.LogWarning("Nome da categoria nulo ou vazio");
                    return new CategoryResponseDto
                    {
                        Message = "Nome da categoria invalido",
                        Status = "invalid_argument",
                    };
                }


                var categoryExists = await _categoryRepository.CategoryExistsAsync(name);

                if (categoryExists)
                {
                    _logger.LogWarning("Categoria já existe no banco de dados: {CategoryName}", name);
                    return new CategoryResponseDto
                    {
                        Message = "Categoria já existe",
                        Status = "conflict",
                    };
                }

                _logger.LogInformation("Criando nova categoria: {CategoryName}", name);
                var newCategory = new Category
                {
                    Name = name,
                    CreatedAt = DateTime.UtcNow,
                };

                await _categoryRepository.AddCategoryAsync(newCategory);
                await _unitOfWork.CommitAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Categoria criada com sucesso: {CategoryName}", name);
                return new CategoryResponseDto
                {
                    Message = "Categoria criada com sucesso",
                    Status = "success",
                    Categories = new List<CategoryDto>
                    {
                        new CategoryDto { Id = newCategory.Id, Name = newCategory.Name }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar categoria na Service.");
                await _unitOfWork.RollbackTransactionAsync();
                return new CategoryResponseDto
                {
                    Message = "Erro ao criar categoria: " + ex.Message,
                    Status = "error",
                };
            }

        }

        public async Task<CategoryResponseDto> DeleteCategoryAsync(int id)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);

                if (category == null)
                {
                    _logger.LogWarning("Tentativa de deletar categoria inexistente: {Id}", id);
                    await _unitOfWork.RollbackTransactionAsync();
                    return new CategoryResponseDto
                    {
                        Message = "Categoria não encontrada",
                        Status = "not_found"
                    };
                }

                _categoryRepository.Delete(category);
                await _unitOfWork.CommitAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new CategoryResponseDto
                {
                    Message = "Categoria removida com sucesso",
                    Status = "success"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Erro ao deletar categoria {Id}", id);
                return new CategoryResponseDto
                {
                    Message = "Erro interno ao deletar",
                    Status = "error"
                };
            }
        }

        public async Task<CategoryResponseDto> GetAllCategoriesAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando serviço para buscar todas as categorias.");

                var categories = await _categoryRepository.GetAllCategoryNames();

                if (categories == null || !categories.Any())
                {
                    _logger.LogWarning("Nenhuma categoria encontrada.");
                    return new CategoryResponseDto
                    {
                        Message = "Nenhuma categoria encontrada",
                        Status = "not_found",
                        Categories = new List<CategoryDto>()
                    };
                }

                _logger.LogInformation("Categorias recuperadas com sucesso. Total: {Count}", categories.Count());

                return new CategoryResponseDto
                {
                    Message = "Categorias listadas com sucesso",
                    Status = "success",
                    Categories = categories.Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name
                    })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao buscar categorias na Service.");
                return new CategoryResponseDto
                {
                    Message = "Erro ao processar categorias: " + ex.Message,
                    Status = "error"
                };
            }
        }



    }
}
