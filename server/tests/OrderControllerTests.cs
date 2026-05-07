using System.Security.Claims;
using application.interfaces;
using domain.dtos.Order;
using domain.enums;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using server.controllers;

namespace tests;

public class OrderControllerTests
{
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly Mock<IValidator<CreateOrderDto>> _validatorMock;
    private readonly OrderController _sut;

    public OrderControllerTests()
    {
        _orderServiceMock = new Mock<IOrderService>();
        _validatorMock = new Mock<IValidator<CreateOrderDto>>();
        _sut = new OrderController(_orderServiceMock.Object, _validatorMock.Object);
    }

    private void SetAuthenticatedUser(int userId)
    {
        var claims = new[] { new Claim("UserId", userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private void SetAnonymousUser()
    {
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    private static void MockValid(Mock<IValidator<CreateOrderDto>> mock)
    {
        mock.Setup(v => v.ValidateAsync(It.IsAny<CreateOrderDto>(), default))
            .ReturnsAsync(new ValidationResult());
    }

    private static void MockInvalid(Mock<IValidator<CreateOrderDto>> mock)
    {
        mock.Setup(v => v.ValidateAsync(It.IsAny<CreateOrderDto>(), default))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("AddressId", "La dirección es obligatoria")
            }));
    }

    // ---- POST /api/order ----

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenAuthenticated()
    {
        SetAuthenticatedUser(1);
        MockValid(_validatorMock);
        var dto = new CreateOrderDto { AddressId = 1, IsRetirement = false };
        var expected = new OrderDto { Id = 1, UserId = 1 };
        _orderServiceMock.Setup(s => s.CreateOrderAsync(1, "", dto)).ReturnsAsync(expected);

        var result = await _sut.Create(dto, null);

        var created = result.Result.ShouldBeOfType<CreatedAtActionResult>();
        created.ActionName.ShouldBe("GetById");
        created.RouteValues.ShouldNotBeNull();
        created.RouteValues["id"].ShouldBe(1);
        created.Value.ShouldBe(expected);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenGuest()
    {
        SetAnonymousUser();
        MockValid(_validatorMock);
        var dto = new CreateOrderDto { AddressId = 1, IsRetirement = false };
        var expected = new OrderDto { Id = 2 };
        _orderServiceMock.Setup(s => s.CreateOrderAsync(0, "abc-123", dto)).ReturnsAsync(expected);

        var result = await _sut.Create(dto, "abc-123");

        var created = result.Result.ShouldBeOfType<CreatedAtActionResult>();
        created.ActionName.ShouldBe("GetById");
        created.RouteValues.ShouldNotBeNull();
        created.RouteValues["id"].ShouldBe(2);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenValidationFails()
    {
        SetAuthenticatedUser(1);
        MockInvalid(_validatorMock);
        var dto = new CreateOrderDto { IsRetirement = false };

        var result = await _sut.Create(dto, null);

        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenCartIsEmpty()
    {
        SetAuthenticatedUser(1);
        MockValid(_validatorMock);
        var dto = new CreateOrderDto { AddressId = 1, IsRetirement = false };
        _orderServiceMock.Setup(s => s.CreateOrderAsync(1, "", dto))
            .ThrowsAsync(new InvalidOperationException("El carrito está vacío"));

        var result = await _sut.Create(dto, null);

        var bad = result.Result.ShouldBeOfType<BadRequestObjectResult>();
        bad.Value.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenInsufficientStock()
    {
        SetAuthenticatedUser(1);
        MockValid(_validatorMock);
        var dto = new CreateOrderDto { AddressId = 1, IsRetirement = false };
        _orderServiceMock.Setup(s => s.CreateOrderAsync(1, "", dto))
            .ThrowsAsync(new InvalidOperationException("Stock insuficiente"));

        var result = await _sut.Create(dto, null);

        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    // ---- GET /api/order ----

    [Fact]
    public async Task GetAll_ShouldReturnOrders_WhenUserHasOrders()
    {
        SetAuthenticatedUser(1);
        var orders = new List<OrderListDto>
        {
            new() { Id = 1, Total = 100, Status = OrderStatus.Pending }
        };
        _orderServiceMock.Setup(s => s.GetByUserAsync(1)).ReturnsAsync(orders);

        var result = await _sut.GetAll();

        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var items = ok.Value.ShouldBeOfType<List<OrderListDto>>();
        items.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyList_WhenUserHasNoOrders()
    {
        SetAuthenticatedUser(1);
        _orderServiceMock.Setup(s => s.GetByUserAsync(1)).ReturnsAsync(new List<OrderListDto>());

        var result = await _sut.GetAll();

        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var items = ok.Value.ShouldBeOfType<List<OrderListDto>>();
        items.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_ShouldThrowUnauthorized_WhenNotAuthenticated()
    {
        SetAnonymousUser();

        var act = () => _sut.GetAll();

        await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    // ---- GET /api/order/{id} ----

    [Fact]
    public async Task GetById_ShouldReturnOrder_WhenOwnOrder()
    {
        SetAuthenticatedUser(1);
        var order = new OrderDto { Id = 1, UserId = 1 };
        _orderServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(order);

        var result = await _sut.GetById(1);

        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(order);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenNotOwnOrder()
    {
        SetAuthenticatedUser(1);
        var order = new OrderDto { Id = 1, UserId = 2 };
        _orderServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(order);

        var result = await _sut.GetById(1);

        result.Result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        SetAuthenticatedUser(1);
        _orderServiceMock.Setup(s => s.GetByIdAsync(999)).ThrowsAsync(new KeyNotFoundException());

        var result = await _sut.GetById(999);

        result.Result.ShouldBeOfType<NotFoundResult>();
    }

    // ---- POST /api/order/{id}/cancel ----

    [Fact]
    public async Task Cancel_ShouldReturnOk_WhenOwnPendingOrder()
    {
        SetAuthenticatedUser(1);
        _orderServiceMock.Setup(s => s.CancelAsync(1, 1)).Returns(Task.CompletedTask);

        var result = await _sut.Cancel(1);

        result.ShouldBeOfType<OkResult>();
    }

    [Fact]
    public async Task Cancel_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        SetAuthenticatedUser(1);
        _orderServiceMock.Setup(s => s.CancelAsync(999, 1)).ThrowsAsync(new KeyNotFoundException());

        var result = await _sut.Cancel(999);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Cancel_ShouldReturnBadRequest_WhenNotPending()
    {
        SetAuthenticatedUser(1);
        _orderServiceMock.Setup(s => s.CancelAsync(1, 1))
            .ThrowsAsync(new InvalidOperationException("Solo se pueden cancelar pedidos pendientes"));

        var result = await _sut.Cancel(1);

        var bad = result.ShouldBeOfType<BadRequestObjectResult>();
        bad.Value.ShouldNotBeNull();
    }

    [Fact]
    public async Task Cancel_ShouldReturnBadRequest_WhenNotOwnOrder()
    {
        SetAuthenticatedUser(1);
        _orderServiceMock.Setup(s => s.CancelAsync(1, 1))
            .ThrowsAsync(new UnauthorizedAccessException("No puedes cancelar un pedido que no te pertenece"));

        var result = await _sut.Cancel(1);

        var bad = result.ShouldBeOfType<BadRequestObjectResult>();
        bad.Value.ShouldNotBeNull();
    }
}
