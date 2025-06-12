import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import type { Product } from '../types/Product';
import { productService } from '../services/productService';
import './ProductList.css';

const ProductList: React.FC = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    loadProducts();
  }, []);

  const loadProducts = async () => {
    try {
      setLoading(true);
      const data = await productService.getAllProducts();
      setProducts(data);
      setError(null);
    } catch (err) {
      setError('Failed to load products. Please make sure the API is running.');
      console.error('Error loading products:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = async () => {
    if (!searchTerm.trim()) {
      loadProducts();
      return;
    }

    try {
      setLoading(true);
      const data = await productService.searchProducts(searchTerm);
      setProducts(data);
      setError(null);
    } catch (err) {
      setError('Failed to search products.');
      console.error('Error searching products:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleClearSearch = () => {
    setSearchTerm('');
    loadProducts();
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this product?')) {
      return;
    }

    try {
      await productService.deleteProduct(id);
      await loadProducts(); // Reload the list
    } catch (err) {
      setError('Failed to delete product.');
      console.error('Error deleting product:', err);
    }
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(price);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  if (loading) {
    return <div className="loading">Loading products...</div>;
  }

  return (
    <div className="product-list">
      <div className="header">
        <h1 data-testid="product-inventory-title">Product Inventory</h1>
        <Link to="/product/new" className="btn btn-primary" data-testid="add-new-product-btn">
          Add New Product
        </Link>
      </div>

      <div className="search-section">
        <div className="search-container">
          <input
            type="text"
            placeholder="Search products by name..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
            className="search-input"
            data-testid="search-input"
          />
          <button onClick={handleSearch} className="btn btn-secondary" data-testid="search-btn">
            Search
          </button>
          <button onClick={handleClearSearch} className="btn btn-secondary" data-testid="clear-search-btn">
            Clear
          </button>
        </div>
      </div>

      {error && <div className="error" data-testid="error-message">{error}</div>}

      {products.length === 0 ? (
        <div className="no-products" data-testid="no-products">
          <p>No products found.</p>
          <Link to="/product/new" className="btn btn-primary" data-testid="create-first-product-btn">
            Create your first product
          </Link>
        </div>
      ) : (
        <div className="products-table-container">
          <table className="products-table" data-testid="products-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Name</th>
                <th>SKU</th>
                <th>Description</th>
                <th>Price</th>
                <th>Stock</th>
                <th>Created</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {products.map((product) => (
                <tr key={product.id} data-testid={`product-row-${product.id}`}>
                  <td data-testid={`product-id-${product.id}`}>{product.id}</td>
                  <td data-testid={`product-name-${product.id}`}>{product.name}</td>
                  <td data-testid={`product-sku-${product.id}`}>{product.sku}</td>
                  <td title={product.description} data-testid={`product-description-${product.id}`}>
                    {product.description.length > 50
                      ? `${product.description.substring(0, 50)}...`
                      : product.description}
                  </td>
                  <td data-testid={`product-price-${product.id}`}>{formatPrice(product.price)}</td>
                  <td className={product.quantityInStock < 10 ? 'low-stock' : ''} data-testid={`product-stock-${product.id}`}>
                    {product.quantityInStock}
                  </td>
                  <td data-testid={`product-created-${product.id}`}>{formatDate(product.createdAt)}</td>
                  <td>
                    <div className="actions">
                      <Link
                        to={`/product/${product.id}`}
                        className="btn btn-small btn-secondary"
                        data-testid={`edit-product-${product.id}`}
                      >
                        Edit
                      </Link>
                      <button
                        onClick={() => handleDelete(product.id)}
                        className="btn btn-small btn-danger"
                        data-testid={`delete-product-${product.id}`}
                      >
                        Delete
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default ProductList;