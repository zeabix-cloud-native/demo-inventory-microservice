describe('Product Create Frontend E2E Tests', () => {
  beforeEach(() => {
    cy.visitProductList()
  })

  it('should display create product form when clicking Add New Product button', () => {
    cy.get('[data-testid="add-new-product-btn"]').click()
    cy.url().should('include', '/product/new')
    cy.get('[data-testid="form-title"]').should('contain.text', 'Create New Product')
    cy.get('[data-testid="product-form"]').should('be.visible')
  })

  it('should create a product with valid data via UI', () => {
    const productData = {
      name: 'New Test Product',
      description: 'A brand new test product',
      sku: 'NEW-TEST-001',
      price: 29.99,
      quantityInStock: 200
    }

    cy.createProductViaUI(productData)
    
    // Should redirect to product list after successful creation
    cy.url().should('eq', Cypress.config().baseUrl + '/')
    
    // Verify product appears in the list
    cy.get('[data-testid="products-table"]').should('be.visible')
    cy.contains(productData.name).should('be.visible')
    cy.contains(productData.sku).should('be.visible')
    cy.contains('$29.99').should('be.visible')
  })

  it('should show validation errors for empty required fields', () => {
    cy.visit('/product/new')
    
    // Try to submit without filling required fields
    cy.get('[data-testid="submit-btn"]').click()
    
    // Should stay on the form page and show validation errors
    cy.url().should('include', '/product/new')
    cy.get('[data-testid="name-error"]').should('be.visible')
    cy.get('[data-testid="sku-error"]').should('be.visible')
    cy.get('[data-testid="description-error"]').should('be.visible')
  })

  it('should validate price field accepts only positive numbers', () => {
    cy.visit('/product/new')
    
    // Fill required fields
    cy.get('[data-testid="product-name-input"]').type('Test Product')
    cy.get('[data-testid="product-sku-input"]').type('TEST-001')
    cy.get('[data-testid="product-description-input"]').type('Test description')
    
    // Try negative price
    cy.get('[data-testid="product-price-input"]').clear().type('-10')
    cy.get('[data-testid="product-quantity-input"]').clear().type('5')
    
    cy.get('[data-testid="submit-btn"]').click()
    cy.get('[data-testid="price-error"]').should('be.visible')
  })

  it('should allow creating product with zero quantity', () => {
    const productData = {
      name: 'Out of Stock Product',
      description: 'This product is out of stock',
      sku: 'OOS-001',
      price: 15.99,
      quantityInStock: 0
    }

    cy.createProductViaUI(productData)
    
    // Should redirect to product list
    cy.url().should('eq', Cypress.config().baseUrl + '/')
    
    // Verify product appears with 0 quantity
    cy.contains(productData.name).should('be.visible')
    cy.get('[data-testid^="product-stock-"]').contains('0').should('be.visible')
  })

  it('should cancel product creation and return to list', () => {
    cy.visit('/product/new')
    
    // Fill some data
    cy.get('[data-testid="product-name-input"]').type('Test Product')
    cy.get('[data-testid="product-description-input"]').type('Test description')
    
    // Click cancel
    cy.get('[data-testid="cancel-btn"]').click()
    
    // Should return to product list
    cy.url().should('eq', Cypress.config().baseUrl + '/')
    cy.get('[data-testid="product-inventory-title"]').should('be.visible')
  })

  it('should handle form validation and show field-specific errors', () => {
    cy.visit('/product/new')
    
    // Submit empty form
    cy.get('[data-testid="submit-btn"]').click()
    
    // All required field errors should be visible
    cy.get('[data-testid="name-error"]').should('contain.text', 'required')
    cy.get('[data-testid="sku-error"]').should('contain.text', 'required')
    cy.get('[data-testid="description-error"]').should('contain.text', 'required')
    
    // Fill name field - its error should disappear
    cy.get('[data-testid="product-name-input"]').type('Test Product')
    cy.get('[data-testid="product-sku-input"]').click() // Trigger blur
    cy.get('[data-testid="name-error"]').should('not.exist')
    
    // Other errors should still exist
    cy.get('[data-testid="sku-error"]').should('be.visible')
    cy.get('[data-testid="description-error"]').should('be.visible')
  })
})