# Postman/Newman API Tests

This directory contains Postman collection and Newman test scripts for the Demo Inventory Microservice API.

## Files

- `collection.json` - Main Postman collection with all API endpoint tests
- `environment.json` - Environment variables for local development (port 5126)
- `docker-environment.json` - Environment variables for Docker deployment (port 5000)
- `run-newman.sh` - Bash script with automatic environment detection
- `run-newman.js` - Node.js script with automatic environment detection
- `package.json` - NPM package configuration with test scripts

## Running Tests

### Automatic Environment Detection (Recommended)

The scripts automatically detect whether the API is running in local development or Docker mode:

```bash
# Using bash script
./run-newman.sh

# Using Node.js script
node run-newman.js

# Using npm
npm run test
```

### Manual Environment Selection

You can explicitly specify which environment to use:

```bash
# Local development environment (port 5126)
./run-newman.sh local
npm run test:local

# Docker environment (port 5000)
./run-newman.sh docker
npm run test:docker
```

### With Additional Newman Options

You can pass additional Newman options to the scripts:

```bash
# Generate HTML report
./run-newman.sh auto --reporters cli,htmlextra --reporter-htmlextra-export newman-report.html

# Run with verbose output
./run-newman.sh auto --verbose

# Bail on first failure
./run-newman.sh auto --bail
```

## Environment Files

### Local Development (`environment.json`)
- Base URL: `http://localhost:5126`
- Used when running the API with `dotnet run`

### Docker (`docker-environment.json`)
- Base URL: `http://localhost:5000`
- Used when running the API with `docker-compose up`

## Test Collection

The collection includes comprehensive tests for all Product endpoints:

- **Get All Products** - Retrieve product list
- **Create Product** - Add new product with validation
- **Get Product by ID** - Retrieve specific product
- **Get Product by SKU** - Search by SKU
- **Search Products** - Search by name
- **Update Product** - Modify existing product
- **Delete Product** - Remove product
- **Error Scenarios** - 404 and validation error tests

Each test includes:
- Request/response validation
- Status code verification
- Content-Type checks
- Response schema validation
- Error handling verification

## Prerequisites

1. Install Newman globally: `npm install -g newman`
2. Ensure the API is running in either local or Docker mode
3. API endpoints should be accessible at the configured base URL

## Troubleshooting

### Connection Refused Errors
If you see `ECONNREFUSED` errors:
1. Verify the API is running
2. Check if the correct port is being used (5126 for local, 5000 for Docker)
3. Try manual environment detection: `./run-newman.sh local` or `./run-newman.sh docker`

### Environment Detection Issues
If auto-detection fails:
1. The scripts check for API availability at `/api/products` endpoint
2. Use manual environment selection if auto-detection is unreliable
3. Verify the API is fully started before running tests