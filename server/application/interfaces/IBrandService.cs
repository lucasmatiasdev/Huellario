using application.dtos.Brand;

namespace application.interfaces;

public interface IBrandService
{
    Task<BrandDto> GetByIdAsync(int id);
    Task<BrandDto> GetBySlugAsync(string slug);
    Task<IEnumerable<BrandDto>> GetAllAsync(bool activeOnly = true);
    Task<BrandDto> AddAsync(CreateBrandDto dto);
    Task UpdateAsync(int id, UpdateBrandDto dto);
    Task DeleteAsync(int id);
}
