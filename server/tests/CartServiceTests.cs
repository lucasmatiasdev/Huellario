using application.implementations;
using application.dtos.CartItem;
using domain.entities;
using domain.interfaces;
using Moq;
using Shouldly;

namespace tests;

public class CartServiceTests
{
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CartService _sut;

    public CartServiceTests()
    {
        _cartRepoMock = new Mock<ICartRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.CartItems).Returns(_cartRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(default)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync(default)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync(default)).Returns(Task.CompletedTask);

        _sut = new CartService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetCartAsync_ShouldReturnCartWithItems_WhenUserIsAuthenticated()
    {
        var items = new List<CartItem>
        {
            new() { UserId = 1, SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 2 },
            new() { UserId = 1, SessionId = string.Empty, ProductId = 2, VariantId = 2, Quantity = 1 }
        };
        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(items);

        var result = await _sut.GetCartAsync(1, string.Empty);

        result.Items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetCartAsync_ShouldReturnCartWithItems_WhenUserIsAnonymous()
    {
        var items = new List<CartItem>
        {
            new() { UserId = 0, SessionId = "anon-session", ProductId = 1, VariantId = 1, Quantity = 1 }
        };
        _cartRepoMock.Setup(r => r.GetItemsAsync(0, "anon-session")).ReturnsAsync(items);

        var result = await _sut.GetCartAsync(0, "anon-session");

        result.Items.Count.ShouldBe(1);
        _cartRepoMock.Verify(r => r.GetItemsAsync(0, "anon-session"), Times.Once);
    }

    [Fact]
    public async Task GetCartAsync_ShouldReturnEmptyCart_WhenNoItems()
    {
        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem>());

        var result = await _sut.GetCartAsync(1, string.Empty);

        result.Items.ShouldBeEmpty();
        result.TotalItems.ShouldBe(0);
        result.Subtotal.ShouldBe(0);
    }

    [Fact]
    public async Task AddItemAsync_ShouldAddNewItem_WhenProductNotInCart()
    {
        var dto = new CreateCartItemDto { SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 2 };
        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem>());
        _productRepoMock.Setup(r => r.GetVariantByIdAsync(1)).ReturnsAsync(new Variant { Id = 1, Stock = 10 });
        _cartRepoMock.Setup(r => r.AddItemAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);

        var result = await _sut.AddItemAsync(1, dto);

        _cartRepoMock.Verify(r => r.AddItemAsync(It.Is<CartItem>(i =>
            i.UserId == 1 &&
            i.SessionId == string.Empty &&
            i.ProductId == 1 &&
            i.VariantId == 1 &&
            i.Quantity == 2)), Times.Once);
        _cartRepoMock.Verify(r => r.UpdateQuantityAsync(It.IsAny<CartItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_ShouldIncrementQuantity_WhenProductAlreadyInCart()
    {
        var existingItem = new CartItem { UserId = 1, SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 2 };
        var dto = new CreateCartItemDto { SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 3 };
        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem> { existingItem });
        _productRepoMock.Setup(r => r.GetVariantByIdAsync(1)).ReturnsAsync(new Variant { Id = 1, Stock = 10 });

        var result = await _sut.AddItemAsync(1, dto);

        existingItem.Quantity.ShouldBe(5);
        _cartRepoMock.Verify(r => r.UpdateQuantityAsync(existingItem), Times.Once);
        _cartRepoMock.Verify(r => r.AddItemAsync(It.IsAny<CartItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_ShouldSetSessionId_WhenUserIsAnonymous()
    {
        var dto = new CreateCartItemDto { SessionId = "anon-456", ProductId = 1, VariantId = 1, Quantity = 1 };
        _cartRepoMock.Setup(r => r.GetItemsAsync(0, "anon-456")).ReturnsAsync(new List<CartItem>());
        _productRepoMock.Setup(r => r.GetVariantByIdAsync(1)).ReturnsAsync(new Variant { Id = 1, Stock = 10 });

        await _sut.AddItemAsync(0, dto);

        _cartRepoMock.Verify(r => r.AddItemAsync(It.Is<CartItem>(i =>
            i.UserId == 0 && i.SessionId == "anon-456")), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrowInvalidOperationException_WhenVariantNotFound()
    {
        var dto = new CreateCartItemDto { SessionId = string.Empty, ProductId = 1, VariantId = 99, Quantity = 1 };
        _productRepoMock.Setup(r => r.GetVariantByIdAsync(99)).ReturnsAsync((Variant?)null);

        var act = () => _sut.AddItemAsync(1, dto);

        var ex = await act.ShouldThrowAsync<InvalidOperationException>();
        ex.Message.ShouldContain("stock");
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrowInvalidOperationException_WhenInsufficientStockForNewItem()
    {
        var dto = new CreateCartItemDto { SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 10 };
        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem>());
        _productRepoMock.Setup(r => r.GetVariantByIdAsync(1)).ReturnsAsync(new Variant { Id = 1, Stock = 5 });

        var act = () => _sut.AddItemAsync(1, dto);

        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrowInvalidOperationException_WhenInsufficientStockForCombinedQuantity()
    {
        var existingItem = new CartItem { UserId = 1, SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 8 };
        var dto = new CreateCartItemDto { SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 5 };
        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem> { existingItem });
        _productRepoMock.Setup(r => r.GetVariantByIdAsync(1)).ReturnsAsync(new Variant { Id = 1, Stock = 10 });

        var act = () => _sut.AddItemAsync(1, dto);

        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldRemoveItem_WhenItemExists()
    {
        var item = new CartItem { UserId = 1, SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 1 };
        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem> { item });
        _cartRepoMock.Setup(r => r.RemoveItemAsync(item)).Returns(Task.CompletedTask);

        await _sut.RemoveItemAsync(1, string.Empty, 1, 1);

        _cartRepoMock.Verify(r => r.RemoveItemAsync(item), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldNotThrow_WhenItemDoesNotExist()
    {
        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem>());

        await _sut.RemoveItemAsync(1, string.Empty, 1, 1);

        _cartRepoMock.Verify(r => r.RemoveItemAsync(It.IsAny<CartItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateQuantityAsync_ShouldUpdateQuantity_WhenItemExists()
    {
        var item = new CartItem { UserId = 1, SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 2 };
        var dto = new UpdateCartItemDto { SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 5 };
        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem> { item });
        _productRepoMock.Setup(r => r.GetVariantByIdAsync(1)).ReturnsAsync(new Variant { Id = 1, Stock = 10 });

        await _sut.UpdateQuantityAsync(1, dto);

        item.Quantity.ShouldBe(5);
        _cartRepoMock.Verify(r => r.UpdateQuantityAsync(item), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateQuantityAsync_ShouldNotThrow_WhenItemDoesNotExist()
    {
        var dto = new UpdateCartItemDto { SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 5 };
        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem>());
        _productRepoMock.Setup(r => r.GetVariantByIdAsync(1)).ReturnsAsync(new Variant { Id = 1, Stock = 10 });

        await _sut.UpdateQuantityAsync(1, dto);

        _cartRepoMock.Verify(r => r.UpdateQuantityAsync(It.IsAny<CartItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateQuantityAsync_ShouldThrowInvalidOperationException_WhenVariantNotFound()
    {
        var dto = new UpdateCartItemDto { SessionId = string.Empty, ProductId = 1, VariantId = 99, Quantity = 5 };
        _productRepoMock.Setup(r => r.GetVariantByIdAsync(99)).ReturnsAsync((Variant?)null);

        var act = () => _sut.UpdateQuantityAsync(1, dto);

        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateQuantityAsync_ShouldThrowInvalidOperationException_WhenInsufficientStock()
    {
        var item = new CartItem { UserId = 1, SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 2 };
        var dto = new UpdateCartItemDto { SessionId = string.Empty, ProductId = 1, VariantId = 1, Quantity = 20 };
        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem> { item });
        _productRepoMock.Setup(r => r.GetVariantByIdAsync(1)).ReturnsAsync(new Variant { Id = 1, Stock = 10 });

        var act = () => _sut.UpdateQuantityAsync(1, dto);

        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ClearCartAsync_ShouldClearAllItems_ForAuthenticatedUser()
    {
        _cartRepoMock.Setup(r => r.ClearCartAsync(1, string.Empty)).Returns(Task.CompletedTask);

        await _sut.ClearCartAsync(1, string.Empty);

        _cartRepoMock.Verify(r => r.ClearCartAsync(1, string.Empty), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ClearCartAsync_ShouldClearAllItems_ForAnonymousUser()
    {
        _cartRepoMock.Setup(r => r.ClearCartAsync(0, "anon-session")).Returns(Task.CompletedTask);

        await _sut.ClearCartAsync(0, "anon-session");

        _cartRepoMock.Verify(r => r.ClearCartAsync(0, "anon-session"), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ClearCartAsync_ShouldReturnEmptyCart()
    {
        _cartRepoMock.Setup(r => r.ClearCartAsync(It.IsAny<int>(), It.IsAny<string>())).Returns(Task.CompletedTask);

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

        _cartRepoMock.Setup(r => r.GetItemsAsync(0, "anon-session")).ReturnsAsync(anonymousItems);
        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(userItems);
        _productRepoMock.Setup(r => r.GetVariantByIdAsync(1)).ReturnsAsync(new Variant { Id = 1, Stock = 10 });
        _productRepoMock.Setup(r => r.GetVariantByIdAsync(3)).ReturnsAsync(new Variant { Id = 3, Stock = 10 });
        _cartRepoMock.Setup(r => r.AddItemAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);
        _cartRepoMock.Setup(r => r.ClearCartAsync(0, "anon-session")).Returns(Task.CompletedTask);

        await _sut.TransferCartAsync(1, "anon-session");

        userItems[0].Quantity.ShouldBe(5);
        _cartRepoMock.Verify(r => r.UpdateQuantityAsync(userItems[0]), Times.Once);
        _cartRepoMock.Verify(r => r.AddItemAsync(It.Is<CartItem>(i =>
            i.UserId == 1 &&
            i.SessionId == string.Empty &&
            i.ProductId == 3 &&
            i.VariantId == 3 &&
            i.Quantity == 1)), Times.Once);
        _cartRepoMock.Verify(r => r.ClearCartAsync(0, "anon-session"), Times.Once);
    }

    [Fact]
    public async Task TransferCartAsync_ShouldNotModifyUserCart_WhenAnonymousCartIsEmpty()
    {
        _cartRepoMock.Setup(r => r.GetItemsAsync(0, "anon-empty")).ReturnsAsync(new List<CartItem>());

        var result = await _sut.TransferCartAsync(1, "anon-empty");

        _cartRepoMock.Verify(r => r.AddItemAsync(It.IsAny<CartItem>()), Times.Never);
        _cartRepoMock.Verify(r => r.UpdateQuantityAsync(It.IsAny<CartItem>()), Times.Never);
        _cartRepoMock.Verify(r => r.ClearCartAsync(0, "anon-empty"), Times.Never);
    }

    [Fact]
    public async Task TransferCartAsync_ShouldThrowInvalidOperationException_WhenVariantNotFound()
    {
        var anonymousItems = new List<CartItem>
        {
            new() { UserId = 0, SessionId = "anon-session", ProductId = 1, VariantId = 99, Quantity = 1 }
        };

        _cartRepoMock.Setup(r => r.GetItemsAsync(0, "anon-session")).ReturnsAsync(anonymousItems);
        _productRepoMock.Setup(r => r.GetVariantByIdAsync(99)).ReturnsAsync((Variant?)null);

        var act = () => _sut.TransferCartAsync(1, "anon-session");

        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task TransferCartAsync_ShouldThrowInvalidOperationException_WhenInsufficientStock()
    {
        var anonymousItems = new List<CartItem>
        {
            new() { UserId = 0, SessionId = "anon-session", ProductId = 1, VariantId = 1, Quantity = 5 }
        };

        _cartRepoMock.Setup(r => r.GetItemsAsync(0, "anon-session")).ReturnsAsync(anonymousItems);
        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem>());
        _productRepoMock.Setup(r => r.GetVariantByIdAsync(1)).ReturnsAsync(new Variant { Id = 1, Stock = 3 });

        var act = () => _sut.TransferCartAsync(1, "anon-session");

        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task TransferCartAsync_ShouldRollbackTransaction_WhenErrorOccurs()
    {
        var anonymousItems = new List<CartItem>
        {
            new() { UserId = 0, SessionId = "anon-session", ProductId = 1, VariantId = 1, Quantity = 1 }
        };

        _cartRepoMock.Setup(r => r.GetItemsAsync(0, "anon-session")).ReturnsAsync(anonymousItems);
        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem>());
        _productRepoMock.Setup(r => r.GetVariantByIdAsync(1)).ReturnsAsync(new Variant { Id = 1, Stock = 10 });
        _cartRepoMock.Setup(r => r.AddItemAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ThrowsAsync(new Exception("DB error"));

        var act = () => _sut.TransferCartAsync(1, "anon-session");

        await act.ShouldThrowAsync<Exception>();
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(default), Times.Once);
    }
}