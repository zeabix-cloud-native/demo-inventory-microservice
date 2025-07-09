using DemoInventory.Domain.Entities;

namespace DemoInventory.Domain.Interfaces;

/// <summary>
/// Repository interface for category analytics operations
/// </summary>
public interface ICategoryAnalyticsRepository
{
    /// <summary>
    /// Gets analytics data for all categories
    /// </summary>
    /// <param name="includeEmptyCategories">Whether to include categories with no products</param>
    /// <param name="startDate">Optional start date filter for product creation</param>
    /// <param name="endDate">Optional end date filter for product creation</param>
    /// <returns>Collection of category analytics data</returns>
    Task<IEnumerable<CategoryAnalyticsData>> GetCategoryAnalyticsAsync(
        bool includeEmptyCategories = true, 
        DateTime? startDate = null, 
        DateTime? endDate = null);
    
    /// <summary>
    /// Gets analytics data for a specific category
    /// </summary>
    /// <param name="categoryId">The category ID to analyze</param>
    /// <param name="startDate">Optional start date filter for product creation</param>
    /// <param name="endDate">Optional end date filter for product creation</param>
    /// <returns>Analytics data for the specified category</returns>
    Task<CategoryAnalyticsData?> GetCategoryAnalyticsByIdAsync(
        int categoryId, 
        DateTime? startDate = null, 
        DateTime? endDate = null);
    
    /// <summary>
    /// Gets top N categories by product count
    /// </summary>
    /// <param name="count">Number of top categories to return</param>
    /// <returns>Top categories by product count</returns>
    Task<IEnumerable<CategoryAnalyticsData>> GetTopCategoriesByProductCountAsync(int count = 10);
    
    /// <summary>
    /// Gets top N categories by inventory value
    /// </summary>
    /// <param name="count">Number of top categories to return</param>
    /// <returns>Top categories by inventory value</returns>
    Task<IEnumerable<CategoryAnalyticsData>> GetTopCategoriesByInventoryValueAsync(int count = 10);
    
    /// <summary>
    /// Gets categories with low stock products
    /// </summary>
    /// <param name="lowStockThreshold">Threshold for considering stock as low</param>
    /// <returns>Categories with products below the stock threshold</returns>
    Task<IEnumerable<CategoryAnalyticsData>> GetCategoriesWithLowStockAsync(int lowStockThreshold = 10);
    
    /// <summary>
    /// Gets overall metrics across all categories
    /// </summary>
    /// <returns>Overall category metrics</returns>
    Task<OverallCategoryMetrics> GetOverallCategoryMetricsAsync();
    
    /// <summary>
    /// Gets category trend data based on recent activity
    /// </summary>
    /// <param name="daysPeriod">Number of days to analyze for trends</param>
    /// <returns>Category trend information</returns>
    Task<IEnumerable<CategoryTrendData>> GetCategoryTrendsAsync(int daysPeriod = 30);
}

/// <summary>
/// Data model for category analytics
/// </summary>
public class CategoryAnalyticsData
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryDescription { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
    public int TotalStockQuantity { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public DateTime? LastProductAdded { get; set; }
    public DateTime CategoryCreatedAt { get; set; }
    public DateTime CategoryUpdatedAt { get; set; }
}

/// <summary>
/// Data model for overall category metrics
/// </summary>
public class OverallCategoryMetrics
{
    public int TotalCategories { get; set; }
    public int TotalProducts { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int CategoriesWithProducts { get; set; }
    public int EmptyCategories { get; set; }
    public decimal AverageProductsPerCategory { get; set; }
}

/// <summary>
/// Data model for category trend analysis
/// </summary>
public class CategoryTrendData
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TrendScore { get; set; }
    public int TrendRank { get; set; }
    public decimal ProductGrowthRate { get; set; }
    public decimal InventoryTurnover { get; set; }
    public int RecentProductsAdded { get; set; }
    public DateTime AnalysisPeriodStart { get; set; }
    public DateTime AnalysisPeriodEnd { get; set; }
}
