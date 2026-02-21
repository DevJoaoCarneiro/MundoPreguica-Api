using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(AppDbContext context, ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Product> AddAsync(Product product)
        {
            try
            {
                _logger.LogInformation("Tentando persistir novo produto no banco. ID: {ProductId}", product.Id);

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Produto persistido com sucesso no banco. ID: {ProductId}", product.Id);
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao adicionar produto no banco de dados. Nome: {ProductName}", product.Name);
                return null;
            }
        }

        public async Task<(IEnumerable<IGrouping<string, Product>> Products, int TotalCount)> GetByFiltersAsync(
            string? name,
            int? categoryId,
            int? gender,
            bool? isPromotion,
            ProductStatus? status,
            ProductSize? size,
            int page,
            int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Executando consulta filtrada de produtos. Página: {Page}", page);

                var query = _context.Products
                    .Include(p => p.Category)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(p => p.Name.ToLower().Contains(name.ToLower()));

                if (categoryId.HasValue && categoryId > 0)
                    query = query.Where(p => p.CategoryId == categoryId.Value);

                if (gender.HasValue && gender > 0)
                    query = query.Where(p => p.Gender == gender.Value);

                if (isPromotion.HasValue)
                    query = query.Where(p => p.IsPromotion == isPromotion.Value);

                if (status.HasValue)
                    query = query.Where(p => p.Status == status.Value);

                if (size.HasValue)
                    query = query.Where(p => p.Size == size.Value);

                var groupedQuery = query
                    .GroupBy(p => p.Name)
                    .Select(g => new
                    {
                        Name = g.Key,
                        MaxCreatedAt = g.Max(p => p.CreatedAt)
                    });

                var totalCount = await groupedQuery.CountAsync();

                var pageNames = await groupedQuery
                    .OrderByDescending(g => g.MaxCreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(g => g.Name)
                    .ToListAsync();

                var products = await query
                    .Where(p => pageNames.Contains(p.Name))
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var nameOrder = pageNames
                    .Select((name, index) => new { name, index })
                    .ToDictionary(x => x.name, x => x.index);

                var groupedProducts = products
                    .GroupBy(p => p.Name)
                    .OrderBy(g => nameOrder[g.Key])
                    .ToList();

                _logger.LogInformation("Consulta finalizada. {Count} produtos na página. Total no banco: {Total}", groupedProducts.Count, totalCount);

                return (groupedProducts, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar consulta filtrada no banco de dados.");
                throw;
            }
        }

        public async Task<Product?> GetByIdAsync(Guid productId)
        {
            _logger.LogInformation("Buscando produto por ID no banco: {ProductId}", productId);

            var product = await _context.Products
             .Include(p => p.Category)
             .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                _logger.LogWarning("Nenhum registro encontrado no banco para o ID: {ProductId}", productId);

            return product;
        }

        public async Task<Product?> UpdateAsync(Product product)
        {
            try
            {
                _logger.LogInformation("Iniciando atualização do registro no banco. ID: {ProductId}", product.Id);

                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Registro atualizado com sucesso no banco. ID: {ProductId}", product.Id);
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar produto no banco de dados. ID: {ProductId}", product.Id);
                return null;
            }
        }

        public async Task<IEnumerable<Product>> GetByNameAsync(string name)
        {
            return await _context.Products
                .Where(p => p.Name.ToLower() == name.ToLower())
                .ToListAsync();
        }
    }
}