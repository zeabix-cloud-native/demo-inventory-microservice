namespace DemoInventory.Domain.Entities;

/// <summary>
/// Represents a product in the inventory system
/// </summary>
public class Product
{
    private string _name = string.Empty;
    private string _sku = string.Empty;
    private decimal _price;
    private int _quantityInStock;

    public int Id { get; set; }
    
    public string Name 
    { 
        get => _name;
        set 
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Product name cannot be null or empty.", nameof(Name));
            if (value.Length > 200)
                throw new ArgumentException("Product name cannot exceed 200 characters.", nameof(Name));
            _name = value.Trim();
        }
    }
    
    public string Description { get; set; } = string.Empty;
    
    public string SKU 
    { 
        get => _sku;
        set 
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("SKU cannot be null or empty.", nameof(SKU));
            if (value.Length > 50)
                throw new ArgumentException("SKU cannot exceed 50 characters.", nameof(SKU));
            
            var normalizedSku = value.Trim().ToUpperInvariant();
            // SKU should contain only alphanumeric characters and hyphens
            if (!System.Text.RegularExpressions.Regex.IsMatch(normalizedSku, @"^[A-Z0-9\-]+$"))
                throw new ArgumentException("SKU must contain only uppercase letters, numbers, and hyphens.", nameof(SKU));
            _sku = normalizedSku;
        }
    }
    
    public decimal Price 
    { 
        get => _price;
        set 
        {
            if (value < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(Price));
            if (value > 999999.99m)
                throw new ArgumentException("Price cannot exceed 999,999.99.", nameof(Price));
            _price = value;
        }
    }
    
    public int QuantityInStock 
    { 
        get => _quantityInStock;
        set 
        {
            if (value < 0)
                throw new ArgumentException("Quantity in stock cannot be negative.", nameof(QuantityInStock));
            _quantityInStock = value;
        }
    }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Validates the product's description length
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when description exceeds maximum length</exception>
    public void ValidateDescription()
    {
        if (Description.Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters.", nameof(Description));
    }

    /// <summary>
    /// Validates all product properties by triggering their setters
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when any property validation fails</exception>
    public void Validate()
    {
        // Trigger validation for all properties
        var tempName = Name;
        var tempSku = SKU;
        var tempPrice = Price;
        var tempQuantity = QuantityInStock;
        ValidateDescription();
    }
}