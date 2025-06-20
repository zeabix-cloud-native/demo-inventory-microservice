# VS Code Copilot Agent Instructions

This document provides specific guidance for working with GitHub Copilot in VS Code agent mode for the Demo Inventory Microservice project.

## Agent Mode Configuration

### Recommended VS Code Extensions

Before using Copilot agent mode, ensure these extensions are installed:

**Essential Extensions:**
- **GitHub Copilot** - AI pair programmer
- **GitHub Copilot Chat** - AI assistant in VS Code
- **C# Dev Kit** - C# language support and tools
- **Thunder Client** - API testing within VS Code
- **Docker** - Container management
- **PostgreSQL** - Database management

**Frontend Extensions:**
- **ES7+ React/Redux/React-Native snippets** - React development
- **TypeScript Importer** - Auto import for TypeScript
- **Auto Rename Tag** - HTML/JSX tag management
- **Prettier - Code formatter** - Code formatting
- **ESLint** - JavaScript/TypeScript linting

### Workspace Settings

Create or update `.vscode/settings.json` with project-specific configurations:

```json
{
  "github.copilot.enable": {
    "*": true,
    "plaintext": false,
    "markdown": false,
    "scminput": false
  },
  "github.copilot.advanced": {
    "length": 500,
    "temperature": 0.1,
    "top_p": 1,
    "listCount": 3
  },
  "dotnet.defaultSolution": "DemoInventory.sln",
  "typescript.preferences.importModuleSpecifier": "relative",
  "eslint.workingDirectories": ["frontend"],
  "prettier.configPath": "frontend/.prettierrc"
}
```

## Agent Interaction Patterns

### Context-Aware Prompts

When working with Copilot agent, provide context about the layer you're working in:

#### Domain Layer Context
```
I'm working in the Domain layer of a Clean Architecture .NET project. 
I need to create a new entity for [EntityName] that follows DDD principles.
The entity should have rich behavior and enforce business rules.
```

#### Application Layer Context
```
I'm working in the Application layer. I need to create a service that handles 
[UseCase] following CQRS pattern. The service should use dependency injection 
and handle errors appropriately.
```

#### Infrastructure Layer Context
```
I'm working in the Infrastructure layer using EF Core with PostgreSQL.
I need to implement a repository for [Entity] with optimized queries.
```

#### Frontend Context
```
I'm working on a React TypeScript component using functional components and hooks.
The component should follow the existing patterns in the project and use proper TypeScript types.
```

### Project-Specific Agent Commands

Use these specialized commands when working with Copilot Chat:

#### Backend Development
```
@workspace Create a new product feature following Clean Architecture
@workspace Generate EF Core migration for [change description]
@workspace Create unit tests for [ServiceName] using xUnit
@workspace Add API endpoint for [operation] with proper validation
@workspace Implement repository pattern for [EntityName]
```

#### Frontend Development
```
@workspace Create React component for [ComponentName] with TypeScript
@workspace Generate API service for [EntityName] using axios
@workspace Create form component with validation for [Entity]
@workspace Add routing for [FeatureName] using React Router
@workspace Create custom hook for [functionality]
```

### Code Generation Guidelines

#### When Asking for Code Generation

1. **Specify the layer**: "Generate for Domain/Application/Infrastructure/Presentation layer"
2. **Include context**: "This is for product management functionality"
3. **Mention patterns**: "Follow repository pattern" or "Use CQRS"
4. **Request tests**: "Include unit tests with xUnit and NSubstitute"

#### Example Prompts

```
Generate a Product entity for the Domain layer that:
- Follows DDD principles with private setters
- Includes business validation rules
- Has a factory method for creation
- Includes proper ToString() and Equals() methods
```

```
Create an API controller for Product management that:
- Follows RESTful conventions
- Uses dependency injection for services
- Includes proper error handling
- Has OpenAPI documentation attributes
- Returns appropriate HTTP status codes
```

## Testing with Copilot Agent

### Test Generation Commands

```
@workspace Generate unit tests for [ClassName] using xUnit, NSubstitute, and FluentAssertions
@workspace Create integration tests for [ControllerName] API endpoints
@workspace Generate Postman collection for [FeatureName] API endpoints
@workspace Create Cypress E2E tests for [UserStory]
```

### Test-Driven Development

Use Copilot to help with TDD workflow:

1. **Generate test cases**: Ask Copilot to create failing tests first
2. **Implement code**: Use Copilot to suggest implementation
3. **Refactor**: Ask for refactoring suggestions while keeping tests green

```
I'm following TDD. Create failing unit tests for a ProductService.CreateProductAsync method that should:
- Validate product data
- Check for duplicate SKU
- Save to repository
- Return success/failure result
```

## Database Development

### Entity Framework with Copilot

```
@workspace Generate EF Core entity configuration for [EntityName]
@workspace Create migration for adding [FieldName] to [TableName]
@workspace Generate repository implementation using EF Core for [EntityName]
@workspace Create database seeding data for [EntityName]
```

### Database Query Optimization

```
Help me optimize this EF Core query for better performance:
[paste your LINQ query]

Consider:
- Proper includes for related data
- Pagination for large datasets
- Indexing recommendations
- N+1 query prevention
```

## Frontend Development Patterns

### Component Generation

```
@workspace Create a TypeScript interface for [EntityName] based on the backend DTO
@workspace Generate React component for [ComponentName] with proper TypeScript props
@workspace Create custom hook for managing [EntityName] state
@workspace Generate form validation schema for [EntityName]
```

### API Integration

```
@workspace Create API service methods for [EntityName] CRUD operations
@workspace Generate TypeScript types from OpenAPI specification
@workspace Create error handling wrapper for API calls
@workspace Add loading states and error boundaries for [ComponentName]
```

## Debugging with Copilot

### Debug Session Commands

```
@workspace Analyze this exception and suggest fixes: [paste exception]
@workspace Help debug why this unit test is failing: [paste test and error]
@workspace Suggest performance improvements for this code: [paste code]
@workspace Review this code for potential bugs: [paste code]
```

### Code Review Assistance

```
@workspace Review this pull request changes for:
- Clean Architecture compliance
- Security vulnerabilities
- Performance issues
- Testing coverage
- Code style consistency
```

## Docker and DevOps

### Container Development

```
@workspace Help optimize this Dockerfile for .NET application
@workspace Create docker-compose service for [ServiceName]
@workspace Generate health check endpoint for [ServiceName]
@workspace Suggest Docker best practices for this setup
```

### CI/CD Pipeline

```
@workspace Create GitHub Actions workflow for:
- Building and testing .NET application
- Running Postman tests
- Building and deploying React app
- Docker image building and publishing
```

## Best Practices for Agent Mode

### Do's
- ✅ **Provide context** about the architectural layer you're working in
- ✅ **Reference existing patterns** in the codebase
- ✅ **Ask for tests** along with implementation code
- ✅ **Request documentation** for complex features
- ✅ **Specify technology constraints** (.NET 9, React 19, etc.)

### Don'ts
- ❌ **Don't ignore architecture boundaries** - respect Clean Architecture layers
- ❌ **Don't skip validation** - always include proper input validation
- ❌ **Don't forget error handling** - include appropriate error handling
- ❌ **Don't ignore existing conventions** - follow established patterns
- ❌ **Don't skip security considerations** - always consider security implications

### Effective Agent Conversation Flow

1. **Start with context**: Explain what you're building and which layer
2. **Reference existing code**: Point to similar implementations
3. **Specify requirements**: Be clear about functional and non-functional requirements
4. **Ask for tests**: Request test code along with implementation
5. **Iterate and refine**: Use follow-up questions to improve the solution
6. **Document decisions**: Ask for documentation of complex logic

### Example Conversation Flow

```
User: I need to add a new Category entity to the Domain layer of our Clean Architecture project.

Agent: I'll help you create a Category entity following DDD principles. Let me generate...

User: Great! Now I need the repository interface for this entity in the Application layer.

Agent: I'll create the ICategoryRepository interface that follows the existing repository pattern...

User: Perfect! Can you also create the EF Core implementation in the Infrastructure layer?

Agent: I'll implement the CategoryRepository using EF Core, following the existing patterns...

User: Finally, I need unit tests for the Category entity using our testing stack.

Agent: I'll create comprehensive unit tests using xUnit, NSubstitute, and FluentAssertions...
```

## Project-Specific Prompts Library

Save these prompts for quick access:

### Quick Start Prompts
```
Create new feature following project patterns
Generate API endpoint with validation
Create React component with TypeScript types
Add unit tests for existing code
Optimize database query performance
Create migration for schema change
Generate API documentation
Create E2E test scenario
```

### Architecture Compliance Prompts
```
Review code for Clean Architecture compliance
Ensure proper dependency direction
Validate separation of concerns
Check for proper abstraction usage
Verify SOLID principles adherence
```

### Code Quality Prompts
```
Review for security vulnerabilities
Check performance optimization opportunities
Validate error handling completeness
Ensure proper logging implementation
Review test coverage adequacy
```

---

**Remember**: Copilot agent mode is most effective when you provide clear context about the architectural layer, existing patterns, and specific requirements. Always review generated code for compliance with project standards and security considerations.