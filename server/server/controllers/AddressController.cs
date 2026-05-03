using System.Security.Claims;
using application.interfaces;
using domain.dtos.Address;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace server.controllers;

[ApiController]
[Route("api/users/me/addresses")]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AddressDto>>> GetAll()
    {
        var userId = GetUserId();
        var addresses = await _addressService.GetByUserIdAsync(userId);
        return Ok(addresses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AddressDto>> GetById(int id)
    {
        try
        {
            var userId = GetUserId();
            var address = await _addressService.GetByIdAsync(userId, id);
            return Ok(address);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<AddressDto>> Create(CreateAddressDto dto)
    {
        try
        {
            var userId = GetUserId();
            var address = await _addressService.AddAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = address.Id }, address);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateAddressDto dto)
    {
        try
        {
            var userId = GetUserId();
            await _addressService.UpdateAsync(userId, id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var userId = GetUserId();
            await _addressService.DeleteAsync(userId, id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id}/default")]
    public async Task<ActionResult<AddressDto>> SetDefault(int id)
    {
        try
        {
            var userId = GetUserId();
            var address = await _addressService.SetDefaultAsync(userId, id);
            return Ok(address);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue("UserId");
        if (claim == null)
            throw new UnauthorizedAccessException("Usuario no autenticado");
        return int.Parse(claim);
    }
}
