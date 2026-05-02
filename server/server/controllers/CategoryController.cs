using application.interfaces;
using domain.dtos.Category;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace server.controllers;

[ApiController]
[Route("api/category")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IValidator<CreateCategoryDto> _createValidator;
    private readonly IValidator<UpdateCategoryDto> _updateValidator;

    public CategoryController(
        ICategoryService categoryService,
        IValidator<CreateCategoryDto> createValidator,
        IValidator<UpdateCategoryDto> updateValidator)
    {
        _categoryService = categoryService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
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
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var category = await _categoryService.AddAsync(dto);
        return CreatedAtAction(nameof(GetBySlug), new { slug = category.Slug }, category);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateCategoryDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

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
