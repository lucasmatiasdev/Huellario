using domain.interfaces;
using infrastructure.data;
using infrastructure.repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace infrastructure.implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly HuellarioDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(HuellarioDbContext context)
    {
        _context = context;
    }

    private ICategoryRepository? _categories;
    private IBrandRepository? _brands;
    private IUserRepository? _users;
    private IProductRepository? _products;
    private IAddressRepository? _addresses;
    private IOrderRepository? _orders;
    private ICartRepository? _cartItems;

    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public IBrandRepository Brands => _brands ??= new BrandRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public IAddressRepository Addresses => _addresses ??= new AddressRepository(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
    public ICartRepository CartItems => _cartItems ??= new CartRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
