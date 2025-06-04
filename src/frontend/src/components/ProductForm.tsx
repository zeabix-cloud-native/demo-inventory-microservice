import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import type { CreateProductDto, UpdateProductDto } from '../types/Product';
import { productService } from '../services/productService';
import './ProductForm.css';

const ProductForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEditing = id !== 'new' && id !== undefined;

  const [formData, setFormData] = useState({
    name: '',
    description: '',
    sku: '',
    price: 0,
    quantityInStock: 0,
  });
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (isEditing && id) {
      loadProduct(parseInt(id));
    }
  }, [isEditing, id]);

  const loadProduct = async (productId: number) => {
    try {
      setLoading(true);
      const product = await productService.getProductById(productId);
      setFormData({
        name: product.name,
        description: product.description,
        sku: product.sku,
        price: product.price,
        quantityInStock: product.quantityInStock,
      });
      setError(null);
    } catch (err) {
      setError('Failed to load product.');
      console.error('Error loading product:', err);
    } finally {
      setLoading(false);
    }
  };

  const validateForm = (): boolean => {
    const errors: Record<string, string> = {};

    if (!formData.name.trim()) {
      errors.name = 'Product name is required';
    }

    if (!formData.sku.trim()) {
      errors.sku = 'SKU is required';
    }

    if (!formData.description.trim()) {
      errors.description = 'Description is required';
    }

    if (formData.price <= 0) {
      errors.price = 'Price must be greater than 0';
    }

    if (formData.quantityInStock < 0) {
      errors.quantityInStock = 'Quantity cannot be negative';
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'price' || name === 'quantityInStock' ? parseFloat(value) || 0 : value,
    }));
    
    // Clear validation error for this field
    if (validationErrors[name]) {
      setValidationErrors(prev => ({
        ...prev,
        [name]: '',
      }));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    setLoading(true);
    setError(null);

    try {
      if (isEditing && id) {
        const updateData: UpdateProductDto = {
          name: formData.name,
          description: formData.description,
          price: formData.price,
          quantityInStock: formData.quantityInStock,
        };
        await productService.updateProduct(parseInt(id), updateData);
      } else {
        const createData: CreateProductDto = {
          name: formData.name,
          description: formData.description,
          sku: formData.sku,
          price: formData.price,
          quantityInStock: formData.quantityInStock,
        };
        await productService.createProduct(createData);
      }
      
      navigate('/');
    } catch (err: any) {
      if (err.response?.status === 400) {
        setError('Invalid data. Please check your input.');
      } else {
        setError(`Failed to ${isEditing ? 'update' : 'create'} product.`);
      }
      console.error(`Error ${isEditing ? 'updating' : 'creating'} product:`, err);
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    navigate('/');
  };

  if (loading && isEditing) {
    return <div className="loading">Loading product...</div>;
  }

  return (
    <div className="product-form">
      <div className="form-header">
        <h1>{isEditing ? 'Edit Product' : 'Create New Product'}</h1>
      </div>

      {error && <div className="error">{error}</div>}

      <form onSubmit={handleSubmit} className="form">
        <div className="form-group">
          <label htmlFor="name">Product Name *</label>
          <input
            type="text"
            id="name"
            name="name"
            value={formData.name}
            onChange={handleInputChange}
            className={validationErrors.name ? 'error' : ''}
            placeholder="Enter product name"
          />
          {validationErrors.name && <span className="field-error">{validationErrors.name}</span>}
        </div>

        <div className="form-group">
          <label htmlFor="sku">SKU *</label>
          <input
            type="text"
            id="sku"
            name="sku"
            value={formData.sku}
            onChange={handleInputChange}
            disabled={isEditing} // SKU should not be editable
            className={validationErrors.sku ? 'error' : ''}
            placeholder="Enter SKU"
          />
          {validationErrors.sku && <span className="field-error">{validationErrors.sku}</span>}
          {isEditing && <span className="field-note">SKU cannot be changed</span>}
        </div>

        <div className="form-group">
          <label htmlFor="description">Description *</label>
          <textarea
            id="description"
            name="description"
            value={formData.description}
            onChange={handleInputChange}
            className={validationErrors.description ? 'error' : ''}
            placeholder="Enter product description"
            rows={4}
          />
          {validationErrors.description && <span className="field-error">{validationErrors.description}</span>}
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="price">Price * ($)</label>
            <input
              type="number"
              id="price"
              name="price"
              value={formData.price}
              onChange={handleInputChange}
              className={validationErrors.price ? 'error' : ''}
              placeholder="0.00"
              step="0.01"
              min="0"
            />
            {validationErrors.price && <span className="field-error">{validationErrors.price}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="quantityInStock">Quantity in Stock *</label>
            <input
              type="number"
              id="quantityInStock"
              name="quantityInStock"
              value={formData.quantityInStock}
              onChange={handleInputChange}
              className={validationErrors.quantityInStock ? 'error' : ''}
              placeholder="0"
              min="0"
            />
            {validationErrors.quantityInStock && <span className="field-error">{validationErrors.quantityInStock}</span>}
          </div>
        </div>

        <div className="form-actions">
          <button
            type="button"
            onClick={handleCancel}
            className="btn btn-secondary"
            disabled={loading}
          >
            Cancel
          </button>
          <button
            type="submit"
            className="btn btn-primary"
            disabled={loading}
          >
            {loading ? 'Saving...' : (isEditing ? 'Update Product' : 'Create Product')}
          </button>
        </div>
      </form>
    </div>
  );
};

export default ProductForm;