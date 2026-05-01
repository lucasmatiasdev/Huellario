using application.interfaces;
using domain.dtos.Brand;
using Microsoft.AspNetCore.Mvc;

namespace Huellario.Server.controllers;

[ApiController]
[Route("api/brand")]
public class BrandController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandController(IBrandService brandService)
    {
        _brandService = brandService;
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
        var brand = await _brandService.AddAsync(dto);
        return CreatedAtAction(nameof(GetBySlug), new { slug = brand.Slug }, brand);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateBrandDto dto)
    {
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
