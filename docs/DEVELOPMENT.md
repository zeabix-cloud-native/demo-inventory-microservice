# Development Guide

## Prerequisites

### Required Software

- **[.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)** - Backend development
- **[Node.js 18+](https://nodejs.org/)** - Frontend development and testing tools
- **[PostgreSQL 13+](https://www.postgresql.org/)** - Database (or use Docker)
- **[Docker Desktop](https://www.docker.com/products/docker-desktop)** - Containerization (optional)

### Recommended Development Tools

- **[Visual Studio 2022](https://visualstudio.microsoft.com/vs/)** or **[VS Code](https://code.visualstudio.com/)** - IDE
- **[Postman](https://www.postman.com/)** - API testing
- **[Git](https://git-scm.com/)** - Version control

### VS Code Extensions (Recommended)

- C# DevKit
- Thunder Client (API testing)
- ES7+ React/Redux/React-Native snippets
- Prettier - Code formatter
- Auto Rename Tag

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/zeabix-cloud-native/demo-inventory-microservice.git
cd demo-inventory-microservice
```

### 2. Backend Setup

#### Restore Dependencies

```bash
dotnet restore
```

#### Build the Solution

```bash
dotnet build
```

#### Run Unit Tests

```bash
dotnet test
```

#### Start the API

```bash
dotnet run --project backend/src/DemoInventory.API
```

The API will be available at:
- **API Base URL**: `http://localhost:5126`
- **Swagger Documentation**: `http://localhost:5126/swagger`

### 3. Frontend Setup

#### Install Dependencies

```bash
cd frontend
npm install
```

#### Configure Environment

```bash
# Copy environment template
cp .env.example .env

# Edit .env if needed (default API URL: http://localhost:5126/api)
```

#### Start Development Server

```bash
npm run dev
```

The frontend will be available at:
- **Frontend URL**: `http://localhost:5173`

### 4. Database Setup

#### Option A: Using Docker (Recommended)

```bash
# Start PostgreSQL container
docker run --name demo-inventory-db -e POSTGRES_PASSWORD=password -e POSTGRES_DB=demo_inventory -p 5432:5432 -d postgres:13

# Or use docker-compose
docker-compose up -d db
```

#### Option B: Local PostgreSQL Installation

1. Install PostgreSQL
2. Create database: `demo_inventory`
3. Update connection string in `appsettings.json`

## Development Workflow

### 1. Code Organization

```
backend/
├── src/
│   ├── DemoInventory.Domain/           # Business logic and entities
│   ├── DemoInventory.Application/      # Use cases and services
│   ├── DemoInventory.Infrastructure/   # Data access and external services
│   └── DemoInventory.API/             # Controllers and configuration
└── tests/                             # Unit and integration tests

frontend/
├── src/
│   ├── components/                    # React components
│   ├── services/                      # API communication
│   ├── types/                         # TypeScript type definitions
│   └── ...
└── ...

tests/
├── postman/                           # API test collections
└── e2e/                              # End-to-end tests
```

### 2. Backend Development

#### Adding New Features

1. **Start with Domain Layer**: Define entities and business rules
2. **Application Layer**: Create services and DTOs
3. **Infrastructure Layer**: Implement repositories and data access
4. **API Layer**: Add controllers and configure endpoints

#### Code Standards

- **Naming**: Use PascalCase for public members, camelCase for private
- **Architecture**: Follow Clean Architecture principles
- **Testing**: Write unit tests for business logic
- **Documentation**: Add XML documentation for public APIs

#### Example: Adding New Entity

```csharp
// 1. Domain Layer - Entity
public class Category
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    
    public Category(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}

// 2. Application Layer - DTO
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// 3. Application Layer - Service
public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
}

// 4. API Layer - Controller
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    
    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }
}
```

### 3. Frontend Development

#### Project Structure

```typescript
// types/Product.ts - Type definitions
export interface Product {
  id: number;
  name: string;
  sku: string;
  price: number;
  stockQuantity: number;
}

// services/productService.ts - API communication
export const productService = {
  async getProducts(): Promise<Product[]> {
    const response = await axios.get<Product[]>('/api/products');
    return response.data;
  }
};

// components/ProductList.tsx - React component
export const ProductList: React.FC = () => {
  const [products, setProducts] = useState<Product[]>([]);
  
  useEffect(() => {
    productService.getProducts()
      .then(setProducts)
      .catch(console.error);
  }, []);
  
  return (
    <div>
      {products.map(product => (
        <ProductCard key={product.id} product={product} />
      ))}
    </div>
  );
};
```

#### Code Standards

- **TypeScript**: Use strict typing, no `any` types
- **Components**: Functional components with hooks
- **State Management**: useState/useEffect for local state
- **Styling**: CSS modules or styled-components
- **Error Handling**: Proper error boundaries and user feedback

### 4. Database Development

#### Entity Framework Commands

```bash
# Add migration
dotnet ef migrations add MigrationName --project backend/src/DemoInventory.Infrastructure

# Update database
dotnet ef database update --project backend/src/DemoInventory.API

# Drop database (development only)
dotnet ef database drop --project backend/src/DemoInventory.API
```

#### Seeding Data

```csharp
// Infrastructure/Data/Seed/DataSeeder.cs
public static class DataSeeder
{
    public static async Task SeedAsync(InventoryDbContext context)
    {
        if (!context.Products.Any())
        {
            var products = new[]
            {
                new Product("Laptop", "LAP001", 999.99m, 10),
                new Product("Mouse", "MOU001", 29.99m, 50)
            };
            
            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }
    }
}
```

## Testing

### Unit Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test backend/tests/DemoInventory.Domain.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### API Testing with Postman

```bash
# Install Newman (Postman CLI)
npm install -g newman

# Run Postman tests
newman run tests/postman/collection.json --environment tests/postman/environment.json
```

### End-to-End Testing

```bash
# Install Cypress dependencies
cd tests/e2e
npm install

# Run Cypress tests (headless)
npm run test:e2e

# Open Cypress GUI
npm run cypress:open
```

## Debugging

### Backend Debugging

#### Visual Studio / VS Code

1. Set breakpoints in your code
2. Press F5 or select "Debug" → "Start Debugging"
3. Use the Debug Console for expressions

#### Command Line Debugging

```bash
# Enable detailed logging
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project backend/src/DemoInventory.API --verbosity detailed
```

### Frontend Debugging

#### Browser DevTools

1. Open Chrome DevTools (F12)
2. Use Console, Network, and Sources tabs
3. Set breakpoints in TypeScript source files

#### VS Code Debugging

1. Install "Debugger for Chrome" extension
2. Use launch configuration:

```json
{
  "type": "chrome",
  "request": "launch",
  "name": "Debug React App",
  "url": "http://localhost:5173",
  "webRoot": "${workspaceFolder}/frontend/src"
}
```

## Docker Development

### Full Stack with Docker

```bash
# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Rebuild specific service
docker-compose build api
docker-compose up -d api

# Stop all services
docker-compose down
```

### Development with Docker

```bash
# Start only database
docker-compose up -d db

# Run API locally, frontend in Docker
docker-compose up -d frontend
dotnet run --project backend/src/DemoInventory.API
```

## Performance Optimization

### Backend Performance

- **Database Queries**: Use EF Core query optimization
- **Async/Await**: Proper async patterns for I/O operations
- **Caching**: Implement response caching where appropriate
- **Connection Pooling**: Configure database connection pooling

### Frontend Performance

- **Code Splitting**: Use React.lazy for route-based splitting
- **Memoization**: Use React.memo and useMemo for expensive operations
- **Bundle Analysis**: Analyze bundle size with `npm run build --analyze`
- **Image Optimization**: Optimize images and use proper formats

## Common Development Tasks

### Adding New API Endpoint

1. Define DTO in Application layer
2. Add method to service interface
3. Implement service method
4. Add controller action
5. Update Swagger documentation
6. Write unit tests
7. Add Postman test case

### Adding New React Component

1. Create component file in appropriate directory
2. Define TypeScript interfaces
3. Implement component with proper props typing
4. Add styling (CSS module or styled-components)
5. Export component from index file
6. Add to route or parent component

### Database Schema Changes

1. Modify entity in Domain layer
2. Add migration: `dotnet ef migrations add MigrationName`
3. Review generated migration
4. Update database: `dotnet ef database update`
5. Update seed data if necessary

## Troubleshooting

### Common Issues

#### Backend Issues

- **Port conflicts**: Check if port 5126 is available
- **Database connection**: Verify PostgreSQL is running and connection string is correct
- **Missing dependencies**: Run `dotnet restore`
- **Build errors**: Check .NET version compatibility

#### Frontend Issues

- **Node version**: Ensure Node.js 18+ is installed
- **Dependency conflicts**: Delete `node_modules` and run `npm install`
- **CORS errors**: Verify API CORS configuration
- **Environment variables**: Check `.env` file configuration

#### Docker Issues

- **Container not starting**: Check Docker daemon is running
- **Port conflicts**: Ensure ports are not already in use
- **Volume mount issues**: Check file permissions and paths

### Getting Help

1. **Check Logs**: Review application logs for error details
2. **Stack Overflow**: Search for similar issues
3. **GitHub Issues**: Check project issues for known problems
4. **Documentation**: Review Microsoft docs for .NET and React docs
5. **Community**: Ask questions in relevant Discord/Slack channels