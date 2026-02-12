using Application.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly ILogger<UnitOfWork> _logger;

        public UnitOfWork(
            AppDbContext context,
            IDomainEventDispatcher domainEventDispatcher,
            ILogger<UnitOfWork> logger)
        {
            _context = context;
            _domainEventDispatcher = domainEventDispatcher;
            _logger = logger;
        }

        public async Task<bool> CommitAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
            await PublishDomainEventsAsync();
        }

        public async Task RollbackTransactionAsync() => await _context.Database.RollbackTransactionAsync();

        public void Dispose() => _context.Dispose();

        private async Task PublishDomainEventsAsync()
        {
            var entitiesWithEvents = _context.ChangeTracker
                .Entries<Entity>()
                .Select(entry => entry.Entity)
                .Where(entity => entity.DomainEvents.Any())
                .ToList();

            if (!entitiesWithEvents.Any())
            {
                return;
            }

            var domainEvents = entitiesWithEvents
                .SelectMany(entity => entity.DomainEvents)
                .ToList();

            try
            {
                await _domainEventDispatcher.DispatchAsync(domainEvents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao publicar eventos de domínio após commit da transação.");
            }
            finally
            {
                foreach (var entity in entitiesWithEvents)
                {
                    entity.ClearDomainEvents();
                }
            }
        }
    }
}
