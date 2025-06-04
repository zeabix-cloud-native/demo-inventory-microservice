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

### Frontend Layer (`src/frontend`)
- **Components**: React components for UI
- **Services**: API communication layer
- **Types**: TypeScript type definitions
- **Styles**: CSS styling for components

## Project Structure

```
src/
├── DemoInventory.Domain/           # Core business logic
├── DemoInventory.Application/      # Use cases and application services
├── DemoInventory.Infrastructure/   # Data access and external services
├── DemoInventory.API/             # Web API controllers and configuration
└── frontend/                      # React frontend application
    ├── src/
    │   ├── components/            # React components
    │   ├── services/              # API service layer
    │   ├── types/                 # TypeScript type definitions
    │   └── ...
    └── ...

tests/
├── DemoInventory.Domain.Tests/     # Domain layer unit tests
└── DemoInventory.Application.Tests/ # Application layer unit tests
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+ and npm
- Visual Studio 2022 or VS Code

### Building the Solution
```bash
# Build the .NET solution
dotnet build

# Install frontend dependencies
cd src/frontend
npm install
```

### Running Tests
```bash
dotnet test
```

### Running the Application

#### Start the API
```bash
dotnet run --project src/DemoInventory.API
```

The API will be available at `http://localhost:5126` with Swagger documentation at `/swagger`.

#### Start the Frontend (in a separate terminal)
```bash
cd src/frontend
npm run dev
```

The frontend will be available at `http://localhost:5173`.

### Building for Production

#### Build the API
```bash
dotnet publish src/DemoInventory.API -c Release -o ./publish
```

#### Build the Frontend
```bash
cd src/frontend
npm run build
```

The built frontend files will be in `src/frontend/dist/`.

## API Endpoints

### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/sku/{sku}` - Get product by SKU
- `GET /api/products/search?searchTerm={term}` - Search products by name
- `POST /api/products` - Create a new product
- `PUT /api/products/{id}` - Update a product
- `DELETE /api/products/{id}` - Delete a product

## Frontend Features

### Product List Page
- Display all products in a responsive table
- Search functionality by product name
- View product details including stock levels
- Edit and delete product actions
- Navigation to create new products
- Low stock warning indicators

### Product Form Page
- Create new products with validation
- Edit existing products (SKU field disabled for edits)
- Form validation with error messages
- Responsive design for mobile devices
- Cancel functionality with navigation back to list

## Features

- Clean Architecture implementation
- In-memory repository (easily replaceable with database)
- RESTful API design
- Swagger/OpenAPI documentation
- Unit tests with mocking
- Dependency injection
- CRUD operations for products
- React frontend with TypeScript
- Responsive web design
- CORS enabled for frontend communication

## Technologies Used

### Backend
- .NET 8
- ASP.NET Core Web API
- xUnit for testing
- Moq for mocking
- Swagger/OpenAPI

### Frontend
- React 18
- TypeScript
- Vite (build tool)
- Axios (HTTP client)
- React Router (navigation)
- CSS3 (responsive styling)

## Future Enhancements

- Entity Framework Core integration
- Authentication and authorization
- Logging and monitoring
- Docker containerization
- Integration tests
- API versioning
- State management (Redux/Zustand)
- End-to-end testing
- Progressive Web App features