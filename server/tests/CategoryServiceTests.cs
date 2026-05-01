using application.implementations;
using domain.dtos.Category;
using domain.entities;
using domain.interfaces;
using Moq;
using Shouldly;

namespace tests;

public class CategoryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Categories).Returns(_repositoryMock.Object);
        _sut = new CategoryService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategoryDto_WhenCategoryExists()
    {
        var category = new Category { Id = 1, Name = "Perros", Slug = "perros" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

        var result = await _sut.GetByIdAsync(1);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
        result.Name.ShouldBe("Perros");
        result.Slug.ShouldBe("perros");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowKeyNotFoundException_WhenCategoryDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category?)null);

        var act = () => _sut.GetByIdAsync(999);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnCategoryDto_WhenCategoryExists()
    {
        var category = new Category { Id = 1, Name = "Perros", Slug = "perros" };
        _repositoryMock.Setup(r => r.GetBySlugAsync("perros")).ReturnsAsync(category);

        var result = await _sut.GetBySlugAsync("perros");

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Perros");
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldThrowKeyNotFoundException_WhenCategoryDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetBySlugAsync(It.IsAny<string>())).ReturnsAsync((Category?)null);

        var act = () => _sut.GetBySlugAsync("inexistente");

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Perros", Slug = "perros" },
            new() { Id = 2, Name = "Gatos", Slug = "gatos" }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        var result = await _sut.GetAllAsync();

        result.Count().ShouldBe(2);
    }

    [Fact]
    public async Task AddAsync_ShouldCreateCategoryAndReturnDto()
    {
        var dto = new CreateCategoryDto { Name = "Perros", Slug = "perros" };
        Category? captured = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Category>()))
            .Callback<Category>(c => captured = c);

        var result = await _sut.AddAsync(dto);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        result.Name.ShouldBe("Perros");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingCategory()
    {
        var existing = new Category { Id = 1, Name = "Perros", Slug = "perros" };
        var dto = new UpdateCategoryDto { Name = "Perros Actualizado", Slug = "perros-actualizado" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

        await _sut.UpdateAsync(1, dto);

        existing.Name.ShouldBe("Perros Actualizado");
        existing.Slug.ShouldBe("perros-actualizado");
        _repositoryMock.Verify(r => r.UpdateAsync(existing), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenCategoryDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category?)null);

        var act = () => _sut.UpdateAsync(999, new UpdateCategoryDto());

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveCategory()
    {
        _repositoryMock.Setup(r => r.DeleteAsync(1));

        await _sut.DeleteAsync(1);

        _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
