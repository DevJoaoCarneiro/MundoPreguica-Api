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

        public async Task<IEnumerable<Product>> GetByFiltersAsync(
            string? name,
            int? categoryId,
            ProductStatus? status,
            int page,
            ProductSize? size)
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

                if (status.HasValue)
                    query = query.Where(p => p.Status == status.Value);

                if (size.HasValue)
                    query = query.Where(p => p.Size == size.Value);

                var products = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * 10)
                    .Take(10)
                    .ToListAsync();

                _logger.LogInformation("Consulta finalizada. {Count} produtos retornados do banco.", products.Count);
                return products;
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