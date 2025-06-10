import axios from 'axios';
import type { Product, CreateProductDto, UpdateProductDto } from '../types/Product';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5126/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const productService = {
  // Get all products
  getAllProducts: async (): Promise<Product[]> => {
    const response = await api.get<Product[]>('/products');
    return response.data;
  },

  // Get product by ID
  getProductById: async (id: number): Promise<Product> => {
    const response = await api.get<Product>(`/products/${id}`);
    return response.data;
  },

  // Get product by SKU
  getProductBySku: async (sku: string): Promise<Product> => {
    const response = await api.get<Product>(`/products/sku/${sku}`);
    return response.data;
  },

  // Search products
  searchProducts: async (searchTerm: string): Promise<Product[]> => {
    const response = await api.get<Product[]>(`/products/search?searchTerm=${encodeURIComponent(searchTerm)}`);
    return response.data;
  },

  // Create new product
  createProduct: async (product: CreateProductDto): Promise<Product> => {
    const response = await api.post<Product>('/products', product);
    return response.data;
  },

  // Update product
  updateProduct: async (id: number, product: UpdateProductDto): Promise<Product> => {
    const response = await api.put<Product>(`/products/${id}`, product);
    return response.data;
  },

  // Delete product
  deleteProduct: async (id: number): Promise<void> => {
    await api.delete(`/products/${id}`);
  },
};