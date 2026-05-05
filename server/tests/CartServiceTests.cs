using application.implementations;
using domain.dtos.CartItem;
using domain.entities;
using domain.interfaces;
using Moq;
using Shouldly;

namespace tests;

public class CartServiceTests
{
    private readonly Mock<ICartRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CartService _sut;

    public CartServiceTests()
    {
        _repositoryMock = new Mock<ICartRepository>();
        _repositoryMock.Setup(r => r.AddItemAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.RemoveItemAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.UpdateQuantityAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.ClearCartAsync(It.IsAny<int>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new CartService(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetCartAsync_ShouldReturnCartWithItems_WhenUserIsAuthenticated()
    {
        var items = new List<CartItem>
        {
            new() { UserId = 1, SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 2 },
            new() { UserId = 1, SessionId = string.Empty, ProductId = 2, VariantId = 2, Quantity = 1 }
        };
        _repositoryMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(items);

        var result = await _sut.GetCartAsync(1, string.Empty);

        result.Items.Count.ShouldBe(2);
        result.Items[0].ProductId.ShouldBe(1);
        result.Items[0].Quantity.ShouldBe(2);
        result.Items[1].ProductId.ShouldBe(2);
        result.Items[1].Quantity.ShouldBe(1);
        result.TotalItems.ShouldBe(3);
    }

    [Fact]
    public async Task GetCartAsync_ShouldReturnCartWithItems_WhenUserIsAnonymous()
    {
        var items = new List<CartItem>
        {
            new() { UserId = 0, SessionId = "anon-session", ProductId = 1, VariantId = 1, Quantity = 1 }
        };
        _repositoryMock.Setup(r => r.GetItemsAsync(0, "anon-session")).ReturnsAsync(items);

        var result = await _sut.GetCartAsync(0, "anon-session");

        result.Items.Count.ShouldBe(1);
        result.TotalItems.ShouldBe(1);
        _repositoryMock.Verify(r => r.GetItemsAsync(0, "anon-session"), Times.Once);
    }

    [Fact]
    public async Task GetCartAsync_ShouldReturnEmptyCart_WhenNoItems()
    {
        _repositoryMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem>());

        var result = await _sut.GetCartAsync(1, string.Empty);

        result.Items.ShouldBeEmpty();
        result.TotalItems.ShouldBe(0);
        result.Subtotal.ShouldBe(0);
    }

    [Fact]
    public async Task AddItemAsync_ShouldAddNewItem_WhenProductNotInCart()
    {
        var dto = new CreateCartItemDto { SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 2 };
        _repositoryMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem>());

        await _sut.AddItemAsync(1, dto);

        _repositoryMock.Verify(r => r.AddItemAsync(It.Is<CartItem>(i =>
            i.UserId == 1 &&
            i.SessionId == string.Empty &&
            i.ProductId == 1 &&
            i.VariantId == 1 &&
            i.Quantity == 2)), Times.Once);
        _repositoryMock.Verify(r => r.UpdateQuantityAsync(It.IsAny<CartItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_ShouldIncrementQuantity_WhenProductAlreadyInCart()
    {
        var existingItem = new CartItem { UserId = 1, SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 2 };
        var dto = new CreateCartItemDto { SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 3 };
        _repositoryMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem> { existingItem });

        await _sut.AddItemAsync(1, dto);

        existingItem.Quantity.ShouldBe(5);
        _repositoryMock.Verify(r => r.UpdateQuantityAsync(existingItem), Times.Once);
        _repositoryMock.Verify(r => r.AddItemAsync(It.IsAny<CartItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_ShouldSetSessionId_WhenUserIsAnonymous()
    {
        var dto = new CreateCartItemDto { SessionId = "anon-456", ProductId = 1, VariantId = 1, Quantity = 1 };
        _repositoryMock.Setup(r => r.GetItemsAsync(0, "anon-456")).ReturnsAsync(new List<CartItem>());

        await _sut.AddItemAsync(0, dto);

        _repositoryMock.Verify(r => r.AddItemAsync(It.Is<CartItem>(i =>
            i.UserId == 0 && i.SessionId == "anon-456")), Times.Once);
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldRemoveItem_WhenItemExists()
    {
        var item = new CartItem { UserId = 1, SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 1 };
        _repositoryMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem> { item });

        await _sut.RemoveItemAsync(1, string.Empty, 1, 1);

        _repositoryMock.Verify(r => r.RemoveItemAsync(item), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldNotThrow_WhenItemDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem>());

        await _sut.RemoveItemAsync(1, string.Empty, 1, 1);

        _repositoryMock.Verify(r => r.RemoveItemAsync(It.IsAny<CartItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateQuantityAsync_ShouldUpdateQuantity_WhenItemExists()
    {
        var item = new CartItem { UserId = 1, SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 2 };
        var dto = new UpdateCartItemDto { SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 5 };
        _repositoryMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem> { item });

        await _sut.UpdateQuantityAsync(1, dto);

        item.Quantity.ShouldBe(5);
        _repositoryMock.Verify(r => r.UpdateQuantityAsync(item), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateQuantityAsync_ShouldNotThrow_WhenItemDoesNotExist()
    {
        var dto = new UpdateCartItemDto { SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 5 };
        _repositoryMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem>());

        await _sut.UpdateQuantityAsync(1, dto);

        _repositoryMock.Verify(r => r.UpdateQuantityAsync(It.IsAny<CartItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateQuantityAsync_ShouldSetQuantityToZero_WhenDtoQuantityIsZero()
    {
        var item = new CartItem { UserId = 1, SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 2 };
        var dto = new UpdateCartItemDto { SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 0 };
        _repositoryMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem> { item });

        await _sut.UpdateQuantityAsync(1, dto);

        item.Quantity.ShouldBe(0);
        _repositoryMock.Verify(r => r.UpdateQuantityAsync(item), Times.Once);
    }

    [Fact]
    public async Task ClearCartAsync_ShouldClearAllItems_ForAuthenticatedUser()
    {
        await _sut.ClearCartAsync(1, string.Empty);

        _repositoryMock.Verify(r => r.ClearCartAsync(1, string.Empty), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ClearCartAsync_ShouldClearAllItems_ForAnonymousUser()
    {
        await _sut.ClearCartAsync(0, "anon-session");

        _repositoryMock.Verify(r => r.ClearCartAsync(0, "anon-session"), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ClearCartAsync_ShouldReturnEmptyCart()
    {
        var result = await _sut.ClearCartAsync(1, string.Empty);

        result.Items.ShouldBeEmpty();
        result.TotalItems.ShouldBe(0);
        result.Subtotal.ShouldBe(0);
    }

    [Fact]
    public async Task TransferCartAsync_ShouldMergeItems_WhenAnonymousCartHasItems()
    {
        var anonymousItems = new List<CartItem>
        {
            new() { UserId = 0, SessionId = "anon-session", ProductId = 1, VariantId = 1, Quantity = 2 },
            new() { UserId = 0, SessionId = "anon-session", ProductId = 3, VariantId = 3, Quantity = 1 }
        };
        var userItems = new List<CartItem>
        {
            new() { UserId = 1, SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 3 }
        };

        _repositoryMock.Setup(r => r.GetItemsAsync(0, "anon-session")).ReturnsAsync(anonymousItems);
        _repositoryMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(userItems);

        await _sut.TransferCartAsync(1, "anon-session");

        // Product 1 existed → should increment (3 + 2 = 5)
        userItems[0].Quantity.ShouldBe(5);
        _repositoryMock.Verify(r => r.UpdateQuantityAsync(userItems[0]), Times.Once);

        // Product 3 was new → should add
        _repositoryMock.Verify(r => r.AddItemAsync(It.Is<CartItem>(i =>
            i.UserId == 1 &&
            i.SessionId == string.Empty &&
            i.ProductId == 3 &&
            i.VariantId == 3 &&
            i.Quantity == 1)), Times.Once);

        // Anonymous cart should be cleared
        _repositoryMock.Verify(r => r.ClearCartAsync(0, "anon-session"), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task TransferCartAsync_ShouldNotModifyUserCart_WhenAnonymousCartIsEmpty()
    {
        _repositoryMock.Setup(r => r.GetItemsAsync(0, "anon-empty")).ReturnsAsync(new List<CartItem>());
        _repositoryMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem>());

        await _sut.TransferCartAsync(1, "anon-empty");

        _repositoryMock.Verify(r => r.AddItemAsync(It.IsAny<CartItem>()), Times.Never);
        _repositoryMock.Verify(r => r.UpdateQuantityAsync(It.IsAny<CartItem>()), Times.Never);
        _repositoryMock.Verify(r => r.ClearCartAsync(0, "anon-empty"), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
}
