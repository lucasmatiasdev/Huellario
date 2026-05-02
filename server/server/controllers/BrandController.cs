using application.interfaces;
using domain.dtos.Brand;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace server.controllers;

[ApiController]
[Route("api/brand")]
public class BrandController : ControllerBase
{
    private readonly IBrandService _brandService;
    private readonly IValidator<CreateBrandDto> _createValidator;
    private readonly IValidator<UpdateBrandDto> _updateValidator;

    public BrandController(
        IBrandService brandService,
        IValidator<CreateBrandDto> createValidator,
        IValidator<UpdateBrandDto> updateValidator)
    {
        _brandService = brandService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BrandDto>>> GetAll()
    {
        var brands = await _brandService.GetAllAsync();
        return Ok(brands);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<BrandDto>> GetBySlug(string slug)
    {
        try
        {
            var brand = await _brandService.GetBySlugAsync(slug);
            return Ok(brand);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<BrandDto>> Create(CreateBrandDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var brand = await _brandService.AddAsync(dto);
        return CreatedAtAction(nameof(GetBySlug), new { slug = brand.Slug }, brand);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateBrandDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        try
        {
            await _brandService.UpdateAsync(id, dto);
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
        await _brandService.DeleteAsync(id);
        return NoContent();
    }
}
