using application.implementations;
using application.dtos.Order;
using domain.entities;
using domain.enums;
using domain.interfaces;
using Moq;
using Shouldly;

namespace tests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _cartRepoMock = new Mock<ICartRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.CartItems).Returns(_cartRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(default)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync(default)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync(default)).Returns(Task.CompletedTask);

        _sut = new OrderService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        var order = new Order
        {
            Id = 1, UserId = 1, Status = OrderStatus.Pending, Total = 100,
            OrderLines = new List<OrderLine>(), User = new User { Name = "Juan", Surname = "Perez" }
        };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        var result = await _sut.GetByIdAsync(1);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowKeyNotFoundException_WhenOrderDoesNotExist()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Order?)null);

        var act = () => _sut.GetByIdAsync(999);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetByUserAsync_ShouldReturnOrdersForUser()
    {
        var orders = new List<Order>
        {
            new() { Id = 1, UserId = 1, Status = OrderStatus.Pending, OrderLines = new List<OrderLine> { new() { Quantity = 2 } } },
            new() { Id = 2, UserId = 1, Status = OrderStatus.Confirmed, OrderLines = new List<OrderLine> { new() { Quantity = 1 } } }
        };
        _orderRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(orders);

        var result = await _sut.GetByUserAsync(1);

        result.Count().ShouldBe(2);
    }

    [Fact]
    public async Task GetByUserAsync_ShouldReturnEmptyList_WhenUserHasNoOrders()
    {
        _orderRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(new List<Order>());

        var result = await _sut.GetByUserAsync(1);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllOrders()
    {
        var orders = new List<Order>
        {
            new() { Id = 1, UserId = 1, Status = OrderStatus.Pending, OrderLines = new List<OrderLine>(), User = new User { Name = "A", Surname = "B" } },
            new() { Id = 2, UserId = 2, Status = OrderStatus.Confirmed, OrderLines = new List<OrderLine>(), User = new User { Name = "C", Surname = "D" } }
        };
        _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);

        var result = await _sut.GetAllAsync();

        result.Count().ShouldBe(2);
    }

    [Fact]
    public async Task CancelAsync_ShouldCancelOrder_WhenPending()
    {
        var variant = new Variant { Id = 1, Stock = 10 };
        var order = new Order
        {
            Id = 1, UserId = 1, Status = OrderStatus.Pending,
            OrderLines = new List<OrderLine> { new() { VariantId = 1, Quantity = 3, Variant = variant } }
        };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        await _sut.CancelAsync(1, 1);

        order.Status.ShouldBe(OrderStatus.Cancelled);
        variant.Stock.ShouldBe(13);
        _orderRepoMock.Verify(r => r.UpdateAsync(order), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(default), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowKeyNotFoundException_WhenOrderDoesNotExist()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Order?)null);

        var act = () => _sut.CancelAsync(999, 1);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowUnauthorizedAccessException_WhenNotOwnOrder()
    {
        var order = new Order { Id = 1, UserId = 2, Status = OrderStatus.Pending };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        var act = () => _sut.CancelAsync(1, 1);

        await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowInvalidOperationException_WhenNotPending()
    {
        var order = new Order { Id = 1, UserId = 1, Status = OrderStatus.Confirmed };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        var act = () => _sut.CancelAsync(1, 1);

        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowInvalidOperationException_WhenAlreadyCancelled()
    {
        var order = new Order { Id = 1, UserId = 1, Status = OrderStatus.Cancelled };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        var act = () => _sut.CancelAsync(1, 1);

        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CancelBySessionAsync_ShouldCancelOrder_WhenPendingAndOwnSession()
    {
        var order = new Order
        {
            Id = 1, SessionId = "abc", Status = OrderStatus.Pending,
            OrderLines = new List<OrderLine>()
        };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        await _sut.CancelBySessionAsync(1, "abc");

        order.Status.ShouldBe(OrderStatus.Cancelled);
        _orderRepoMock.Verify(r => r.UpdateAsync(order), Times.Once);
    }

    [Fact]
    public async Task CancelBySessionAsync_ShouldThrowUnauthorizedAccessException_WhenDifferentSession()
    {
        var order = new Order { Id = 1, SessionId = "abc", Status = OrderStatus.Pending };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        var act = () => _sut.CancelBySessionAsync(1, "different");

        await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateToConfirmed_WhenPending()
    {
        var order = new Order { Id = 1, Status = OrderStatus.Pending, OrderLines = new List<OrderLine>() };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        var result = await _sut.UpdateStatusAsync(1, OrderStatus.Confirmed);

        order.Status.ShouldBe(OrderStatus.Confirmed);
        result.Status.ShouldBe(OrderStatus.Confirmed);
        _orderRepoMock.Verify(r => r.UpdateAsync(order), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateToShipping_WhenConfirmed()
    {
        var order = new Order { Id = 1, Status = OrderStatus.Confirmed, OrderLines = new List<OrderLine>() };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        var result = await _sut.UpdateStatusAsync(1, OrderStatus.Shipping);

        order.Status.ShouldBe(OrderStatus.Shipping);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateToDelivered_WhenShipping()
    {
        var order = new Order { Id = 1, Status = OrderStatus.Shipping, OrderLines = new List<OrderLine>() };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        var result = await _sut.UpdateStatusAsync(1, OrderStatus.Delivered);

        order.Status.ShouldBe(OrderStatus.Delivered);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateToCancelled_WhenPending()
    {
        var order = new Order { Id = 1, Status = OrderStatus.Pending, OrderLines = new List<OrderLine>() };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        var result = await _sut.UpdateStatusAsync(1, OrderStatus.Cancelled);

        order.Status.ShouldBe(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateToCancelled_WhenConfirmed()
    {
        var order = new Order { Id = 1, Status = OrderStatus.Confirmed, OrderLines = new List<OrderLine>() };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        var result = await _sut.UpdateStatusAsync(1, OrderStatus.Cancelled);

        order.Status.ShouldBe(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrowInvalidOperationException_WhenTransitionFromDelivered()
    {
        var order = new Order { Id = 1, Status = OrderStatus.Delivered };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        var act = () => _sut.UpdateStatusAsync(1, OrderStatus.Confirmed);

        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrowInvalidOperationException_WhenTransitionFromCancelled()
    {
        var order = new Order { Id = 1, Status = OrderStatus.Cancelled };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        var act = () => _sut.UpdateStatusAsync(1, OrderStatus.Pending);

        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrowInvalidOperationException_WhenPendingToDelivered()
    {
        var order = new Order { Id = 1, Status = OrderStatus.Pending };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        var act = () => _sut.UpdateStatusAsync(1, OrderStatus.Delivered);

        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrowKeyNotFoundException_WhenOrderDoesNotExist()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Order?)null);

        var act = () => _sut.UpdateStatusAsync(999, OrderStatus.Confirmed);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrowInvalidOperationException_WhenCartIsEmpty()
    {
        var dto = new CreateOrderDto { AddressId = 1, IsPickup = false };
        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem>());

        var act = () => _sut.CreateOrderAsync(1, string.Empty, dto);

        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldCreateOrder_WhenCartHasItems()
    {
        var product = new Product { Id = 1, Name = "Test Product", Price = 50 };
        var variant = new Variant { Id = 1, ProductId = 1, Stock = 10, Price = 10 };
        var cartItem = new CartItem { UserId = 1, SessionId = "", ProductId = 1, VariantId = 1, Quantity = 2, Product = product, Variant = variant };
        var dto = new CreateOrderDto { AddressId = 1, IsPickup = false };

        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem> { cartItem });
        _cartRepoMock.Setup(r => r.ClearCartAsync(1, string.Empty)).Returns(Task.CompletedTask);
        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

        var act = () => _sut.CreateOrderAsync(1, string.Empty, dto);

        await act.ShouldNotThrowAsync();
        _orderRepoMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldSetTotalBasedOnUnitPrice_WhenProductAndVariant()
    {
        var product = new Product { Id = 1, Name = "Test Product", Price = 50 };
        var variant = new Variant { Id = 1, ProductId = 1, Stock = 10, Price = 10 };
        var cartItem = new CartItem { UserId = 1, SessionId = "", ProductId = 1, VariantId = 1, Quantity = 3, Product = product, Variant = variant };
        var dto = new CreateOrderDto { AddressId = 1, IsPickup = false };

        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem> { cartItem });
        _cartRepoMock.Setup(r => r.ClearCartAsync(1, string.Empty)).Returns(Task.CompletedTask);
        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<Order>())).Callback<Order>(o => o.Id = 1);

        var order = await _sut.CreateOrderAsync(1, string.Empty, dto);

        order.Total.ShouldBe(180);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrowInvalidOperationException_WhenProductNotFound()
    {
        var cartItem = new CartItem { UserId = 1, SessionId = "", ProductId = 1, VariantId = 1, Quantity = 2, Product = null, Variant = new Variant { Id = 1, Stock = 5 } };
        var dto = new CreateOrderDto { AddressId = 1, IsPickup = false };

        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem> { cartItem });

        var act = () => _sut.CreateOrderAsync(1, string.Empty, dto);

        var ex = await act.ShouldThrowAsync<InvalidOperationException>();
        ex.Message.ShouldContain("Producto");
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrowInvalidOperationException_WhenVariantNotFound()
    {
        var cartItem = new CartItem { UserId = 1, SessionId = "", ProductId = 1, VariantId = 1, Quantity = 2, Product = new Product { Id = 1, Name = "Test", Price = 50 }, Variant = null };
        var dto = new CreateOrderDto { AddressId = 1, IsPickup = false };

        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem> { cartItem });

        var act = () => _sut.CreateOrderAsync(1, string.Empty, dto);

        var ex = await act.ShouldThrowAsync<InvalidOperationException>();
        ex.Message.ShouldContain("Variante");
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrowInvalidOperationException_WhenInsufficientStock()
    {
        var product = new Product { Id = 1, Name = "Test Product", Price = 50 };
        var variant = new Variant { Id = 1, ProductId = 1, Stock = 1, Price = 0 };
        var cartItem = new CartItem { UserId = 1, SessionId = "", ProductId = 1, VariantId = 1, Quantity = 5, Product = product, Variant = variant };
        var dto = new CreateOrderDto { AddressId = 1, IsPickup = false };

        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem> { cartItem });

        var act = () => _sut.CreateOrderAsync(1, string.Empty, dto);

        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldDecrementVariantStock()
    {
        var product = new Product { Id = 1, Name = "Test Product", Price = 50 };
        var variant = new Variant { Id = 1, ProductId = 1, Stock = 10, Price = 5 };
        var cartItem = new CartItem { UserId = 1, SessionId = "", ProductId = 1, VariantId = 1, Quantity = 3, Product = product, Variant = variant };
        var dto = new CreateOrderDto { AddressId = 1, IsPickup = false };

        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem> { cartItem });
        _cartRepoMock.Setup(r => r.ClearCartAsync(1, string.Empty)).Returns(Task.CompletedTask);
        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

        await _sut.CreateOrderAsync(1, string.Empty, dto);

        variant.Stock.ShouldBe(7);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldRollbackOnFailure()
    {
        var product = new Product { Id = 1, Name = "Test Product", Price = 50 };
        var variant = new Variant { Id = 1, ProductId = 1, Stock = 10, Price = 5 };
        var cartItem = new CartItem { UserId = 1, SessionId = "", ProductId = 1, VariantId = 1, Quantity = 2, Product = product, Variant = variant };
        var dto = new CreateOrderDto { AddressId = 1, IsPickup = false };

        _cartRepoMock.Setup(r => r.GetItemsAsync(1, string.Empty)).ReturnsAsync(new List<CartItem> { cartItem });
        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ThrowsAsync(new Exception("DB error"));

        var act = () => _sut.CreateOrderAsync(1, string.Empty, dto);

        await act.ShouldThrowAsync<Exception>();
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(default), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldRestoreStock_WhenOrderCancelled()
    {
        var variant = new Variant { Id = 1, Stock = 5 };
        var order = new Order
        {
            Id = 1, UserId = 1, Status = OrderStatus.Pending,
            OrderLines = new List<OrderLine>
            {
                new() { VariantId = 1, Quantity = 3, Variant = variant }
            }
        };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        await _sut.CancelAsync(1, 1);

        variant.Stock.ShouldBe(8);
    }

    [Fact]
    public async Task CancelAsync_ShouldNotRestoreStock_WhenVariantIsNull()
    {
        var order = new Order
        {
            Id = 1, UserId = 1, Status = OrderStatus.Pending,
            OrderLines = new List<OrderLine>
            {
                new() { VariantId = 1, Quantity = 3, Variant = null }
            }
        };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        await _sut.CancelAsync(1, 1);

        order.Status.ShouldBe(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task GetBySessionIdAsync_ShouldReturnOrdersForSession()
    {
        var orders = new List<Order>
        {
            new() { Id = 1, SessionId = "abc", Status = OrderStatus.Pending, OrderLines = new List<OrderLine> { new() { Quantity = 1 } } }
        };
        _orderRepoMock.Setup(r => r.GetBySessionIdAsync("abc")).ReturnsAsync(orders);

        var result = await _sut.GetBySessionIdAsync("abc");

        result.Count().ShouldBe(1);
    }
}