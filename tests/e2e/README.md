# Demo Inventory Microservice - E2E Tests

This directory contains end-to-end tests for the Demo Inventory Microservice using Cypress.

## Prerequisites

- Node.js (v18 or higher)
- npm (v8 or higher)
- Demo Inventory API running on `http://localhost:5126`

## Setup

1. Navigate to the e2e tests directory:
   ```bash
   cd tests/e2e
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

## Running Tests

### Prerequisites - Start the API Server

Before running the E2E tests, make sure the Demo Inventory API is running:

```bash
# From the project root directory
dotnet run --project src/DemoInventory.API
```

The API should be available at `http://localhost:5126`.

### Run Tests in Headless Mode

```bash
npm run test:e2e
```

### Run Tests in Headed Mode (with browser UI)

```bash
npm run test:e2e:headed
```

### Open Cypress Test Runner

```bash
npm run cypress:open
```

## Test Structure

The E2E tests are organized into two main test suites:

### 1. Product View Tests (`cypress/e2e/products-view.cy.js`)

Tests for viewing and retrieving products:
- Get all products (empty list initially)
- Get all products after creating some
- Get product by ID
- Handle 404 for non-existent products
- Get product by SKU
- Search products by name

### 2. Product Create Tests (`cypress/e2e/products-create.cy.js`)

Tests for creating products:
- Create product with valid data
- Create multiple products successfully
- Create product with minimum required fields
- Create product with zero quantity
- Create product with high price
- Handle decimal prices correctly
- End-to-end validation flow

## Test Data

The tests use the following sample data structure:

```javascript
{
  name: 'Product Name',
  description: 'Product Description',
  sku: 'UNIQUE-SKU',
  price: 29.99,
  quantityInStock: 100
}
```

## Custom Commands

The tests include several custom Cypress commands defined in `cypress/support/commands.js`:

- `cy.createProduct(productData)` - Create a product via API
- `cy.getAllProducts()` - Get all products
- `cy.getProductById(id)` - Get product by ID
- `cy.deleteProduct(id)` - Delete product by ID
- `cy.waitForApi()` - Wait for API to be ready
- `cy.clearDatabase()` - Clear database (logs message for in-memory repo)

## Configuration

The Cypress configuration is defined in `cypress.config.js`:

- Base URL: `http://localhost:5126`
- API URL: `http://localhost:5126/api` (via environment variable)
- Viewport: 1280x720
- Screenshots on failure: enabled
- Video recording: disabled

## Notes

- The Demo Inventory Microservice uses an in-memory repository, so data is reset when the API is restarted
- Tests are designed to be independent and can run in any order
- The tests focus on API endpoints rather than UI interactions since this is a microservice
- Each test suite can be run independently