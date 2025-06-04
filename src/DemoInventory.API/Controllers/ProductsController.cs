using DemoInventory.Application.DTOs;
using DemoInventory.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DemoInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get product by ID", Description = "Retrieves a specific product by its ID")]
    [SwaggerResponse(200, "Success", typeof(ProductDto))]
    [SwaggerResponse(404, "Product not found")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
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
    [HttpGet("sku/{sku}")]
    [SwaggerOperation(Summary = "Get product by SKU", Description = "Retrieves a specific product by its SKU")]
    [SwaggerResponse(200, "Success", typeof(ProductDto))]
    [SwaggerResponse(404, "Product not found")]
    public async Task<ActionResult<ProductDto>> GetProductBySku(string sku)
    {
        var product = await _productService.GetProductBySkuAsync(sku);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    /// <summary>
    /// Search for products by name
    /// </summary>
    /// <param name="searchTerm">The search term to filter products by name</param>
    /// <returns>A list of products matching the search term</returns>
    /// <response code="200">Returns the list of matching products</response>
    [HttpGet("search")]
    [SwaggerOperation(Summary = "Search products", Description = "Search for products by name using a search term")]
    [SwaggerResponse(200, "Success", typeof(IEnumerable<ProductDto>))]
    public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts([FromQuery] string searchTerm)
    {
        var products = await _productService.SearchProductsAsync(searchTerm);
        return Ok(products);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="createProductDto">The product information to create</param>
    /// <returns>The created product</returns>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid product data</response>
    [HttpPost]
    [SwaggerOperation(Summary = "Create product", Description = "Creates a new product in the inventory")]
    [SwaggerResponse(201, "Product created successfully", typeof(ProductDto))]
    [SwaggerResponse(400, "Invalid product data")]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
    {
        var product = await _productService.CreateProductAsync(createProductDto);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
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
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update product", Description = "Updates an existing product in the inventory")]
    [SwaggerResponse(200, "Product updated successfully", typeof(ProductDto))]
    [SwaggerResponse(404, "Product not found")]
    [SwaggerResponse(400, "Invalid product data")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, UpdateProductDto updateProductDto)
    {
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
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete product", Description = "Deletes a product from the inventory")]
    [SwaggerResponse(204, "Product deleted successfully")]
    [SwaggerResponse(404, "Product not found")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _productService.DeleteProductAsync(id);
        return NoContent();
    }
}