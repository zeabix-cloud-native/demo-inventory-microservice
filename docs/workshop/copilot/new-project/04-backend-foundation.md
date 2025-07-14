# Backend Foundation Development
## AI-Assisted Clean Architecture Implementation

**Duration: 90 minutes**  
**Difficulty: Advanced**

## 🎯 Learning Objectives

Master AI-assisted backend development using Clean Architecture:
- **Implement domain models** with business rules using AI
- **Create application services** and use cases efficiently
- **Build infrastructure layer** with data access patterns
- **Design API controllers** with proper validation and error handling
- **Establish testing foundations** for all architectural layers

## 📋 Prerequisites

- Completed [Project Setup](03-project-setup.md)
- Understanding of Clean Architecture principles
- Familiarity with .NET 8, Entity Framework Core, and CQRS patterns
- Knowledge of Domain-Driven Design concepts

## 🏗️ Step 1: Domain Layer Implementation (25 minutes)

### 1.1 Core Domain Entities Design

**Copilot Chat Prompt:**
```
Implement the core domain entities for the Line Dealer BYD system using Domain-Driven Design principles:

1. Dealer Aggregate
   - Dealer (root entity) with multi-location support
   - DealerLocation value object
   - BusinessInformation value object
   - Contact information and address management

2. Vehicle Aggregate  
   - Vehicle entity with EV-specific properties
   - VehicleSpecification value object
   - VIN management and validation
   - Battery and charging capability tracking

3. Customer Aggregate
   - Customer entity (individual and business)
   - CustomerType enumeration
   - ContactInformation value object
   - CreditInformation value object

4. Base Infrastructure
   - BaseEntity with common properties (Id, CreatedAt, UpdatedAt)
   - IAggregateRoot interface
   - Domain events and INotificationDomainEvent
   - Audit trail and soft delete support

Implement using C# 12 features, proper encapsulation, and rich domain models with business rules.
```

### 1.2 Value Objects and Domain Services

**Copilot Chat Prompt:**
```
Create value objects and domain services for the Line Dealer BYD system:

1. Value Objects
   - VIN (Vehicle Identification Number) with validation
   - Money with currency support
   - PhoneNumber with international formatting
   - EmailAddress with validation
   - Address with geocoding capability

2. Domain Services
   - VehiclePricingService for price calculations
   - InventoryAvailabilityService for stock management
   - CustomerCreditService for credit assessments
   - DealerLocationService for territory management

3. Domain Events
   - VehicleReceivedEvent
   - VehicleSoldEvent
   - CustomerCreatedEvent
   - DealerLocationAddedEvent

4. Business Rules Implementation
   - Vehicle allocation rules between dealers
   - Customer eligibility for financing
   - Pricing rules and discount calculations
   - Inventory management business logic

Ensure immutability for value objects and proper domain event publishing.
```

### 1.3 Domain Exceptions and Validation

**Copilot Chat Prompt:**
```
Implement domain exceptions and validation for the Line Dealer BYD system:

1. Domain Exceptions
   - DealerNotFoundException
   - VehicleNotAvailableException
   - InvalidCustomerDataException
   - BusinessRuleViolationException

2. Validation Framework
   - FluentValidation for complex business rules
   - Domain invariant checking
   - Cross-aggregate validation rules
   - Validation result patterns

3. Business Rule Enforcement
   - Vehicle can only be sold once
   - Dealer can only access assigned vehicles
   - Customer data integrity rules
   - Financial transaction validation

4. Error Handling Patterns
   - Result pattern for operation outcomes
   - Domain event handling for validation failures
   - Compensating actions for business rule violations
   - Audit logging for domain exceptions

Provide comprehensive validation rules with clear error messages and recovery strategies.
```

## 📊 Step 2: Application Layer Implementation (25 minutes)

### 2.1 CQRS and MediatR Setup

**Copilot Chat Prompt:**
```
Implement CQRS pattern with MediatR for the Line Dealer BYD application layer:

1. Command and Query Infrastructure
   - ICommand and IQuery base interfaces
   - Command and query handlers with MediatR
   - Pipeline behaviors for cross-cutting concerns
   - Request/response patterns

2. Dealer Management Commands and Queries
   - CreateDealerCommand with validation
   - UpdateDealerInformationCommand
   - GetDealerByIdQuery with projection
   - GetDealerLocationsQuery with filtering

3. Vehicle Management Commands and Queries
   - AddVehicleToInventoryCommand
   - UpdateVehicleStatusCommand
   - GetAvailableVehiclesQuery with advanced filtering
   - GetVehicleDetailsQuery with full information

4. Customer Management Commands and Queries
   - CreateCustomerCommand with validation
   - UpdateCustomerInformationCommand
   - GetCustomersByDealerQuery
   - GetCustomerPurchaseHistoryQuery

Implement with proper validation, error handling, and logging at the application layer.
```

### 2.2 DTOs and Mapping Profiles

**Copilot Chat Prompt:**
```
Create DTOs and mapping profiles for the Line Dealer BYD application:

1. Response DTOs
   - DealerDto with nested location information
   - VehicleDto with comprehensive specifications
   - CustomerDto with contact and credit information
   - Paginated response DTOs for list operations

2. Command DTOs
   - CreateDealerRequest with validation attributes
   - UpdateVehicleRequest with change tracking
   - CreateCustomerRequest with business validation
   - Bulk operation DTOs for efficiency

3. AutoMapper Profiles
   - DealerMappingProfile with custom resolvers
   - VehicleMappingProfile with conditional mapping
   - CustomerMappingProfile with privacy protection
   - Reverse mapping for update operations

4. Validation Integration
   - FluentValidation for DTOs
   - Business rule validation in mapping
   - Custom validation attributes
   - Error aggregation and reporting

Ensure proper data transformation and validation at all mapping boundaries.
```

### 2.3 Application Services and Use Cases

**Copilot Chat Prompt:**
```
Implement application services and use cases for core business operations:

1. Dealer Management Service
   - Dealer registration and onboarding workflow
   - Multi-location management
   - Dealer performance analytics
   - Integration with BYD corporate systems

2. Vehicle Inventory Service
   - Vehicle receiving and inspection workflow
   - Inventory allocation between dealers
   - Real-time availability tracking
   - Price management and updates

3. Customer Management Service
   - Customer registration and KYC process
   - Credit assessment workflow
   - Customer communication management
   - Privacy and data protection compliance

4. Integration Services
   - External system integration interfaces
   - Data synchronization services
   - Event publishing and handling
   - Error handling and retry mechanisms

Implement with proper transaction management, event handling, and monitoring.
```

## 🗄️ Step 3: Infrastructure Layer Implementation (25 minutes)

### 3.1 Entity Framework Configuration

**Copilot Chat Prompt:**
```
Implement Entity Framework Core configuration for the Line Dealer BYD system:

1. DbContext Configuration
   - LineDealerBydDbContext with proper conventions
   - Entity configurations using IEntityTypeConfiguration
   - Database relationship mappings
   - Performance optimization (query splitting, indexing)

2. Entity Configurations
   - Dealer entity configuration with owned types
   - Vehicle entity configuration with complex properties
   - Customer entity configuration with privacy settings
   - Audit entity configuration for change tracking

3. Value Object Mapping
   - VIN value object conversion
   - Money value object with precision handling
   - Address value object with geographic data
   - Contact information owned entity types

4. Advanced Features
   - Database migrations with seeding
   - Query filters for soft delete and multi-tenancy
   - Interceptors for audit logging
   - Connection string management and pooling

Provide complete EF Core configuration with performance optimizations.
```

### 3.2 Repository Pattern Implementation

**Copilot Chat Prompt:**
```
Implement repository pattern with advanced querying capabilities:

1. Generic Repository Interface
   - IRepository<T> with common CRUD operations
   - IQueryableRepository<T> for complex queries
   - Specification pattern for query composition
   - Unit of work pattern for transaction management

2. Specific Repository Implementations
   - DealerRepository with location-based queries
   - VehicleRepository with advanced filtering
   - CustomerRepository with privacy-aware queries
   - Audit repository for change tracking

3. Query Optimization
   - Include strategies for related data
   - Projection queries for performance
   - Caching strategies for reference data
   - Batch operations for bulk updates

4. Repository Features
   - Specification pattern implementation
   - Dynamic query building
   - Pagination and sorting support
   - Database transaction management

Implement with proper abstractions and performance considerations.
```

### 3.3 External Service Integration

**Copilot Chat Prompt:**
```
Implement external service integration for the Line Dealer BYD system:

1. BYD Corporate Integration
   - Vehicle catalog and specification service
   - Inventory allocation and ordering service
   - Warranty and service information service
   - Authentication and authorization integration

2. Financial Service Integration
   - Credit bureau integration for customer assessment
   - Payment processing service integration
   - Insurance provider API integration
   - Banking and loan processing services

3. Government Service Integration
   - Vehicle registration service integration
   - Tax calculation and reporting services
   - Compliance and regulatory reporting
   - Environmental and safety certifications

4. Integration Infrastructure
   - HTTP client configuration with policies
   - Retry and circuit breaker patterns
   - Request/response logging and monitoring
   - Error handling and fallback strategies

Provide robust integration patterns with proper error handling and monitoring.
```

## 🌐 Step 4: API Layer Implementation (15 minutes)

### 4.1 API Controllers and Endpoints

**Copilot Chat Prompt:**
```
Implement comprehensive API controllers for the Line Dealer BYD system:

1. Dealer Management Controller
   - CRUD operations with proper HTTP verbs
   - Advanced search and filtering endpoints
   - Bulk operations for dealer management
   - File upload for dealer documentation

2. Vehicle Inventory Controller
   - Real-time inventory queries with filtering
   - Vehicle specification lookup endpoints
   - Batch vehicle updates and status changes
   - Image and document management for vehicles

3. Customer Management Controller
   - Customer registration and profile management
   - Privacy-compliant data access endpoints
   - Customer communication history
   - Credit assessment and approval workflows

4. API Standards Implementation
   - Consistent response formatting
   - Proper HTTP status code usage
   - API versioning strategy
   - Comprehensive error handling

Implement following RESTful principles with OpenAPI documentation.
```

### 4.2 Authentication and Authorization

**Copilot Chat Prompt:**
```
Implement comprehensive authentication and authorization:

1. JWT Authentication Setup
   - JWT token generation and validation
   - Refresh token implementation
   - Token expiration and renewal
   - Secure token storage and transmission

2. Role-Based Authorization
   - Dealer principal, manager, salesperson roles
   - Location-based access control
   - Feature-based permissions
   - Dynamic permission evaluation

3. Multi-Tenant Security
   - Dealer isolation and data segregation
   - Cross-dealer access prevention
   - Location-specific data access
   - Administrative override capabilities

4. API Security Features
   - Rate limiting and throttling
   - Request validation and sanitization
   - CORS policy configuration
   - Security headers and protection

Provide complete authentication and authorization implementation with security best practices.
```

## 🧪 Step 5: Testing Foundation (20 minutes)

### 5.1 Unit Testing Setup

**Copilot Chat Prompt:**
```
Create comprehensive unit testing foundation for all layers:

1. Domain Layer Testing
   - Entity behavior and business rule testing
   - Value object validation testing
   - Domain service logic testing
   - Domain event testing

2. Application Layer Testing
   - Command and query handler testing
   - Application service testing with mocking
   - DTO validation and mapping testing
   - Use case workflow testing

3. Infrastructure Layer Testing
   - Repository testing with in-memory database
   - External service integration testing with mocks
   - Database configuration testing
   - Caching and performance testing

4. Testing Infrastructure
   - Test data builders and object mothers
   - Custom test assertions and matchers
   - Test database setup and teardown
   - Shared test utilities and helpers

Use xUnit, NSubstitute, FluentAssertions, and Testcontainers for comprehensive testing.
```

### 5.2 Integration Testing Implementation

**Copilot Chat Prompt:**
```
Implement integration testing for API endpoints and workflows:

1. API Integration Testing
   - Controller endpoint testing with TestServer
   - Authentication and authorization testing
   - Request/response validation testing
   - Error handling and edge case testing

2. Database Integration Testing
   - Repository integration testing with real database
   - Migration testing and schema validation
   - Performance testing for complex queries
   - Transaction and concurrency testing

3. External Service Integration Testing
   - Mock external service responses
   - Integration failure scenario testing
   - Timeout and retry mechanism testing
   - Data transformation and mapping testing

4. End-to-End Workflow Testing
   - Complete business process testing
   - Multi-service interaction testing
   - Event-driven workflow testing
   - Performance and load testing

Provide comprehensive integration test scenarios covering all critical paths.
```

## ✅ Implementation Validation

### Domain Layer Validation Checklist

**Entity Implementation**
- [ ] All entities properly implement business rules
- [ ] Value objects are immutable and validated
- [ ] Domain events are published correctly
- [ ] Aggregate boundaries are respected
- [ ] Encapsulation prevents invalid state changes

**Business Logic Validation**
- [ ] Business rules are enforced in domain layer
- [ ] Domain services handle complex business logic
- [ ] Validation provides clear error messages
- [ ] Exception handling follows domain patterns
- [ ] Audit and change tracking implemented

### Application Layer Validation Checklist

**CQRS Implementation**
- [ ] Commands and queries properly separated
- [ ] Handlers implement single responsibility
- [ ] Pipeline behaviors handle cross-cutting concerns
- [ ] Validation occurs at appropriate boundaries
- [ ] Error handling provides meaningful responses

**Service Implementation**
- [ ] Application services coordinate use cases
- [ ] DTOs properly map domain objects
- [ ] Transaction boundaries are well-defined
- [ ] Integration points are abstracted
- [ ] Performance is optimized for common scenarios

### Infrastructure Layer Validation Checklist

**Data Access**
- [ ] Entity Framework configurations are complete
- [ ] Repository pattern provides clean abstractions
- [ ] Database migrations work correctly
- [ ] Query performance is optimized
- [ ] Connection management is efficient

**External Integrations**
- [ ] Service integrations handle errors gracefully
- [ ] Retry and circuit breaker patterns implemented
- [ ] Authentication and authorization work correctly
- [ ] Data transformation is accurate
- [ ] Monitoring and logging are comprehensive

### API Layer Validation Checklist

**Controller Implementation**
- [ ] RESTful principles are followed
- [ ] HTTP status codes are used correctly
- [ ] Request/response models are consistent
- [ ] Authentication and authorization work
- [ ] Error handling provides useful information

**API Quality**
- [ ] OpenAPI documentation is complete
- [ ] Validation provides clear error messages
- [ ] Performance meets requirements
- [ ] Security measures are implemented
- [ ] Versioning strategy is in place

## 🎯 Performance and Quality Metrics

### Code Quality Metrics
- **Test Coverage**: >90% for domain and application layers
- **Cyclomatic Complexity**: <10 for all methods
- **Code Duplication**: <5% across the codebase
- **Security Vulnerabilities**: Zero high/critical issues
- **Performance**: API responses <200ms for 95% of requests

### Architecture Compliance
- **Dependency Rules**: No violations of Clean Architecture dependencies
- **SOLID Principles**: All classes follow SOLID principles
- **DRY Principle**: Minimal code duplication
- **Separation of Concerns**: Clear responsibility boundaries
- **Domain Purity**: Domain layer has no external dependencies

## 🚀 Next Steps

After completing backend foundation:

1. **Code Review**: Conduct thorough review of all implementations
2. **Performance Testing**: Validate performance requirements
3. **Security Review**: Ensure security best practices
4. **Documentation**: Complete API and architecture documentation
5. **Frontend Integration**: Begin frontend development with API integration

### Foundation Validation Script

```bash
#!/bin/bash
# validate-backend-foundation.sh

echo "🔍 Validating backend foundation..."

# Build all projects
dotnet build backend/LineDealerByd.sln

# Run unit tests
dotnet test backend/tests/ --collect:"XPlat Code Coverage"

# Run integration tests
dotnet test backend/tests/ --filter Category=Integration

# Validate database migrations
dotnet ef migrations list --project backend/src/LineDealerByd.Infrastructure

# Check code quality
dotnet sonarscanner begin
dotnet build backend/LineDealerByd.sln
dotnet sonarscanner end

echo "✅ Backend foundation validation completed!"
```

---

**🎯 Success Indicator**: You've successfully completed backend foundation when all layers are implemented following Clean Architecture principles, comprehensive tests pass, and the API endpoints are functional and documented.

**Next**: [API Development & Documentation](05-api-development.md) - Complete API implementation with comprehensive documentation and testing.