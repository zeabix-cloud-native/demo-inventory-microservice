# Demo Inventory Microservice - E2E Tests

This directory contains end-to-end tests for the Demo Inventory Microservice frontend using Cypress.

## Prerequisites

- Node.js (v20 or higher)
- npm (v8 or higher)
- Demo Inventory API running
- Demo Inventory frontend application running

## Setup

1. Navigate to the e2e tests directory:
   ```bash
   cd tests/e2e
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. (Optional) Configure recording settings:
   ```bash
   # Copy the example environment file
   cp .env.example .env
   
   # Edit .env file to add your Cypress Dashboard credentials (if using recording)
   # CYPRESS_PROJECT_ID=your-project-id
   # CYPRESS_RECORD_KEY=your-record-key
   ```

## Running Tests

### Option 1: With Local Development Stack (Default)

Prerequisites - Start both the API server and frontend application locally:

**Start the API Server:**
```bash
# From the project root directory
dotnet run --project backend/src/DemoInventory.API
```

The API should be available at `http://localhost:5126`.

**Start the Frontend Application:**
```bash
# From the project root directory
cd frontend
npm install
npm run dev
```

The frontend should be available at `http://localhost:5173`.

**Run Frontend E2E Tests:**

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

### Recording Test Results

The test suite supports recording test results for better debugging and reporting:

**Run Tests with Video Recording**

```bash
npm run test:e2e:video
```

**Run Tests with Video Recording (Headed Mode)**

```bash
npm run test:e2e:video:headed
```

**Run Tests with Cypress Dashboard Recording**

```bash
# Set up Cypress Dashboard project and record key first
export CYPRESS_PROJECT_ID=your-project-id
export CYPRESS_RECORD_KEY=your-record-key

npm run test:e2e:record
```

**Run Tests with Cypress Dashboard Recording (Headed Mode)**

```bash
npm run test:e2e:record:headed
```

### Option 2: With Docker Stack

Prerequisites - Start the full Docker stack:

```bash
# From the project root directory
docker-compose up -d
```

This starts:
- Backend API at `http://localhost:5000`
- Frontend at `http://localhost:3000`
- Database (PostgreSQL)

**Run E2E Tests against Docker stack:**

```bash
npm run test:e2e:docker
```

**Run Tests in Headed Mode against Docker**

```bash
npm run test:e2e:docker:headed
```

**Run Tests with Recording against Docker**

```bash
# Video recording
npm run test:e2e:docker:video

# Cypress Dashboard recording (requires CYPRESS_PROJECT_ID and CYPRESS_RECORD_KEY)
npm run test:e2e:docker:record
```

**Open Cypress GUI against Docker**

```bash
npm run cypress:open:docker
```

### Option 3: Fully Containerized Testing

```bash
# Start main services
docker-compose up -d

# Run Cypress in container (one-time)
docker-compose run --rm cypress

# Or include Cypress service in the stack
docker-compose --profile test up -d
```

## Test Structure

The tests are organized as follows:

```
cypress/
├── e2e/
│   ├── products-create.cy.js      # Product creation UI tests
│   ├── products-view.cy.js        # Product listing and viewing UI tests
│   └── products-e2e-flow.cy.js    # Complete frontend workflow tests
├── support/
│   ├── e2e.js                     # Global configuration and setup
│   └── commands.js                # Custom Cypress commands for UI interactions
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

The tests include custom Cypress commands for common UI operations:

- `cy.createProductViaUI(productData)` - Creates a product through the UI form
- `cy.searchProductsViaUI(searchTerm)` - Searches for products using the search box
- `cy.clearSearchViaUI()` - Clears the search input
- `cy.visitProductList()` - Navigates to the product list page
- `cy.waitForFrontend()` - Waits for the frontend to be ready

Legacy API commands are also available for setup/teardown operations:
- `cy.createProduct(productData)` - Creates a product via API (for test setup)
- `cy.getAllProducts()` - Gets all products via API
- `cy.deleteProduct(id)` - Deletes a product via API (for cleanup)

## Configuration

The Cypress configuration is defined in `cypress.config.js` and supports flexible environments:

### Default Configuration (Local Development)
- Frontend Base URL: `http://localhost:5173`
- API URL: `http://localhost:5126/api`

### Docker Environment Configuration
Set via environment variables:
- `CYPRESS_FRONTEND_BASE_URL=http://localhost:3000`
- `CYPRESS_API_BASE_URL=http://localhost:5000`
- `CYPRESS_API_URL=http://localhost:5000/api`

### Environment Variables

- `CYPRESS_FRONTEND_BASE_URL` - Override the frontend application URL
- `CYPRESS_API_BASE_URL` - Override the backend API base URL  
- `CYPRESS_API_URL` - Override the API endpoint URL (used for setup/teardown)
- `CYPRESS_VIDEO` - Set to 'true' to enable video recording (alternative to --config video=true)
- `CYPRESS_PROJECT_ID` - Cypress Dashboard project ID for recording test results
- `CYPRESS_RECORD_KEY` - Cypress Dashboard record key for uploading test results

### Recording Configuration

The configuration supports multiple recording modes:

**Video Recording:**
- Videos are saved to `cypress/videos/` directory
- Configurable compression level (set to 32 for balance of quality/size)
- Can be enabled via `CYPRESS_VIDEO=true` environment variable or `--config video=true` flag

**Screenshots:**
- Screenshots on test failure are always enabled
- Saved to `cypress/screenshots/` directory

**Cypress Dashboard Recording:**
- Requires setup of `CYPRESS_PROJECT_ID` and `CYPRESS_RECORD_KEY`
- Automatically uploads test results, videos, and screenshots to Cypress Dashboard
- Provides test result history and analytics
- Enables parallel test execution across multiple machines

## Notes

- The Demo Inventory Microservice uses an in-memory repository, so data is reset when the API is restarted
- Tests are designed to be independent and can run in any order
- The tests focus on frontend UI interactions and user workflows
- Each test suite includes proper setup and cleanup
- Configuration automatically adapts to different environments (local vs Docker)
- Data attributes (`data-testid`) are used for reliable element selection