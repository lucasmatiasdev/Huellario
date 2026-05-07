using application.interfaces;
using domain.dtos.Order;
using domain.enums;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using server.controllers.Admin;

namespace tests;

public class AdminOrderControllerTests
{
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly Mock<IValidator<UpdateOrderStatusDto>> _validatorMock;
    private readonly OrderController _sut;

    public AdminOrderControllerTests()
    {
        _orderServiceMock = new Mock<IOrderService>();
        _validatorMock = new Mock<IValidator<UpdateOrderStatusDto>>();
        _sut = new OrderController(_orderServiceMock.Object, _validatorMock.Object);
    }

    private static void MockValid(Mock<IValidator<UpdateOrderStatusDto>> mock)
    {
        mock.Setup(v => v.ValidateAsync(It.IsAny<UpdateOrderStatusDto>(), default))
            .ReturnsAsync(new ValidationResult());
    }

    private static void MockInvalid(Mock<IValidator<UpdateOrderStatusDto>> mock)
    {
        mock.Setup(v => v.ValidateAsync(It.IsAny<UpdateOrderStatusDto>(), default))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("Status", "Estado inválido")
            }));
    }

    // ---- GET /api/admin/orders ----

    [Fact]
    public async Task GetAll_ShouldReturnAllOrders()
    {
        var orders = new List<OrderListDto>
        {
            new() { Id = 1, Total = 100, Status = OrderStatus.Pending },
            new() { Id = 2, Total = 200, Status = OrderStatus.Confirmed }
        };
        _orderServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(orders);

        var result = await _sut.GetAll();

        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var items = ok.Value.ShouldBeOfType<List<OrderListDto>>();
        items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyList_WhenNoOrders()
    {
        _orderServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<OrderListDto>());

        var result = await _sut.GetAll();

        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var items = ok.Value.ShouldBeOfType<List<OrderListDto>>();
        items.ShouldBeEmpty();
    }

    // ---- GET /api/admin/orders/{id} ----

    [Fact]
    public async Task GetById_ShouldReturnOrder_WhenExists()
    {
        var order = new OrderDto { Id = 1, UserId = 1, Total = 100 };
        _orderServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(order);

        var result = await _sut.GetById(1);

        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(order);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        _orderServiceMock.Setup(s => s.GetByIdAsync(999)).ThrowsAsync(new KeyNotFoundException());

        var result = await _sut.GetById(999);

        result.Result.ShouldBeOfType<NotFoundResult>();
    }

    // ---- PUT /api/admin/orders/{id}/status ----

    [Fact]
    public async Task UpdateStatus_ShouldReturnUpdatedOrder_WhenValid()
    {
        MockValid(_validatorMock);
        var dto = new UpdateOrderStatusDto { Status = OrderStatus.Confirmed };
        var updated = new OrderDto { Id = 1, Status = OrderStatus.Confirmed };
        _orderServiceMock.Setup(s => s.UpdateStatusAsync(1, OrderStatus.Confirmed)).ReturnsAsync(updated);

        var result = await _sut.UpdateStatus(1, dto);

        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var order = ok.Value.ShouldBeOfType<OrderDto>();
        order.Status.ShouldBe(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnBadRequest_WhenInvalidStatus()
    {
        MockInvalid(_validatorMock);
        var dto = new UpdateOrderStatusDto { Status = (OrderStatus)999 };

        var result = await _sut.UpdateStatus(1, dto);

        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        MockValid(_validatorMock);
        var dto = new UpdateOrderStatusDto { Status = OrderStatus.Confirmed };
        _orderServiceMock.Setup(s => s.UpdateStatusAsync(999, OrderStatus.Confirmed))
            .ThrowsAsync(new KeyNotFoundException());

        var result = await _sut.UpdateStatus(999, dto);

        result.Result.ShouldBeOfType<NotFoundResult>();
    }
}
