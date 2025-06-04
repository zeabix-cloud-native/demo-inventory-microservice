# Test Documentation

This directory contains test files for the Demo Inventory Microservice.

## Current Test Structure

### Unit Tests (xUnit)
- **DemoInventory.Domain.Tests**: Domain layer unit tests
- **DemoInventory.Application.Tests**: Application layer unit tests

These tests are currently integrated into the CI pipeline and run automatically on every build.

## Future Test Integration

The CI pipeline is prepared for additional test types. To enable them:

### Postman API Tests

1. Create a `tests/postman/` directory
2. Add your Postman collection file: `tests/postman/collection.json`
3. Add environment file: `tests/postman/environment.json`
4. In `.github/workflows/ci.yml`, change `if: false` to `if: true` for the `postman-tests` job

Example collection structure:
```
tests/
├── postman/
│   ├── collection.json
│   └── environment.json
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

### Postman Tests (when available)
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