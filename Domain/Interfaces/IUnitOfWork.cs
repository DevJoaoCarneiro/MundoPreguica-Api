namespace Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        Task<bool> CommitAsync();

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}