# Architecture Documentation

## Overview

The Demo Inventory Microservice is a modern, full-stack application built using Clean Architecture principles with .NET 9 for the backend and React with TypeScript for the frontend. The system is designed to demonstrate best practices in microservice architecture, including proper layering, dependency injection, and comprehensive testing.

## Architectural Patterns

### Clean Architecture

The application follows Clean Architecture principles with clear separation of concerns across distinct layers:

```
┌─────────────────────────────────────────────────────────────┐
│                      Presentation Layer                      │
│  ┌─────────────────────┐  ┌─────────────────────────────────┐│
│  │   React Frontend    │  │      Web API Controllers       ││
│  │   (TypeScript)      │  │        (.NET 9)               ││
│  └─────────────────────┘  └─────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                      Application Layer                       │
│  ┌─────────────────────────────────────────────────────────┐│
│  │         Services & Use Cases                           ││
│  │      DTOs & Application Interfaces                     ││
│  └─────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                        Domain Layer                          │
│  ┌─────────────────────────────────────────────────────────┐│
│  │    Entities, Value Objects, Domain Services            ││
│  │         Interfaces & Domain Events                     ││
│  └─────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                      │
│  ┌─────────────────────────────────────────────────────────┐│
│  │     Data Access, External Services                     ││
│  │      Repository Implementations                        ││
│  └─────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
```

## Layer Details

### Domain Layer (`backend/src/DemoInventory.Domain`)

The core business logic layer containing:

- **Entities**: Core business entities (e.g., `Product`)
- **Value Objects**: Immutable objects representing domain concepts
- **Domain Services**: Business logic that doesn't naturally fit in entities
- **Repository Interfaces**: Contracts for data access
- **Domain Events**: Events triggered by business operations
- **Enums**: Domain-specific enumerations

**Key Principles:**
- No dependencies on external layers
- Contains pure business logic
- Framework-agnostic

### Application Layer (`backend/src/DemoInventory.Application`)

The use case orchestration layer containing:

- **Services**: Application business logic and use case orchestration
- **DTOs**: Data Transfer Objects for API communication
- **Interfaces**: Service contracts and abstractions
- **Use Cases**: Specific business operations
- **Common**: Shared application utilities and behaviors

**Key Principles:**
- Depends only on Domain layer
- Orchestrates business operations
- Defines application-specific interfaces

### Infrastructure Layer (`backend/src/DemoInventory.Infrastructure`)

The external concerns implementation layer containing:

- **Repositories**: Data access implementations
- **Data Context**: Entity Framework Core configuration
- **External Services**: Third-party service integrations
- **Persistence**: Database configuration and mappings

**Key Principles:**
- Implements Domain and Application interfaces
- Contains framework-specific code
- Handles external dependencies

### Presentation Layer (`backend/src/DemoInventory.API`)

The API interface layer containing:

- **Controllers**: Web API endpoints and HTTP handling
- **Configuration**: Application startup and dependency injection
- **Middleware**: Request/response processing pipeline
- **Program.cs**: Application entry point and service configuration

**Key Principles:**
- Handles HTTP concerns
- Minimal business logic
- Delegates to Application layer

### Frontend Layer (`frontend/`)

The user interface layer containing:

- **Components**: React components for UI rendering
- **Services**: API communication and external service integration
- **Types**: TypeScript type definitions
- **Routing**: Navigation and route configuration
- **Styles**: Component styling and themes

## Technology Stack

### Backend Technologies

- **.NET 9**: Runtime and framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core**: Object-relational mapping
- **PostgreSQL**: Primary database
- **Swagger/OpenAPI**: API documentation
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework for tests

### Frontend Technologies

- **React 19**: UI library
- **TypeScript**: Type-safe JavaScript
- **Vite**: Build tool and development server
- **Axios**: HTTP client for API communication
- **React Router**: Client-side routing
- **CSS3**: Styling with responsive design

### Testing Technologies

- **xUnit**: Unit testing for .NET
- **Postman/Newman**: API testing
- **Cypress**: End-to-end testing
- **CTRF**: Common Test Report Format for unified reporting

### DevOps & Infrastructure

- **Docker**: Containerization
- **Docker Compose**: Multi-container orchestration
- **GitHub Actions**: CI/CD pipeline
- **PostgreSQL**: Database

## Data Flow

### Request Flow

1. **HTTP Request** → API Controller
2. **Controller** → Application Service
3. **Application Service** → Domain Service/Repository
4. **Repository** → Database/External Service
5. **Response** flows back through the same layers

### Frontend Communication

1. **User Interaction** → React Component
2. **Component** → Service Layer (Axios)
3. **Service** → Backend API
4. **API Response** → Component State Update
5. **State Change** → UI Re-render

## Security Considerations

### Current Implementation

- **CORS**: Configured for frontend communication
- **Input Validation**: DTO validation and sanitization
- **Error Handling**: Structured error responses
- **API Documentation**: Swagger for API exploration

### Future Enhancements

- Authentication and authorization (JWT)
- Rate limiting
- Input sanitization
- SQL injection prevention
- Security headers

## Scalability Considerations

### Current Architecture Benefits

- **Microservice Ready**: Clean separation allows easy extraction
- **Database Independence**: Repository pattern abstracts data access
- **Testable**: Comprehensive testing at all layers
- **Containerized**: Docker support for deployment flexibility

### Scaling Strategies

- **Horizontal Scaling**: Multiple API instances behind load balancer
- **Database Scaling**: Read replicas, connection pooling
- **Caching**: Redis integration for performance
- **Message Queues**: Event-driven architecture for async processing

## Quality Assurance

### Testing Strategy

- **Unit Tests**: Domain and Application layer logic
- **Integration Tests**: API endpoints and database interactions
- **End-to-End Tests**: Complete user workflows
- **API Tests**: Postman collections for API validation

### Monitoring & Observability

- **Logging**: Structured logging with .NET logging framework
- **Health Checks**: API health monitoring
- **Metrics**: Application performance metrics
- **Tracing**: Request tracing for debugging

## Deployment Architecture

### Development Environment

- **Local Development**: dotnet run + npm dev server
- **Database**: PostgreSQL container
- **Hot Reload**: Both backend and frontend support

### Production Environment

- **Containerized Deployment**: Docker containers
- **Reverse Proxy**: Nginx for static file serving
- **Database**: Managed PostgreSQL service
- **CI/CD**: GitHub Actions pipeline

## Future Architecture Considerations

### Potential Enhancements

- **Event Sourcing**: For audit trails and state reconstruction
- **CQRS**: Separate read/write models for complex queries
- **Service Mesh**: For advanced microservice communication
- **API Gateway**: Centralized API management
- **Distributed Caching**: Redis for performance
- **Message Brokers**: RabbitMQ/Apache Kafka for async processing