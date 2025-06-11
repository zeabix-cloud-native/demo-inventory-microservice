describe('Product View Frontend E2E Tests', () => {
  beforeEach(() => {
    cy.visitProductList()
  })

  it('should display empty state when no products exist', () => {
    cy.get('[data-testid="no-products"]').should('be.visible')
    cy.contains('No products found.').should('be.visible')
    cy.get('[data-testid="create-first-product-btn"]').should('be.visible')
  })

  it('should display product list after creating products via UI', () => {
    const product1 = {
      name: 'Test Product 1',
      description: 'Test Description 1',
      sku: 'TEST-001',
      price: 10.99,
      quantityInStock: 100
    }

    const product2 = {
      name: 'Test Product 2',
      description: 'Test Description 2',
      sku: 'TEST-002',
      price: 20.99,
      quantityInStock: 50
    }

    // Create first product
    cy.createProductViaUI(product1)
    cy.get('[data-testid="products-table"]').should('be.visible')
    
    // Create second product
    cy.get('[data-testid="add-new-product-btn"]').click()
    cy.createProductViaUI(product2)
    
    // Verify both products are displayed
    cy.get('[data-testid="products-table"]').should('be.visible')
    cy.contains(product1.name).should('be.visible')
    cy.contains(product2.name).should('be.visible')
    cy.contains('$10.99').should('be.visible')
    cy.contains('$20.99').should('be.visible')
  })

  it('should display product details correctly in the table', () => {
    const productData = {
      name: 'Single Test Product',
      description: 'Single Test Description',
      sku: 'SINGLE-001',
      price: 15.99,
      quantityInStock: 75
    }

    cy.createProductViaUI(productData)
    
    // Verify all product details are displayed
    cy.get('[data-testid="products-table"]').should('be.visible')
    cy.contains(productData.name).should('be.visible')
    cy.contains(productData.sku).should('be.visible')
    cy.contains(productData.description.substring(0, 20)).should('be.visible') // Partial description
    cy.contains('$15.99').should('be.visible')
    cy.contains('75').should('be.visible')
    
    // Verify action buttons are present
    cy.get('[data-testid^="edit-product-"]').should('be.visible')
    cy.get('[data-testid^="delete-product-"]').should('be.visible')
  })

  it('should navigate to edit form when clicking Edit button', () => {
    const productData = {
      name: 'Edit Test Product',
      description: 'Edit Test Description',
      sku: 'EDIT-001',
      price: 25.99,
      quantityInStock: 30
    }

    cy.createProductViaUI(productData)
    
    // Click edit button for the product
    cy.get('[data-testid^="edit-product-"]').first().click()
    
    // Should navigate to edit form
    cy.url().should('include', '/product/')
    cy.get('[data-testid="form-title"]').should('contain.text', 'Edit Product')
    cy.get('[data-testid="product-name-input"]').should('have.value', productData.name)
    cy.get('[data-testid="product-sku-input"]').should('have.value', productData.sku)
    cy.get('[data-testid="product-sku-input"]').should('be.disabled') // SKU should be disabled in edit mode
  })

  it('should handle product deletion via UI', () => {
    const productData = {
      name: 'Delete Test Product',
      description: 'Delete Test Description',
      sku: 'DELETE-001',
      price: 35.99,
      quantityInStock: 40
    }

    cy.createProductViaUI(productData)
    
    // Verify product exists
    cy.contains(productData.name).should('be.visible')
    
    // Mock the confirm dialog to return true
    cy.window().then((win) => {
      cy.stub(win, 'confirm').returns(true)
    })
    
    // Click delete button
    cy.get('[data-testid^="delete-product-"]').first().click()
    
    // Product should be removed from the list
    cy.contains(productData.name).should('not.exist')
    cy.get('[data-testid="no-products"]').should('be.visible')
  })

  it('should search products by name via UI', () => {
    const product1 = {
      name: 'Laptop Computer',
      description: 'High performance laptop',
      sku: 'LAPTOP-001',
      price: 999.99,
      quantityInStock: 10
    }

    const product2 = {
      name: 'Desktop Computer',
      description: 'Powerful desktop',
      sku: 'DESKTOP-001',
      price: 1299.99,
      quantityInStock: 5
    }

    const product3 = {
      name: 'Wireless Mouse',
      description: 'Ergonomic mouse',
      sku: 'MOUSE-001',
      price: 29.99,
      quantityInStock: 100
    }

    // Create products
    cy.createProductViaUI(product1)
    cy.get('[data-testid="add-new-product-btn"]').click()
    cy.createProductViaUI(product2)
    cy.get('[data-testid="add-new-product-btn"]').click()
    cy.createProductViaUI(product3)
    
    // All products should be visible initially
    cy.contains(product1.name).should('be.visible')
    cy.contains(product2.name).should('be.visible')
    cy.contains(product3.name).should('be.visible')
    
    // Search for "Computer"
    cy.searchProductsViaUI('Computer')
    
    // Only computer products should be visible
    cy.contains(product1.name).should('be.visible')
    cy.contains(product2.name).should('be.visible')
    cy.contains(product3.name).should('not.exist')
    
    // Search for "Laptop"
    cy.searchProductsViaUI('Laptop')
    
    // Only laptop should be visible
    cy.contains(product1.name).should('be.visible')
    cy.contains(product2.name).should('not.exist')
    cy.contains(product3.name).should('not.exist')
    
    // Clear search
    cy.clearSearchViaUI()
    
    // All products should be visible again
    cy.contains(product1.name).should('be.visible')
    cy.contains(product2.name).should('be.visible')
    cy.contains(product3.name).should('be.visible')
  })

  it('should display low stock warning for products with quantity less than 10', () => {
    const lowStockProduct = {
      name: 'Low Stock Product',
      description: 'Product with low stock',
      sku: 'LOW-001',
      price: 15.99,
      quantityInStock: 5
    }

    const normalStockProduct = {
      name: 'Normal Stock Product',
      description: 'Product with normal stock',
      sku: 'NORMAL-001',
      price: 25.99,
      quantityInStock: 50
    }

    cy.createProductViaUI(lowStockProduct)
    cy.get('[data-testid="add-new-product-btn"]').click()
    cy.createProductViaUI(normalStockProduct)
    
    // Low stock product should have low-stock class
    cy.get('[data-testid^="product-stock-"]').contains('5').should('have.class', 'low-stock')
    
    // Normal stock product should not have low-stock class
    cy.get('[data-testid^="product-stock-"]').contains('50').should('not.have.class', 'low-stock')
  })

  it('should handle search with no results', () => {
    const productData = {
      name: 'Test Product',
      description: 'Test Description',
      sku: 'TEST-001',
      price: 15.99,
      quantityInStock: 25
    }

    cy.createProductViaUI(productData)
    
    // Search for non-existent product
    cy.searchProductsViaUI('NonExistentProduct')
    
    // Should show no products message
    cy.get('[data-testid="no-products"]').should('be.visible')
    cy.contains('No products found.').should('be.visible')
    
    // Clear search to show products again
    cy.clearSearchViaUI()
    cy.contains(productData.name).should('be.visible')
  })
})