using DemoInventory.Application.DTOs;
using DemoInventory.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DemoInventory.API.Controllers;

/// <summary>
/// Controller for category analytics operations
/// </summary>
[ApiController]
[Route("api/categories/analytics")]
[SwaggerTag("Category analytics and reporting operations")]
public class CategoryAnalyticsController : ControllerBase
{
    private readonly ICategoryAnalyticsService _categoryAnalyticsService;
    private readonly ILogger<CategoryAnalyticsController> _logger;

    public CategoryAnalyticsController(
        ICategoryAnalyticsService categoryAnalyticsService,
        ILogger<CategoryAnalyticsController> logger)
    {
        _categoryAnalyticsService = categoryAnalyticsService ?? throw new ArgumentNullException(nameof(categoryAnalyticsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get comprehensive category metrics and analytics
    /// </summary>
    /// <param name="startDate">Optional start date for filtering products</param>
    /// <param name="endDate">Optional end date for filtering products</param>
    /// <param name="includeOnlyActiveCategories">Include only categories with products</param>
    /// <param name="lowStockThreshold">Threshold for low stock analysis</param>
    /// <returns>Complete category metrics and analytics data</returns>
    /// <response code="200">Returns category metrics and analytics</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get category metrics", 
        Description = "Retrieves comprehensive analytics and metrics for all categories")]
    [SwaggerResponse(200, "Success", typeof(CategoryMetricsDto))]
    [SwaggerResponse(400, "Bad request")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<ActionResult<CategoryMetricsDto>> GetCategoryMetrics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] bool includeOnlyActiveCategories = true,
        [FromQuery] int lowStockThreshold = 10)
    {
        _logger.LogInformation("Getting category metrics with filters: StartDate={StartDate}, EndDate={EndDate}, ActiveOnly={ActiveOnly}", 
            startDate, endDate, includeOnlyActiveCategories);

        try
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest("Start date cannot be greater than end date");
            }

            if (lowStockThreshold < 0)
            {
                return BadRequest("Low stock threshold must be non-negative");
            }

            var dateRange = new AnalyticsDateRangeDto
            {
                StartDate = startDate,
                EndDate = endDate,
                IncludeOnlyActiveCategories = includeOnlyActiveCategories,
                LowStockThreshold = lowStockThreshold
            };

            var metrics = await _categoryAnalyticsService.GetCategoryMetricsAsync(dateRange);
            
            _logger.LogInformation("Retrieved metrics for {CategoryCount} categories", metrics.TotalCategories);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category metrics");
            return StatusCode(500, "An error occurred while retrieving category metrics");
        }
    }

    /// <summary>
    /// Get analytics for a specific category
    /// </summary>
    /// <param name="categoryId">The category ID to analyze</param>
    /// <param name="startDate">Optional start date for filtering products</param>
    /// <param name="endDate">Optional end date for filtering products</param>
    /// <returns>Analytics data for the specified category</returns>
    /// <response code="200">Returns category analytics</response>
    /// <response code="404">Category not found</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{categoryId}")]
    [SwaggerOperation(
        Summary = "Get category analytics by ID", 
        Description = "Retrieves analytics data for a specific category")]
    [SwaggerResponse(200, "Success", typeof(CategoryAnalyticsDto))]
    [SwaggerResponse(404, "Category not found")]
    [SwaggerResponse(400, "Bad request")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<ActionResult<CategoryAnalyticsDto>> GetCategoryAnalyticsById(
        int categoryId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        _logger.LogInformation("Getting analytics for category {CategoryId}", categoryId);

        try
        {
            if (categoryId <= 0)
            {
                return BadRequest("Category ID must be greater than 0");
            }

            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest("Start date cannot be greater than end date");
            }

            var dateRange = new AnalyticsDateRangeDto
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var analytics = await _categoryAnalyticsService.GetCategoryAnalyticsByIdAsync(categoryId, dateRange);
            
            if (analytics == null)
            {
                _logger.LogWarning("Category {CategoryId} not found", categoryId);
                return NotFound($"Category with ID {categoryId} not found");
            }

            _logger.LogInformation("Retrieved analytics for category {CategoryId}: {ProductCount} products", 
                categoryId, analytics.TotalProducts);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics for category {CategoryId}", categoryId);
            return StatusCode(500, "An error occurred while retrieving category analytics");
        }
    }

    /// <summary>
    /// Get top performing categories by various metrics
    /// </summary>
    /// <param name="count">Number of top categories to return (max 50)</param>
    /// <param name="sortBy">Sort by: ProductCount, InventoryValue, AveragePrice</param>
    /// <returns>Top categories by the specified metric</returns>
    /// <response code="200">Returns top categories</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("top")]
    [SwaggerOperation(
        Summary = "Get top categories", 
        Description = "Retrieves top performing categories by various metrics")]
    [SwaggerResponse(200, "Success", typeof(IEnumerable<CategoryAnalyticsDto>))]
    [SwaggerResponse(400, "Bad request")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<ActionResult<IEnumerable<CategoryAnalyticsDto>>> GetTopCategories(
        [FromQuery] int count = 10,
        [FromQuery] string sortBy = "ProductCount")
    {
        _logger.LogInformation("Getting top {Count} categories sorted by {SortBy}", count, sortBy);

        try
        {
            if (count <= 0 || count > 50)
            {
                return BadRequest("Count must be between 1 and 50");
            }

            var validSortOptions = new[] { "ProductCount", "InventoryValue", "AveragePrice" };
            if (!validSortOptions.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest($"Invalid sortBy parameter. Valid options: {string.Join(", ", validSortOptions)}");
            }

            var topCategories = await _categoryAnalyticsService.GetTopCategoriesAsync(count, sortBy);
            
            _logger.LogInformation("Retrieved {ResultCount} top categories", topCategories.Count());
            return Ok(topCategories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top categories");
            return StatusCode(500, "An error occurred while retrieving top categories");
        }
    }

    /// <summary>
    /// Get trending categories based on recent activity
    /// </summary>
    /// <param name="daysPeriod">Number of days to analyze for trends (7-365)</param>
    /// <param name="count">Number of trending categories to return (max 20)</param>
    /// <returns>Trending category data</returns>
    /// <response code="200">Returns trending categories</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("trends")]
    [SwaggerOperation(
        Summary = "Get category trends", 
        Description = "Retrieves trending categories based on recent activity")]
    [SwaggerResponse(200, "Success", typeof(IEnumerable<CategoryTrendDto>))]
    [SwaggerResponse(400, "Bad request")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<ActionResult<IEnumerable<CategoryTrendDto>>> GetCategoryTrends(
        [FromQuery] int daysPeriod = 30,
        [FromQuery] int count = 10)
    {
        _logger.LogInformation("Getting category trends for {DaysPeriod} days, top {Count}", daysPeriod, count);

        try
        {
            if (daysPeriod < 7 || daysPeriod > 365)
            {
                return BadRequest("Days period must be between 7 and 365");
            }

            if (count <= 0 || count > 20)
            {
                return BadRequest("Count must be between 1 and 20");
            }

            var trends = await _categoryAnalyticsService.GetCategoryTrendsAsync(daysPeriod, count);
            
            _logger.LogInformation("Retrieved {TrendCount} category trends", trends.Count());
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category trends");
            return StatusCode(500, "An error occurred while retrieving category trends");
        }
    }

    /// <summary>
    /// Get inventory distribution across categories
    /// </summary>
    /// <returns>Category inventory distribution data</returns>
    /// <response code="200">Returns inventory distribution</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("distribution")]
    [SwaggerOperation(
        Summary = "Get inventory distribution", 
        Description = "Retrieves inventory distribution across categories")]
    [SwaggerResponse(200, "Success", typeof(IEnumerable<CategoryInventoryDistributionDto>))]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<ActionResult<IEnumerable<CategoryInventoryDistributionDto>>> GetInventoryDistribution()
    {
        _logger.LogInformation("Getting category inventory distribution");

        try
        {
            var distribution = await _categoryAnalyticsService.GetCategoryInventoryDistributionAsync();
            
            _logger.LogInformation("Retrieved distribution for {CategoryCount} categories", distribution.Count());
            return Ok(distribution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory distribution");
            return StatusCode(500, "An error occurred while retrieving inventory distribution");
        }
    }

    /// <summary>
    /// Get categories with stock issues (low stock or out of stock)
    /// </summary>
    /// <param name="lowStockThreshold">Threshold for considering stock as low (1-100)</param>
    /// <returns>Categories with stock issues</returns>
    /// <response code="200">Returns categories with stock issues</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("stock-issues")]
    [SwaggerOperation(
        Summary = "Get categories with stock issues", 
        Description = "Retrieves categories with low stock or out of stock products")]
    [SwaggerResponse(200, "Success", typeof(IEnumerable<CategoryAnalyticsDto>))]
    [SwaggerResponse(400, "Bad request")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<ActionResult<IEnumerable<CategoryAnalyticsDto>>> GetCategoriesWithStockIssues(
        [FromQuery] int lowStockThreshold = 10)
    {
        _logger.LogInformation("Getting categories with stock issues (threshold: {Threshold})", lowStockThreshold);

        try
        {
            if (lowStockThreshold < 1 || lowStockThreshold > 100)
            {
                return BadRequest("Low stock threshold must be between 1 and 100");
            }

            var categoriesWithIssues = await _categoryAnalyticsService.GetCategoriesWithStockIssuesAsync(lowStockThreshold);
            
            _logger.LogInformation("Found {CategoryCount} categories with stock issues", categoriesWithIssues.Count());
            return Ok(categoriesWithIssues);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories with stock issues");
            return StatusCode(500, "An error occurred while retrieving categories with stock issues");
        }
    }

    /// <summary>
    /// Get analytics summary for dashboard display
    /// </summary>
    /// <returns>Key metrics for dashboard</returns>
    /// <response code="200">Returns analytics summary</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("summary")]
    [SwaggerOperation(
        Summary = "Get analytics summary", 
        Description = "Retrieves key analytics metrics for dashboard display")]
    [SwaggerResponse(200, "Success")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<ActionResult<object>> GetAnalyticsSummary()
    {
        _logger.LogInformation("Getting analytics summary");

        try
        {
            var summary = await _categoryAnalyticsService.GetAnalyticsSummaryAsync();
            
            _logger.LogInformation("Retrieved analytics summary");
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics summary");
            return StatusCode(500, "An error occurred while retrieving analytics summary");
        }
    }
}
