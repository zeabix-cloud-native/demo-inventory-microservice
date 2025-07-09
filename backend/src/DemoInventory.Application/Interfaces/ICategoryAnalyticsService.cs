using DemoInventory.Application.DTOs;

namespace DemoInventory.Application.Interfaces;

/// <summary>
/// Service interface for category analytics operations
/// </summary>
public interface ICategoryAnalyticsService
{
    /// <summary>
    /// Gets comprehensive analytics for all categories
    /// </summary>
    /// <param name="dateRange">Optional date range filter</param>
    /// <returns>Complete category metrics and analytics data</returns>
    Task<CategoryMetricsDto> GetCategoryMetricsAsync(AnalyticsDateRangeDto? dateRange = null);
    
    /// <summary>
    /// Gets analytics for a specific category
    /// </summary>
    /// <param name="categoryId">The category ID to analyze</param>
    /// <param name="dateRange">Optional date range filter</param>
    /// <returns>Analytics data for the specified category</returns>
    Task<CategoryAnalyticsDto?> GetCategoryAnalyticsByIdAsync(int categoryId, AnalyticsDateRangeDto? dateRange = null);
    
    /// <summary>
    /// Gets top performing categories by various metrics
    /// </summary>
    /// <param name="count">Number of top categories to return</param>
    /// <param name="sortBy">Metric to sort by (ProductCount, InventoryValue, AveragePrice)</param>
    /// <returns>Top categories by the specified metric</returns>
    Task<IEnumerable<CategoryAnalyticsDto>> GetTopCategoriesAsync(int count = 10, string sortBy = "ProductCount");
    
    /// <summary>
    /// Gets trending categories based on recent activity
    /// </summary>
    /// <param name="daysPeriod">Number of days to analyze for trends</param>
    /// <param name="count">Number of trending categories to return</param>
    /// <returns>Trending category data</returns>
    Task<IEnumerable<CategoryTrendDto>> GetCategoryTrendsAsync(int daysPeriod = 30, int count = 10);
    
    /// <summary>
    /// Gets inventory distribution across categories
    /// </summary>
    /// <returns>Category inventory distribution data</returns>
    Task<IEnumerable<CategoryInventoryDistributionDto>> GetCategoryInventoryDistributionAsync();
    
    /// <summary>
    /// Gets categories with low stock or out of stock products
    /// </summary>
    /// <param name="lowStockThreshold">Threshold for considering stock as low</param>
    /// <returns>Categories with stock issues</returns>
    Task<IEnumerable<CategoryAnalyticsDto>> GetCategoriesWithStockIssuesAsync(int lowStockThreshold = 10);
    
    /// <summary>
    /// Gets analytics summary for dashboard display
    /// </summary>
    /// <returns>Key metrics for dashboard</returns>
    Task<object> GetAnalyticsSummaryAsync();
}
