namespace domain.interfaces;

public interface IUnitOfWork
{
    ICategoryRepository Categories { get; }
    IBrandRepository Brands { get; }
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    IAddressRepository Addresses { get; }
    ICartRepository CartItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
