using DemoInventory.Application.DTOs;
using DemoInventory.Application.Services;
using DemoInventory.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using FluentAssertions;

namespace DemoInventory.Application.Tests;

/// <summary>
/// Unit tests for CategoryAnalyticsService
/// </summary>
public class CategoryAnalyticsServiceTests
{
    private readonly ICategoryAnalyticsRepository _mockRepository;
    private readonly ILogger<CategoryAnalyticsService> _mockLogger;
    private readonly CategoryAnalyticsService _service;

    public CategoryAnalyticsServiceTests()
    {
        _mockRepository = Substitute.For<ICategoryAnalyticsRepository>();
        _mockLogger = Substitute.For<ILogger<CategoryAnalyticsService>>();
        _service = new CategoryAnalyticsService(_mockRepository, _mockLogger);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_Should_Throw_ArgumentNullException_When_Repository_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new CategoryAnalyticsService(null!, _mockLogger));
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentNullException_When_Logger_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new CategoryAnalyticsService(_mockRepository, null!));
    }

    #endregion

    #region GetCategoryMetricsAsync Tests

    [Fact]
    public async Task GetCategoryMetricsAsync_Should_Return_Complete_Metrics_When_Data_Available()
    {
        // Arrange
        var analyticsData = CreateSampleAnalyticsData();
        var overallMetrics = CreateSampleOverallMetrics();

        _mockRepository.GetCategoryAnalyticsAsync(Arg.Any<bool>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
            .Returns(analyticsData);
        _mockRepository.GetOverallCategoryMetricsAsync()
            .Returns(overallMetrics);

        // Act
        var result = await _service.GetCategoryMetricsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalCategories.Should().Be(overallMetrics.TotalCategories);
        result.TotalProducts.Should().Be(overallMetrics.TotalProducts);
        result.TotalInventoryValue.Should().Be(overallMetrics.TotalInventoryValue);
        result.CategoryAnalytics.Should().HaveCount(analyticsData.Count());
    }

    [Fact]
    public async Task GetCategoryMetricsAsync_Should_Apply_Date_Range_Filter_When_Provided()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var dateRange = new AnalyticsDateRangeDto
        {
            StartDate = startDate,
            EndDate = endDate,
            IncludeOnlyActiveCategories = true
        };

        var analyticsData = CreateSampleAnalyticsData();
        var overallMetrics = CreateSampleOverallMetrics();

        _mockRepository.GetCategoryAnalyticsAsync(false, startDate, endDate)
            .Returns(analyticsData);
        _mockRepository.GetOverallCategoryMetricsAsync()
            .Returns(overallMetrics);

        // Act
        var result = await _service.GetCategoryMetricsAsync(dateRange);

        // Assert
        await _mockRepository.Received(1).GetCategoryAnalyticsAsync(false, startDate, endDate);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCategoryMetricsAsync_Should_Log_Information_And_Handle_Exceptions()
    {
        // Arrange
        _mockRepository.GetCategoryAnalyticsAsync(Arg.Any<bool>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.GetCategoryMetricsAsync());
        
        _mockLogger.Received().LogInformation(Arg.Any<string>(), Arg.Any<object[]>());
        _mockLogger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>());
    }

    #endregion

    #region GetCategoryAnalyticsByIdAsync Tests

    [Fact]
    public async Task GetCategoryAnalyticsByIdAsync_Should_Return_Analytics_When_Category_Exists()
    {
        // Arrange
        var categoryId = 1;
        var analyticsData = CreateSampleAnalyticsData().First();
        var overallMetrics = CreateSampleOverallMetrics();

        _mockRepository.GetCategoryAnalyticsByIdAsync(categoryId, Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
            .Returns(analyticsData);
        _mockRepository.GetOverallCategoryMetricsAsync()
            .Returns(overallMetrics);

        // Act
        var result = await _service.GetCategoryAnalyticsByIdAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result!.CategoryId.Should().Be(categoryId);
        result.CategoryName.Should().Be(analyticsData.CategoryName);
        result.TotalProducts.Should().Be(analyticsData.TotalProducts);
    }

    [Fact]
    public async Task GetCategoryAnalyticsByIdAsync_Should_Return_Null_When_Category_Not_Found()
    {
        // Arrange
        var categoryId = 999;
        _mockRepository.GetCategoryAnalyticsByIdAsync(categoryId, Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
            .Returns((CategoryAnalyticsData?)null);

        // Act
        var result = await _service.GetCategoryAnalyticsByIdAsync(categoryId);

        // Assert
        result.Should().BeNull();
        _mockLogger.Received().LogWarning(Arg.Is<string>(s => s.Contains("not found")), categoryId);
    }

    #endregion

    #region GetTopCategoriesAsync Tests

    [Fact]
    public async Task GetTopCategoriesAsync_Should_Return_Top_Categories_By_Product_Count()
    {
        // Arrange
        var count = 5;
        var sortBy = "ProductCount";
        var analyticsData = CreateSampleAnalyticsData();
        var overallMetrics = CreateSampleOverallMetrics();

        _mockRepository.GetTopCategoriesByProductCountAsync(count)
            .Returns(analyticsData);
        _mockRepository.GetOverallCategoryMetricsAsync()
            .Returns(overallMetrics);

        // Act
        var result = await _service.GetTopCategoriesAsync(count, sortBy);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(analyticsData.Count());
        await _mockRepository.Received(1).GetTopCategoriesByProductCountAsync(count);
    }

    [Fact]
    public async Task GetTopCategoriesAsync_Should_Return_Top_Categories_By_Inventory_Value()
    {
        // Arrange
        var count = 3;
        var sortBy = "InventoryValue";
        var analyticsData = CreateSampleAnalyticsData();
        var overallMetrics = CreateSampleOverallMetrics();

        _mockRepository.GetTopCategoriesByInventoryValueAsync(count)
            .Returns(analyticsData);
        _mockRepository.GetOverallCategoryMetricsAsync()
            .Returns(overallMetrics);

        // Act
        var result = await _service.GetTopCategoriesAsync(count, sortBy);

        // Assert
        result.Should().NotBeNull();
        await _mockRepository.Received(1).GetTopCategoriesByInventoryValueAsync(count);
    }

    [Fact]
    public async Task GetTopCategoriesAsync_Should_Default_To_Product_Count_For_Invalid_Sort_Parameter()
    {
        // Arrange
        var count = 5;
        var sortBy = "InvalidSort";
        var analyticsData = CreateSampleAnalyticsData();
        var overallMetrics = CreateSampleOverallMetrics();

        _mockRepository.GetTopCategoriesByProductCountAsync(count)
            .Returns(analyticsData);
        _mockRepository.GetOverallCategoryMetricsAsync()
            .Returns(overallMetrics);

        // Act
        var result = await _service.GetTopCategoriesAsync(count, sortBy);

        // Assert
        await _mockRepository.Received(1).GetTopCategoriesByProductCountAsync(count);
        _mockLogger.Received().LogWarning(Arg.Is<string>(s => s.Contains("Invalid sort parameter")), sortBy);
    }

    #endregion

    #region GetCategoryTrendsAsync Tests

    [Fact]
    public async Task GetCategoryTrendsAsync_Should_Return_Trend_Data()
    {
        // Arrange
        var daysPeriod = 30;
        var count = 10;
        var trendData = CreateSampleTrendData();

        _mockRepository.GetCategoryTrendsAsync(daysPeriod)
            .Returns(trendData);

        // Act
        var result = await _service.GetCategoryTrendsAsync(daysPeriod, count);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountLessOrEqualTo(count);
        await _mockRepository.Received(1).GetCategoryTrendsAsync(daysPeriod);
    }

    #endregion

    #region GetCategoryInventoryDistributionAsync Tests

    [Fact]
    public async Task GetCategoryInventoryDistributionAsync_Should_Calculate_Percentages_Correctly()
    {
        // Arrange
        var analyticsData = CreateSampleAnalyticsData();
        var overallMetrics = CreateSampleOverallMetrics();

        _mockRepository.GetCategoryAnalyticsAsync(false, null, null)
            .Returns(analyticsData);
        _mockRepository.GetOverallCategoryMetricsAsync()
            .Returns(overallMetrics);

        // Act
        var result = await _service.GetCategoryInventoryDistributionAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(analyticsData.Count());
        
        var firstItem = result.First();
        firstItem.ProductPercentage.Should().BeGreaterThan(0);
        firstItem.ValuePercentage.Should().BeGreaterThan(0);
    }

    #endregion

    #region GetCategoriesWithStockIssuesAsync Tests

    [Fact]
    public async Task GetCategoriesWithStockIssuesAsync_Should_Return_Categories_With_Stock_Problems()
    {
        // Arrange
        var lowStockThreshold = 10;
        var analyticsData = CreateSampleAnalyticsDataWithStockIssues();
        var overallMetrics = CreateSampleOverallMetrics();

        _mockRepository.GetCategoriesWithLowStockAsync(lowStockThreshold)
            .Returns(analyticsData);
        _mockRepository.GetOverallCategoryMetricsAsync()
            .Returns(overallMetrics);

        // Act
        var result = await _service.GetCategoriesWithStockIssuesAsync(lowStockThreshold);

        // Assert
        result.Should().NotBeNull();
        result.Should().OnlyContain(c => c.LowStockProducts > 0 || c.OutOfStockProducts > 0);
    }

    #endregion

    #region GetAnalyticsSummaryAsync Tests

    [Fact]
    public async Task GetAnalyticsSummaryAsync_Should_Return_Dashboard_Summary()
    {
        // Arrange
        var overallMetrics = CreateSampleOverallMetrics();
        var topCategories = CreateSampleAnalyticsData();
        var lowStockCategories = CreateSampleAnalyticsDataWithStockIssues();

        _mockRepository.GetOverallCategoryMetricsAsync()
            .Returns(overallMetrics);
        _mockRepository.GetTopCategoriesByInventoryValueAsync(5)
            .Returns(topCategories);
        _mockRepository.GetCategoriesWithLowStockAsync(10)
            .Returns(lowStockCategories);

        // Act
        var result = await _service.GetAnalyticsSummaryAsync();

        // Assert
        result.Should().NotBeNull();
        var summary = result as dynamic;
        // Note: In real tests, you'd use a concrete type instead of dynamic
    }

    #endregion

    #region Helper Methods

    private static IEnumerable<CategoryAnalyticsData> CreateSampleAnalyticsData()
    {
        return new List<CategoryAnalyticsData>
        {
            new()
            {
                CategoryId = 1,
                CategoryName = "Electronics",
                CategoryDescription = "Electronic devices and gadgets",
                TotalProducts = 15,
                TotalStockQuantity = 150,
                AveragePrice = 299.99m,
                MinPrice = 19.99m,
                MaxPrice = 999.99m,
                TotalInventoryValue = 45000m,
                LowStockProducts = 2,
                OutOfStockProducts = 0,
                CategoryCreatedAt = DateTime.UtcNow.AddDays(-100),
                CategoryUpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                CategoryId = 2,
                CategoryName = "Books",
                CategoryDescription = "Books and literature",
                TotalProducts = 25,
                TotalStockQuantity = 300,
                AveragePrice = 24.99m,
                MinPrice = 9.99m,
                MaxPrice = 49.99m,
                TotalInventoryValue = 7500m,
                LowStockProducts = 1,
                OutOfStockProducts = 0,
                CategoryCreatedAt = DateTime.UtcNow.AddDays(-80),
                CategoryUpdatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };
    }

    private static IEnumerable<CategoryAnalyticsData> CreateSampleAnalyticsDataWithStockIssues()
    {
        return new List<CategoryAnalyticsData>
        {
            new()
            {
                CategoryId = 3,
                CategoryName = "Clothing",
                CategoryDescription = "Apparel and accessories",
                TotalProducts = 10,
                TotalStockQuantity = 50,
                AveragePrice = 39.99m,
                MinPrice = 14.99m,
                MaxPrice = 89.99m,
                TotalInventoryValue = 2000m,
                LowStockProducts = 5,
                OutOfStockProducts = 2,
                CategoryCreatedAt = DateTime.UtcNow.AddDays(-60),
                CategoryUpdatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };
    }

    private static OverallCategoryMetrics CreateSampleOverallMetrics()
    {
        return new OverallCategoryMetrics
        {
            TotalCategories = 5,
            TotalProducts = 50,
            TotalInventoryValue = 52500m,
            CategoriesWithProducts = 4,
            EmptyCategories = 1,
            AverageProductsPerCategory = 10m
        };
    }

    private static IEnumerable<CategoryTrendData> CreateSampleTrendData()
    {
        return new List<CategoryTrendData>
        {
            new()
            {
                CategoryId = 1,
                CategoryName = "Electronics",
                TrendScore = 85.5m,
                TrendRank = 1,
                ProductGrowthRate = 15.5m,
                InventoryTurnover = 25.0m,
                RecentProductsAdded = 5,
                AnalysisPeriodStart = DateTime.UtcNow.AddDays(-30),
                AnalysisPeriodEnd = DateTime.UtcNow
            },
            new()
            {
                CategoryId = 2,
                CategoryName = "Books",
                TrendScore = 72.3m,
                TrendRank = 2,
                ProductGrowthRate = 8.2m,
                InventoryTurnover = 18.5m,
                RecentProductsAdded = 3,
                AnalysisPeriodStart = DateTime.UtcNow.AddDays(-30),
                AnalysisPeriodEnd = DateTime.UtcNow
            }
        };
    }

    #endregion
}
