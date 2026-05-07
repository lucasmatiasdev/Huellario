using System.Security.Claims;
using application.interfaces;
using domain.dtos.Order;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace server.controllers;

[ApiController]
[Route("api/order")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderDto> _createValidator;

    public OrderController(
        IOrderService orderService,
        IValidator<CreateOrderDto> createValidator)
    {
        _orderService = orderService;
        _createValidator = createValidator;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(
        CreateOrderDto dto,
        [FromQuery] string? sessionId)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var userId = GetUserId();
        var sid = ResolveSessionId(userId, sessionId);

        try
        {
            var order = await _orderService.CreateOrderAsync(userId, sid, dto);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderListDto>>> GetAll()
    {
        var userId = GetUserIdRequired();
        var orders = await _orderService.GetByUserAsync(userId);
        return Ok(orders);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var userId = GetUserIdRequired();

        try
        {
            var order = await _orderService.GetByIdAsync(id);

            if (order.UserId != userId)
                return NotFound();

            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize]
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult> Cancel(int id)
    {
        var userId = GetUserIdRequired();

        try
        {
            await _orderService.CancelAsync(id, userId);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue("UserId");
        return claim != null ? int.Parse(claim) : 0;
    }

    private int GetUserIdRequired()
    {
        var claim = User.FindFirstValue("UserId");
        if (claim == null)
            throw new UnauthorizedAccessException("Usuario no autenticado");
        return int.Parse(claim);
    }

    private static string ResolveSessionId(int userId, string? sessionId)
    {
        return userId > 0 ? string.Empty : (sessionId ?? string.Empty);
    }
}
