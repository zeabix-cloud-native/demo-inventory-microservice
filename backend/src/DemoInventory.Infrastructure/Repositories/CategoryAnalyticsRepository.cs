using DemoInventory.Domain.Interfaces;
using DemoInventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DemoInventory.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for category analytics operations using Entity Framework Core
/// </summary>
public class CategoryAnalyticsRepository : ICategoryAnalyticsRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CategoryAnalyticsRepository> _logger;

    public CategoryAnalyticsRepository(
        ApplicationDbContext context,
        ILogger<CategoryAnalyticsRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets analytics data for all categories
    /// </summary>
    public async Task<IEnumerable<CategoryAnalyticsData>> GetCategoryAnalyticsAsync(
        bool includeEmptyCategories = true, 
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        _logger.LogInformation("Retrieving category analytics data. IncludeEmpty: {IncludeEmpty}, DateRange: {StartDate} - {EndDate}", 
            includeEmptyCategories, startDate, endDate);

        try
        {
            var query = _context.Categories.AsQueryable();

            if (!includeEmptyCategories)
            {
                query = query.Where(c => c.Products.Any());
            }

            var result = await query
                .Select(c => new CategoryAnalyticsData
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    CategoryDescription = c.Description,
                    CategoryCreatedAt = c.CreatedAt,
                    CategoryUpdatedAt = c.UpdatedAt,
                    TotalProducts = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Count(),
                    TotalStockQuantity = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Sum(p => (int?)p.QuantityInStock) ?? 0,
                    AveragePrice = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Average(p => (decimal?)p.Price) ?? 0,
                    MinPrice = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Min(p => (decimal?)p.Price) ?? 0,
                    MaxPrice = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Max(p => (decimal?)p.Price) ?? 0,
                    TotalInventoryValue = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Sum(p => (decimal?)(p.Price * p.QuantityInStock)) ?? 0,
                    LowStockProducts = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Count(p => p.QuantityInStock > 0 && p.QuantityInStock < 10),
                    OutOfStockProducts = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Count(p => p.QuantityInStock == 0),
                    LastProductAdded = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Max(p => (DateTime?)p.CreatedAt)
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved analytics data for {CategoryCount} categories", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category analytics data");
            throw;
        }
    }

    /// <summary>
    /// Gets analytics data for a specific category
    /// </summary>
    public async Task<CategoryAnalyticsData?> GetCategoryAnalyticsByIdAsync(
        int categoryId, 
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        _logger.LogInformation("Retrieving analytics data for category {CategoryId}", categoryId);

        try
        {
            var result = await _context.Categories
                .Where(c => c.Id == categoryId)
                .Select(c => new CategoryAnalyticsData
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    CategoryDescription = c.Description,
                    CategoryCreatedAt = c.CreatedAt,
                    CategoryUpdatedAt = c.UpdatedAt,
                    TotalProducts = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Count(),
                    TotalStockQuantity = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Sum(p => (int?)p.QuantityInStock) ?? 0,
                    AveragePrice = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Average(p => (decimal?)p.Price) ?? 0,
                    MinPrice = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Min(p => (decimal?)p.Price) ?? 0,
                    MaxPrice = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Max(p => (decimal?)p.Price) ?? 0,
                    TotalInventoryValue = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Sum(p => (decimal?)(p.Price * p.QuantityInStock)) ?? 0,
                    LowStockProducts = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Count(p => p.QuantityInStock > 0 && p.QuantityInStock < 10),
                    OutOfStockProducts = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Count(p => p.QuantityInStock == 0),
                    LastProductAdded = c.Products
                        .Where(p => startDate == null || p.CreatedAt >= startDate)
                        .Where(p => endDate == null || p.CreatedAt <= endDate)
                        .Max(p => (DateTime?)p.CreatedAt)
                })
                .FirstOrDefaultAsync();

            if (result != null)
            {
                _logger.LogInformation("Retrieved analytics data for category {CategoryId}: {ProductCount} products", 
                    categoryId, result.TotalProducts);
            }
            else
            {
                _logger.LogWarning("Category {CategoryId} not found", categoryId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics data for category {CategoryId}", categoryId);
            throw;
        }
    }

    /// <summary>
    /// Gets top N categories by product count
    /// </summary>
    public async Task<IEnumerable<CategoryAnalyticsData>> GetTopCategoriesByProductCountAsync(int count = 10)
    {
        _logger.LogInformation("Retrieving top {Count} categories by product count", count);

        try
        {
            var result = await _context.Categories
                .Where(c => c.Products.Any())
                .Select(c => new CategoryAnalyticsData
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    CategoryDescription = c.Description,
                    CategoryCreatedAt = c.CreatedAt,
                    CategoryUpdatedAt = c.UpdatedAt,
                    TotalProducts = c.Products.Count(),
                    TotalStockQuantity = c.Products.Sum(p => p.QuantityInStock),
                    AveragePrice = c.Products.Average(p => p.Price),
                    MinPrice = c.Products.Min(p => p.Price),
                    MaxPrice = c.Products.Max(p => p.Price),
                    TotalInventoryValue = c.Products.Sum(p => p.Price * p.QuantityInStock),
                    LowStockProducts = c.Products.Count(p => p.QuantityInStock > 0 && p.QuantityInStock < 10),
                    OutOfStockProducts = c.Products.Count(p => p.QuantityInStock == 0),
                    LastProductAdded = c.Products.Max(p => (DateTime?)p.CreatedAt)
                })
                .OrderByDescending(c => c.TotalProducts)
                .Take(count)
                .ToListAsync();

            _logger.LogInformation("Retrieved {ResultCount} top categories by product count", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top categories by product count");
            throw;
        }
    }

    /// <summary>
    /// Gets top N categories by inventory value
    /// </summary>
    public async Task<IEnumerable<CategoryAnalyticsData>> GetTopCategoriesByInventoryValueAsync(int count = 10)
    {
        _logger.LogInformation("Retrieving top {Count} categories by inventory value", count);

        try
        {
            var result = await _context.Categories
                .Where(c => c.Products.Any())
                .Select(c => new CategoryAnalyticsData
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    CategoryDescription = c.Description,
                    CategoryCreatedAt = c.CreatedAt,
                    CategoryUpdatedAt = c.UpdatedAt,
                    TotalProducts = c.Products.Count(),
                    TotalStockQuantity = c.Products.Sum(p => p.QuantityInStock),
                    AveragePrice = c.Products.Average(p => p.Price),
                    MinPrice = c.Products.Min(p => p.Price),
                    MaxPrice = c.Products.Max(p => p.Price),
                    TotalInventoryValue = c.Products.Sum(p => p.Price * p.QuantityInStock),
                    LowStockProducts = c.Products.Count(p => p.QuantityInStock > 0 && p.QuantityInStock < 10),
                    OutOfStockProducts = c.Products.Count(p => p.QuantityInStock == 0),
                    LastProductAdded = c.Products.Max(p => (DateTime?)p.CreatedAt)
                })
                .OrderByDescending(c => c.TotalInventoryValue)
                .Take(count)
                .ToListAsync();

            _logger.LogInformation("Retrieved {ResultCount} top categories by inventory value", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top categories by inventory value");
            throw;
        }
    }

    /// <summary>
    /// Gets categories with low stock products
    /// </summary>
    public async Task<IEnumerable<CategoryAnalyticsData>> GetCategoriesWithLowStockAsync(int lowStockThreshold = 10)
    {
        _logger.LogInformation("Retrieving categories with low stock (threshold: {Threshold})", lowStockThreshold);

        try
        {
            var result = await _context.Categories
                .Where(c => c.Products.Any(p => p.QuantityInStock <= lowStockThreshold))
                .Select(c => new CategoryAnalyticsData
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    CategoryDescription = c.Description,
                    CategoryCreatedAt = c.CreatedAt,
                    CategoryUpdatedAt = c.UpdatedAt,
                    TotalProducts = c.Products.Count(),
                    TotalStockQuantity = c.Products.Sum(p => p.QuantityInStock),
                    AveragePrice = c.Products.Average(p => (decimal?)p.Price) ?? 0,
                    MinPrice = c.Products.Min(p => (decimal?)p.Price) ?? 0,
                    MaxPrice = c.Products.Max(p => (decimal?)p.Price) ?? 0,
                    TotalInventoryValue = c.Products.Sum(p => (decimal?)(p.Price * p.QuantityInStock)) ?? 0,
                    LowStockProducts = c.Products.Count(p => p.QuantityInStock > 0 && p.QuantityInStock < lowStockThreshold),
                    OutOfStockProducts = c.Products.Count(p => p.QuantityInStock == 0),
                    LastProductAdded = c.Products.Max(p => (DateTime?)p.CreatedAt)
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved {CategoryCount} categories with low stock", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories with low stock");
            throw;
        }
    }

    /// <summary>
    /// Gets overall metrics across all categories
    /// </summary>
    public async Task<OverallCategoryMetrics> GetOverallCategoryMetricsAsync()
    {
        _logger.LogInformation("Retrieving overall category metrics");

        try
        {
            var totalCategories = await _context.Categories.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalInventoryValue = await _context.Products.SumAsync(p => (decimal?)(p.Price * p.QuantityInStock)) ?? 0;
            var categoriesWithProducts = await _context.Categories.CountAsync(c => c.Products.Any());
            var emptyCategories = totalCategories - categoriesWithProducts;
            var averageProductsPerCategory = totalCategories > 0 ? (decimal)totalProducts / totalCategories : 0;

            var result = new OverallCategoryMetrics
            {
                TotalCategories = totalCategories,
                TotalProducts = totalProducts,
                TotalInventoryValue = totalInventoryValue,
                CategoriesWithProducts = categoriesWithProducts,
                EmptyCategories = emptyCategories,
                AverageProductsPerCategory = averageProductsPerCategory
            };

            _logger.LogInformation("Retrieved overall metrics: {TotalCategories} categories, {TotalProducts} products, {TotalValue:C} value", 
                result.TotalCategories, result.TotalProducts, result.TotalInventoryValue);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overall category metrics");
            throw;
        }
    }

    /// <summary>
    /// Gets category trend data based on recent activity
    /// </summary>
    public async Task<IEnumerable<CategoryTrendData>> GetCategoryTrendsAsync(int daysPeriod = 30)
    {
        _logger.LogInformation("Retrieving category trends for {DaysPeriod} days", daysPeriod);

        try
        {
            var analysisStartDate = DateTime.UtcNow.AddDays(-daysPeriod);
            var analysisEndDate = DateTime.UtcNow;

            var result = await _context.Categories
                .Where(c => c.Products.Any())
                .Select(c => new
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    TotalProducts = c.Products.Count(),
                    RecentProductsAdded = c.Products.Count(p => p.CreatedAt >= analysisStartDate),
                    InventoryValue = c.Products.Sum(p => p.Price * p.QuantityInStock),
                    CategoryAge = EF.Functions.DateDiffDay(c.CreatedAt, DateTime.UtcNow)
                })
                .ToListAsync();

            var trendData = result.Select((item, index) => new CategoryTrendData
            {
                CategoryId = item.CategoryId,
                CategoryName = item.CategoryName,
                TrendScore = CalculateTrendScore(item.RecentProductsAdded, item.TotalProducts, item.CategoryAge),
                TrendRank = index + 1, // Will be recalculated after sorting
                ProductGrowthRate = item.TotalProducts > 0 ? (decimal)item.RecentProductsAdded / item.TotalProducts * 100 : 0,
                InventoryTurnover = CalculateInventoryTurnover(item.InventoryValue, daysPeriod),
                RecentProductsAdded = item.RecentProductsAdded,
                AnalysisPeriodStart = analysisStartDate,
                AnalysisPeriodEnd = analysisEndDate
            })
            .OrderByDescending(t => t.TrendScore)
            .Select((trend, index) => { trend.TrendRank = index + 1; return trend; })
            .ToList();

            _logger.LogInformation("Retrieved trend data for {CategoryCount} categories", trendData.Count);
            return trendData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category trends");
            throw;
        }
    }

    #region Private Helper Methods

    private static decimal CalculateTrendScore(int recentProducts, int totalProducts, int categoryAge)
    {
        // Simple trend score calculation based on recent activity and category maturity
        var activityScore = recentProducts * 10;
        var maturityBonus = categoryAge > 365 ? 5 : (categoryAge > 90 ? 2 : 0);
        var productDensityScore = totalProducts > 0 ? Math.Min(totalProducts / 10.0m, 5) : 0;
        
        return activityScore + maturityBonus + productDensityScore;
    }

    private static decimal CalculateInventoryTurnover(decimal inventoryValue, int daysPeriod)
    {
        // Simplified inventory turnover calculation
        // In a real system, this would use actual sales/movement data
        if (inventoryValue <= 0) return 0;
        
        var estimatedMonthlySales = inventoryValue * 0.1m; // Assume 10% monthly turnover
        var periodTurnover = estimatedMonthlySales * (daysPeriod / 30.0m);
        
        return Math.Round(periodTurnover / inventoryValue * 100, 2);
    }

    #endregion
}
