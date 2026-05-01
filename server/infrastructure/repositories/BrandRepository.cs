using domain.entities;
using domain.interfaces;
using infrastructure.data;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.repositories;

public class BrandRepository : IBrandRepository
{
    private readonly HuellarioDbContext _context;

    public BrandRepository(HuellarioDbContext context)
    {
        _context = context;
    }

    public async Task<Brand?> GetByIdAsync(int id)
    {
        return await _context.Brands.FindAsync(id);
    }

    public async Task<Brand?> GetBySlugAsync(string slug)
    {
        return await _context.Brands.FirstOrDefaultAsync(b => b.Slug == slug);
    }

    public async Task<IEnumerable<Brand>> GetAllAsync()
    {
        return await _context.Brands.ToListAsync();
    }

    public async Task AddAsync(Brand brand)
    {
        await _context.Brands.AddAsync(brand);
    }

    public async Task UpdateAsync(Brand brand)
    {
        _context.Brands.Update(brand);
    }

    public async Task DeleteAsync(int id)
    {
        var brand = await GetByIdAsync(id);
        if (brand != null)
        {
            _context.Brands.Remove(brand);
        }
    }
}