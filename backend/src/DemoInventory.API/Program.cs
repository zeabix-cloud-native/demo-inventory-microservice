using DemoInventory.Application.Interfaces;
using DemoInventory.Application.Services;
using DemoInventory.Domain.Interfaces;
using DemoInventory.Infrastructure.Data;
using DemoInventory.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {

        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:5126", "http://localhost:8080", "http://host.docker.internal:8080", "file://") // Vite default port, CRA port, API port, Swagger UI port, Docker internal Swagger UI, and file protocol for local HTML
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add DbContext
var useInMemoryDb = builder.Configuration.GetValue<bool>("USE_IN_MEMORY_DB", false);
if (useInMemoryDb)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("DemoInventoryDb"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Register dependencies
builder.Services.AddScoped<IProductRepository, PostgreSqlProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Demo Inventory Microservice API",
        Version = "v1",
        Description = "A .NET 8 Clean Architecture implementation for an inventory management microservice",
        Contact = new OpenApiContact
        {
            Name = "Zeabix Cloud Native Team",
            Email = "support@zeabix.com",
            Url = new Uri("https://github.com/zeabix-cloud-native")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Set the comments path for the Swagger JSON and UI
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    
    // Include XML comments from Application layer for DTOs
    var applicationXmlFile = "DemoInventory.Application.xml";
    var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFile);
    if (File.Exists(applicationXmlPath))
    {
        c.IncludeXmlComments(applicationXmlPath);
    }


    // Enable annotations for better documentation
    c.EnableAnnotations();

    
    // Configure schema IDs to avoid conflicts
    c.CustomSchemaIds(type => type.FullName);
});

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Could not initialize database: {Message}", ex.Message);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Make Swagger available in all environments for OpenAPI spec export
app.UseSwagger();

// Add endpoint to export OpenAPI spec
app.MapGet("/openapi.json", (IServiceProvider serviceProvider) =>
{
    var swaggerProvider = serviceProvider.GetRequiredService<ISwaggerProvider>();
    var swagger = swaggerProvider.GetSwagger("v1");
    return Results.Json(swagger);
})
.WithName("ExportOpenApiSpec")
.WithTags("OpenAPI")
.WithSummary("Export OpenAPI specification")
.WithDescription("Exports the OpenAPI specification as JSON");

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowReactApp");

app.MapControllers();

app.Run();
