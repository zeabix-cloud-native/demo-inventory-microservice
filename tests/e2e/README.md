# Demo Inventory Microservice - E2E Tests

This directory contains end-to-end tests for the Demo Inventory Microservice using Cypress.

## Prerequisites

- Node.js (v18 or higher)
- npm (v8 or higher)
- Demo Inventory API running on the appropriate port (see Running Tests section)

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

### Option 1: With Local API (CI Environment)

Prerequisites - Start the API Server locally:

```bash
# From the project root directory
dotnet run --project backend/src/DemoInventory.API
```

The API should be available at `http://localhost:5126`.

**Run Tests in Headless Mode**

```bash
npm run test:e2e
```

**Run Tests in Headed Mode (with browser UI)**

```bash
npm run test:e2e:headed
```

**Open Cypress Test Runner**

```bash
npm run cypress:open
```

### Option 2: With Docker Stack

This approach runs tests against the Docker-based API (port 5000).

**Prerequisites - Start the Docker Stack**

```bash
# From the project root directory
docker compose up -d
```

The API will be available at `http://localhost:5000`.

**Run Tests Against Docker Stack**

```bash
# Headless mode
npm run test:e2e:docker

# Headed mode
npm run test:e2e:docker:headed

# Open Cypress GUI against Docker
npm run cypress:open:docker
```

### Option 3: Fully Containerized Testing

Run Cypress tests completely within Docker containers:

```bash
# From the project root directory
# Start the main stack first
docker compose up -d

# Run Cypress tests in container (one-time)
docker compose run --rm cypress

# Or start services with test profile to include Cypress
docker compose --profile test up -d
```

## Test Structure

The tests are organized as follows:

```
cypress/
├── e2e/
│   ├── products-create.cy.js      # Product creation tests
│   ├── products-view.cy.js        # Product retrieval tests
│   └── products-e2e-flow.cy.js    # Complete workflow tests
├── support/
│   ├── e2e.js                     # Global configuration
│   └── commands.js                # Custom Cypress commands
└── reports/                       # Test reports (CTRF format)
```

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

The Cypress configuration is defined in `cypress.config.js` and supports flexible environments:

### Default Configuration (CI Environment)
- Base URL: `http://localhost:5126`
- API URL: `http://localhost:5126/api`

### Docker Environment Configuration
Set via environment variables:
- `CYPRESS_API_BASE_URL=http://localhost:5000`
- `CYPRESS_API_URL=http://localhost:5000/api`

### Environment Variables

- `CYPRESS_API_BASE_URL` - Override the base URL for the application
- `CYPRESS_API_URL` - Override the API endpoint URL

## Notes

- The Demo Inventory Microservice uses an in-memory repository, so data is reset when the API is restarted
- Tests are designed to be independent and can run in any order
- The tests focus on API endpoints rather than UI interactions since this is a microservice
- Each test suite can be run independently
- Configuration automatically adapts to different environments (CI vs Docker)