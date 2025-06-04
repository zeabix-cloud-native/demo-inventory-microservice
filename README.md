# Demo Inventory Microservice

A .NET 8 Clean Architecture implementation for an inventory management microservice with a React frontend.

## Architecture

This project follows Clean Architecture principles with the following layers:

### Domain Layer (`DemoInventory.Domain`)
- **Entities**: Core business entities (e.g., `Product`)
- **Interfaces**: Repository contracts and domain services
- **Value Objects**: Immutable objects representing concepts
- **Enums**: Domain-specific enumerations
- **Events**: Domain events

### Application Layer (`DemoInventory.Application`)
- **Services**: Application business logic
- **DTOs**: Data Transfer Objects for API communication
- **Interfaces**: Service contracts
- **Use Cases**: Specific business operations
- **Common**: Shared application utilities

### Infrastructure Layer (`DemoInventory.Infrastructure`)
- **Repositories**: Data access implementations
- **Data**: Database context and configurations
- **Services**: External service implementations

### Presentation Layer (`DemoInventory.API`)
- **Controllers**: Web API endpoints
- **Program.cs**: Application configuration and dependency injection

### Frontend Layer (`DemoInventory.Frontend`)
- **React**: Component-based UI library
- **TypeScript**: Type-safe JavaScript
- **Vite**: Fast build tool and development server
- **Tailwind CSS**: Utility-first CSS framework

## Project Structure

```
src/
├── DemoInventory.Domain/           # Core business logic
├── DemoInventory.Application/      # Use cases and application services
├── DemoInventory.Infrastructure/   # Data access and external services
├── DemoInventory.API/             # Web API controllers and configuration
└── DemoInventory.Frontend/        # React frontend application

tests/
├── DemoInventory.Domain.Tests/     # Domain layer unit tests
└── DemoInventory.Application.Tests/ # Application layer unit tests
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+ and npm
- Visual Studio 2022 or VS Code

### Building the Backend Solution
```bash
dotnet build
```

### Running Backend Tests
```bash
dotnet test
```

### Running the API
```bash
dotnet run --project src/DemoInventory.API
```

The API will be available at `https://localhost:5001` or `http://localhost:5000` with Swagger documentation at `/swagger`.

### Setting up the Frontend

Navigate to the frontend directory:
```bash
cd src/DemoInventory.Frontend
```

Install dependencies:
```bash
npm install
```

Start the development server:
```bash
npm run dev
```

The frontend will be available at `http://localhost:5173/`.

### Building the Frontend for Production
```bash
cd src/DemoInventory.Frontend
npm run build
```

### Frontend Scripts
- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run lint` - Type check with TypeScript
- `npm run preview` - Preview production build

## API Endpoints

### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/sku/{sku}` - Get product by SKU
- `GET /api/products/search?searchTerm={term}` - Search products by name
- `POST /api/products` - Create a new product
- `PUT /api/products/{id}` - Update a product
- `DELETE /api/products/{id}` - Delete a product

## Features

- Clean Architecture implementation
- In-memory repository (easily replaceable with database)
- RESTful API design
- Swagger/OpenAPI documentation
- Unit tests with mocking
- Dependency injection
- CRUD operations for products

## Technologies Used

### Backend
- .NET 8
- ASP.NET Core Web API
- xUnit for testing
- Moq for mocking
- Swagger/OpenAPI

### Frontend
- React 19
- TypeScript 5
- Vite 6
- Tailwind CSS 4
- PostCSS

## Future Enhancements

- Entity Framework Core integration
- Authentication and authorization
- Logging and monitoring
- Docker containerization
- Integration tests
- API versioning
- Frontend integration with backend API
- State management (Redux/Zustand)
- Unit tests for frontend components