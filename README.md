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
├── DemoInventory.Application.Tests/ # Application layer unit tests
└── e2e/                           # End-to-end tests with Cypress
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+ and npm
- Visual Studio 2022 or VS Code
- Node.js (v18 or higher) - for E2E tests

### Building the Backend Solution
```bash
# Build the .NET solution
dotnet build

# Install frontend dependencies
cd src/frontend
npm install
```

### Running Backend Tests

```bash
dotnet test
```

#### End-to-End Tests
```bash
# Start the API first
dotnet run --project src/DemoInventory.API

# In another terminal, run E2E tests
cd tests/e2e
npm install
npm run test:e2e
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


### Alternative Demo Frontend

For a simple demonstration of Axios integration, there's also an HTML/JS demo:

#### Running the HTML Demo Frontend
```bash
# Navigate to the demo frontend directory
cd frontend-html-demo

# Open index.html in your browser, or serve it with a simple HTTP server:
python -m http.server 8080
# Then open http://localhost:8080 in your browser
```
See `frontend-html-demo/README.md` for detailed demo frontend setup instructions.

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
- End-to-end tests with Cypress
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
- Cypress for E2E testing
- Swagger/OpenAPI
- Docker containerization

## Docker Support

The application includes Docker containerization with:

- **Dockerfile** - Multi-stage build for optimized production image
- **docker-compose.yml** - Basic backend service composition
- **docker-compose.full.yml** - Backend + standalone Swagger UI frontend

### Quick Docker Start

```bash
# Build and run with docker-compose
docker compose up -d

# Access the API
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

See [Docker.README.md](Docker.README.md) for detailed Docker usage instructions.

### Frontend
- React 18
- TypeScript
- Vite (build tool)
- Axios (HTTP client)
- React Router (navigation)
- CSS3 (responsive styling)


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
- API versioning
- Integration tests
- State management (Redux/Zustand)
- End-to-end testing
- Progressive Web App features
