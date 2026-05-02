namespace domain.interfaces;

public interface IUnitOfWork
{
    ICategoryRepository Categories { get; }
    IBrandRepository Brands { get; }
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
