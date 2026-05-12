using domain.entities;

namespace domain.interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id);
    Task<Category?> GetBySlugAsync(string slug);
    Task<IEnumerable<Category>> GetAllAsync(bool activeOnly = true);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    void Remove(Category category);
}