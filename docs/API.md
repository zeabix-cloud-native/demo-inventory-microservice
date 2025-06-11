# API Documentation

## Overview

The Demo Inventory Microservice provides a RESTful API for managing product inventory. The API follows REST principles, uses JSON for data exchange, and includes comprehensive Swagger/OpenAPI documentation.

## Base Information

- **Base URL**: `http://localhost:5126/api` (Development)
- **Production URL**: `https://your-domain.com/api`
- **API Version**: v1
- **Content-Type**: `application/json`
- **Documentation**: Available at `/swagger` endpoint

## Authentication

**Current**: No authentication required (demo purposes)
**Future**: JWT Bearer token authentication

```http
Authorization: Bearer <your-jwt-token>
```

## Products API

### Get All Products

Retrieve a list of all products in the inventory.

**Endpoint**: `GET /api/products`

**Response**: `200 OK`

```json
[
  {
    "id": 1,
    "name": "Laptop Computer",
    "sku": "LAP001",
    "price": 999.99,
    "stockQuantity": 15,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  },
  {
    "id": 2,
    "name": "Wireless Mouse",
    "sku": "MOU001",
    "price": 29.99,
    "stockQuantity": 50,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
]
```

**Example cURL**:
```bash
curl -X GET "http://localhost:5126/api/products" \
  -H "Accept: application/json"
```

### Get Product by ID

Retrieve a specific product by its unique identifier.

**Endpoint**: `GET /api/products/{id}`

**Path Parameters**:
- `id` (integer, required): The unique identifier of the product

**Response**: `200 OK` | `404 Not Found`

```json
{
  "id": 1,
  "name": "Laptop Computer",
  "sku": "LAP001",
  "price": 999.99,
  "stockQuantity": 15,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

**Error Response**: `404 Not Found`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Product with ID 999 not found."
}
```

**Example cURL**:
```bash
curl -X GET "http://localhost:5126/api/products/1" \
  -H "Accept: application/json"
```

### Get Product by SKU

Retrieve a specific product by its Stock Keeping Unit (SKU).

**Endpoint**: `GET /api/products/sku/{sku}`

**Path Parameters**:
- `sku` (string, required): The SKU of the product

**Response**: `200 OK` | `404 Not Found`

```json
{
  "id": 1,
  "name": "Laptop Computer",
  "sku": "LAP001",
  "price": 999.99,
  "stockQuantity": 15,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

**Example cURL**:
```bash
curl -X GET "http://localhost:5126/api/products/sku/LAP001" \
  -H "Accept: application/json"
```

### Search Products

Search for products by name using a search term.

**Endpoint**: `GET /api/products/search`

**Query Parameters**:
- `searchTerm` (string, required): The search term to match against product names

**Response**: `200 OK`

```json
[
  {
    "id": 1,
    "name": "Laptop Computer",
    "sku": "LAP001",
    "price": 999.99,
    "stockQuantity": 15,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
]
```

**Example cURL**:
```bash
curl -X GET "http://localhost:5126/api/products/search?searchTerm=laptop" \
  -H "Accept: application/json"
```

### Create Product

Create a new product in the inventory.

**Endpoint**: `POST /api/products`

**Request Body**:
```json
{
  "name": "New Product",
  "sku": "NEW001",
  "price": 199.99,
  "stockQuantity": 25
}
```

**Request Schema**:
- `name` (string, required): Product name (max 100 characters)
- `sku` (string, required): Unique stock keeping unit (max 50 characters)
- `price` (decimal, required): Product price (must be greater than 0)
- `stockQuantity` (integer, required): Initial stock quantity (must be >= 0)

**Response**: `201 Created`

```json
{
  "id": 3,
  "name": "New Product",
  "sku": "NEW001",
  "price": 199.99,
  "stockQuantity": 25,
  "createdAt": "2024-01-15T14:30:00Z",
  "updatedAt": "2024-01-15T14:30:00Z"
}
```

**Validation Errors**: `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": [
      "The Name field is required."
    ],
    "Price": [
      "Price must be greater than 0."
    ]
  }
}
```

**Example cURL**:
```bash
curl -X POST "http://localhost:5126/api/products" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
    "name": "New Product",
    "sku": "NEW001",
    "price": 199.99,
    "stockQuantity": 25
  }'
```

### Update Product

Update an existing product's information.

**Endpoint**: `PUT /api/products/{id}`

**Path Parameters**:
- `id` (integer, required): The unique identifier of the product to update

**Request Body**:
```json
{
  "name": "Updated Product Name",
  "sku": "UPD001",
  "price": 299.99,
  "stockQuantity": 30
}
```

**Request Schema**: Same as Create Product

**Response**: `200 OK` | `404 Not Found`

```json
{
  "id": 1,
  "name": "Updated Product Name",
  "sku": "UPD001",
  "price": 299.99,
  "stockQuantity": 30,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T15:45:00Z"
}
```

**Example cURL**:
```bash
curl -X PUT "http://localhost:5126/api/products/1" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
    "name": "Updated Product Name",
    "sku": "UPD001",
    "price": 299.99,
    "stockQuantity": 30
  }'
```

### Delete Product

Remove a product from the inventory.

**Endpoint**: `DELETE /api/products/{id}`

**Path Parameters**:
- `id` (integer, required): The unique identifier of the product to delete

**Response**: `204 No Content` | `404 Not Found`

**Example cURL**:
```bash
curl -X DELETE "http://localhost:5126/api/products/1" \
  -H "Accept: application/json"
```

## Data Models

### Product

The core entity representing a product in the inventory system.

```json
{
  "id": "integer (read-only)",
  "name": "string (required, max: 100)",
  "sku": "string (required, unique, max: 50)",
  "price": "decimal (required, > 0)",
  "stockQuantity": "integer (required, >= 0)",
  "createdAt": "datetime (read-only, ISO 8601)",
  "updatedAt": "datetime (read-only, ISO 8601)"
}
```

**Field Descriptions**:
- `id`: Unique identifier, auto-generated
- `name`: Human-readable product name
- `sku`: Stock Keeping Unit, must be unique across all products
- `price`: Product price in decimal format
- `stockQuantity`: Current stock level
- `createdAt`: Timestamp when the product was created
- `updatedAt`: Timestamp when the product was last modified

### CreateProductRequest

Request model for creating a new product.

```json
{
  "name": "string (required, max: 100)",
  "sku": "string (required, max: 50)",
  "price": "decimal (required, > 0)",
  "stockQuantity": "integer (required, >= 0)"
}
```

### UpdateProductRequest

Request model for updating an existing product.

```json
{
  "name": "string (required, max: 100)",
  "sku": "string (required, max: 50)",
  "price": "decimal (required, > 0)",
  "stockQuantity": "integer (required, >= 0)"
}
```

## HTTP Status Codes

The API uses standard HTTP status codes to indicate the success or failure of requests:

### Success Codes

- `200 OK`: Request successful, response body contains data
- `201 Created`: Resource successfully created, response body contains the new resource
- `204 No Content`: Request successful, no response body (typically for DELETE operations)

### Client Error Codes

- `400 Bad Request`: Invalid request format or validation errors
- `404 Not Found`: Requested resource does not exist
- `409 Conflict`: Resource conflict (e.g., duplicate SKU)
- `422 Unprocessable Entity`: Request format is valid but contains semantic errors

### Server Error Codes

- `500 Internal Server Error`: Unexpected server error
- `503 Service Unavailable`: Service temporarily unavailable

## Error Handling

The API follows RFC 7807 Problem Details for HTTP APIs standard for error responses.

### Error Response Format

```json
{
  "type": "string (URI identifying the problem type)",
  "title": "string (short, human-readable summary)",
  "status": "integer (HTTP status code)",
  "detail": "string (human-readable explanation)",
  "instance": "string (optional, URI reference identifying the specific occurrence)"
}
```

### Validation Error Response

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "fieldName": [
      "Validation error message for this field"
    ]
  }
}
```

### Example Error Responses

**Product Not Found**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Product with ID 999 not found."
}
```

**Validation Errors**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": [
      "The Name field is required."
    ],
    "SKU": [
      "The SKU field is required.",
      "SKU must be unique."
    ],
    "Price": [
      "Price must be greater than 0."
    ]
  }
}
```

## Rate Limiting

**Current**: No rate limiting implemented
**Future**: Rate limiting will be implemented with the following limits:

- **Unauthenticated requests**: 100 requests per hour per IP
- **Authenticated requests**: 1000 requests per hour per user

Rate limit headers will be included in responses:
```http
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1640995200
```

## Versioning

**Current Version**: v1 (implicit in URL structure)
**Future Versioning Strategy**: URL versioning (e.g., `/api/v2/products`)

## CORS Policy

The API supports Cross-Origin Resource Sharing (CORS) for the following origins:

**Development**:
- `http://localhost:3000` (React development server)
- `http://localhost:5173` (Vite development server)

**Production**: Configure based on your frontend domain

## OpenAPI/Swagger Specification

The complete API specification is available via Swagger UI:

- **Swagger UI**: `http://localhost:5126/swagger`
- **OpenAPI JSON**: `http://localhost:5126/swagger/v1/swagger.json`

## Client SDKs

### JavaScript/TypeScript

```typescript
// Example TypeScript client
class ProductService {
  private baseUrl = 'http://localhost:5126/api';

  async getProducts(): Promise<Product[]> {
    const response = await fetch(`${this.baseUrl}/products`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return response.json();
  }

  async getProduct(id: number): Promise<Product> {
    const response = await fetch(`${this.baseUrl}/products/${id}`);
    if (!response.ok) {
      if (response.status === 404) {
        throw new Error('Product not found');
      }
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return response.json();
  }

  async createProduct(product: CreateProductRequest): Promise<Product> {
    const response = await fetch(`${this.baseUrl}/products`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(product),
    });
    
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.detail || `HTTP error! status: ${response.status}`);
    }
    
    return response.json();
  }
}
```

### C# Client

```csharp
// Example C# client using HttpClient
public class ProductService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ProductService(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/products");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Product>>(json);
    }

    public async Task<Product> GetProductAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/products/{id}");
        
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new ProductNotFoundException($"Product with ID {id} not found");
        }
        
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Product>(json);
    }

    public async Task<Product> CreateProductAsync(CreateProductRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"{_baseUrl}/products", content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Product>(responseJson);
    }
}
```

## Testing the API

### Postman Collection

A comprehensive Postman collection is available at `tests/postman/collection.json` with:

- All API endpoints
- Pre-request scripts for data setup
- Test assertions for response validation
- Environment variables for different deployment environments

### Example Test Cases

```javascript
// Postman test script examples

// Test successful product creation
pm.test("Product creation returns 201", function () {
    pm.response.to.have.status(201);
});

pm.test("Created product has correct structure", function () {
    const responseJson = pm.response.json();
    pm.expect(responseJson).to.have.property('id');
    pm.expect(responseJson).to.have.property('name');
    pm.expect(responseJson).to.have.property('sku');
    pm.expect(responseJson).to.have.property('price');
    pm.expect(responseJson).to.have.property('stockQuantity');
    pm.expect(responseJson).to.have.property('createdAt');
    pm.expect(responseJson).to.have.property('updatedAt');
});

// Test validation errors
pm.test("Validation error returns 400", function () {
    pm.response.to.have.status(400);
});

pm.test("Validation error has correct structure", function () {
    const responseJson = pm.response.json();
    pm.expect(responseJson).to.have.property('type');
    pm.expect(responseJson).to.have.property('title');
    pm.expect(responseJson).to.have.property('status', 400);
    pm.expect(responseJson).to.have.property('errors');
});
```

## Performance Considerations

### Response Times

Target response times for different operations:

- **GET /products**: < 200ms
- **GET /products/{id}**: < 100ms
- **POST /products**: < 300ms
- **PUT /products/{id}**: < 300ms
- **DELETE /products/{id}**: < 200ms

### Caching

**Current**: No caching implemented
**Future**: Response caching for GET operations

```http
Cache-Control: public, max-age=300
ETag: "12345678"
```

### Pagination

**Current**: All products returned (suitable for demo)
**Future**: Implement pagination for large datasets

```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 100,
    "totalPages": 5
  }
}
```

## Security

### Input Validation

All input is validated using:
- Data annotations on DTOs
- Model validation in controllers
- SQL injection prevention via Entity Framework

### HTTPS

**Development**: HTTP (localhost)
**Production**: HTTPS required

### Future Security Enhancements

- JWT authentication
- API key authentication
- Rate limiting
- Request logging and monitoring
- Input sanitization