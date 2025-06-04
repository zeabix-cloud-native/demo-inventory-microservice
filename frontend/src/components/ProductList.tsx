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
        <h1>Product Inventory</h1>
        <Link to="/product/new" className="btn btn-primary">
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
          />
          <button onClick={handleSearch} className="btn btn-secondary">
            Search
          </button>
          <button onClick={loadProducts} className="btn btn-secondary">
            Clear
          </button>
        </div>
      </div>

      {error && <div className="error">{error}</div>}

      {products.length === 0 ? (
        <div className="no-products">
          <p>No products found.</p>
          <Link to="/product/new" className="btn btn-primary">
            Create your first product
          </Link>
        </div>
      ) : (
        <div className="products-table-container">
          <table className="products-table">
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
                <tr key={product.id}>
                  <td>{product.id}</td>
                  <td>{product.name}</td>
                  <td>{product.sku}</td>
                  <td title={product.description}>
                    {product.description.length > 50
                      ? `${product.description.substring(0, 50)}...`
                      : product.description}
                  </td>
                  <td>{formatPrice(product.price)}</td>
                  <td className={product.quantityInStock < 10 ? 'low-stock' : ''}>
                    {product.quantityInStock}
                  </td>
                  <td>{formatDate(product.createdAt)}</td>
                  <td>
                    <div className="actions">
                      <Link
                        to={`/product/${product.id}`}
                        className="btn btn-small btn-secondary"
                      >
                        Edit
                      </Link>
                      <button
                        onClick={() => handleDelete(product.id)}
                        className="btn btn-small btn-danger"
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