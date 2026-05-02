using domain.dtos;
using domain.dtos.Product;

namespace application.interfaces;

public interface IProductService
{
    Task<ProductDto> GetByIdAsync(int id);
    Task<ProductDto> GetBySlugAsync(string slug);
    Task<IEnumerable<ProductListDto>> GetAllAsync();
    Task<PagedResult<ProductListDto>> GetPagedAsync(int page, int pageSize, int? categoryId, int? brandId, string? search, decimal? minPrice, decimal? maxPrice);
    Task<ProductDto> AddAsync(CreateProductDto dto);
    Task UpdateAsync(int id, UpdateProductDto dto);
    Task DeleteAsync(int id);
    Task<ProductImageDto> AddImageAsync(int productId, ProductImageDto dto);
    Task DeleteImageAsync(int productId, int imageId);
    Task<VariantDto> AddVariantAsync(int productId, CreateVariantDto dto);
    Task UpdateVariantAsync(int productId, int variantId, CreateVariantDto dto);
    Task UpdateStockAsync(int productId, int variantId, int stock);
    Task DeleteVariantAsync(int productId, int variantId);
}
