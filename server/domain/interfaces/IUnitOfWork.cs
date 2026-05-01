namespace domain.interfaces;

public interface IUnitOfWork
{
    ICategoryRepository Categories { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
