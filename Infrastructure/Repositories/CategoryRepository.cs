using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryRepository> _logger;

        public CategoryRepository(AppDbContext context, ILogger<CategoryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<IEnumerable<Category>> GetAllCategoryNames()
        {
            _logger.LogInformation("Iniciando a busca de todas as categorias no banco de dados.");

            try
            {
                var categories = await _context.Category
                    .ToListAsync();

                _logger.LogInformation("Categorias recuperadas: {Count}", categories.Count);
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar buscar as categorias no banco de dados.");
                throw;
            }
        }

        public async Task AddCategoryAsync(Category category)
        {
            await _context.Category.AddAsync(category);
        }

        public async Task<bool> CategoryExistsAsync(string name)
        {
            return await _context.Category
                .AnyAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Category.FindAsync(id);
        }

        public void Delete(Category category)
        {
            _context.Category.Remove(category);
        }


    }
}
