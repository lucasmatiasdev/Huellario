using application.interfaces;
using domain.dtos.Category;
using domain.entities;
using domain.interfaces;
using Mapster;

namespace application.implementations;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
            throw new KeyNotFoundException("Categoría no encontrada");
        return category.Adapt<CategoryDto>();
    }
    public async Task<CategoryDto> GetBySlugAsync(string slug)
    {
        var category = await _unitOfWork.Categories.GetBySlugAsync(slug);
        if (category == null)
            throw new KeyNotFoundException("Categoría no encontrada");
        return category.Adapt<CategoryDto>();
    }
    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return categories.Adapt<IEnumerable<CategoryDto>>();
    }
    public async Task<CategoryDto> AddAsync(CreateCategoryDto dto)
    {
        var category = dto.Adapt<Category>();
        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();
        return category.Adapt<CategoryDto>();
    }
    public async Task UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
            throw new KeyNotFoundException("Categoría no encontrada");
        dto.Adapt(category);
        await _unitOfWork.Categories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();
    }
    public async Task DeleteAsync(int id)
    {
        await _unitOfWork.Categories.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }
}