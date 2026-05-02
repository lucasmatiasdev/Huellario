using domain.entities;
using domain.interfaces;
using infrastructure.data;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.repositories;

public class ProductRepository : IProductRepository
{
    private readonly HuellarioDbContext _context;
    public ProductRepository(HuellarioDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetBySlugAsync(string slug)
    {
        return await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        int? categoryId, int? brandId,
        string? search, decimal? minPrice, decimal? maxPrice)
    {
        var query = _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.Name != null && p.Name.Contains(search) ||
                p.Description != null && p.Description.Contains(search));

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
        }
    }
}
