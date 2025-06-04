# Demo Inventory Microservice

A .NET 8 Clean Architecture implementation for an inventory management microservice.

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

## Project Structure

```
src/
├── DemoInventory.Domain/           # Core business logic
├── DemoInventory.Application/      # Use cases and application services
├── DemoInventory.Infrastructure/   # Data access and external services
└── DemoInventory.API/             # Web API controllers and configuration

frontend/                          # Frontend demo application
├── index.html                     # Main HTML page
├── app.js                        # JavaScript with Axios integration
└── README.md                     # Frontend setup instructions

tests/
├── DemoInventory.Domain.Tests/     # Domain layer unit tests
└── DemoInventory.Application.Tests/ # Application layer unit tests
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code

### Building the Solution
```bash
dotnet build
```

### Running Tests
```bash
dotnet test
```

### Running the API
```bash
dotnet run --project src/DemoInventory.API
```

The API will be available at `http://localhost:[port]` with Swagger documentation at `/swagger`.

### Running the Frontend Demo
```bash
# Navigate to the frontend directory
cd frontend

# Open index.html in your browser, or serve it with a simple HTTP server:
python -m http.server 8080
# Then open http://localhost:8080 in your browser
```

See `frontend/README.md` for detailed frontend setup instructions.

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

### Backend
- Clean Architecture implementation
- In-memory repository (easily replaceable with database)
- RESTful API design
- Swagger/OpenAPI documentation
- Unit tests with mocking
- Dependency injection
- CRUD operations for products
- CORS enabled for frontend connections

### Frontend
- HTML/JavaScript frontend with Axios integration
- Complete product management UI (Create, Read, Update, Delete)
- Product search functionality
- Real-time API communication
- Responsive design
- Error handling and user feedback

## Technologies Used

### Backend
- .NET 8
- ASP.NET Core Web API
- xUnit for testing
- Moq for mocking
- Swagger/OpenAPI

### Frontend
- HTML5
- Vanilla JavaScript (ES6+)
- Axios for HTTP requests
- CSS3 for styling

## Future Enhancements

- Entity Framework Core integration
- Authentication and authorization
- Logging and monitoring
- Docker containerization
- Integration tests
- API versioning