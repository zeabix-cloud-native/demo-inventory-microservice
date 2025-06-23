# Backend Development Exercise
## Building a Complete Feature with Clean Architecture

In this exercise, you'll use GitHub Copilot to build a complete backend feature following Clean Architecture principles. We'll create a **Category Management** feature from scratch.

## 🎯 Learning Objectives

- Master Clean Architecture layer development with Copilot
- Generate domain entities with business rules
- Create application services with proper separation of concerns
- Implement repository patterns and EF Core configurations
- Build RESTful API controllers with validation
- Generate comprehensive unit tests

## 📋 Prerequisites

- Completed [Getting Started Guide](../getting-started.md)
- VS Code with GitHub Copilot configured
- Demo Inventory project cloned and buildable

## 🏗️ Exercise Overview

We'll build a **Category Management** feature with these components:

```
📁 Domain Layer
  └── Category entity with business rules
  
📁 Application Layer  
  ├── CategoryDto and CreateCategoryDto
  ├── ICategoryService interface
  └── CategoryService implementation
  
📁 Infrastructure Layer
  ├── ICategoryRepository interface
  ├── CategoryRepository implementation
  └── EF Core configuration
  
📁 API Layer
  └── CategoriesController with CRUD operations
  
📁 Tests
  └── Comprehensive unit tests
```

## 🚀 Step 1: Domain Layer - Category Entity

Let's start with the core domain entity.

### 1.1 Create the Category Entity

1. **Open** `backend/src/DemoInventory.Domain/Entities/`
2. **Create** new file `Category.cs`
3. **Use Copilot Chat** with this prompt:

```
@workspace I need to create a Category entity in the Domain layer following Clean Architecture and DDD principles. Based on the existing entity patterns in the project, create a Category entity with:

- Id (Guid)
- Name (string, required, max 100 chars)
- Description (string, optional, max 500 chars)  
- IsActive (bool, default true)
- CreatedAt and UpdatedAt (DateTime)
- CreatedBy and UpdatedBy (string)

The entity should:
- Follow the existing BaseEntity pattern if available
- Have private setters for encapsulation
- Include proper validation rules
- Have a factory method for creation
- Include business rules for name uniqueness
- Override ToString() and Equals() methods
```

### 1.2 Review and Refine

After Copilot generates the entity, review it:

**Check for:**
- ✅ Proper encapsulation (private setters)
- ✅ Validation rules in constructor
- ✅ Business logic encapsulation
- ✅ Consistent with existing patterns

**Follow-up prompts if needed:**
```
Add validation to ensure Name is not empty or whitespace
Add a method to update the category that validates the input
Add domain events for category creation and updates
```

### 1.3 Expected Result

Your Category entity should look similar to:

```csharp
namespace DemoInventory.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private Category() { } // For EF Core

    public Category(string name, string? description = null)
    {
        ValidateName(name);
        
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description)
    {
        ValidateName(name);
        
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty", nameof(name));
            
        if (name.Length > 100)
            throw new ArgumentException("Category name cannot exceed 100 characters", nameof(name));
    }
}
```

## 🔧 Step 2: Application Layer - DTOs and Services

### 2.1 Create DTOs

1. **Navigate to** `backend/src/DemoInventory.Application/DTOs/`
2. **Create** `CategoryDto.cs`

**Copilot Chat Prompt:**
```
@workspace Create CategoryDto and CreateCategoryDto classes in the Application layer following the existing DTO patterns in the project. 

CategoryDto should include all Category properties for API responses.
CreateCategoryDto should include only the properties needed for creating a new category.

Follow the existing naming conventions and property patterns used in other DTOs.
```

### 2.2 Create Service Interface

1. **Navigate to** `backend/src/DemoInventory.Application/Interfaces/`
2. **Create** `ICategoryService.cs`

**Copilot Chat Prompt:**
```
@workspace Create ICategoryService interface in the Application layer following SOLID principles. The service should provide methods for:

- GetAllCategoriesAsync() - returns IEnumerable<CategoryDto>
- GetCategoryByIdAsync(Guid id) - returns CategoryDto?
- CreateCategoryAsync(CreateCategoryDto dto) - returns CategoryDto
- UpdateCategoryAsync(Guid id, CreateCategoryDto dto) - returns CategoryDto?
- DeleteCategoryAsync(Guid id) - returns bool
- ActivateCategoryAsync(Guid id) - returns bool
- DeactivateCategoryAsync(Guid id) - returns bool

Follow the existing service interface patterns in the project.
```

### 2.3 Create Service Implementation

1. **Navigate to** `backend/src/DemoInventory.Application/Services/`
2. **Create** `CategoryService.cs`

**Copilot Chat Prompt:**
```
@workspace Create CategoryService implementation for ICategoryService following the existing service patterns in the project. The service should:

- Use dependency injection for ICategoryRepository and ILogger
- Include proper error handling and logging
- Map between entities and DTOs (use AutoMapper if available)
- Handle business rules and validation
- Use async/await for all operations
- Return appropriate results for success/failure scenarios

Follow the existing service implementation patterns, error handling, and logging strategies used in the project.
```

## 🗄️ Step 3: Infrastructure Layer - Repository Pattern

### 3.1 Create Repository Interface

1. **Navigate to** `backend/src/DemoInventory.Application/Interfaces/` (or wherever repository interfaces are defined)
2. **Create** `ICategoryRepository.cs`

**Copilot Chat Prompt:**
```
@workspace Create ICategoryRepository interface following the existing repository patterns in the project. The repository should extend the base repository interface if available and include:

- GetByNameAsync(string name) - for uniqueness validation
- GetActiveAsync() - get only active categories
- GetInactiveAsync() - get only inactive categories
- SearchAsync(string searchTerm) - search categories by name/description

Follow the existing repository interface patterns in the project.
```

### 3.2 Create Repository Implementation

1. **Navigate to** `backend/src/DemoInventory.Infrastructure/Repositories/`
2. **Create** `CategoryRepository.cs`

**Copilot Chat Prompt:**
```
@workspace Create CategoryRepository implementation using Entity Framework Core following the existing repository patterns in the project. 

The repository should:
- Inherit from the base repository if available
- Use the ApplicationDbContext
- Implement all interface methods efficiently
- Include proper query optimization
- Handle database exceptions appropriately
- Use proper async/await patterns

Follow the existing repository implementation patterns in the project.
```

### 3.3 Create EF Core Configuration

1. **Navigate to** `backend/src/DemoInventory.Infrastructure/Configurations/`
2. **Create** `CategoryConfiguration.cs`

**Copilot Chat Prompt:**
```
@workspace Create EF Core configuration for the Category entity following the existing configuration patterns in the project. The configuration should:

- Configure table name and schema
- Set up primary key
- Configure string properties with proper lengths
- Set up indexes for performance
- Configure relationships if needed
- Follow the existing configuration patterns in the project
```

## 🌐 Step 4: API Layer - Controllers

### 4.1 Create Categories Controller

1. **Navigate to** `backend/src/DemoInventory.API/Controllers/`
2. **Create** `CategoriesController.cs`

**Copilot Chat Prompt:**
```
@workspace Create CategoriesController following the existing controller patterns in the project. The controller should:

- Inherit from the base controller if available
- Use dependency injection for ICategoryService and ILogger
- Implement RESTful endpoints for CRUD operations:
  - GET /api/categories - get all categories
  - GET /api/categories/{id} - get category by id
  - POST /api/categories - create new category
  - PUT /api/categories/{id} - update category
  - DELETE /api/categories/{id} - delete category
  - PATCH /api/categories/{id}/activate - activate category
  - PATCH /api/categories/{id}/deactivate - deactivate category

Include:
- Proper HTTP status codes
- Model validation
- Error handling
- OpenAPI/Swagger documentation attributes
- Async operations

Follow the existing controller patterns, response formats, and error handling strategies in the project.
```

### 4.2 Add Dependency Injection

1. **Open** `backend/src/DemoInventory.API/Program.cs` (or wherever DI is configured)
2. **Use Copilot** to add the necessary service registrations

**Copilot Chat Prompt:**
```
@workspace I need to register the CategoryService and CategoryRepository in the dependency injection container following the existing patterns in the project. Show me the code to add to the DI configuration.
```

## 🧪 Step 5: Testing

### 5.1 Create Unit Tests for Domain Entity

1. **Navigate to** `backend/tests/DemoInventory.Domain.Tests/Entities/`
2. **Create** `CategoryTests.cs`

**Copilot Chat Prompt:**
```
@workspace Generate comprehensive unit tests for the Category entity using xUnit, FluentAssertions, and following the existing test patterns in the project. Include tests for:

- Entity creation with valid data
- Validation rules (name requirements, length limits)
- Business methods (UpdateDetails, Activate, Deactivate)
- Edge cases and error conditions
- ToString and Equals behavior

Follow the existing unit test patterns, naming conventions, and test organization in the project.
```

### 5.2 Create Unit Tests for Service

1. **Navigate to** `backend/tests/DemoInventory.Application.Tests/Services/`
2. **Create** `CategoryServiceTests.cs`

**Copilot Chat Prompt:**
```
@workspace Generate comprehensive unit tests for CategoryService using xUnit, NSubstitute, FluentAssertions, and following the existing service test patterns in the project. Include tests for:

- All service methods with valid inputs
- Error handling scenarios
- Repository interaction mocking
- Logging verification
- Mapping between entities and DTOs
- Business rule validation

Follow the existing service test patterns, mocking strategies, and assertions in the project.
```

### 5.3 Create Integration Tests

1. **Navigate to** `backend/tests/DemoInventory.API.Tests/Controllers/`
2. **Create** `CategoriesControllerTests.cs`

**Copilot Chat Prompt:**
```
@workspace Generate integration tests for CategoriesController using the existing integration test patterns in the project. Include tests for:

- All HTTP endpoints (GET, POST, PUT, DELETE, PATCH)
- Request/response validation
- Error scenarios (404, 400, 500)
- Authentication/authorization if applicable
- Database integration

Follow the existing integration test patterns, test setup, and database seeding strategies in the project.
```

## 🔍 Step 6: Testing and Validation

### 6.1 Build and Test

```bash
# Build the solution
dotnet build

# Run unit tests
dotnet test

# Check for any errors
dotnet test --logger trx --results-directory ./TestResults
```

### 6.2 Manual API Testing

1. **Start the application:**
```bash
cd backend/src/DemoInventory.API
dotnet run
```

2. **Test endpoints using Thunder Client or curl:**

```bash
# Create a category
curl -X POST http://localhost:5000/api/categories \
  -H "Content-Type: application/json" \
  -d '{"name": "Electronics", "description": "Electronic products"}'

# Get all categories  
curl http://localhost:5000/api/categories

# Get category by ID
curl http://localhost:5000/api/categories/{id}
```

### 6.3 Validation Checklist

Before completing the exercise, verify:

- [ ] Category entity follows Clean Architecture principles
- [ ] DTOs are properly structured
- [ ] Service layer implements business logic correctly
- [ ] Repository pattern is implemented properly
- [ ] EF Core configuration is correct
- [ ] API controller follows RESTful conventions
- [ ] All tests pass
- [ ] API endpoints work correctly
- [ ] Proper error handling is implemented
- [ ] Code follows project conventions

## 🎓 Learning Reflection

### What You've Accomplished

✅ **Built a complete feature** using Clean Architecture  
✅ **Used Copilot effectively** for code generation  
✅ **Followed established patterns** and conventions  
✅ **Created comprehensive tests** for all layers  
✅ **Implemented proper error handling** and validation  
✅ **Used dependency injection** correctly  

### Key Takeaways

1. **Context is King**: Providing good context with `@workspace` dramatically improves code quality
2. **Iterative Refinement**: Use follow-up prompts to refine and improve generated code
3. **Pattern Consistency**: Copilot learns from existing patterns in your codebase
4. **Test-Driven Approach**: Generate tests alongside implementation code
5. **Review Everything**: Always review and understand generated code before accepting

### Common Pitfalls and Solutions

**Pitfall**: Generated code doesn't follow Clean Architecture  
**Solution**: Be explicit about layer responsibilities in prompts

**Pitfall**: Missing error handling or validation  
**Solution**: Specifically request error handling and validation in prompts

**Pitfall**: Inconsistent naming conventions  
**Solution**: Reference existing patterns and ask Copilot to follow them

## 🚀 Next Steps

1. **[Frontend Development Exercise](frontend-exercise.md)** - Create React components for the Category feature
2. **[Testing Exercise](testing-exercise.md)** - Advanced testing scenarios
3. **[Bug Fixing Exercise](bug-fixing-exercise.md)** - Debug and fix issues

### Advanced Challenges

Want to go further? Try these extensions:

1. **Add Category Hierarchy**: Parent/child categories with tree operations
2. **Add Soft Delete**: Implement soft delete pattern
3. **Add Audit Trail**: Track all changes with audit logging
4. **Add Search**: Implement full-text search capabilities
5. **Add Caching**: Implement Redis caching for categories

## 💡 Pro Tips

- **Use specific prompts**: "Following Clean Architecture" vs "Create a service"
- **Reference existing code**: "Like the ProductService" vs generic requests
- **Ask for tests**: Always request tests with implementation
- **Iterate gradually**: Build and test incrementally
- **Document decisions**: Ask Copilot to explain complex logic

---

## 🎯 Exercise Completion

You've successfully built a complete backend feature using GitHub Copilot! You should now be comfortable using AI assistance for:

- Domain-driven design
- Clean Architecture implementation  
- Repository patterns
- RESTful API development
- Comprehensive testing

Ready for the next challenge? Let's build the frontend! 🚀