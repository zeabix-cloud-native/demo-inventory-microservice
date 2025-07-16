using System.ComponentModel.DataAnnotations;

namespace DemoInventory.Application.DTOs;

/// <summary>
/// Product data transfer object representing a complete product
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Unique identifier for the product
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Name of the product
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the product
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Stock Keeping Unit - unique product identifier
    /// </summary>
    public string SKU { get; set; } = string.Empty;
    
    /// <summary>
    /// Price of the product
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Current quantity available in stock
    /// </summary>
    public int QuantityInStock { get; set; }
    
    /// <summary>
    /// Date and time when the product was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Date and time when the product was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Data transfer object for creating a new product
/// </summary>
public class CreateProductDto
{
    /// <summary>
    /// Name of the product (required, 1-200 characters)
    /// </summary>
    /// <example>Sample Product</example>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the product (up to 1000 characters)
    /// </summary>
    /// <example>This is a sample product description</example>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Stock Keeping Unit - unique product identifier (required, 3-50 characters)
    /// </summary>
    /// <example>SKU-001</example>
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string SKU { get; set; } = string.Empty;
    
    /// <summary>
    /// Price of the product (must be greater than 0)
    /// </summary>
    /// <example>19.99</example>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
    
    /// <summary>
    /// Initial quantity to stock (must be non-negative)
    /// </summary>
    /// <example>100</example>
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
    public int QuantityInStock { get; set; }
}

/// <summary>
/// Data transfer object for updating an existing product
/// </summary>
public class UpdateProductDto
{
    /// <summary>
    /// Updated name of the product (required, 1-200 characters)
    /// </summary>
    /// <example>Updated Product Name</example>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Updated description of the product (up to 1000 characters)
    /// </summary>
    /// <example>This is an updated product description</example>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Updated price of the product (must be greater than 0)
    /// </summary>
    /// <example>24.99</example>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
    
    /// <summary>
    /// Updated quantity in stock (must be non-negative)
    /// </summary>
    /// <example>150</example>
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
    public int QuantityInStock { get; set; }
}

/// <summary>
/// Data transfer object for price range queries
/// </summary>
public class PriceRangeDto
{
    /// <summary>
    /// The minimum price (inclusive)
    /// </summary>
    public decimal MinPrice { get; set; }
    
    /// <summary>
    /// The maximum price (inclusive)
    /// </summary>
    public decimal MaxPrice { get; set; }
}