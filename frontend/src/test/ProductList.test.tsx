import { render, screen, waitFor } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import { vi, describe, it, beforeEach, expect } from 'vitest'
import ProductList from '../components/ProductList'
import type { Product } from '../types/Product'

// Mock the productService
const mockProducts: Product[] = [
  {
    id: 1,
    name: 'Low Stock Product',
    description: 'Product with low stock',
    sku: 'LOW-001',
    price: 15.99,
    quantityInStock: 5,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z'
  },
  {
    id: 2,
    name: 'Normal Stock Product',
    description: 'Product with normal stock',
    sku: 'NORMAL-001',
    price: 25.99,
    quantityInStock: 50,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z'
  },
  {
    id: 3,
    name: 'Out of Stock Product',
    description: 'Product with no stock',
    sku: 'OOS-001',
    price: 19.99,
    quantityInStock: 0,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z'
  }
]

vi.mock('../services/productService', () => ({
  productService: {
    getAllProducts: vi.fn(),
    searchProducts: vi.fn(),
    deleteProduct: vi.fn(),
  }
}))

import { productService } from '../services/productService'

const renderProductList = () => {
  return render(
    <BrowserRouter>
      <ProductList />
    </BrowserRouter>
  )
}

describe('ProductList', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    ;(productService.getAllProducts as any).mockResolvedValue(mockProducts)
  })

  it('should render product list title', async () => {
    renderProductList()
    
    expect(screen.getByTestId('product-inventory-title')).toHaveTextContent('Product Inventory')
  })

  it('should display low stock warning for products with quantity less than 10', async () => {
    renderProductList()
    
    await waitFor(() => {
      expect(screen.getByTestId('products-table')).toBeInTheDocument()
    })

    // Check that low stock product (quantity: 5) has low-stock class
    const lowStockCell = screen.getByTestId('product-stock-1')
    expect(lowStockCell).toHaveTextContent('5')
    expect(lowStockCell).toHaveClass('low-stock')

    // Check that normal stock product (quantity: 50) does not have low-stock class
    const normalStockCell = screen.getByTestId('product-stock-2')
    expect(normalStockCell).toHaveTextContent('50')
    expect(normalStockCell).not.toHaveClass('low-stock')

    // Check that out of stock product (quantity: 0) has low-stock class
    const outOfStockCell = screen.getByTestId('product-stock-3')
    expect(outOfStockCell).toHaveTextContent('0')
    expect(outOfStockCell).toHaveClass('low-stock')
  })

  it('should format prices correctly', async () => {
    renderProductList()
    
    await waitFor(() => {
      expect(screen.getByTestId('products-table')).toBeInTheDocument()
    })

    // Check price formatting
    expect(screen.getByTestId('product-price-1')).toHaveTextContent('$15.99')
    expect(screen.getByTestId('product-price-2')).toHaveTextContent('$25.99')
    expect(screen.getByTestId('product-price-3')).toHaveTextContent('$19.99')
  })

  it('should truncate long descriptions', async () => {
    const longDescriptionProduct: Product = {
      id: 4,
      name: 'Long Description Product',
      description: 'This is a very long description that should be truncated when displayed in the table to ensure proper formatting and readability',
      sku: 'LONG-001',
      price: 99.99,
      quantityInStock: 25,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z'
    }

    ;(productService.getAllProducts as any).mockResolvedValue([longDescriptionProduct])
    
    renderProductList()
    
    await waitFor(() => {
      expect(screen.getByTestId('products-table')).toBeInTheDocument()
    })

    const descriptionCell = screen.getByTestId('product-description-4')
    expect(descriptionCell).toHaveTextContent('This is a very long description that should be...')
    expect(descriptionCell.textContent).toHaveLength(53) // 50 chars + "..."
  })

  it('should display no products message when list is empty', async () => {
    ;(productService.getAllProducts as any).mockResolvedValue([])
    
    renderProductList()
    
    await waitFor(() => {
      expect(screen.getByTestId('no-products')).toBeVisible()
      expect(screen.getByText('No products found.')).toBeVisible()
    })
  })

  it('should display action buttons for each product', async () => {
    renderProductList()
    
    await waitFor(() => {
      expect(screen.getByTestId('products-table')).toBeInTheDocument()
    })

    // Check that edit and delete buttons exist for each product
    expect(screen.getByTestId('edit-product-1')).toBeInTheDocument()
    expect(screen.getByTestId('delete-product-1')).toBeInTheDocument()
    expect(screen.getByTestId('edit-product-2')).toBeInTheDocument()
    expect(screen.getByTestId('delete-product-2')).toBeInTheDocument()
    expect(screen.getByTestId('edit-product-3')).toBeInTheDocument()
    expect(screen.getByTestId('delete-product-3')).toBeInTheDocument()
  })
})