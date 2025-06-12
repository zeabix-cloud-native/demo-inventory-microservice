# Demo Inventory Microservice

A modern, full-stack inventory management microservice demonstrating Clean Architecture principles with .NET 9 backend and React frontend. Features comprehensive testing, Docker containerization, and CI/CD pipeline integration.

[![CI Pipeline](https://github.com/zeabix-cloud-native/demo-inventory-microservice/actions/workflows/ci.yml/badge.svg)](https://github.com/zeabix-cloud-native/demo-inventory-microservice/actions/workflows/ci.yml)

## ✨ Features

- **Clean Architecture**: Well-structured, maintainable codebase following SOLID principles
- **Full-Stack Application**: .NET 9 Web API with React TypeScript frontend
- **Comprehensive Testing**: Unit, integration, API, and E2E tests with CTRF reporting
- **Docker Ready**: Complete containerization with Docker Compose
- **Production Ready**: CI/CD pipeline, health checks, and monitoring capabilities
- **Developer Friendly**: Hot reload, comprehensive documentation, and development tools

## 🚀 Quick Start

### Using Docker (Recommended)

```bash
# Clone the repository
git clone https://github.com/zeabix-cloud-native/demo-inventory-microservice.git
cd demo-inventory-microservice

# Start the complete stack
docker-compose up -d

# Access the application
# - API: http://localhost:5000
# - Swagger UI: http://localhost:5000/swagger  
# - Frontend: http://localhost:3000
# - Database: localhost:5432
```

### Local Development

**Prerequisites**: .NET 9 SDK, Node.js 20+, PostgreSQL

```bash
# Backend setup
dotnet restore
dotnet run --project backend/src/DemoInventory.API

# Frontend setup (in new terminal)
cd frontend
npm install
npm run dev

# Access: API at http://localhost:5126, Frontend at http://localhost:5173
```

## 📋 Table of Contents

- [Architecture Overview](#architecture-overview)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [API Documentation](#api-documentation)
- [Testing](#testing)
- [Docker & Deployment](#docker--deployment)
- [Contributing](#contributing)
- [Documentation](#documentation)

## 🏗️ Architecture Overview

This project implements **Clean Architecture** with clear separation of concerns:

- **Domain Layer**: Core business logic and entities
- **Application Layer**: Use cases and business workflows  
- **Infrastructure Layer**: Data access and external integrations
- **Presentation Layer**: API controllers and user interfaces

```
┌─────────────────────────────────────────────────────────────┐
│                   Presentation Layer                        │
│  ┌─────────────────────┐  ┌─────────────────────────────────┐│
│  │   React Frontend    │  │      Web API Controllers       ││
│  │   (TypeScript)      │  │        (.NET 9)               ││
│  └─────────────────────┘  └─────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
│           Services, DTOs, Use Cases                         │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                            │
│        Entities, Value Objects, Business Rules              │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                        │
│       Data Access, External Services, Persistence           │
└─────────────────────────────────────────────────────────────┘
```

## 🛠️ Technology Stack

### Backend
- **.NET 9** - Runtime and framework
- **ASP.NET Core** - Web API framework  
- **Entity Framework Core** - ORM and data access
- **PostgreSQL** - Primary database
- **Swagger/OpenAPI** - API documentation

### Frontend  
- **React 19** - UI library
- **TypeScript** - Type-safe JavaScript
- **Vite** - Build tool and dev server
- **Axios** - HTTP client

### Testing & Quality
- **xUnit** - Unit testing framework
- **Postman/Newman** - API testing  
- **Cypress** - End-to-end testing
- **CTRF** - Unified test reporting

### DevOps
- **Docker** - Containerization
- **GitHub Actions** - CI/CD pipeline
- **Docker Compose** - Multi-container orchestration

## 📁 Project Structure

```
├── backend/
│   ├── src/
│   │   ├── DemoInventory.Domain/           # Business logic & entities
│   │   ├── DemoInventory.Application/      # Use cases & services
│   │   ├── DemoInventory.Infrastructure/   # Data access & external services
│   │   └── DemoInventory.API/             # Web API controllers
│   └── tests/                             # Unit & integration tests
├── frontend/                              # React TypeScript application
├── tests/
│   ├── postman/                          # API test collections
│   └── e2e/                              # Cypress end-to-end tests
├── docs/                                 # Comprehensive documentation
├── docker-compose.yml                    # Multi-container setup
└── README.md
```

## 🚀 Getting Started

### Prerequisites

- **[.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)** (for local development)
- **[Node.js 20+](https://nodejs.org/)** and npm  
- **[Docker Desktop](https://www.docker.com/products/docker-desktop)** (recommended)
- **[PostgreSQL 13+](https://www.postgresql.org/)** (if not using Docker)

### Option 1: Docker Compose (Recommended)

```bash
# Clone the repository
git clone https://github.com/zeabix-cloud-native/demo-inventory-microservice.git
cd demo-inventory-microservice

# Start all services
docker-compose up -d

# Verify services are running
docker-compose ps

# View logs
docker-compose logs -f
```

**Services Available:**
- **Backend API**: http://localhost:5000
- **Swagger Documentation**: http://localhost:5000/swagger
- **React Frontend**: http://localhost:3000  
- **PostgreSQL Database**: localhost:5432

### Option 2: Local Development

#### Backend Setup

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Start the API
dotnet run --project backend/src/DemoInventory.API
```

The API will be available at `http://localhost:5126` with Swagger at `/swagger`.

#### Frontend Setup

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Configure environment (optional)
cp .env.example .env

# Start development server
npm run dev
```

The frontend will be available at `http://localhost:5173`.

#### Database Setup

**Using Docker:**
```bash
docker run --name demo-inventory-db \
  -e POSTGRES_PASSWORD=password \
  -e POSTGRES_DB=demo_inventory \
  -p 5432:5432 -d postgres:15
```

**Local PostgreSQL:**
1. Install PostgreSQL
2. Create database: `demo_inventory`
3. Update connection string in `appsettings.json`

## 📚 API Documentation

### Core Endpoints

The API provides RESTful endpoints for product management:

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/products` | Get all products |
| `GET` | `/api/products/{id}` | Get product by ID |
| `GET` | `/api/products/sku/{sku}` | Get product by SKU |
| `GET` | `/api/products/search?searchTerm={term}` | Search products |
| `POST` | `/api/products` | Create new product |
| `PUT` | `/api/products/{id}` | Update product |
| `DELETE` | `/api/products/{id}` | Delete product |

### Interactive Documentation

- **Swagger UI**: http://localhost:5126/swagger (local) or http://localhost:5000/swagger (Docker)
- **OpenAPI Specification**: Available at `/swagger/v1/swagger.json`

### Example Usage

```javascript
// Get all products
const response = await fetch('http://localhost:5126/api/products');
const products = await response.json();

// Create a new product
const newProduct = {
  name: "Laptop Computer",
  sku: "LAP001", 
  price: 999.99,
  stockQuantity: 15
};

const response = await fetch('http://localhost:5126/api/products', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(newProduct)
});
```

For detailed API documentation, see [docs/API.md](docs/API.md).

## 🧪 Testing

The project implements a comprehensive testing strategy:

### Test Types

- **Unit Tests**: Domain and application logic (xUnit)
- **API Tests**: REST endpoint validation (Postman/Newman)  
- **End-to-End Tests**: Complete user workflows (Cypress)
- **Integration Tests**: Database and external service integration

### Running Tests

```bash
# Run all unit tests
dotnet test

# Run API tests (requires running API)
newman run tests/postman/collection.json \
  --environment tests/postman/environment.json

# Run E2E tests (requires running application)
cd tests/e2e && npm run test:e2e
```

### Test Reporting

The project uses **CTRF (Common Test Report Format)** for unified test reporting:

- Automated test result aggregation
- GitHub Actions integration
- Rich test summaries and PR comments
- Comprehensive test metrics

For detailed testing information, see [docs/TESTING.md](docs/TESTING.md).

## 🐳 Docker & Deployment

### Docker Compose Configurations

- **`docker-compose.yml`**: Complete stack (API + Frontend + Database)
- **`docker-compose.full.yml`**: Full stack + standalone Swagger UI

### Quick Commands

```bash
# Start complete stack
docker-compose up -d

# Start with standalone Swagger UI
docker-compose -f docker-compose.full.yml up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Production Deployment

The application supports various deployment strategies:

- **Container Registry**: Docker Hub, AWS ECR, Azure ACR
- **Orchestration**: Docker Swarm, Kubernetes
- **Cloud Platforms**: Azure Container Apps, AWS ECS, Google Cloud Run

For detailed deployment guides, see [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md).


## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](docs/CONTRIBUTING.md) for detailed information on:

- Setting up the development environment
- Coding standards and best practices  
- Pull request process
- Issue reporting guidelines

### Quick Contribution Steps

1. **Fork** the repository
2. **Create** a feature branch: `git checkout -b feature/amazing-feature`
3. **Commit** your changes: `git commit -m 'Add amazing feature'`
4. **Push** to the branch: `git push origin feature/amazing-feature`
5. **Open** a Pull Request

## 📖 Documentation

Comprehensive documentation is available in the `/docs` directory:

| Document | Description |
|----------|-------------|
| [Architecture Guide](docs/ARCHITECTURE.md) | Detailed system architecture and design patterns |
| [Development Guide](docs/DEVELOPMENT.md) | Setup, coding standards, and development workflow |
| [API Documentation](docs/API.md) | Complete API reference and examples |
| [Testing Guide](docs/TESTING.md) | Testing strategy, tools, and best practices |
| [Deployment Guide](docs/DEPLOYMENT.md) | Docker, cloud deployment, and production setup |
| [Contributing Guidelines](docs/CONTRIBUTING.md) | How to contribute to the project |

## 🎯 Key Features Showcase

### Clean Architecture Implementation
- **Domain-Driven Design**: Pure business logic in Domain layer
- **Dependency Inversion**: Abstractions don't depend on concretions
- **Separation of Concerns**: Clear layer responsibilities
- **Testability**: Comprehensive unit test coverage

### Modern Development Practices
- **CI/CD Pipeline**: Automated testing and deployment
- **Code Quality**: Linting, formatting, and static analysis
- **Documentation**: Auto-generated API docs and comprehensive guides
- **Monitoring**: Health checks and application metrics

### Production-Ready Features
- **Containerization**: Docker support with multi-stage builds
- **Database Management**: EF Core migrations and seeding
- **Error Handling**: Structured error responses and logging
- **Security**: CORS, input validation, and secure defaults

## 🚀 Future Roadmap

- **Authentication & Authorization**: JWT-based security
- **Advanced Search**: Elasticsearch integration
- **Caching**: Redis for performance optimization
- **Event Sourcing**: Audit trails and state reconstruction
- **API Versioning**: Backward compatibility support
- **Monitoring**: Application insights and metrics
- **Mobile App**: React Native companion app

## 📜 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Clean Architecture** pattern by Robert C. Martin
- **.NET Community** for excellent frameworks and tools
- **React Community** for modern frontend development
- **Open Source Contributors** who make projects like this possible

---

**Demo Inventory Microservice** - Showcasing modern full-stack development with Clean Architecture principles.

For questions, issues, or contributions, please visit our [GitHub repository](https://github.com/zeabix-cloud-native/demo-inventory-microservice).
