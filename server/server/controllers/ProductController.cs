using application.interfaces;
using application.dtos;
using application.dtos.Product;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace server.controllers;

[ApiController]
[Route("api/product")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<UpdateProductDto> _updateValidator;
    private readonly IValidator<CreateVariantDto> _createVariantValidator;
    private readonly IValidator<UpdateStockDto> _stockValidator;

    public ProductController(
        IProductService productService,
        IValidator<CreateProductDto> createValidator,
        IValidator<UpdateProductDto> updateValidator,
        IValidator<CreateVariantDto> createVariantValidator,
        IValidator<UpdateStockDto> stockValidator)
    {
        _productService = productService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _createVariantValidator = createVariantValidator;
        _stockValidator = stockValidator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? brandId = null,
        [FromQuery] string? search = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null)
    {
        var paged = await _productService.GetPagedAsync(
            page > 0 ? page : 1,
            pageSize > 0 ? pageSize : 10,
            categoryId, brandId, search, minPrice, maxPrice);
        return Ok(paged);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ProductDto>> GetBySlug(string slug)
    {
        try
        {
            var product = await _productService.GetBySlugAsync(slug);
            return Ok(product);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ProductDto>> Create(CreateProductDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var product = await _productService.AddAsync(dto);
        return CreatedAtAction(nameof(GetBySlug), new { slug = product.Slug }, product);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> Update(int id, UpdateProductDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        try
        {
            await _productService.UpdateAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await _productService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        
    }

    [HttpPost("{id}/images")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ProductImageDto>> AddImage(int id, ProductImageDto dto)
    {
        try
        {
            var image = await _productService.AddImageAsync(id, dto);
            var product = await _productService.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetBySlug), new { slug = product.Slug }, image);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}/images/{imageId}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> DeleteImage(int id, int imageId)
    {
        try
        {
            await _productService.DeleteImageAsync(id, imageId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id}/images/{imageId}/main")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> SetMainImage(int id, int imageId)
    {
        try
        {
            await _productService.SetMainImageAsync(id, imageId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/variants")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<VariantDto>> AddVariant(int id, CreateVariantDto dto)
    {
        var validation = await _createVariantValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        try
        {
            var variant = await _productService.AddVariantAsync(id, dto);
            var product = await _productService.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetBySlug), new { slug = product.Slug }, variant);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("{id}/variants/{variantId}")]
    public async Task<ActionResult> UpdateVariant(int id, int variantId, CreateVariantDto dto)
    {
        var validation = await _createVariantValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        try
        {
            await _productService.UpdateVariantAsync(id, variantId, dto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id}/variants/{variantId}/stock")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> UpdateStock(int id, int variantId, UpdateStockDto dto)
    {
        var validation = await _stockValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        try
        {
            await _productService.UpdateStockAsync(id, variantId, dto.Stock);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}/variants/{variantId}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> DeleteVariant(int id, int variantId)
    {
        try
        {
            await _productService.DeleteVariantAsync(id, variantId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
