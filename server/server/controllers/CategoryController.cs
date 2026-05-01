using application.interfaces;
using domain.dtos.Category;
using Microsoft.AspNetCore.Mvc;

namespace Huellario.Server.controllers;

[ApiController]
[Route("api/category")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<CategoryDto>> GetBySlug(string slug)
    {
        try
        {
            var category = await _categoryService.GetBySlugAsync(slug);
            return Ok(category);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto)
    {
        var category = await _categoryService.AddAsync(dto);
        return CreatedAtAction(nameof(GetBySlug), new { slug = category.Slug }, category);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateCategoryDto dto)
    {
        try
        {
            await _categoryService.UpdateAsync(id, dto);
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
        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}
