using application.interfaces;
using application.dtos.Order;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace server.controllers.Admin;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IValidator<UpdateOrderStatusDto> _updateStatusValidator;

    public OrderController(
        IOrderService orderService,
        IValidator<UpdateOrderStatusDto> updateStatusValidator)
    {
        _orderService = orderService;
        _updateStatusValidator = updateStatusValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderListDto>>> GetAll()
    {
        var orders = await _orderService.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        try
        {
            var order = await _orderService.GetByIdAsync(id);
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(int id, UpdateOrderStatusDto dto)
    {
        var validation = await _updateStatusValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        try
        {
            var order = await _orderService.UpdateStatusAsync(id, dto.Status);
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
