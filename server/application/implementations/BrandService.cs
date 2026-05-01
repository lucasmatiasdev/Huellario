using application.interfaces;
using domain.dtos.Brand;
using domain.entities;
using domain.interfaces;
using Mapster;

namespace application.implementations;

public class BrandService : IBrandService
{
    private readonly IUnitOfWork _unitOfWork;

    public BrandService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BrandDto> GetByIdAsync(int id)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand == null)
            throw new KeyNotFoundException("Marca no encontrada");
        return brand.Adapt<BrandDto>();
    }

    public async Task<BrandDto> GetBySlugAsync(string slug)
    {
        var brand = await _unitOfWork.Brands.GetBySlugAsync(slug);
        if (brand == null)
            throw new KeyNotFoundException("Marca no encontrada");
        return brand.Adapt<BrandDto>();
    }

    public async Task<IEnumerable<BrandDto>> GetAllAsync()
    {
        var brands = await _unitOfWork.Brands.GetAllAsync();
        return brands.Adapt<IEnumerable<BrandDto>>();
    }

    public async Task<BrandDto> AddAsync(CreateBrandDto dto)
    {
        var brand = dto.Adapt<Brand>();
        await _unitOfWork.Brands.AddAsync(brand);
        await _unitOfWork.SaveChangesAsync();
        return brand.Adapt<BrandDto>();
    }

    public async Task UpdateAsync(int id, UpdateBrandDto dto)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand == null)
            throw new KeyNotFoundException("Marca no encontrada");
        dto.Adapt(brand);
        await _unitOfWork.Brands.UpdateAsync(brand);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await _unitOfWork.Brands.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }
}