using DemoInventory.API.Controllers;
using DemoInventory.Application.DTOs;
using DemoInventory.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using FluentAssertions;

namespace DemoInventory.API.Tests;

/// <summary>
/// Unit tests for CategoryAnalyticsController
/// </summary>
public class CategoryAnalyticsControllerTests
{
    private readonly ICategoryAnalyticsService _mockService;
    private readonly ILogger<CategoryAnalyticsController> _mockLogger;
    private readonly CategoryAnalyticsController _controller;

    public CategoryAnalyticsControllerTests()
    {
        _mockService = Substitute.For<ICategoryAnalyticsService>();
        _mockLogger = Substitute.For<ILogger<CategoryAnalyticsController>>();
        _controller = new CategoryAnalyticsController(_mockService, _mockLogger);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_Should_Throw_ArgumentNullException_When_Service_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new CategoryAnalyticsController(null!, _mockLogger));
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentNullException_When_Logger_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new CategoryAnalyticsController(_mockService, null!));
    }

    #endregion

    #region GetCategoryMetrics Tests

    [Fact]
    public async Task GetCategoryMetrics_Should_Return_Ok_With_Metrics_When_Successful()
    {
        // Arrange
        var expectedMetrics = CreateSampleCategoryMetrics();
        _mockService.GetCategoryMetricsAsync(Arg.Any<AnalyticsDateRangeDto?>())
            .Returns(expectedMetrics);

        // Act
        var result = await _controller.GetCategoryMetrics();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var metrics = okResult.Value.Should().BeOfType<CategoryMetricsDto>().Subject;
        metrics.TotalCategories.Should().Be(expectedMetrics.TotalCategories);
    }

    [Fact]
    public async Task GetCategoryMetrics_Should_Return_BadRequest_When_StartDate_Greater_Than_EndDate()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = await _controller.GetCategoryMetrics(startDate, endDate);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Start date cannot be greater than end date");
    }

    [Fact]
    public async Task GetCategoryMetrics_Should_Return_BadRequest_When_LowStockThreshold_Negative()
    {
        // Act
        var result = await _controller.GetCategoryMetrics(lowStockThreshold: -1);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Low stock threshold must be non-negative");
    }

    [Fact]
    public async Task GetCategoryMetrics_Should_Return_InternalServerError_When_Service_Throws_Exception()
    {
        // Arrange
        _mockService.GetCategoryMetricsAsync(Arg.Any<AnalyticsDateRangeDto?>())
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetCategoryMetrics();

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetCategoryAnalyticsById Tests

    [Fact]
    public async Task GetCategoryAnalyticsById_Should_Return_Ok_When_Category_Found()
    {
        // Arrange
        var categoryId = 1;
        var expectedAnalytics = CreateSampleCategoryAnalytics();
        _mockService.GetCategoryAnalyticsByIdAsync(categoryId, Arg.Any<AnalyticsDateRangeDto?>())
            .Returns(expectedAnalytics);

        // Act
        var result = await _controller.GetCategoryAnalyticsById(categoryId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var analytics = okResult.Value.Should().BeOfType<CategoryAnalyticsDto>().Subject;
        analytics.CategoryId.Should().Be(expectedAnalytics.CategoryId);
    }

    [Fact]
    public async Task GetCategoryAnalyticsById_Should_Return_NotFound_When_Category_Not_Found()
    {
        // Arrange
        var categoryId = 999;
        _mockService.GetCategoryAnalyticsByIdAsync(categoryId, Arg.Any<AnalyticsDateRangeDto?>())
            .Returns((CategoryAnalyticsDto?)null);

        // Act
        var result = await _controller.GetCategoryAnalyticsById(categoryId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Category with ID {categoryId} not found");
    }

    [Fact]
    public async Task GetCategoryAnalyticsById_Should_Return_BadRequest_When_CategoryId_Invalid()
    {
        // Act
        var result = await _controller.GetCategoryAnalyticsById(0);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Category ID must be greater than 0");
    }

    #endregion

    #region GetTopCategories Tests

    [Fact]
    public async Task GetTopCategories_Should_Return_Ok_With_Top_Categories()
    {
        // Arrange
        var count = 5;
        var sortBy = "ProductCount";
        var expectedCategories = CreateSampleCategoryAnalyticsList();
        _mockService.GetTopCategoriesAsync(count, sortBy)
            .Returns(expectedCategories);

        // Act
        var result = await _controller.GetTopCategories(count, sortBy);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var categories = okResult.Value.Should().BeAssignableTo<IEnumerable<CategoryAnalyticsDto>>().Subject;
        categories.Should().HaveCount(expectedCategories.Count());
    }

    [Fact]
    public async Task GetTopCategories_Should_Return_BadRequest_When_Count_Invalid()
    {
        // Act
        var result = await _controller.GetTopCategories(0);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Count must be between 1 and 50");
    }

    [Fact]
    public async Task GetTopCategories_Should_Return_BadRequest_When_SortBy_Invalid()
    {
        // Act
        var result = await _controller.GetTopCategories(5, "InvalidSort");

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Contain("Invalid sortBy parameter");
    }

    #endregion

    #region GetCategoryTrends Tests

    [Fact]
    public async Task GetCategoryTrends_Should_Return_Ok_With_Trends()
    {
        // Arrange
        var daysPeriod = 30;
        var count = 10;
        var expectedTrends = CreateSampleCategoryTrends();
        _mockService.GetCategoryTrendsAsync(daysPeriod, count)
            .Returns(expectedTrends);

        // Act
        var result = await _controller.GetCategoryTrends(daysPeriod, count);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var trends = okResult.Value.Should().BeAssignableTo<IEnumerable<CategoryTrendDto>>().Subject;
        trends.Should().HaveCount(expectedTrends.Count());
    }

    [Fact]
    public async Task GetCategoryTrends_Should_Return_BadRequest_When_DaysPeriod_Invalid()
    {
        // Act
        var result = await _controller.GetCategoryTrends(5, 10);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Days period must be between 7 and 365");
    }

    [Fact]
    public async Task GetCategoryTrends_Should_Return_BadRequest_When_Count_Invalid()
    {
        // Act
        var result = await _controller.GetCategoryTrends(30, 25);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Count must be between 1 and 20");
    }

    #endregion

    #region GetInventoryDistribution Tests

    [Fact]
    public async Task GetInventoryDistribution_Should_Return_Ok_With_Distribution()
    {
        // Arrange
        var expectedDistribution = CreateSampleInventoryDistribution();
        _mockService.GetCategoryInventoryDistributionAsync()
            .Returns(expectedDistribution);

        // Act
        var result = await _controller.GetInventoryDistribution();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distribution = okResult.Value.Should().BeAssignableTo<IEnumerable<CategoryInventoryDistributionDto>>().Subject;
        distribution.Should().HaveCount(expectedDistribution.Count());
    }

    #endregion

    #region GetCategoriesWithStockIssues Tests

    [Fact]
    public async Task GetCategoriesWithStockIssues_Should_Return_Ok_With_Categories()
    {
        // Arrange
        var lowStockThreshold = 10;
        var expectedCategories = CreateSampleCategoryAnalyticsList();
        _mockService.GetCategoriesWithStockIssuesAsync(lowStockThreshold)
            .Returns(expectedCategories);

        // Act
        var result = await _controller.GetCategoriesWithStockIssues(lowStockThreshold);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var categories = okResult.Value.Should().BeAssignableTo<IEnumerable<CategoryAnalyticsDto>>().Subject;
        categories.Should().HaveCount(expectedCategories.Count());
    }

    [Fact]
    public async Task GetCategoriesWithStockIssues_Should_Return_BadRequest_When_Threshold_Invalid()
    {
        // Act
        var result = await _controller.GetCategoriesWithStockIssues(0);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Low stock threshold must be between 1 and 100");
    }

    #endregion

    #region GetAnalyticsSummary Tests

    [Fact]
    public async Task GetAnalyticsSummary_Should_Return_Ok_With_Summary()
    {
        // Arrange
        var expectedSummary = new { TotalCategories = 5, TotalProducts = 50 };
        _mockService.GetAnalyticsSummaryAsync()
            .Returns(expectedSummary);

        // Act
        var result = await _controller.GetAnalyticsSummary();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedSummary);
    }

    #endregion

    #region Helper Methods

    private static CategoryMetricsDto CreateSampleCategoryMetrics()
    {
        return new CategoryMetricsDto
        {
            TotalCategories = 5,
            TotalProducts = 50,
            TotalInventoryValue = 25000m,
            AverageProductsPerCategory = 10m,
            CategoriesWithProducts = 4,
            EmptyCategories = 1,
            CategoryAnalytics = CreateSampleCategoryAnalyticsList()
        };
    }

    private static CategoryAnalyticsDto CreateSampleCategoryAnalytics()
    {
        return new CategoryAnalyticsDto
        {
            CategoryId = 1,
            CategoryName = "Electronics",
            CategoryDescription = "Electronic devices",
            TotalProducts = 15,
            TotalStockQuantity = 150,
            AveragePrice = 299.99m,
            MinPrice = 19.99m,
            MaxPrice = 999.99m,
            TotalInventoryValue = 45000m,
            InventoryValuePercentage = 65.5m,
            LowStockProducts = 2,
            OutOfStockProducts = 0
        };
    }

    private static IEnumerable<CategoryAnalyticsDto> CreateSampleCategoryAnalyticsList()
    {
        return new List<CategoryAnalyticsDto>
        {
            CreateSampleCategoryAnalytics(),
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
                InventoryValuePercentage = 15.2m,
                LowStockProducts = 1,
                OutOfStockProducts = 0
            }
        };
    }

    private static IEnumerable<CategoryTrendDto> CreateSampleCategoryTrends()
    {
        return new List<CategoryTrendDto>
        {
            new()
            {
                CategoryId = 1,
                CategoryName = "Electronics",
                TrendScore = 85.5m,
                TrendRank = 1,
                ProductGrowthRate = 15.5m,
                InventoryTurnover = 25.0m
            },
            new()
            {
                CategoryId = 2,
                CategoryName = "Books",
                TrendScore = 72.3m,
                TrendRank = 2,
                ProductGrowthRate = 8.2m,
                InventoryTurnover = 18.5m
            }
        };
    }

    private static IEnumerable<CategoryInventoryDistributionDto> CreateSampleInventoryDistribution()
    {
        return new List<CategoryInventoryDistributionDto>
        {
            new()
            {
                CategoryId = 1,
                CategoryName = "Electronics",
                ProductPercentage = 30.0m,
                ValuePercentage = 60.0m,
                StockPercentage = 35.0m
            },
            new()
            {
                CategoryId = 2,
                CategoryName = "Books",
                ProductPercentage = 50.0m,
                ValuePercentage = 15.0m,
                StockPercentage = 65.0m
            }
        };
    }

    #endregion
}
