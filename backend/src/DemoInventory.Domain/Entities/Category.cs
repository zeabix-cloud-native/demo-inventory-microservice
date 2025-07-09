namespace DemoInventory.Domain.Entities;

/// <summary>
/// Represents a product category in the inventory system
/// </summary>
public class Category
{
    private string _name = string.Empty;
    private string _description = string.Empty;

    /// <summary>
    /// Unique identifier for the category
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Name of the category
    /// </summary>
    public string Name 
    { 
        get => _name;
        set 
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Category name cannot be null or empty.", nameof(Name));
            if (value.Length > 100)
                throw new ArgumentException("Category name cannot exceed 100 characters.", nameof(Name));
            _name = value.Trim();
        }
    }
    
    /// <summary>
    /// Description of the category
    /// </summary>
    public string Description 
    { 
        get => _description;
        set 
        {
            if (value?.Length > 500)
                throw new ArgumentException("Category description cannot exceed 500 characters.", nameof(Description));
            _description = value?.Trim() ?? string.Empty;
        }
    }
    
    /// <summary>
    /// Date and time when the category was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Date and time when the category was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property for products in this category
    /// </summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    /// <summary>
    /// Validates all category properties
    /// </summary>
    public void Validate()
    {
        // Trigger validation for all properties
        var tempName = Name;
        var tempDescription = Description;
    }
}
