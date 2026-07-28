using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RedisCommerce.API.Extensions;
using RedisCommerce.API.Middleware;
using RedisCommerce.Application.DTOs;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private const string VisitorHeaderName = "X-Visitor-Id";

    private readonly IProductService _productService;
    private readonly IProductPopularityService _popularityService;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;

    public ProductsController(
        IProductService productService,
        IProductPopularityService popularityService,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator)
    {
        _productService = productService;
        _popularityService = popularityService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("popular")]
    [ProducesResponseType(typeof(IEnumerable<PopularProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PopularProductResponse>>> GetPopular()
    {
        var popular = await _popularityService.GetTopProductsAsync(20);
        return Ok(popular);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> Search([FromQuery] string query)
    {
        var results = await _productService.SearchAsync(query, HttpContext.GetSessionUserId());
        return Ok(results);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var visitorId = Request.Headers.TryGetValue(VisitorHeaderName, out var header) ? header.ToString() : null;
        var product = await _productService.GetByIdAsync(id, HttpContext.GetSessionUserId(), visitorId);
        return Ok(product);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
    {
        if (!this.ApplyValidationErrors(await _createValidator.ValidateAsync(request)))
        {
            return ValidationProblem(ModelState);
        }

        var created = await _productService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request)
    {
        if (!this.ApplyValidationErrors(await _updateValidator.ValidateAsync(request)))
        {
            return ValidationProblem(ModelState);
        }

        var updated = await _productService.UpdateAsync(id, request);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteAsync(id);
        return NoContent();
    }
}
