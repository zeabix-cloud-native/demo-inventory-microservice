// Configuration
const API_BASE_URL = 'http://localhost:5126/api'; // Update this if your API runs on a different port

// Axios configuration
axios.defaults.baseURL = API_BASE_URL;
axios.defaults.headers.common['Content-Type'] = 'application/json';

// Global variables
let isEditing = false;
let editingProductId = null;

// DOM elements
const productForm = document.getElementById('productForm');
const submitBtn = document.getElementById('submitBtn');
const messageDiv = document.getElementById('message');
const productsTableBody = document.getElementById('productsTableBody');
const productsTable = document.getElementById('productsTable');
const loadingMessage = document.getElementById('loadingMessage');
const searchInput = document.getElementById('searchTerm');

// Event listeners
document.addEventListener('DOMContentLoaded', function() {
    loadAllProducts();
    productForm.addEventListener('submit', handleFormSubmit);
    
    // Allow search on Enter key
    searchInput.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            searchProducts();
        }
    });
});

// API Functions using Axios
async function getAllProducts() {
    try {
        const response = await axios.get('/products');
        return response.data;
    } catch (error) {
        console.error('Error fetching products:', error);
        throw error;
    }
}

async function getProduct(id) {
    try {
        const response = await axios.get(`/products/${id}`);
        return response.data;
    } catch (error) {
        console.error('Error fetching product:', error);
        throw error;
    }
}

async function searchProductsAPI(searchTerm) {
    try {
        const response = await axios.get(`/products/search?searchTerm=${encodeURIComponent(searchTerm)}`);
        return response.data;
    } catch (error) {
        console.error('Error searching products:', error);
        throw error;
    }
}

async function createProduct(productData) {
    try {
        const response = await axios.post('/products', productData);
        return response.data;
    } catch (error) {
        console.error('Error creating product:', error);
        throw error;
    }
}

async function updateProduct(id, productData) {
    try {
        const response = await axios.put(`/products/${id}`, productData);
        return response.data;
    } catch (error) {
        console.error('Error updating product:', error);
        throw error;
    }
}

async function deleteProduct(id) {
    try {
        await axios.delete(`/products/${id}`);
    } catch (error) {
        console.error('Error deleting product:', error);
        throw error;
    }
}

// UI Functions
function showMessage(message, type = 'success') {
    messageDiv.innerHTML = `<div class="${type}">${message}</div>`;
    setTimeout(() => {
        messageDiv.innerHTML = '';
    }, 3000);
}

function showLoading() {
    loadingMessage.style.display = 'block';
    productsTable.style.display = 'none';
}

function hideLoading() {
    loadingMessage.style.display = 'none';
    productsTable.style.display = 'table';
}

async function loadAllProducts() {
    showLoading();
    try {
        const products = await getAllProducts();
        displayProducts(products);
        hideLoading();
    } catch (error) {
        showMessage('Error loading products: ' + error.message, 'error');
        hideLoading();
    }
}

function displayProducts(products) {
    productsTableBody.innerHTML = '';
    
    if (products.length === 0) {
        productsTableBody.innerHTML = '<tr><td colspan="8">No products found</td></tr>';
        return;
    }
    
    products.forEach(product => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${product.id}</td>
            <td>${product.name}</td>
            <td>${product.description}</td>
            <td>${product.sku}</td>
            <td>$${product.price.toFixed(2)}</td>
            <td>${product.quantityInStock}</td>
            <td>${new Date(product.createdAt).toLocaleDateString()}</td>
            <td class="actions">
                <button onclick="editProduct(${product.id})" style="background-color: #28a745;">Edit</button>
                <button onclick="deleteProductConfirm(${product.id})" style="background-color: #dc3545;">Delete</button>
            </td>
        `;
        productsTableBody.appendChild(row);
    });
}

async function handleFormSubmit(event) {
    event.preventDefault();
    
    const formData = new FormData(event.target);
    const productData = {
        name: formData.get('name'),
        description: formData.get('description'),
        sku: formData.get('sku'),
        price: parseFloat(formData.get('price')),
        quantityInStock: parseInt(formData.get('quantity'))
    };
    
    try {
        if (isEditing) {
            await updateProduct(editingProductId, productData);
            showMessage('Product updated successfully!');
        } else {
            await createProduct(productData);
            showMessage('Product created successfully!');
        }
        
        resetForm();
        loadAllProducts();
    } catch (error) {
        showMessage('Error saving product: ' + error.message, 'error');
    }
}

async function editProduct(id) {
    try {
        const product = await getProduct(id);
        
        // Fill form with product data
        document.getElementById('name').value = product.name;
        document.getElementById('description').value = product.description;
        document.getElementById('sku').value = product.sku;
        document.getElementById('price').value = product.price;
        document.getElementById('quantity').value = product.quantityInStock;
        
        // Change form state
        isEditing = true;
        editingProductId = id;
        submitBtn.textContent = 'Update Product';
        
        // Scroll to form
        document.getElementById('productForm').scrollIntoView({ behavior: 'smooth' });
    } catch (error) {
        showMessage('Error loading product for editing: ' + error.message, 'error');
    }
}

async function deleteProductConfirm(id) {
    if (confirm('Are you sure you want to delete this product?')) {
        try {
            await deleteProduct(id);
            showMessage('Product deleted successfully!');
            loadAllProducts();
        } catch (error) {
            showMessage('Error deleting product: ' + error.message, 'error');
        }
    }
}

function resetForm() {
    productForm.reset();
    isEditing = false;
    editingProductId = null;
    submitBtn.textContent = 'Add Product';
}

async function searchProducts() {
    const searchTerm = searchInput.value.trim();
    if (!searchTerm) {
        loadAllProducts();
        return;
    }
    
    showLoading();
    try {
        const products = await searchProductsAPI(searchTerm);
        displayProducts(products);
        hideLoading();
    } catch (error) {
        showMessage('Error searching products: ' + error.message, 'error');
        hideLoading();
    }
}

// Add some sample data on page load for demo purposes
async function addSampleData() {
    const sampleProducts = [
        {
            name: "Laptop",
            description: "High-performance laptop for work and gaming",
            sku: "LAP-001",
            price: 999.99,
            quantityInStock: 10
        },
        {
            name: "Wireless Mouse",
            description: "Ergonomic wireless mouse with long battery life",
            sku: "MOU-001",
            price: 29.99,
            quantityInStock: 50
        },
        {
            name: "USB-C Cable",
            description: "High-quality USB-C charging cable",
            sku: "CAB-001",
            price: 15.99,
            quantityInStock: 100
        }
    ];
    
    try {
        for (const product of sampleProducts) {
            await createProduct(product);
        }
        showMessage('Sample data added successfully!');
        loadAllProducts();
    } catch (error) {
        console.log('Sample data might already exist or error occurred:', error.message);
    }
}

// Uncomment the line below to add sample data on first load
// setTimeout(addSampleData, 1000);