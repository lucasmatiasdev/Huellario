using application.implementations;
using domain.dtos.Brand;
using domain.entities;
using domain.interfaces;
using Moq;
using Shouldly;

namespace tests;

public class BrandServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBrandRepository> _repositoryMock;
    private readonly BrandService _sut;

    public BrandServiceTests()
    {
        _repositoryMock = new Mock<IBrandRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Brands).Returns(_repositoryMock.Object);
        _sut = new BrandService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBrandDto_WhenBrandExists()
    {
        var brand = new Brand { Id = 1, Name = "Royal Canin", Slug = "royal-canin" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(brand);

        var result = await _sut.GetByIdAsync(1);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
        result.Name.ShouldBe("Royal Canin");
        result.Slug.ShouldBe("royal-canin");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowKeyNotFoundException_WhenBrandDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Brand?)null);

        var act = () => _sut.GetByIdAsync(999);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnBrandDto_WhenBrandExists()
    {
        var brand = new Brand { Id = 1, Name = "Royal Canin", Slug = "royal-canin" };
        _repositoryMock.Setup(r => r.GetBySlugAsync("royal-canin")).ReturnsAsync(brand);

        var result = await _sut.GetBySlugAsync("royal-canin");

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Royal Canin");
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldThrowKeyNotFoundException_WhenBrandDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetBySlugAsync(It.IsAny<string>())).ReturnsAsync((Brand?)null);

        var act = () => _sut.GetBySlugAsync("inexistente");

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllBrands()
    {
        var brands = new List<Brand>
        {
            new() { Id = 1, Name = "Royal Canin", Slug = "royal-canin" },
            new() { Id = 2, Name = "Purina", Slug = "purina" }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(brands);

        var result = await _sut.GetAllAsync();

        result.Count().ShouldBe(2);
    }

    [Fact]
    public async Task AddAsync_ShouldCreateBrandAndReturnDto()
    {
        var dto = new CreateBrandDto { Name = "Royal Canin", Slug = "royal-canin" };
        Brand? captured = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Brand>()))
            .Callback<Brand>(c => captured = c);

        var result = await _sut.AddAsync(dto);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Brand>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        result.Name.ShouldBe("Royal Canin");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingBrand()
    {
        var existing = new Brand { Id = 1, Name = "Royal Canin", Slug = "royal-canin" };
        var dto = new UpdateBrandDto { Name = "Royal Canin Actualizado", Slug = "royal-canin-actualizado" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

        await _sut.UpdateAsync(1, dto);

        existing.Name.ShouldBe("Royal Canin Actualizado");
        existing.Slug.ShouldBe("royal-canin-actualizado");
        _repositoryMock.Verify(r => r.UpdateAsync(existing), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenBrandDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Brand?)null);

        var act = () => _sut.UpdateAsync(999, new UpdateBrandDto());

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveBrand()
    {
        _repositoryMock.Setup(r => r.DeleteAsync(1));

        await _sut.DeleteAsync(1);

        _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
