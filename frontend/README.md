# Frontend Demo for Inventory Management

This is a simple HTML/JavaScript frontend that demonstrates connecting to the Demo Inventory Microservice backend using Axios.

## Features

- **Product CRUD Operations**: Create, Read, Update, and Delete products
- **Search Functionality**: Search products by name
- **Real-time Updates**: UI updates immediately after operations
- **Axios Integration**: All API calls use Axios for HTTP communication
- **Responsive Design**: Simple, clean UI that works on different screen sizes

## Setup Instructions

### Prerequisites
- The Demo Inventory API must be running
- A modern web browser
- Internet connection (for Axios CDN)

### Running the Backend API
1. Navigate to the root directory of the project
2. Run the API:
   ```bash
   dotnet run --project src/DemoInventory.API
   ```
3. The API will be available at `https://localhost:5001` or `http://localhost:5000`

### Running the Frontend
1. Navigate to the `frontend` directory
2. Open `index.html` in your web browser, or
3. Serve it using a simple HTTP server:
   ```bash
   # Using Python 3
   python -m http.server 8080
   
   # Using Node.js (if you have http-server installed)
   npx http-server -p 8080
   
   # Using PHP
   php -S localhost:8080
   ```
4. Open `http://localhost:8080` in your browser

### Configuration
- If your API is running on a different port or protocol, update the `API_BASE_URL` in `app.js`
- Default configuration assumes the API is running on `https://localhost:5001`

## API Integration Details

The frontend uses Axios to communicate with the following API endpoints:

- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/search?searchTerm={term}` - Search products
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update existing product
- `DELETE /api/products/{id}` - Delete product

## Usage

1. **Add Products**: Fill out the form at the top and click "Add Product"
2. **View Products**: All products are displayed in the table below
3. **Search Products**: Use the search box to filter products by name
4. **Edit Products**: Click the "Edit" button on any product to modify it
5. **Delete Products**: Click the "Delete" button and confirm to remove a product

## Sample Data

Uncomment the last line in `app.js` to automatically add sample data when the page loads:
```javascript
setTimeout(addSampleData, 1000);
```

## Technical Details

- **Frontend Framework**: Vanilla JavaScript (no framework dependencies)
- **HTTP Client**: Axios (loaded from CDN)
- **Styling**: Pure CSS (no CSS framework)
- **CORS**: Backend is configured to allow cross-origin requests

## Troubleshooting

1. **"Network Error" or "ERR_CONNECTION_REFUSED"**:
   - Make sure the backend API is running
   - Check the API_BASE_URL in app.js matches your API address

2. **CORS Errors**:
   - The backend includes CORS configuration
   - If issues persist, check browser dev tools for specific CORS errors

3. **HTTPS Certificate Warnings**:
   - If using localhost with HTTPS, you may need to accept the self-signed certificate
   - Alternatively, use HTTP by changing the API_BASE_URL to `http://localhost:5000`