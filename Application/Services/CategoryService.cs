using Application.Interfaces;
using Application.Response;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ILogger<CategoryService> _logger;
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ILogger<CategoryService> logger, ICategoryRepository categoryRepository)
        {
            _logger = logger;
            _categoryRepository = categoryRepository;
        }

        public async Task<CategoryResponseDto> GetAllCategoriesAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando serviço para buscar todas as categorias.");

                var categories = await _categoryRepository.GetAllCategoryNames();

                if (categories == null || !categories.Any())
                {
                    _logger.LogWarning("Nenhuma categoria encontrada no banco de dados.");
                    return new CategoryResponseDto
                    {
                        Message = "Nenhuma categoria encontrada",
                        Status = "not_found",
                        CategoryName = string.Empty
                    };
                }

                _logger.LogInformation("Categorias recuperadas com sucesso. Total: {Count}", categories.Count());

                return new CategoryResponseDto
                {
                    Message = "Categorias listadas com sucesso",
                    Status = "success",
                    CategoryName = string.Join(", ", categories)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao buscar categorias na Service.");
                return new CategoryResponseDto
                {
                    Message = "Erro ao processar categorias: " + ex.Message,
                    Status = "error",
                    CategoryName = string.Empty
                };
            }
        }

    }
}
