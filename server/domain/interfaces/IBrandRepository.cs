using domain.entities;

namespace domain.interfaces;

public interface IBrandRepository
{
    Task<Brand?> GetByIdAsync(int id);
    Task<Brand?> GetBySlugAsync(string slug);
    Task<IEnumerable<Brand>> GetAllAsync();
    Task AddAsync(Brand brand);
    Task UpdateAsync(Brand brand);
    Task DeleteAsync(int id);
}

