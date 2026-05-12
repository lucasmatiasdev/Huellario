using application.dtos.Category;

namespace application.interfaces;

public interface ICategoryService
{
    Task<CategoryDto> GetByIdAsync(int id);
    Task<CategoryDto> GetBySlugAsync(string slug);
    Task<IEnumerable<CategoryDto>> GetAllAsync(bool activeOnly = true);
    Task<CategoryDto> AddAsync(CreateCategoryDto dto);
    Task UpdateAsync(int id, UpdateCategoryDto dto);
    Task DeleteAsync(int id);
}
