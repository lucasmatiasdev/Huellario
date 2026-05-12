using application.dtos.Address;
using application.dtos.Brand;
using application.dtos.CartItem;
using application.dtos.Category;
using application.dtos.Order;
using application.dtos.Product;
using domain.entities;
using Mapster;

namespace application.mappers;

public static class MappingProfile
{
    public static void Configure()
    {
        TypeAdapterConfig<Category, CategoryDto>.NewConfig();
        TypeAdapterConfig<CreateCategoryDto, Category>.NewConfig();
        TypeAdapterConfig<UpdateCategoryDto, Category>.NewConfig()
            .IgnoreNullValues(true);
        TypeAdapterConfig<Brand, BrandDto>.NewConfig();
        TypeAdapterConfig<CreateBrandDto, Brand>.NewConfig();
        TypeAdapterConfig<UpdateBrandDto, Brand>.NewConfig()
            .IgnoreNullValues(true);
        TypeAdapterConfig<Product, ProductDto>.NewConfig()
            .Map(dest => dest.Images, src => src.Images)
            .Map(dest => dest.Variants, src => src.Variants);
        TypeAdapterConfig<Product, ProductListDto>.NewConfig()
            .Map(dest => dest.MainImageUrl, src =>
                src.Images.Where(i => i.IsMain).Select(i => i.Url).FirstOrDefault()
                ?? src.Images.Select(i => i.Url).FirstOrDefault());
        TypeAdapterConfig<CreateProductDto, Product>.NewConfig();
        TypeAdapterConfig<UpdateProductDto, Product>.NewConfig()
            .IgnoreNullValues(true);
        TypeAdapterConfig<Variant, VariantDto>.NewConfig();
        TypeAdapterConfig<CreateVariantDto, Variant>.NewConfig();
        TypeAdapterConfig<ProductImage, ProductImageDto>.NewConfig();
        TypeAdapterConfig<CartItem, CartItemDto>.NewConfig()
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : string.Empty)
            .Map(dest => dest.ProductSlug, src => src.Product != null ? src.Product.Slug : string.Empty)
            .Map(dest => dest.ProductImageUrl, src =>
                src.Product != null
                    ? src.Product.Images.Where(i => i.IsMain).Select(i => i.Url).FirstOrDefault()
                      ?? src.Product.Images.Select(i => i.Url).FirstOrDefault()
                    : null)
            .Map(dest => dest.VariantName, src => src.Variant != null ? src.Variant.Name : string.Empty)
            .Map(dest => dest.UnitPrice, src =>
                src.Variant != null ? src.Variant.Price
                : src.Product != null ? src.Product.Price
                : 0)
            .Map(dest => dest.CartItemId, src => $"{src.ProductId}-{src.VariantId}");
        TypeAdapterConfig<CreateCartItemDto, CartItem>.NewConfig();
        TypeAdapterConfig<UpdateCartItemDto, CartItem>.NewConfig();
        TypeAdapterConfig<UpdateAddressDto, Address>.NewConfig()
            .IgnoreNullValues(true);
        TypeAdapterConfig<Order, OrderListDto>.NewConfig();
        TypeAdapterConfig<OrderLine, OrderLineDto>.NewConfig()
            .Map(dest => dest.ProductImageUrl, src => 
                src.Product != null 
                    ? src.Product.Images.Where(i => i.IsMain).Select(i => i.Url)
                    .FirstOrDefault()
                    : null);
    }
}
