using System.ComponentModel.DataAnnotations;

namespace DemoInventory.Application.DTOs;

/// <summary>
/// DTO for category analytics data
/// </summary>
public class CategoryAnalyticsDto
{
    /// <summary>
    /// Category identifier
    /// </summary>
    public int CategoryId { get; set; }
    
    /// <summary>
    /// Category name
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;
    
    /// <summary>
    /// Category description
    /// </summary>
    public string CategoryDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// Total number of products in this category
    /// </summary>
    public int TotalProducts { get; set; }
    
    /// <summary>
    /// Total quantity of all products in stock for this category
    /// </summary>
    public int TotalStockQuantity { get; set; }
    
    /// <summary>
    /// Average price of products in this category
    /// </summary>
    public decimal AveragePrice { get; set; }
    
    /// <summary>
    /// Minimum price among products in this category
    /// </summary>
    public decimal MinPrice { get; set; }
    
    /// <summary>
    /// Maximum price among products in this category
    /// </summary>
    public decimal MaxPrice { get; set; }
    
    /// <summary>
    /// Total value of inventory for this category (price * quantity)
    /// </summary>
    public decimal TotalInventoryValue { get; set; }
    
    /// <summary>
    /// Percentage of total inventory value this category represents
    /// </summary>
    public decimal InventoryValuePercentage { get; set; }
    
    /// <summary>
    /// Number of products with low stock (less than 10 units)
    /// </summary>
    public int LowStockProducts { get; set; }
    
    /// <summary>
    /// Number of products that are out of stock
    /// </summary>
    public int OutOfStockProducts { get; set; }
}

/// <summary>
/// DTO for category metrics summary
/// </summary>
public class CategoryMetricsDto
{
    /// <summary>
    /// Total number of categories
    /// </summary>
    public int TotalCategories { get; set; }
    
    /// <summary>
    /// Total number of products across all categories
    /// </summary>
    public int TotalProducts { get; set; }
    
    /// <summary>
    /// Total inventory value across all categories
    /// </summary>
    public decimal TotalInventoryValue { get; set; }
    
    /// <summary>
    /// Average number of products per category
    /// </summary>
    public decimal AverageProductsPerCategory { get; set; }
    
    /// <summary>
    /// Number of categories with products
    /// </summary>
    public int CategoriesWithProducts { get; set; }
    
    /// <summary>
    /// Number of empty categories (no products)
    /// </summary>
    public int EmptyCategories { get; set; }
    
    /// <summary>
    /// List of category analytics data
    /// </summary>
    public IEnumerable<CategoryAnalyticsDto> CategoryAnalytics { get; set; } = new List<CategoryAnalyticsDto>();
}

/// <summary>
/// DTO for category trend data
/// </summary>
public class CategoryTrendDto
{
    /// <summary>
    /// Category identifier
    /// </summary>
    public int CategoryId { get; set; }
    
    /// <summary>
    /// Category name
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;
    
    /// <summary>
    /// Trend score based on inventory activity (higher = more active)
    /// </summary>
    public decimal TrendScore { get; set; }
    
    /// <summary>
    /// Rank of this category by trend score (1 = highest trending)
    /// </summary>
    public int TrendRank { get; set; }
    
    /// <summary>
    /// Growth rate of products in this category
    /// </summary>
    public decimal ProductGrowthRate { get; set; }
    
    /// <summary>
    /// Average inventory turnover for this category
    /// </summary>
    public decimal InventoryTurnover { get; set; }
}

/// <summary>
/// DTO for category inventory distribution
/// </summary>
public class CategoryInventoryDistributionDto
{
    /// <summary>
    /// Category identifier
    /// </summary>
    public int CategoryId { get; set; }
    
    /// <summary>
    /// Category name
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;
    
    /// <summary>
    /// Percentage of total products this category represents
    /// </summary>
    public decimal ProductPercentage { get; set; }
    
    /// <summary>
    /// Percentage of total inventory value this category represents
    /// </summary>
    public decimal ValuePercentage { get; set; }
    
    /// <summary>
    /// Percentage of total stock quantity this category represents
    /// </summary>
    public decimal StockPercentage { get; set; }
}

/// <summary>
/// DTO for analytics date range filter
/// </summary>
public class AnalyticsDateRangeDto
{
    /// <summary>
    /// Start date for analytics (optional)
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// End date for analytics (optional)
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Include only categories with products
    /// </summary>
    public bool IncludeOnlyActiveCategories { get; set; } = true;
    
    /// <summary>
    /// Minimum stock threshold for low stock analysis
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Minimum stock threshold must be non-negative")]
    public int LowStockThreshold { get; set; } = 10;
}
