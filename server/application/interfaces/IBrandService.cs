using domain.dtos.Brand;

namespace application.interfaces;

public interface IBrandService
{
    Task<BrandDto> GetByIdAsync(int id);
    Task<BrandDto> GetBySlugAsync(string slug);
    Task<IEnumerable<BrandDto>> GetAllAsync();
    Task<BrandDto> AddAsync(CreateBrandDto dto);
    Task UpdateAsync(int id, UpdateBrandDto dto);
    Task DeleteAsync(int id);
}
