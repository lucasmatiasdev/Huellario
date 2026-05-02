using domain.entities;

namespace domain.interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetBySlugAsync(string slug);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, int? categoryId, int? brandId, string? search, decimal? minPrice, decimal? maxPrice);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}
