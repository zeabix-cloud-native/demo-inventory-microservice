import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import { vi, describe, it, beforeEach, expect } from 'vitest'
import ProductForm from '../components/ProductForm'

// Mock the productService
vi.mock('../services/productService', () => ({
  productService: {
    createProduct: vi.fn(),
    updateProduct: vi.fn(),
    getProductById: vi.fn(),
  }
}))

// Mock react-router-dom hooks
const mockNavigate = vi.fn()
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useNavigate: () => mockNavigate,
    useParams: () => ({ id: 'new' }),
  }
})

const renderProductForm = () => {
  return render(
    <BrowserRouter>
      <ProductForm />
    </BrowserRouter>
  )
}

describe('ProductForm', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should render create form with correct title', () => {
    renderProductForm()
    
    expect(screen.getByTestId('form-title')).toHaveTextContent('Create New Product')
    expect(screen.getByTestId('product-name-input')).toBeInTheDocument()
    expect(screen.getByTestId('product-sku-input')).toBeInTheDocument()
    expect(screen.getByTestId('product-description-input')).toBeInTheDocument()
    expect(screen.getByTestId('product-price-input')).toBeInTheDocument()
    expect(screen.getByTestId('product-quantity-input')).toBeInTheDocument()
  })

  it('should show validation errors for empty required fields', async () => {
    renderProductForm()
    
    const submitButton = screen.getByTestId('submit-btn')
    fireEvent.click(submitButton)
    
    await waitFor(() => {
      expect(screen.getByTestId('name-error')).toBeVisible()
      expect(screen.getByTestId('sku-error')).toBeVisible()
      expect(screen.getByTestId('description-error')).toBeVisible()
    })
  })

  it('should show price validation error for negative values', async () => {
    renderProductForm()
    
    // Fill required fields
    fireEvent.change(screen.getByTestId('product-name-input'), {
      target: { value: 'Test Product' }
    })
    fireEvent.change(screen.getByTestId('product-sku-input'), {
      target: { value: 'TEST-001' }
    })
    fireEvent.change(screen.getByTestId('product-description-input'), {
      target: { value: 'Test description' }
    })
    
    // Enter negative price
    fireEvent.change(screen.getByTestId('product-price-input'), {
      target: { value: '-10' }
    })
    
    const submitButton = screen.getByTestId('submit-btn')
    fireEvent.click(submitButton)
    
    await waitFor(() => {
      expect(screen.getByTestId('price-error')).toBeVisible()
      expect(screen.getByTestId('price-error')).toHaveTextContent('Price must be greater than 0')
    })
  })

  it('should show price validation error for zero values', async () => {
    renderProductForm()
    
    // Fill required fields
    fireEvent.change(screen.getByTestId('product-name-input'), {
      target: { value: 'Test Product' }
    })
    fireEvent.change(screen.getByTestId('product-sku-input'), {
      target: { value: 'TEST-001' }
    })
    fireEvent.change(screen.getByTestId('product-description-input'), {
      target: { value: 'Test description' }
    })
    
    // Enter zero price
    fireEvent.change(screen.getByTestId('product-price-input'), {
      target: { value: '0' }
    })
    
    const submitButton = screen.getByTestId('submit-btn')
    fireEvent.click(submitButton)
    
    await waitFor(() => {
      expect(screen.getByTestId('price-error')).toBeVisible()
      expect(screen.getByTestId('price-error')).toHaveTextContent('Price must be greater than 0')
    })
  })

  it('should clear validation errors when user starts typing', async () => {
    renderProductForm()
    
    // Trigger validation errors first
    const submitButton = screen.getByTestId('submit-btn')
    fireEvent.click(submitButton)
    
    await waitFor(() => {
      expect(screen.getByTestId('name-error')).toBeVisible()
    })
    
    // Start typing in name field
    fireEvent.change(screen.getByTestId('product-name-input'), {
      target: { value: 'Test' }
    })
    
    await waitFor(() => {
      expect(screen.queryByTestId('name-error')).not.toBeInTheDocument()
    })
  })

  it('should allow positive prices', async () => {
    renderProductForm()
    
    // Fill all required fields with valid values
    fireEvent.change(screen.getByTestId('product-name-input'), {
      target: { value: 'Test Product' }
    })
    fireEvent.change(screen.getByTestId('product-sku-input'), {
      target: { value: 'TEST-001' }
    })
    fireEvent.change(screen.getByTestId('product-description-input'), {
      target: { value: 'Test description' }
    })
    fireEvent.change(screen.getByTestId('product-price-input'), {
      target: { value: '99.99' }
    })
    fireEvent.change(screen.getByTestId('product-quantity-input'), {
      target: { value: '10' }
    })
    
    const submitButton = screen.getByTestId('submit-btn')
    fireEvent.click(submitButton)
    
    // Should not show price error
    await waitFor(() => {
      expect(screen.queryByTestId('price-error')).not.toBeInTheDocument()
    })
  })

  it('should not auto-append 0 when clearing price and quantity fields', async () => {
    renderProductForm()
    
    const priceInput = screen.getByTestId('product-price-input') as HTMLInputElement
    const quantityInput = screen.getByTestId('product-quantity-input') as HTMLInputElement
    
    // Type a value then clear it
    fireEvent.change(priceInput, { target: { value: '25.99' } })
    expect(priceInput.value).toBe('25.99')
    
    // Clear the price field
    fireEvent.change(priceInput, { target: { value: '' } })
    expect(priceInput.value).toBe('')
    
    // Type a value in quantity then clear it
    fireEvent.change(quantityInput, { target: { value: '100' } })
    expect(quantityInput.value).toBe('100')
    
    // Clear the quantity field
    fireEvent.change(quantityInput, { target: { value: '' } })
    expect(quantityInput.value).toBe('')
    
    // Start typing new values - should not auto-append 0
    fireEvent.change(priceInput, { target: { value: '1' } })
    expect(priceInput.value).toBe('1')
    
    fireEvent.change(quantityInput, { target: { value: '5' } })
    expect(quantityInput.value).toBe('5')
  })
})