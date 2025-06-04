# Postman Collection for Demo Inventory API

This directory contains Postman collection and environment files for testing all Product endpoints in the Demo Inventory Microservice.

## Files

- `tests/postman/collection.json` - Main Postman collection with all Product endpoints
- `tests/postman/environment.json` - Environment configuration for the collection

Note: Legacy files `Demo-Inventory-API.postman_collection.json` and `Demo-Inventory-Environment.postman_environment.json` are also available in the root directory for backwards compatibility.

## How to Use

### 1. Import the Collection and Environment

1. Open Postman
2. Click "Import" 
3. Drag and drop both JSON files from `tests/postman/` directory or browse to select them:
   - `tests/postman/collection.json`
   - `tests/postman/environment.json`
4. The collection "Demo Inventory API" and environment "Demo Inventory Environment" will be imported

### 2. Set the Environment

1. In the top-right corner of Postman, select "Demo Inventory Environment" from the environment dropdown
2. The base URL will be automatically set to `http://localhost:5126`

### 3. Start the API

Before running the collection, make sure the Demo Inventory API is running:

```bash
cd src/DemoInventory.API
dotnet run
```

The API should start on `http://localhost:5126` (or update the environment variable if it starts on a different port).

### 4. Run the Collection

You can run individual requests or the entire collection:

- **Individual requests**: Click on any request in the collection and click "Send"
- **Full collection**: Click the three dots (...) next to the collection name and select "Run collection"

## Collection Structure

The collection includes the following endpoints with comprehensive test scripts:

### Main Endpoints
1. **Get All Products** (`GET /api/products`)
2. **Create Product** (`POST /api/products`)
3. **Get Product by ID** (`GET /api/products/{id}`)
4. **Get Product by SKU** (`GET /api/products/sku/{sku}`)
5. **Search Products** (`GET /api/products/search?searchTerm={term}`)
6. **Update Product** (`PUT /api/products/{id}`)
7. **Delete Product** (`DELETE /api/products/{id}`)

### Error Testing Endpoints
8. **Get Product by ID (Not Found)** - Tests 404 response
9. **Get Product by SKU (Not Found)** - Tests 404 response  
10. **Update Product (Not Found)** - Tests 404 response

## Test Scripts

Each request includes comprehensive test scripts that validate:

- **Status codes** (200, 201, 204, 404)
- **Response structure** and data types
- **Business logic** (e.g., product properties, timestamps)
- **Error handling** (404 responses for non-existent resources)

## Collection Variables

The collection uses variables to chain requests together:

- `{{baseUrl}}` - Base URL for the API (set in environment)
- `{{createdProductId}}` - Stores the ID of created products for subsequent requests

## Sample Test Flow

The collection is designed to run in sequence:

1. **Get All Products** - Should return empty array initially
2. **Create Product** - Creates a test product and stores its ID
3. **Get Product by ID** - Retrieves the created product
4. **Get Product by SKU** - Retrieves the product by its SKU
5. **Search Products** - Searches for the created product
6. **Update Product** - Updates the created product
7. **Delete Product** - Deletes the created product
8. **Error tests** - Test various 404 scenarios

## Important Notes

### Repository Limitation

The current implementation uses an in-memory repository with **scoped** dependency injection, which means data doesn't persist between separate HTTP requests. This has the following implications:

- Products created in one request may not be available in subsequent separate requests
- For best results, run the collection as a complete sequence using Postman's collection runner
- Individual requests may need to be run in quick succession

### Recommended Usage

1. Use Postman's **Collection Runner** to execute all requests in sequence
2. Or run requests individually but in quick succession
3. If testing individual endpoints, you may need to create test data first within the same test session

## Example Request Bodies

### Create Product
```json
{
  "name": "Test Product",
  "description": "A test product for API testing",
  "sku": "TEST-001",
  "price": 29.99,
  "quantityInStock": 100
}
```

### Update Product
```json
{
  "name": "Updated Test Product",
  "description": "An updated test product description",
  "price": 39.99,
  "quantityInStock": 75
}
```

## Environment Variables

You can modify the environment variables as needed:

- `baseUrl`: Change this if your API runs on a different port or host

## Test Results

When running the collection, you should see:

- ✅ All status code validations pass
- ✅ Response structure validations pass  
- ✅ Business logic validations pass
- ✅ Error handling validations pass

If any tests fail, check that:
1. The API is running on the correct port
2. The environment is selected in Postman
3. The requests are being run in sequence (for data persistence)