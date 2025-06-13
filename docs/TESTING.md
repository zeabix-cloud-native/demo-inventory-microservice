# Testing Documentation

## Overview

The Demo Inventory Microservice implements a comprehensive testing strategy that covers all layers of the application and includes automated test reporting using the Common Test Report Format (CTRF). The testing pyramid includes unit tests, integration tests, API tests, and end-to-end tests.

## Testing Strategy

```
    ┌─────────────────────────────────────┐
    │            E2E Tests                │  ← Few, high-value
    │         (Cypress)                   │
    └─────────────────────────────────────┘
           ┌─────────────────────────────────────┐
           │          API Tests                  │  ← More focused
           │        (Postman/Newman)             │
           └─────────────────────────────────────┘
                  ┌─────────────────────────────────────┐
                  │         Integration Tests           │  ← Some
                  │       (API Controllers)             │
                  └─────────────────────────────────────┘
                         ┌─────────────────────────────────────┐
                         │           Unit Tests                │  ← Many, fast
                         │   (Domain & Application Logic)      │
                         └─────────────────────────────────────┘
```

## Test Types

### 1. Unit Tests (.NET xUnit)

#### Coverage
- **Domain Layer**: Business logic, entities, value objects
- **Application Layer**: Services, use cases, DTOs
- **Infrastructure Layer**: Repository implementations
- **API Layer**: Controller logic

#### Location
```
backend/tests/
├── DemoInventory.Domain.Tests/
├── DemoInventory.Application.Tests/
├── DemoInventory.Infrastructure.Tests/
└── DemoInventory.API.Tests/
```

#### Running Unit Tests

```bash
# Run all unit tests
dotnet test

# Run specific test project
dotnet test backend/tests/DemoInventory.Domain.Tests

# Run with detailed output
dotnet test --verbosity normal

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

#### Example Unit Test

```csharp
[Fact]
public void Product_Creation_Should_Set_Properties_Correctly()
{
    // Arrange
    var name = "Test Product";
    var sku = "TEST001";
    var price = 99.99m;
    var stockQuantity = 10;

    // Act
    var product = new Product(name, sku, price, stockQuantity);

    // Assert
    Assert.Equal(name, product.Name);
    Assert.Equal(sku, product.SKU);
    Assert.Equal(price, product.Price);
    Assert.Equal(stockQuantity, product.StockQuantity);
}

[Theory]
[InlineData("")]
[InlineData(null)]
[InlineData("   ")]
public void Product_Creation_Should_Throw_When_Name_Invalid(string invalidName)
{
    // Act & Assert
    Assert.Throws<ArgumentException>(() => 
        new Product(invalidName, "SKU001", 99.99m, 10));
}
```

### 2. API Tests (Postman/Newman)

#### Coverage
- **REST API Endpoints**: All CRUD operations
- **Request/Response Validation**: Data formats and validation
- **Error Handling**: 404, 400, 500 error scenarios
- **Business Logic**: End-to-end business workflows

#### Location
```
tests/postman/
├── collection.json       # Main test collection
└── environment.json      # Environment variables
```

#### Running API Tests

```bash
# Install Newman globally
npm install -g newman

# Option 1: Automatic environment detection (recommended)
cd tests/postman && ./run-newman.sh

# Option 2: Specify environment explicitly
cd tests/postman
./run-newman.sh local    # For local development (port 5126)
./run-newman.sh docker   # For Docker environment (port 5000)

# Option 3: Use Node.js runner (with npm)
cd tests/postman
npm run test          # Auto-detect environment
npm run test:local    # Local environment
npm run test:docker   # Docker environment
npm run test:report   # With HTML report generation

# Option 4: Direct Newman commands (manual environment selection)
newman run tests/postman/collection.json \
  --environment tests/postman/environment.json

# Run with detailed reporting
newman run tests/postman/collection.json \
  --environment tests/postman/environment.json \
  --reporters cli,htmlextra \
  --reporter-htmlextra-export report.html

# Run with CTRF reporting
newman run tests/postman/collection.json \
  --environment tests/postman/environment.json \
  --reporters ctrf-json \
  --reporter-ctrf-json-export api-test-results.json
```

#### Test Collection Structure

The Postman collection includes:

- **Get All Products**: Retrieve product list
- **Create Product**: Add new product with validation
- **Get Product by ID**: Retrieve specific product
- **Get Product by SKU**: Search by SKU
- **Search Products**: Search by name
- **Update Product**: Modify existing product
- **Delete Product**: Remove product
- **Error Scenarios**: 404 and validation errors

#### Example Test Scripts

```javascript
// Test: Create Product
pm.test("Status code is 201", function () {
    pm.response.to.have.status(201);
});

pm.test("Response has correct structure", function () {
    const responseJson = pm.response.json();
    pm.expect(responseJson).to.have.property('id');
    pm.expect(responseJson).to.have.property('name');
    pm.expect(responseJson).to.have.property('sku');
    pm.expect(responseJson).to.have.property('price');
});

pm.test("Product name matches request", function () {
    const requestJson = JSON.parse(pm.request.body.raw);
    const responseJson = pm.response.json();
    pm.expect(responseJson.name).to.eql(requestJson.name);
});

// Store product ID for subsequent tests
if (pm.response.code === 201) {
    const responseJson = pm.response.json();
    pm.environment.set("productId", responseJson.id);
}
```

### 3. Frontend Component Tests (Vitest + React Testing Library)

#### Coverage
- **Component Logic**: Individual component validation
- **Form Validation**: Price validation, required fields, error clearing
- **UI State**: Low stock warnings, price formatting, description truncation
- **User Interactions**: Form submission, input handling

#### Location
```
frontend/src/test/
├── ProductForm.test.tsx      # Form validation tests
├── ProductList.test.tsx      # List display and formatting tests
└── setup.ts                  # Test configuration
```

#### Running Component Tests

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Run tests
npm test

# Run tests with UI
npm run test:ui

# Run with coverage
npm run test:coverage
```

#### Example Component Test

```typescript
describe('ProductForm', () => {
  it('should show price validation error for negative values', async () => {
    renderProductForm()
    
    fireEvent.change(screen.getByTestId('product-price-input'), {
      target: { value: '-10' }
    })
    
    fireEvent.click(screen.getByTestId('submit-btn'))
    
    await waitFor(() => {
      expect(screen.getByTestId('price-error')).toBeVisible()
      expect(screen.getByTestId('price-error')).toHaveTextContent('Price must be greater than 0')
    })
  })
})
```

### 4. End-to-End Tests (Cypress)

#### Coverage
- **User Workflows**: Complete user journeys
- **UI Integration**: Frontend-backend integration
- **Cross-browser Testing**: Multiple browser support
- **Visual Testing**: UI component rendering

#### Location
```
tests/e2e/
├── cypress/
│   ├── e2e/           # Test specifications
│   ├── fixtures/      # Test data
│   ├── support/       # Helper functions
│   └── reports/       # Test reports
├── cypress.config.js  # Cypress configuration
└── package.json       # Dependencies
```

#### Running E2E Tests

```bash
# Navigate to E2E test directory
cd tests/e2e

# Install dependencies
npm install

# Run tests headlessly
npm run test:e2e

# Open Cypress GUI
npm run cypress:open

# Run specific test
npx cypress run --spec "cypress/e2e/products.cy.js"
```

#### Recording Test Results

Cypress tests can be run with recording capabilities for better debugging and reporting:

```bash
# Run tests with video recording
npm run test:e2e:video

# Run tests with video recording (headed mode)
npm run test:e2e:video:headed

# Run tests with Cypress Dashboard recording (requires setup)
export CYPRESS_PROJECT_ID=your-project-id
export CYPRESS_RECORD_KEY=your-record-key
npm run test:e2e:record

# Docker environment with recording
npm run test:e2e:docker:video
npm run test:e2e:docker:record
```

**Recording Features:**
- **Video Recording**: Captures test execution videos for debugging failed tests
- **Screenshots**: Automatically captures screenshots on test failures
- **Cypress Dashboard**: Uploads results to Cypress Dashboard for analytics and reporting
- **CTRF Reports**: Generates standardized test result reports for CI/CD integration

#### Current E2E Test Suite

The test suite includes three main test files:

**products-create.cy.js**: Tests product creation functionality
- Form validation and error handling
- Required field validation
- Price validation (including negative values)
- Successful product creation workflow

**products-view.cy.js**: Tests product listing and interaction
- Product display in table format
- Search functionality
- Edit and delete operations
- Low stock warning display

**products-e2e-flow.cy.js**: Tests complete user workflows
- Full product lifecycle (create, view, edit, delete)
- Complex scenarios with multiple products
- Edge cases and error states
- UI state consistency

#### E2E Test Example

```javascript
describe('Product Create Frontend E2E Tests', () => {
  beforeEach(() => {
    cy.visitProductList()
  })

  it('should validate price field accepts only positive numbers', () => {
    cy.visit('/product/new')
    
    // Fill required fields
    cy.get('[data-testid="product-name-input"]').type('Test Product')
    cy.get('[data-testid="product-sku-input"]').type('TEST-001')
    cy.get('[data-testid="product-description-input"]').type('Test description')
    
    // Try negative price
    cy.get('[data-testid="product-price-input"]').clear().type('-10')
    cy.get('[data-testid="product-quantity-input"]').clear().type('5')
    
    cy.get('[data-testid="submit-btn"]').click()
    cy.get('[data-testid="price-error"]').should('be.visible')
  })
})
  });

  it('should search products', () => {
    cy.get('[data-cy=search-input]').type('Laptop');
    cy.get('[data-cy=search-button]').click();
    
    cy.get('[data-cy=product-item]').each(($el) => {
      cy.wrap($el).should('contain.text', 'Laptop');
    });
  });
});
```

## CTRF Test Result Reporting

### What is CTRF?

Common Test Report Format (CTRF) is a standardized JSON schema for test results that enables:

- **Unified Reporting**: Consistent format across different testing frameworks
- **Aggregation**: Merge results from multiple test suites
- **Rich Integration**: GitHub Actions integration with summaries and PR comments
- **Visual Dashboards**: Consistent test result visualization

### CTRF Integration

The CI pipeline automatically generates CTRF reports for:

- **xUnit Tests**: Converted from JUnit XML using `junit-to-ctrf`
- **Postman API Tests**: Generated directly using `newman-reporter-ctrf-json`
- **Cypress E2E Tests**: Generated using `cypress-ctrf-json-reporter`

### Local CTRF Generation

```bash
# Install CTRF tools
npm install -g ctrf junit-to-ctrf newman-reporter-ctrf-json

# Generate CTRF for unit tests
dotnet test --logger junit --results-directory TestResults
junit-to-ctrf "TestResults/*.xml" -o ctrf-unit-tests.json

# Generate CTRF for API tests
newman run tests/postman/collection.json \
  --environment tests/postman/environment.json \
  --reporters ctrf-json \
  --reporter-ctrf-json-export ctrf-api-tests.json

# Generate CTRF for E2E tests (automatic with Cypress configuration)
cd tests/e2e && npm run test:e2e

# Merge all CTRF reports
ctrf merge . --output merged-ctrf-report.json
```

### CTRF Report Structure

```json
{
  "results": {
    "tool": {
      "name": "xUnit",
      "version": "2.4.2"
    },
    "summary": {
      "tests": 45,
      "passed": 42,
      "failed": 2,
      "skipped": 1,
      "pending": 0,
      "other": 0,
      "start": 1234567890,
      "stop": 1234567920
    },
    "tests": [
      {
        "name": "Product_Creation_Should_Set_Properties_Correctly",
        "status": "passed",
        "duration": 123
      }
    ]
  }
}
```

## Continuous Integration Testing

### GitHub Actions Pipeline

The CI pipeline includes dedicated jobs for each test type:

```yaml
jobs:
  build-and-test:    # Unit tests
  postman-tests:     # API tests  
  cypress-tests:     # E2E tests
  test-results:      # Aggregate and report
```

### Test Execution Flow

1. **Build and Test**: Run unit tests, generate CTRF
2. **API Tests**: Start API, run Postman collection, generate CTRF
3. **E2E Tests**: Start full stack, run Cypress tests, generate CTRF
4. **Aggregate Results**: Merge CTRF reports, publish to GitHub

### Test Reporting Features

- **PR Comments**: Test results automatically commented on pull requests
- **Workflow Summaries**: Rich test summaries in GitHub Actions
- **Artifact Storage**: Test reports and screenshots stored as artifacts
- **Failure Analysis**: Detailed failure information and screenshots

## Test Data Management

### Unit Test Data

```csharp
public class ProductTestData
{
    public static readonly Product ValidProduct = new("Laptop", "LAP001", 999.99m, 10);
    
    public static IEnumerable<object[]> InvalidProductData =>
        new List<object[]>
        {
            new object[] { "", "SKU001", 99.99m, 10 },
            new object[] { "Product", "", 99.99m, 10 },
            new object[] { "Product", "SKU001", -1m, 10 },
            new object[] { "Product", "SKU001", 99.99m, -1 }
        };
}
```

### API Test Data

```json
// tests/postman/environment.json
{
  "id": "test-environment",
  "name": "Demo Inventory Test Environment",
  "values": [
    {
      "key": "baseUrl",
      "value": "http://localhost:5126",
      "enabled": true
    },
    {
      "key": "testProductName",
      "value": "API Test Product",
      "enabled": true
    }
  ]
}
```

### E2E Test Data

```javascript
// cypress/fixtures/products.json
{
  "validProduct": {
    "name": "Cypress Test Product",
    "sku": "CYP001",
    "price": 149.99,
    "stockQuantity": 25
  },
  "products": [
    {
      "name": "Test Laptop",
      "sku": "LAP001",
      "price": 999.99,
      "stockQuantity": 5
    }
  ]
}
```

## Test Environment Setup

### Local Development Testing

```bash
# Terminal 1: Start API
dotnet run --project backend/src/DemoInventory.API

# Terminal 2: Start Frontend (for E2E tests)
cd frontend && npm run dev

# Terminal 3: Run tests
dotnet test                    # Unit tests
newman run tests/postman/...   # API tests
cd tests/e2e && npm run test:e2e  # E2E tests
```

### Docker Testing Environment

```bash
# Start full stack for testing
docker-compose up -d

# Wait for services to be ready
docker-compose logs -f api

# Run tests against Docker environment
newman run tests/postman/collection.json \
  --environment tests/postman/docker-environment.json
```

## Best Practices

### Unit Testing

- **AAA Pattern**: Arrange, Act, Assert
- **Single Responsibility**: One assertion per test
- **Meaningful Names**: Descriptive test method names
- **Independent Tests**: No test dependencies
- **Fast Execution**: Mock external dependencies

### API Testing

- **Data Cleanup**: Clean up test data after execution
- **Environment Variables**: Use environment-specific configurations
- **Error Testing**: Test both success and failure scenarios
- **Response Validation**: Validate response structure and data
- **Performance Testing**: Include response time assertions

### E2E Testing

- **Page Object Pattern**: Organize UI interactions
- **Data Attributes**: Use `data-cy` attributes for element selection
- **Wait Strategies**: Proper waiting for elements and actions
- **Test Isolation**: Each test should be independent
- **Visual Testing**: Include screenshot comparisons where appropriate

### General Testing

- **Test Documentation**: Comment complex test scenarios
- **Continuous Integration**: All tests run on CI/CD
- **Test Reports**: Generate comprehensive test reports
- **Coverage Tracking**: Monitor test coverage metrics
- **Regular Maintenance**: Keep tests updated with code changes

## Troubleshooting

### Common Issues

#### Cypress E2E Test Failures

**Price Validation Issues**
- **Problem**: Tests unable to enter negative prices for validation testing
- **Solution**: Removed HTML `min="0"` constraint from price input to allow negative values for testing validation logic
- **Impact**: JavaScript validation now properly handles negative prices without HTML constraints interfering

**Search Functionality Issues**  
- **Problem**: Search input not cleared when using clear search button
- **Solution**: Added dedicated `handleClearSearch` function that clears both search term state and reloads products
- **Impact**: Search state properly resets after clear button is clicked

**Test Timing Issues**
- **Problem**: Tests failing due to race conditions between API calls and UI updates
- **Solution**: Enhanced `createProductViaUI` custom command to wait for successful navigation and product visibility
- **Impact**: Reduced timing-related test failures

**Text Truncation Mismatches**
- **Problem**: Tests expecting different truncated text than actual implementation
- **Solution**: Updated test expectations to match actual 50-character truncation with "..." suffix
- **Impact**: Tests now correctly validate description truncation behavior

#### Unit Test Failures

- **Database Context**: Use in-memory database for tests
- **Async/Await**: Properly handle async operations in tests
- **Mocking**: Verify mock configurations and expectations

#### API Test Failures

- **Service Availability**: Ensure API is running and accessible
- **Environment Variables**: Verify correct environment configuration
- **Data State**: Check if test data conflicts with existing data

#### E2E Test Failures

- **Element Timing**: Add proper waits for dynamic elements
- **Browser Compatibility**: Test across different browsers
- **Test Data**: Ensure test data is available and consistent

### Debugging Tests

```bash
# Debug unit tests
dotnet test --logger console --verbosity detailed

# Debug API tests with verbose output
newman run collection.json --verbose

# Debug E2E tests with browser open
npx cypress open --config video=false
```