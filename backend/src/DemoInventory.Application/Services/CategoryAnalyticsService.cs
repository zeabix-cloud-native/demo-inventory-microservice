using DemoInventory.Application.DTOs;
using DemoInventory.Application.Interfaces;
using DemoInventory.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DemoInventory.Application.Services;

/// <summary>
/// Service for category analytics operations
/// </summary>
public class CategoryAnalyticsService : ICategoryAnalyticsService
{
    private readonly ICategoryAnalyticsRepository _analyticsRepository;
    private readonly ILogger<CategoryAnalyticsService> _logger;

    public CategoryAnalyticsService(
        ICategoryAnalyticsRepository analyticsRepository,
        ILogger<CategoryAnalyticsService> logger)
    {
        _analyticsRepository = analyticsRepository ?? throw new ArgumentNullException(nameof(analyticsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets comprehensive analytics for all categories
    /// </summary>
    /// <param name="dateRange">Optional date range filter</param>
    /// <returns>Complete category metrics and analytics data</returns>
    public async Task<CategoryMetricsDto> GetCategoryMetricsAsync(AnalyticsDateRangeDto? dateRange = null)
    {
        _logger.LogInformation("Retrieving category metrics with date range: {StartDate} - {EndDate}", 
            dateRange?.StartDate, dateRange?.EndDate);

        try
        {
            var includeEmptyCategories = !(dateRange?.IncludeOnlyActiveCategories ?? true);
            var analyticsData = await _analyticsRepository.GetCategoryAnalyticsAsync(
                includeEmptyCategories, 
                dateRange?.StartDate, 
                dateRange?.EndDate);

            var overallMetrics = await _analyticsRepository.GetOverallCategoryMetricsAsync();

            var categoryAnalytics = analyticsData.Select(data => MapToCategoryAnalyticsDto(data, overallMetrics.TotalInventoryValue));

            var result = new CategoryMetricsDto
            {
                TotalCategories = overallMetrics.TotalCategories,
                TotalProducts = overallMetrics.TotalProducts,
                TotalInventoryValue = overallMetrics.TotalInventoryValue,
                AverageProductsPerCategory = overallMetrics.AverageProductsPerCategory,
                CategoriesWithProducts = overallMetrics.CategoriesWithProducts,
                EmptyCategories = overallMetrics.EmptyCategories,
                CategoryAnalytics = categoryAnalytics
            };

            _logger.LogInformation("Retrieved metrics for {CategoryCount} categories with {ProductCount} total products", 
                result.TotalCategories, result.TotalProducts);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category metrics");
            throw;
        }
    }

    /// <summary>
    /// Gets analytics for a specific category
    /// </summary>
    /// <param name="categoryId">The category ID to analyze</param>
    /// <param name="dateRange">Optional date range filter</param>
    /// <returns>Analytics data for the specified category</returns>
    public async Task<CategoryAnalyticsDto?> GetCategoryAnalyticsByIdAsync(int categoryId, AnalyticsDateRangeDto? dateRange = null)
    {
        _logger.LogInformation("Retrieving analytics for category {CategoryId}", categoryId);

        try
        {
            var analyticsData = await _analyticsRepository.GetCategoryAnalyticsByIdAsync(
                categoryId, 
                dateRange?.StartDate, 
                dateRange?.EndDate);

            if (analyticsData == null)
            {
                _logger.LogWarning("Category {CategoryId} not found", categoryId);
                return null;
            }

            var overallMetrics = await _analyticsRepository.GetOverallCategoryMetricsAsync();
            var result = MapToCategoryAnalyticsDto(analyticsData, overallMetrics.TotalInventoryValue);

            _logger.LogInformation("Retrieved analytics for category {CategoryId}: {ProductCount} products, {InventoryValue:C} value", 
                categoryId, result.TotalProducts, result.TotalInventoryValue);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics for category {CategoryId}", categoryId);
            throw;
        }
    }

    /// <summary>
    /// Gets top performing categories by various metrics
    /// </summary>
    /// <param name="count">Number of top categories to return</param>
    /// <param name="sortBy">Metric to sort by (ProductCount, InventoryValue, AveragePrice)</param>
    /// <returns>Top categories by the specified metric</returns>
    public async Task<IEnumerable<CategoryAnalyticsDto>> GetTopCategoriesAsync(int count = 10, string sortBy = "ProductCount")
    {
        _logger.LogInformation("Retrieving top {Count} categories sorted by {SortBy}", count, sortBy);

        try
        {
            IEnumerable<Domain.Interfaces.CategoryAnalyticsData> analyticsData;

            switch (sortBy.ToLowerInvariant())
            {
                case "productcount":
                    analyticsData = await _analyticsRepository.GetTopCategoriesByProductCountAsync(count);
                    break;
                case "inventoryvalue":
                    analyticsData = await _analyticsRepository.GetTopCategoriesByInventoryValueAsync(count);
                    break;
                default:
                    _logger.LogWarning("Invalid sort parameter {SortBy}, defaulting to ProductCount", sortBy);
                    analyticsData = await _analyticsRepository.GetTopCategoriesByProductCountAsync(count);
                    break;
            }

            var overallMetrics = await _analyticsRepository.GetOverallCategoryMetricsAsync();
            var result = analyticsData.Select(data => MapToCategoryAnalyticsDto(data, overallMetrics.TotalInventoryValue)).ToList();

            _logger.LogInformation("Retrieved {ResultCount} top categories", result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top categories");
            throw;
        }
    }

    /// <summary>
    /// Gets trending categories based on recent activity
    /// </summary>
    /// <param name="daysPeriod">Number of days to analyze for trends</param>
    /// <param name="count">Number of trending categories to return</param>
    /// <returns>Trending category data</returns>
    public async Task<IEnumerable<CategoryTrendDto>> GetCategoryTrendsAsync(int daysPeriod = 30, int count = 10)
    {
        _logger.LogInformation("Retrieving category trends for {DaysPeriod} days, top {Count}", daysPeriod, count);

        try
        {
            var trendData = await _analyticsRepository.GetCategoryTrendsAsync(daysPeriod);
            var result = trendData
                .Take(count)
                .Select(MapToCategoryTrendDto)
                .ToList();

            _logger.LogInformation("Retrieved trends for {TrendCount} categories", result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category trends");
            throw;
        }
    }

    /// <summary>
    /// Gets inventory distribution across categories
    /// </summary>
    /// <returns>Category inventory distribution data</returns>
    public async Task<IEnumerable<CategoryInventoryDistributionDto>> GetCategoryInventoryDistributionAsync()
    {
        _logger.LogInformation("Retrieving category inventory distribution");

        try
        {
            var analyticsData = await _analyticsRepository.GetCategoryAnalyticsAsync(includeEmptyCategories: false);
            var overallMetrics = await _analyticsRepository.GetOverallCategoryMetricsAsync();

            var result = analyticsData.Select(data => new CategoryInventoryDistributionDto
            {
                CategoryId = data.CategoryId,
                CategoryName = data.CategoryName,
                ProductPercentage = overallMetrics.TotalProducts > 0 
                    ? (decimal)data.TotalProducts / overallMetrics.TotalProducts * 100 
                    : 0,
                ValuePercentage = overallMetrics.TotalInventoryValue > 0 
                    ? data.TotalInventoryValue / overallMetrics.TotalInventoryValue * 100 
                    : 0,
                StockPercentage = analyticsData.Sum(x => x.TotalStockQuantity) > 0 
                    ? (decimal)data.TotalStockQuantity / analyticsData.Sum(x => x.TotalStockQuantity) * 100 
                    : 0
            }).ToList();

            _logger.LogInformation("Retrieved distribution for {CategoryCount} categories", result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory distribution");
            throw;
        }
    }

    /// <summary>
    /// Gets categories with low stock or out of stock products
    /// </summary>
    /// <param name="lowStockThreshold">Threshold for considering stock as low</param>
    /// <returns>Categories with stock issues</returns>
    public async Task<IEnumerable<CategoryAnalyticsDto>> GetCategoriesWithStockIssuesAsync(int lowStockThreshold = 10)
    {
        _logger.LogInformation("Retrieving categories with stock issues (threshold: {Threshold})", lowStockThreshold);

        try
        {
            var analyticsData = await _analyticsRepository.GetCategoriesWithLowStockAsync(lowStockThreshold);
            var overallMetrics = await _analyticsRepository.GetOverallCategoryMetricsAsync();

            var result = analyticsData
                .Where(data => data.LowStockProducts > 0 || data.OutOfStockProducts > 0)
                .Select(data => MapToCategoryAnalyticsDto(data, overallMetrics.TotalInventoryValue))
                .ToList();

            _logger.LogInformation("Found {CategoryCount} categories with stock issues", result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories with stock issues");
            throw;
        }
    }

    /// <summary>
    /// Gets analytics summary for dashboard display
    /// </summary>
    /// <returns>Key metrics for dashboard</returns>
    public async Task<object> GetAnalyticsSummaryAsync()
    {
        _logger.LogInformation("Retrieving analytics summary for dashboard");

        try
        {
            var overallMetrics = await _analyticsRepository.GetOverallCategoryMetricsAsync();
            var topCategories = await _analyticsRepository.GetTopCategoriesByInventoryValueAsync(5);
            var lowStockCategories = await _analyticsRepository.GetCategoriesWithLowStockAsync(10);

            var summary = new
            {
                TotalCategories = overallMetrics.TotalCategories,
                TotalProducts = overallMetrics.TotalProducts,
                TotalInventoryValue = overallMetrics.TotalInventoryValue,
                CategoriesWithProducts = overallMetrics.CategoriesWithProducts,
                EmptyCategories = overallMetrics.EmptyCategories,
                AverageProductsPerCategory = overallMetrics.AverageProductsPerCategory,
                TopCategoriesByValue = topCategories.Take(3).Select(c => new { c.CategoryName, c.TotalInventoryValue }),
                CategoriesWithStockIssues = lowStockCategories.Count(c => c.LowStockProducts > 0 || c.OutOfStockProducts > 0),
                LastUpdated = DateTime.UtcNow
            };

            _logger.LogInformation("Generated analytics summary with {TotalCategories} categories and {TotalProducts} products", 
                summary.TotalCategories, summary.TotalProducts);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics summary");
            throw;
        }
    }

    #region Private Helper Methods

    private static CategoryAnalyticsDto MapToCategoryAnalyticsDto(Domain.Interfaces.CategoryAnalyticsData data, decimal totalInventoryValue)
    {
        return new CategoryAnalyticsDto
        {
            CategoryId = data.CategoryId,
            CategoryName = data.CategoryName,
            CategoryDescription = data.CategoryDescription,
            TotalProducts = data.TotalProducts,
            TotalStockQuantity = data.TotalStockQuantity,
            AveragePrice = data.AveragePrice,
            MinPrice = data.MinPrice,
            MaxPrice = data.MaxPrice,
            TotalInventoryValue = data.TotalInventoryValue,
            InventoryValuePercentage = totalInventoryValue > 0 ? (data.TotalInventoryValue / totalInventoryValue * 100) : 0,
            LowStockProducts = data.LowStockProducts,
            OutOfStockProducts = data.OutOfStockProducts
        };
    }

    private static CategoryTrendDto MapToCategoryTrendDto(Domain.Interfaces.CategoryTrendData data)
    {
        return new CategoryTrendDto
        {
            CategoryId = data.CategoryId,
            CategoryName = data.CategoryName,
            TrendScore = data.TrendScore,
            TrendRank = data.TrendRank,
            ProductGrowthRate = data.ProductGrowthRate,
            InventoryTurnover = data.InventoryTurnover
        };
    }

    #endregion
}
