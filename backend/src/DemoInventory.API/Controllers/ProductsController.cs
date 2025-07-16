using DemoInventory.Application.DTOs;
using DemoInventory.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DemoInventory.API.Controllers;

/// <summary>
/// API controller for managing product operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for the entire controller, with explicit [AllowAnonymous] for public endpoints
[SwaggerTag("Products management operations")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get all products from the inventory
    /// </summary>
    /// <returns>A list of all products</returns>
    /// <response code="200">Returns the list of products</response>
    [HttpGet]
    [AllowAnonymous] // Public endpoint for reading product data
    [SwaggerOperation(Summary = "Get all products", Description = "Retrieves all products from the inventory")]
    [SwaggerResponse(200, "Success", typeof(IEnumerable<ProductDto>))]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    /// <param name="id">The ID of the product to retrieve</param>
    /// <returns>The product with the specified ID</returns>
    /// <response code="200">Returns the product</response>
    /// <response code="404">Product not found</response>
    /// <response code="400">Invalid product ID</response>
    [HttpGet("{id}")]
    [AllowAnonymous] // Public endpoint for reading product data
    [SwaggerOperation(Summary = "Get product by ID", Description = "Retrieves a specific product by its ID")]
    [SwaggerResponse(200, "Success", typeof(ProductDto))]
    [SwaggerResponse(404, "Product not found")]
    [SwaggerResponse(400, "Invalid product ID")]
    public async Task<ActionResult<ProductDto>> GetProduct([FromRoute] [Range(1, 2000000000, ErrorMessage = "Product ID must be a positive integer")] int id)
    {
        // Input validation
        if (id <= 0)
        {
            return BadRequest("Product ID must be a positive integer.");
        }

        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    /// <summary>
    /// Get a product by its SKU (Stock Keeping Unit)
    /// </summary>
    /// <param name="sku">The SKU of the product to retrieve</param>
    /// <returns>The product with the specified SKU</returns>
    /// <response code="200">Returns the product</response>
    /// <response code="404">Product not found</response>
    /// <response code="400">Invalid SKU format</response>
    [HttpGet("sku/{sku}")]
    [AllowAnonymous] // Public endpoint for reading product data
    [SwaggerOperation(Summary = "Get product by SKU", Description = "Retrieves a specific product by its SKU")]
    [SwaggerResponse(200, "Success", typeof(ProductDto))]
    [SwaggerResponse(404, "Product not found")]
    [SwaggerResponse(400, "Invalid SKU format")]
    public async Task<ActionResult<ProductDto>> GetProductBySku([FromRoute] [Required] [StringLength(50, MinimumLength = 1, ErrorMessage = "SKU must be between 1 and 50 characters")] string sku)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(sku))
        {
            return BadRequest("SKU cannot be null or empty.");
        }

        if (sku.Length > 50)
        {
            return BadRequest("SKU cannot exceed 50 characters.");
        }

        var product = await _productService.GetProductBySkuAsync(sku);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    /// <summary>
    /// Get a product by its name
    /// </summary>
    /// <param name="name">The name of the product to retrieve</param>
    /// <returns>The product with the specified name</returns>
    /// <response code="200">Returns the product</response>
    /// <response code="404">Product not found</response>
    /// <response code="400">Invalid name format</response>
    [HttpGet("name/{name}")]
    [AllowAnonymous] // Public endpoint for reading product data
    [SwaggerOperation(Summary = "Get product by name", Description = "Retrieves a specific product by its exact name")]
    [SwaggerResponse(200, "Success", typeof(ProductDto))]
    [SwaggerResponse(404, "Product not found")]
    [SwaggerResponse(400, "Invalid name format")]
    public async Task<ActionResult<ProductDto>> GetProductByName([FromRoute] [Required] [StringLength(200, MinimumLength = 1, ErrorMessage = "Product name must be between 1 and 200 characters")] string name)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Product name cannot be null or empty.");
        }

        if (name.Length > 200)
        {
            return BadRequest("Product name cannot exceed 200 characters.");
        }

        var product = await _productService.GetProductByNameAsync(name);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    /// <summary>
    /// Search for products by name
    /// </summary>
    /// <param name="searchTerm">
    /// (Optional) The search term to filter products by name. If null or empty, all products are returned.
    /// </param>
    /// <returns>A list of products matching the search term</returns>
    /// <response code="200">Returns the list of matching products</response>
    /// <response code="400">Invalid search parameters</response>
    [HttpGet("search")]
    [AllowAnonymous] // Public endpoint for reading product data
    [SwaggerOperation(Summary = "Search products", Description = "Search for products by name using a search term")]
    [SwaggerResponse(200, "Success", typeof(IEnumerable<ProductDto>))]
    [SwaggerResponse(400, "Invalid search parameters")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts([FromQuery] [StringLength(200, ErrorMessage = "Search term cannot exceed 200 characters")] string? searchTerm)
    {
        // Input validation
        if (!string.IsNullOrEmpty(searchTerm) && searchTerm.Length > 200)
        {
            return BadRequest("Search term cannot exceed 200 characters.");
        }

        var products = await _productService.SearchProductsAsync(searchTerm ?? string.Empty);
        return Ok(products);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="createProductDto">The product information to create</param>
    /// <returns>The created product</returns>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid product data</response>
    /// <response code="401">Unauthorized - API key required</response>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken] // Add CSRF protection
    [SwaggerOperation(Summary = "Create product", Description = "Creates a new product in the inventory")]
    [SwaggerResponse(201, "Product created successfully", typeof(ProductDto))]
    [SwaggerResponse(400, "Invalid product data")]
    [SwaggerResponse(401, "Unauthorized - API key required")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        // Model validation
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var product = await _productService.CreateProductAsync(createProductDto);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">The ID of the product to update</param>
    /// <param name="updateProductDto">The updated product information</param>
    /// <returns>The updated product</returns>
    /// <response code="200">Product updated successfully</response>
    /// <response code="404">Product not found</response>
    /// <response code="400">Invalid product data</response>
    /// <response code="401">Unauthorized - API key required</response>
    [HttpPut("{id}")]
    [Authorize]
    [ValidateAntiForgeryToken] // Add CSRF protection
    [SwaggerOperation(Summary = "Update product", Description = "Updates an existing product in the inventory")]
    [SwaggerResponse(200, "Product updated successfully", typeof(ProductDto))]
    [SwaggerResponse(404, "Product not found")]
    [SwaggerResponse(400, "Invalid product data")]
    [SwaggerResponse(401, "Unauthorized - API key required")]
    public async Task<ActionResult<ProductDto>> UpdateProduct([FromRoute] [Range(1, 2000000000, ErrorMessage = "Product ID must be a positive integer")] int id, [FromBody] UpdateProductDto updateProductDto)
    {
        // Input validation
        if (id <= 0)
        {
            return BadRequest("Product ID must be a positive integer.");
        }

        // Model validation
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var product = await _productService.UpdateProductAsync(id, updateProductDto);
            return Ok(product);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="id">The ID of the product to delete</param>
    /// <returns>No content</returns>
    /// <response code="204">Product deleted successfully</response>
    /// <response code="404">Product not found</response>
    /// <response code="400">Invalid product ID</response>
    /// <response code="401">Unauthorized - API key required</response>
    [HttpDelete("{id}")]
    [Authorize]
    [ValidateAntiForgeryToken] // Add CSRF protection
    [SwaggerOperation(Summary = "Delete product", Description = "Deletes a product from the inventory")]
    [SwaggerResponse(204, "Product deleted successfully")]
    [SwaggerResponse(404, "Product not found")]
    [SwaggerResponse(400, "Invalid product ID")]
    [SwaggerResponse(401, "Unauthorized - API key required")]
    public async Task<IActionResult> DeleteProduct([FromRoute] [Range(1, 2000000000, ErrorMessage = "Product ID must be a positive integer")] int id)
    {
        // Input validation
        if (id <= 0)
        {
            return BadRequest("Product ID must be a positive integer.");
        }

        await _productService.DeleteProductAsync(id);
        return NoContent();
    }
}