using domain.interfaces;
using infrastructure.data;
using infrastructure.repositories;

namespace infrastructure.implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly HuellarioDbContext _context;

    public UnitOfWork(HuellarioDbContext context)
    {
        _context = context;
    }

    private ICategoryRepository? _categories;
    private IBrandRepository? _brands;
    private IUserRepository? _users;
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public IBrandRepository Brands => _brands ??= new BrandRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
