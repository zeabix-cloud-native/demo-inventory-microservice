# Test Documentation

This directory contains test files for the Demo Inventory Microservice.

## Current Test Structure

### Unit Tests (xUnit)
- **DemoInventory.Domain.Tests**: Domain layer unit tests
- **DemoInventory.Application.Tests**: Application layer unit tests

### Postman API Tests
- **tests/postman/collection.json**: Comprehensive Postman collection with test scripts for all Product endpoints
- **tests/postman/environment.json**: Environment configuration for the collection

These tests are currently integrated into the CI pipeline and run automatically on every build.

## Future Test Integration

The CI pipeline is prepared for additional test types. To enable them:

### Postman API Tests

✅ **Available and Enabled** - The Postman collection is now available and enabled in CI:

- Collection file: `tests/postman/collection.json`
- Environment file: `tests/postman/environment.json`
- CI job enabled in `.github/workflows/ci.yml`

The collection includes comprehensive test scripts for all Product endpoints:
- Get All Products
- Create Product  
- Get Product by ID
- Get Product by SKU
- Search Products
- Update Product
- Delete Product
- Error testing (404 scenarios)

To use locally:
```bash
# Install Newman
npm install -g newman

# Run collection
newman run tests/postman/collection.json --environment tests/postman/environment.json
```

### Cypress E2E Tests

1. Initialize Cypress in the project root:
   ```bash
   npm init -y
   npm install --save-dev cypress
   npx cypress open
   ```
2. Configure `cypress.config.js` in the project root
3. Add E2E tests in `cypress/e2e/`
4. In `.github/workflows/ci.yml`, change `if: false` to `if: true` for the `cypress-tests` job

Example Cypress structure:
```
cypress/
├── e2e/
│   └── api.cy.js
├── fixtures/
└── support/
cypress.config.js
package.json
```

## Running Tests Locally

### Unit Tests
```bash
dotnet test
```

### Postman Tests
```bash
# Install Newman
npm install -g newman

# Run collection
newman run tests/postman/collection.json --environment tests/postman/environment.json
```

### Cypress Tests (when available)
```bash
# Install dependencies
npm install

# Run Cypress
npx cypress run

# Or open Cypress GUI
npx cypress open
```