using application.interfaces;
using domain.dtos;
using domain.dtos.Product;
using domain.entities;
using domain.interfaces;
using Mapster;

namespace application.implementations;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            throw new KeyNotFoundException("Producto no encontrado");
        return product.Adapt<ProductDto>();
    }

    public async Task<ProductDto> GetBySlugAsync(string slug)
    {
        var product = await _unitOfWork.Products.GetBySlugAsync(slug);
        if (product == null)
            throw new KeyNotFoundException("Producto no encontrado");
        return product.Adapt<ProductDto>();
    }

    public async Task<IEnumerable<ProductListDto>> GetAllAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        return products.Adapt<IEnumerable<ProductListDto>>();
    }

    public async Task<PagedResult<ProductListDto>> GetPagedAsync(
        int page, int pageSize,
        int? categoryId, int? brandId,
        string? search, decimal? minPrice, decimal? maxPrice)
    {
        var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(
            page, pageSize, categoryId, brandId, search, minPrice, maxPrice);

        return new PagedResult<ProductListDto>
        {
            Items = items.Adapt<IEnumerable<ProductListDto>>(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductDto> AddAsync(CreateProductDto dto)
    {
        var product = dto.Adapt<Product>();
        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return product.Adapt<ProductDto>();
    }

    public async Task UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            throw new KeyNotFoundException("Producto no encontrado");

        dto.Adapt(product);
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await _unitOfWork.Products.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ProductImageDto> AddImageAsync(int productId, ProductImageDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
            throw new KeyNotFoundException("Producto no encontrado");

        var image = dto.Adapt<ProductImage>();
        image.ProductId = productId;
        product.Images.Add(image);
        await _unitOfWork.SaveChangesAsync();
        return image.Adapt<ProductImageDto>();
    }

    public async Task DeleteImageAsync(int productId, int imageId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
            throw new KeyNotFoundException("Producto no encontrado");

        var image = product.Images.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
            throw new KeyNotFoundException("Imagen no encontrada");

        product.Images.Remove(image);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<VariantDto> AddVariantAsync(int productId, CreateVariantDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
            throw new KeyNotFoundException("Producto no encontrado");

        var variant = dto.Adapt<Variant>();
        variant.ProductId = productId;
        product.Variants.Add(variant);
        await _unitOfWork.SaveChangesAsync();
        return variant.Adapt<VariantDto>();
    }

    public async Task UpdateVariantAsync(int productId, int variantId, CreateVariantDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
            throw new KeyNotFoundException("Producto no encontrado");

        var variant = product.Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant == null)
            throw new KeyNotFoundException("Variante no encontrada");

        dto.Adapt(variant);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateStockAsync(int productId, int variantId, int stock)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
            throw new KeyNotFoundException("Producto no encontrado");

        var variant = product.Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant == null)
            throw new KeyNotFoundException("Variante no encontrada");

        variant.Stock = stock;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteVariantAsync(int productId, int variantId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
            throw new KeyNotFoundException("Producto no encontrado");

        var variant = product.Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant == null)
            throw new KeyNotFoundException("Variante no encontrada");

        product.Variants.Remove(variant);
        await _unitOfWork.SaveChangesAsync();
    }
}
