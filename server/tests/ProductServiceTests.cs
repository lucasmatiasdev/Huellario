using application.implementations;
using domain.dtos.Product;
using domain.entities;
using domain.interfaces;
using Moq;
using Shouldly;

namespace tests;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Products).Returns(_repositoryMock.Object);
        _sut = new ProductService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProductDto_WhenProductExists()
    {
        var product = new Product { Id = 1, Name = "Royal Canin", Slug = "royal-canin", Price = 100 };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(1);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
        result.Name.ShouldBe("Royal Canin");
        result.Slug.ShouldBe("royal-canin");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        var act = () => _sut.GetByIdAsync(999);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnProductDto_WhenProductExists()
    {
        var product = new Product { Id = 1, Name = "Royal Canin", Slug = "royal-canin" };
        _repositoryMock.Setup(r => r.GetBySlugAsync("royal-canin")).ReturnsAsync(product);

        var result = await _sut.GetBySlugAsync("royal-canin");

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Royal Canin");
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetBySlugAsync(It.IsAny<string>())).ReturnsAsync((Product?)null);

        var act = () => _sut.GetBySlugAsync("inexistente");

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProducts()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Producto 1", Slug = "producto-1" },
            new() { Id = 2, Name = "Producto 2", Slug = "producto-2" }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

        var result = await _sut.GetAllAsync();

        result.Count().ShouldBe(2);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResult()
    {
        var products = new List<Product> { new() { Id = 1, Name = "Producto 1", Slug = "producto-1" } };
        _repositoryMock.Setup(r => r.GetPagedAsync(1, 10, null, null, null, null, null))
            .ReturnsAsync((products, 1));

        var result = await _sut.GetPagedAsync(1, 10, null, null, null, null, null);

        result.Items.Count().ShouldBe(1);
        result.TotalCount.ShouldBe(1);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(10);
    }

    [Fact]
    public async Task AddAsync_ShouldCreateProductAndReturnDto()
    {
        var dto = new CreateProductDto { Name = "Producto Nuevo", Slug = "producto-nuevo", Description = "Desc", Price = 100, CategoryId = 1, BrandId = 1 };
        Product? captured = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => captured = p);

        var result = await _sut.AddAsync(dto);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        result.Name.ShouldBe("Producto Nuevo");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingProduct()
    {
        var existing = new Product { Id = 1, Name = "Original", Slug = "original", Price = 50 };
        var dto = new UpdateProductDto { Name = "Actualizado", Slug = "actualizado", Description = "Desc", Price = 100, IsOwnBrand = false, IsActive = true };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

        await _sut.UpdateAsync(1, dto);

        existing.Name.ShouldBe("Actualizado");
        existing.Slug.ShouldBe("actualizado");
        existing.Price.ShouldBe(100);
        _repositoryMock.Verify(r => r.UpdateAsync(existing), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        var act = () => _sut.UpdateAsync(999, new UpdateProductDto { Name = "x", Slug = "x", Description = "x", IsOwnBrand = false, IsActive = true });

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProduct()
    {
        _repositoryMock.Setup(r => r.DeleteAsync(1));

        await _sut.DeleteAsync(1);

        _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AddImageAsync_ShouldAddImage_WhenProductExists()
    {
        var product = new Product { Id = 1, Name = "Producto", Images = new List<ProductImage>() };
        var dto = new ProductImageDto { Url = "/img.jpg", IsMain = true, DisplayOrder = 0 };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var result = await _sut.AddImageAsync(1, dto);

        result.Url.ShouldBe("/img.jpg");
        result.IsMain.ShouldBeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AddImageAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        var act = () => _sut.AddImageAsync(999, new ProductImageDto { Url = "/x.jpg" });

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldRemoveImage_WhenProductAndImageExist()
    {
        var image = new ProductImage { Id = 2, Url = "/img.jpg" };
        var product = new Product { Id = 1, Images = new List<ProductImage> { image } };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        await _sut.DeleteImageAsync(1, 2);

        product.Images.ShouldNotContain(i => i.Id == 2);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        var act = () => _sut.DeleteImageAsync(999, 1);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteImageAsync_ShouldThrowKeyNotFoundException_WhenImageDoesNotExist()
    {
        var product = new Product { Id = 1, Images = new List<ProductImage>() };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var act = () => _sut.DeleteImageAsync(1, 999);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AddVariantAsync_ShouldAddVariant_WhenProductExists()
    {
        var product = new Product { Id = 1, Variants = new List<Variant>() };
        var dto = new CreateVariantDto { Name = "1 kg", Price = 100, Stock = 10 };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var result = await _sut.AddVariantAsync(1, dto);

        result.Name.ShouldBe("1 kg");
        result.Price.ShouldBe(100);
        result.Stock.ShouldBe(10);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AddVariantAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        var act = () => _sut.AddVariantAsync(999, new CreateVariantDto { Name = "x" });

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateVariantAsync_ShouldUpdateVariant_WhenProductAndVariantExist()
    {
        var variant = new Variant { Id = 2, Name = "Original", Price = 50, Stock = 5 };
        var product = new Product { Id = 1, Variants = new List<Variant> { variant } };
        var dto = new CreateVariantDto { Name = "Actualizado", Price = 100, Stock = 10 };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        await _sut.UpdateVariantAsync(1, 2, dto);

        variant.Name.ShouldBe("Actualizado");
        variant.Price.ShouldBe(100);
        variant.Stock.ShouldBe(10);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateVariantAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        var act = () => _sut.UpdateVariantAsync(999, 1, new CreateVariantDto { Name = "x" });

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateVariantAsync_ShouldThrowKeyNotFoundException_WhenVariantDoesNotExist()
    {
        var product = new Product { Id = 1, Variants = new List<Variant>() };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var act = () => _sut.UpdateVariantAsync(1, 999, new CreateVariantDto { Name = "x" });

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateStockAsync_ShouldUpdateStock_WhenProductAndVariantExist()
    {
        var variant = new Variant { Id = 2, Stock = 5 };
        var product = new Product { Id = 1, Variants = new List<Variant> { variant } };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        await _sut.UpdateStockAsync(1, 2, 20);

        variant.Stock.ShouldBe(20);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateStockAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        var act = () => _sut.UpdateStockAsync(999, 1, 10);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateStockAsync_ShouldThrowKeyNotFoundException_WhenVariantDoesNotExist()
    {
        var product = new Product { Id = 1, Variants = new List<Variant>() };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var act = () => _sut.UpdateStockAsync(1, 999, 10);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteVariantAsync_ShouldRemoveVariant_WhenProductAndVariantExist()
    {
        var variant = new Variant { Id = 2 };
        var product = new Product { Id = 1, Variants = new List<Variant> { variant } };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        await _sut.DeleteVariantAsync(1, 2);

        product.Variants.ShouldNotContain(v => v.Id == 2);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteVariantAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        var act = () => _sut.DeleteVariantAsync(999, 1);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteVariantAsync_ShouldThrowKeyNotFoundException_WhenVariantDoesNotExist()
    {
        var product = new Product { Id = 1, Variants = new List<Variant>() };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var act = () => _sut.DeleteVariantAsync(1, 999);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }
}
