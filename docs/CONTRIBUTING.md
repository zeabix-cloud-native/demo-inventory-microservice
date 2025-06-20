# Contributing Guidelines

## Welcome Contributors!

Thank you for your interest in contributing to the Demo Inventory Microservice! This document provides guidelines and information to help you contribute effectively to the project.

## Code of Conduct

We are committed to providing a welcoming and inclusive experience for everyone. We expect all contributors to:

- Use welcoming and inclusive language
- Be respectful of differing viewpoints and experiences
- Accept constructive criticism gracefully
- Focus on what is best for the community
- Show empathy towards other community members

## How to Contribute

### Types of Contributions

We welcome various types of contributions:

1. **Bug Reports**: Help us identify and fix issues
2. **Feature Requests**: Suggest new functionality
3. **Code Contributions**: Submit bug fixes or new features
4. **Documentation**: Improve or add documentation
5. **Testing**: Add or improve test coverage
6. **Performance**: Optimize existing code

### Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally
3. **Set up the development environment** (see [Development Guide](DEVELOPMENT.md))
4. **Create a feature branch** for your changes
5. **Make your changes** following our coding standards
6. **Test your changes** thoroughly
7. **Submit a pull request**

## Development Workflow

This project follows the **Git Flow** branching strategy. Please read our [Git Flow Guide](GITFLOW.md) for complete details.

### Quick Git Flow Reference

#### 🌟 Feature Development
```bash
# Start new feature
git checkout develop
git pull origin develop
git checkout -b feature/your-feature-name

# Work on feature
git add .
git commit -m "feat: add new functionality"
git push origin feature/your-feature-name

# Submit PR to develop
# After approval, feature is merged and branch deleted
```

#### 🚀 Release Process
```bash
# Create release branch
git checkout develop
git pull origin develop
git checkout -b release/v1.2.0

# Prepare release (version bumps, changelog, etc.)
git add .
git commit -m "chore: prepare v1.2.0 release"
git push origin release/v1.2.0

# Test in staging, then PR to main
# After deployment, merge back to develop
```

#### 🔥 Hotfix Process
```bash
# Create hotfix from main
git checkout main
git pull origin main
git checkout -b hotfix/critical-fix

# Make fix
git add .
git commit -m "fix: resolve critical issue"
git push origin hotfix/critical-fix

# Emergency PR to main
# After deployment, merge back to develop
```

### 1. Setting Up Your Environment

```bash
# Fork and clone the repository
git clone https://github.com/YOUR-USERNAME/demo-inventory-microservice.git
cd demo-inventory-microservice

# Add the upstream repository
git remote add upstream https://github.com/zeabix-cloud-native/demo-inventory-microservice.git

# Set up the development environment
dotnet restore
cd frontend && npm install
```

### 2. Git Flow Branch Types

Choose the appropriate branch type for your contribution:

#### 🌟 Feature Branches (`feature/*`)
For new features and enhancements:
```bash
feature/add-product-categories
feature/implement-user-authentication
feature/enhance-search-functionality
```

#### 🐛 Bugfix Branches (`bugfix/*`)
For non-critical bug fixes:
```bash
bugfix/fix-validation-error
bugfix/resolve-ui-alignment-issue
```

#### 🔥 Hotfix Branches (`hotfix/*`)
For critical production fixes (emergency only):
```bash
hotfix/fix-security-vulnerability
hotfix/resolve-data-corruption
```

#### 🚀 Release Branches (`release/*`)
For release preparation (maintainers only):
```bash
release/v1.2.0
release/v2.0.0-beta
```

### 3. Creating a Branch

Create a descriptive branch name that indicates the type of change:

```bash
# Feature branches
git checkout -b feature/add-product-categories
git checkout -b feature/implement-authentication

# Bug fix branches
git checkout -b fix/product-validation-issue
git checkout -b fix/memory-leak-in-service

# Documentation branches
git checkout -b docs/update-api-documentation
git checkout -b docs/add-deployment-guide
```

### 3. Making Changes

#### Backend Development

Follow Clean Architecture principles:

```csharp
// Example: Adding a new feature
// 1. Start with Domain layer
public class Category
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    
    public Category(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}

// 2. Add to Application layer
public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
}

// 3. Implement in Infrastructure layer
public class CategoryService : ICategoryService
{
    // Implementation
}

// 4. Add API controller
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    // Controller implementation
}
```

#### Frontend Development

Follow React and TypeScript best practices:

```typescript
// Example: Adding a new component
interface CategoryListProps {
  categories: Category[];
  onCategorySelect: (category: Category) => void;
}

export const CategoryList: React.FC<CategoryListProps> = ({ 
  categories, 
  onCategorySelect 
}) => {
  return (
    <div className="category-list">
      {categories.map(category => (
        <CategoryItem 
          key={category.id}
          category={category}
          onClick={() => onCategorySelect(category)}
        />
      ))}
    </div>
  );
};
```

### 4. Testing Your Changes

#### Run All Tests

```bash
# Backend unit tests
dotnet test

# API tests
newman run tests/postman/collection.json \
  --environment tests/postman/environment.json

# Frontend tests (if applicable)
cd frontend && npm test

# E2E tests
cd tests/e2e && npm run test:e2e
```

#### Add New Tests

Always add tests for new functionality:

```csharp
// Unit test example
[Fact]
public void Category_Creation_Should_Set_Name()
{
    // Arrange
    var name = "Electronics";
    
    // Act
    var category = new Category(name);
    
    // Assert
    Assert.Equal(name, category.Name);
}

[Theory]
[InlineData("")]
[InlineData(null)]
[InlineData("   ")]
public void Category_Creation_Should_Throw_With_Invalid_Name(string invalidName)
{
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => new Category(invalidName));
}
```

### 5. Committing Changes

#### Commit Message Format

Use conventional commit format:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

**Types:**
- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `style`: Changes that do not affect the meaning of the code
- `refactor`: A code change that neither fixes a bug nor adds a feature
- `perf`: A code change that improves performance
- `test`: Adding missing tests or correcting existing tests
- `chore`: Changes to the build process or auxiliary tools

**Examples:**
```
feat(api): add product categories endpoint

Add new endpoint for managing product categories including
CRUD operations and category assignment to products.

Closes #123

fix(frontend): resolve product list pagination issue

The product list was not properly handling pagination when
the total items were less than the page size.

Fixes #456

docs(readme): update installation instructions

Add missing Node.js version requirement and clarify
database setup steps.
```

#### Making Good Commits

- Make atomic commits (one logical change per commit)
- Write clear, descriptive commit messages
- Keep commits focused and small
- Test each commit independently

```bash
# Stage your changes
git add .

# Commit with a descriptive message
git commit -m "feat(api): add product search functionality"

# Push to your fork
git push origin feature/add-product-search
```

## Coding Standards

### Backend (.NET/C#)

#### General Guidelines

- Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- Use PascalCase for public members
- Use camelCase for private fields and local variables
- Use meaningful and descriptive names
- Add XML documentation for public APIs

#### Code Style

```csharp
// Good
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all products from the inventory.
    /// </summary>
    /// <returns>A collection of products.</returns>
    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        _logger.LogInformation("Retrieving all products");
        
        var products = await _productRepository.GetAllAsync();
        
        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            SKU = p.SKU,
            Price = p.Price,
            StockQuantity = p.StockQuantity
        });
    }
}
```

#### Architecture Guidelines

- Follow Clean Architecture principles
- Keep domain logic in the Domain layer
- Use dependency injection
- Implement proper error handling
- Use async/await for I/O operations

### Frontend (React/TypeScript)

#### General Guidelines

- Use TypeScript for type safety
- Follow React best practices
- Use functional components with hooks
- Implement proper error handling
- Use meaningful component and variable names

#### Code Style

```typescript
// Good
interface ProductCardProps {
  product: Product;
  onEdit: (product: Product) => void;
  onDelete: (productId: number) => void;
}

export const ProductCard: React.FC<ProductCardProps> = ({
  product,
  onEdit,
  onDelete
}) => {
  const [isLoading, setIsLoading] = useState(false);

  const handleDelete = async () => {
    if (!confirm('Are you sure you want to delete this product?')) {
      return;
    }

    setIsLoading(true);
    try {
      await onDelete(product.id);
    } catch (error) {
      console.error('Failed to delete product:', error);
      // Handle error appropriately
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="product-card">
      <h3>{product.name}</h3>
      <p>SKU: {product.sku}</p>
      <p>Price: ${product.price.toFixed(2)}</p>
      <p>Stock: {product.stockQuantity}</p>
      
      <div className="product-card__actions">
        <button onClick={() => onEdit(product)}>
          Edit
        </button>
        <button 
          onClick={handleDelete} 
          disabled={isLoading}
          className="button--danger"
        >
          {isLoading ? 'Deleting...' : 'Delete'}
        </button>
      </div>
    </div>
  );
};
```

### Database

#### Migration Guidelines

- Always create migrations for schema changes
- Use descriptive migration names
- Review generated migrations before applying
- Test migrations on sample data

```bash
# Create migration
dotnet ef migrations add AddProductCategoryTable

# Review the generated migration file
# Update database
dotnet ef database update
```

### Testing Guidelines

#### Unit Tests

- Follow AAA pattern (Arrange, Act, Assert)
- Use descriptive test names
- Test one thing per test
- Mock external dependencies

```csharp
[Fact]
public void ProductService_GetById_Should_Return_Product_When_Exists()
{
    // Arrange
    var productId = 1;
    var expectedProduct = new Product("Test Product", "TEST001", 99.99m, 10);
    
    _mockRepository
        .Setup(r => r.GetByIdAsync(productId))
        .ReturnsAsync(expectedProduct);

    // Act
    var result = await _productService.GetByIdAsync(productId);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(expectedProduct.Name, result.Name);
    Assert.Equal(expectedProduct.SKU, result.SKU);
}
```

#### Integration Tests

- Test complete workflows
- Use test database
- Clean up test data

#### API Tests

- Test all endpoints
- Test error scenarios
- Validate response formats

## Pull Request Process

### Before Submitting

1. **Sync with upstream**: Ensure your branch is up to date
2. **Run tests**: All tests must pass
3. **Check code quality**: Follow coding standards
4. **Update documentation**: Include relevant documentation updates
5. **Self-review**: Review your own changes first

```bash
# Sync with upstream
git fetch upstream
git checkout main
git merge upstream/main
git checkout your-feature-branch
git rebase main

# Run tests
dotnet test
newman run tests/postman/collection.json
```

### Pull Request Template

Use this template for your PR description:

```markdown
## Description
Brief description of the changes made.

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)  
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Changes Made
- List specific changes made
- Include any new files added
- Mention any files deleted

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] API tests added/updated
- [ ] All tests pass locally

## Documentation
- [ ] Code comments added/updated
- [ ] API documentation updated
- [ ] README updated (if needed)

## Screenshots (if applicable)
Add screenshots or GIFs showing the changes.

## Additional Notes
Any additional information or context.

## Checklist
- [ ] My code follows the style guidelines
- [ ] I have performed a self-review
- [ ] I have commented my code where needed
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix is effective or feature works
- [ ] New and existing unit tests pass locally
```

### Review Process

1. **Automated Checks**: CI pipeline runs all tests
2. **Code Review**: Maintainers review the code
3. **Feedback**: Address any feedback or requested changes
4. **Approval**: Once approved, the PR will be merged

### Addressing Feedback

- Respond to all comments
- Make requested changes
- Add new commits for changes (don't squash during review)
- Resolve conversations when addressed

```bash
# Make changes based on feedback
# Commit the changes
git add .
git commit -m "fix: address PR feedback on validation logic"
git push origin your-feature-branch
```

## Issue Reporting

### Bug Reports

Use the following template for bug reports:

```markdown
**Bug Description**
A clear and concise description of the bug.

**Steps to Reproduce**
1. Go to '...'
2. Click on '....'
3. Scroll down to '....'
4. See error

**Expected Behavior**
A clear and concise description of what you expected to happen.

**Actual Behavior**
A clear and concise description of what actually happened.

**Screenshots**
If applicable, add screenshots to help explain your problem.

**Environment:**
- OS: [e.g. Windows 10, macOS Big Sur, Ubuntu 20.04]
- Browser [if applicable]: [e.g. Chrome 91, Firefox 89]
- .NET Version: [e.g. .NET 9.0]
- Node.js Version: [e.g. 18.0.0]

**Additional Context**
Add any other context about the problem here.
```

### Feature Requests

Use the following template for feature requests:

```markdown
**Feature Description**
A clear and concise description of the feature you'd like to see.

**Problem Statement**
Describe the problem this feature would solve.

**Proposed Solution**
Describe how you envision this feature working.

**Alternatives Considered**
Describe any alternative solutions or features you've considered.

**Additional Context**
Add any other context, screenshots, or examples about the feature request.

**Implementation Ideas**
If you have ideas about how this could be implemented, please share them.
```

## Recognition

We value all contributions and will recognize contributors in the following ways:

- Contributors will be listed in the project's contributors section
- Significant contributions will be mentioned in release notes
- Active contributors may be invited to become maintainers

## Getting Help

If you need help or have questions:

1. **Check existing documentation**: README, docs folder, and code comments
2. **Search existing issues**: Someone might have already asked the same question
3. **Create a discussion**: Use GitHub Discussions for questions
4. **Join our community**: [Community channels if available]

## Resources

### Documentation
- [Architecture Guide](ARCHITECTURE.md)
- [Development Guide](DEVELOPMENT.md) 
- [API Documentation](API.md)
- [Testing Guide](TESTING.md)
- [Deployment Guide](DEPLOYMENT.md)

### External Resources
- [Clean Architecture Principles](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [.NET Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [React Best Practices](https://reactjs.org/docs/thinking-in-react.html)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)

Thank you for contributing to the Demo Inventory Microservice! 🎉