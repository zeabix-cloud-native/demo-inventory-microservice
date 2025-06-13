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

✅ **Available and Ready** - The Cypress E2E tests are now available in the `tests/e2e/` directory:

- Test files: `tests/e2e/cypress/e2e/*.cy.js`
- Configuration: `tests/e2e/cypress.config.js`
- Custom commands: `tests/e2e/cypress/support/commands.js`

The test suite includes comprehensive E2E tests for:
- Product creation with form validation
- Product listing and search functionality
- Product editing and deletion
- Low stock warnings
- Complete workflow testing
- Error handling scenarios

To run locally:
```bash
cd tests/e2e
npm install
npm run cypress:open  # Interactive mode
npm run cypress:run   # Headless mode
```

### Frontend Component Tests

✅ **Available and Ready** - Frontend component tests using Vitest and React Testing Library:

- Test files: `frontend/src/test/*.test.tsx`
- Configuration: `frontend/vitest.config.ts`
- Test setup: `frontend/src/test/setup.ts`

The component test suite includes:
- ProductForm validation logic testing
- ProductList low stock warning functionality
- Price formatting and display logic
- Form error handling and clearing

To run locally:
```bash
cd frontend
npm install
npm test          # Run tests
npm run test:ui   # Interactive test UI
npm run test:coverage  # Run with coverage
```

To enable in CI pipeline:
1. In `.github/workflows/ci.yml`, change `if: false` to `if: true` for the `cypress-tests` job

Example Cypress structure:
```
tests/e2e/
├── cypress/
│   ├── e2e/
│   │   ├── products-create.cy.js
│   │   ├── products-view.cy.js
│   │   └── products-e2e-flow.cy.js
│   └── support/
│       ├── commands.js
│       └── e2e.js
├── cypress.config.js
└── package.json
```

Example Frontend component test structure:
```
frontend/
├── src/
│   ├── test/
│   │   ├── ProductForm.test.tsx
│   │   ├── ProductList.test.tsx
│   │   └── setup.ts
│   └── components/
├── vitest.config.ts
└── package.json
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

# Option 1: Automatic environment detection (recommended)
cd tests/postman && ./run-newman.sh

# Option 2: Specify environment
cd tests/postman
./run-newman.sh local    # Local development (port 5126)
./run-newman.sh docker   # Docker environment (port 5000)

# Option 3: Manual environment selection
newman run tests/postman/collection.json --environment tests/postman/environment.json
```

### Cypress Tests (when available)
```bash
# Navigate to E2E test directory
cd tests/e2e

# Install dependencies
npm install

# Run Cypress tests
npm run test:e2e

# Run with video recording
npm run test:e2e:video

# Run with Cypress Dashboard recording (requires setup)
export CYPRESS_PROJECT_ID=your-project-id
export CYPRESS_RECORD_KEY=your-record-key
npm run test:e2e:record

# Open Cypress GUI
npm run cypress:open
```

## CTRF Test Result Reporting

The project now includes **Common Test Report Format (CTRF)** integration for unified test result reporting across all test types. CTRF provides a standardized JSON schema for test results that can be aggregated and displayed in GitHub Actions.

### What is CTRF?

CTRF (Common Test Report Format) is a standardized JSON schema for test results that enables:
- Unified reporting across different testing frameworks
- Aggregation of results from multiple test suites
- Rich GitHub Actions integration with summaries and PR comments
- Consistent test result visualization

### CTRF Integration Features

The CI pipeline automatically generates CTRF reports for:
- **xUnit Tests**: Converted from JUnit XML using `junit-to-ctrf`
- **Postman API Tests**: Generated directly using `newman-reporter-ctrf-json`
- **Cypress E2E Tests**: Generated using `cypress-ctrf-json-reporter`

### GitHub Actions Integration

The CI pipeline includes a dedicated `test-results` job that:
1. Downloads CTRF reports from all test jobs
2. Merges them into a single aggregated report using `ctrf merge`
3. Publishes the results using `github-actions-ctrf`
4. Displays test summaries in workflow runs and PR comments

### Local CTRF Generation

To generate CTRF reports locally:

```bash
# Install CTRF tools
npm install -g ctrf junit-to-ctrf newman-reporter-ctrf-json

# Run xUnit tests and generate CTRF
dotnet test --logger junit --results-directory TestResults
junit-to-ctrf "TestResults/*.xml" -o ctrf-unit-tests.json

# Run Postman tests with CTRF reporter
newman run tests/postman/collection.json \
  --environment tests/postman/environment.json \
  --reporters ctrf-json \
  --reporter-ctrf-json-export ctrf-api-tests.json

# Run Cypress tests (CTRF report generated automatically to cypress/reports/)
cd tests/e2e && npm run test:e2e

# Run Cypress tests with video recording
cd tests/e2e && npm run test:e2e:video

# Run Cypress tests with Dashboard recording
cd tests/e2e && npm run test:e2e:record

# Merge all CTRF reports
ctrf merge . --output merged-ctrf-report.json
```