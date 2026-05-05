using System.Security.Claims;
using application.interfaces;
using domain.dtos.CartItem;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace server.controllers;

[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly IValidator<CreateCartItemDto> _createValidator;
    private readonly IValidator<UpdateCartItemDto> _updateValidator;

    public CartController(
        ICartService cartService,
        IValidator<CreateCartItemDto> createValidator,
        IValidator<UpdateCartItemDto> updateValidator)
    {
        _cartService = cartService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart([FromQuery] string? sessionId)
    {
        var userId = GetUserId();
        var sid = ResolveSessionId(userId, sessionId);
        var cart = await _cartService.GetCartAsync(userId, sid);
        return Ok(cart);
    }

    [AllowAnonymous]
    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem(CreateCartItemDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var userId = GetUserId();
        var cart = await _cartService.AddItemAsync(userId, dto);
        return Ok(cart);
    }

    [AllowAnonymous]
    [HttpPut("items")]
    public async Task<ActionResult<CartDto>> UpdateQuantity(UpdateCartItemDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var userId = GetUserId();
        var cart = await _cartService.UpdateQuantityAsync(userId, dto);
        return Ok(cart);
    }

    [AllowAnonymous]
    [HttpDelete("items/{productId}/{variantId}")]
    public async Task<ActionResult<CartDto>> RemoveItem(
        int productId, int variantId, [FromQuery] string? sessionId)
    {
        var userId = GetUserId();
        var sid = ResolveSessionId(userId, sessionId);
        var cart = await _cartService.RemoveItemAsync(userId, sid, productId, variantId);
        return Ok(cart);
    }

    [AllowAnonymous]
    [HttpDelete]
    public async Task<ActionResult<CartDto>> Clear([FromQuery] string? sessionId)
    {
        var userId = GetUserId();
        var sid = ResolveSessionId(userId, sessionId);
        var cart = await _cartService.ClearCartAsync(userId, sid);
        return Ok(cart);
    }

    [Authorize]
    [HttpPost("transfer")]
    public async Task<ActionResult<CartDto>> TransferCart(TransferCartDto dto)
    {
        var userId = GetUserId();
        var cart = await _cartService.TransferCartAsync(userId, dto.SessionId);
        return Ok(cart);
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue("UserId");
        return claim != null ? int.Parse(claim) : 0;
    }

    private static string ResolveSessionId(int userId, string? sessionId)
    {
        return userId > 0 ? string.Empty : (sessionId ?? string.Empty);
    }
}
