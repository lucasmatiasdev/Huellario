using domain.entities;
using domain.interfaces;
using infrastructure.data;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly HuellarioDbContext _context;
    public CategoryRepository(HuellarioDbContext context)
    {
        _context = context;
    }
    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }
    public async Task<Category?> GetBySlugAsync(string slug)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
    }
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories.ToListAsync();
    }
    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
    }
    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
    }
    public async Task DeleteAsync(int id)
    {
        var category = await GetByIdAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
        }
    }
}