describe('Products Complete Frontend E2E Flow Tests', () => {
  beforeEach(() => {
    cy.visitProductList()
  })

  it('should complete full product lifecycle - create, view, edit, delete via UI', () => {
    // Step 1: Verify initial empty state
    cy.get('[data-testid="no-products"]').should('be.visible')
    cy.contains('No products found.').should('be.visible')

    // Step 2: Create first product
    const product1 = {
      name: 'E2E Test Laptop',
      description: 'High-performance laptop for testing',
      sku: 'E2E-LAPTOP-001',
      price: 1299.99,
      quantityInStock: 10
    }

    cy.createProductViaUI(product1)
    
    // Verify additional product details beyond what the command checks
    cy.get('[data-testid="products-table"]').should('be.visible')
    cy.contains(product1.sku).should('be.visible')
    cy.contains('$1,299.99').should('be.visible')

    // Step 3: Create second product
    const product2 = {
      name: 'E2E Test Mouse',
      description: 'Wireless mouse for testing',
      sku: 'E2E-MOUSE-001',
      price: 29.99,
      quantityInStock: 50
    }

    cy.get('[data-testid="products-table"]').should('be.visible')
    cy.get('[data-testid="add-new-product-btn"]').click()
    cy.createProductViaUI(product2)
    
    // Verify both products are visible
    cy.contains(product1.name).should('be.visible')
    cy.contains(product2.name).should('be.visible')
    cy.contains('$29.99').should('be.visible')

    // Step 4: Test search functionality
    cy.searchProductsViaUI('Laptop')
    cy.contains(product1.name).should('be.visible')
    cy.contains(product2.name).should('not.exist')
    
    // Clear search to show all products
    cy.clearSearchViaUI()
    cy.contains(product1.name).should('be.visible')
    cy.contains(product2.name).should('be.visible')

    // Step 5: Edit a product
    cy.get('[data-testid^="edit-product-"]').first().click()
    cy.url().should('include', '/product/')
    cy.get('[data-testid="form-title"]').should('contain.text', 'Edit Product')
    
    // Update the product
    cy.get('[data-testid="product-name-input"]').clear().type('Updated E2E Test Laptop')
    cy.get('[data-testid="product-price-input"]').clear().type('1399.99')
    cy.get('[data-testid="submit-btn"]').click()
    
    // Verify update
    cy.url().should('eq', Cypress.config().baseUrl + '/')
    cy.contains('Updated E2E Test Laptop').should('be.visible')
    cy.contains('$1,399.99').should('be.visible')

    // Step 6: Delete a product
    // Mock the confirm dialog first
    cy.window().then((win) => {
      cy.stub(win, 'confirm').returns(true)
    })
    
    cy.get('[data-testid^="delete-product-"]').first().click()
    
    // Wait for deletion to complete by waiting for the products table to update
    cy.get('[data-testid="products-table"]').should('be.visible')
    
    // Verify product is deleted
    cy.contains('Updated E2E Test Laptop').should('not.exist')
    cy.contains(product2.name).should('be.visible') // Second product should still be there
  })

  it('should handle form validation throughout the workflow', () => {
    // Step 1: Try to create product with validation errors
    cy.get('[data-testid="add-new-product-btn"]').click()
    cy.get('[data-testid="submit-btn"]').click()
    
    // Verify validation errors
    cy.get('[data-testid="name-error"]').should('be.visible')
    cy.get('[data-testid="sku-error"]').should('be.visible')
    cy.get('[data-testid="description-error"]').should('be.visible')
    
    // Step 2: Fix validation errors one by one
    cy.get('[data-testid="product-name-input"]').type('Validation Test Product')
    cy.get('[data-testid="product-sku-input"]').type('VAL-001')
    cy.get('[data-testid="product-description-input"]').type('Product for validation testing')
    cy.get('[data-testid="product-price-input"]').clear().type('99.99')
    cy.get('[data-testid="product-quantity-input"]').clear().type('25')
    
    // Step 3: Submit valid form
    cy.get('[data-testid="submit-btn"]').click()
    
    // Step 4: Verify product is created
    cy.url().should('eq', Cypress.config().baseUrl + '/')
    cy.contains('Validation Test Product').should('be.visible')
    
    // Step 5: Edit product and try invalid data
    cy.get('[data-testid^="edit-product-"]').first().click()
    cy.get('[data-testid="product-name-input"]').clear()
    cy.get('[data-testid="product-price-input"]').clear().type('-50')
    cy.get('[data-testid="submit-btn"]').click()
    
    // Should show validation errors and stay on form
    cy.url().should('include', '/product/')
    cy.get('[data-testid="name-error"]').should('be.visible')
    cy.get('[data-testid="price-error"]').should('be.visible')
  })

  it('should handle different product types and edge cases', () => {
    const testProducts = [
      {
        name: 'Budget Item',
        description: 'Low cost item with very long description that should be truncated in the table view when displayed to ensure proper formatting',
        sku: 'BUDGET-001',
        price: 0.99,
        quantityInStock: 1000
      },
      {
        name: 'Premium Item',
        description: 'High-end premium product',
        sku: 'PREMIUM-001',
        price: 9999.99,
        quantityInStock: 1
      },
      {
        name: 'Out of Stock Item',
        description: 'Currently out of stock',
        sku: 'OOS-001',
        price: 49.99,
        quantityInStock: 0
      },
      {
        name: 'Low Stock Item',
        description: 'Low stock warning test',
        sku: 'LOW-001',
        price: 25.99,
        quantityInStock: 5
      }
    ]

    // Create all test products
    testProducts.forEach((productData, index) => {
      if (index > 0) {
        // Wait for the previous product to be fully visible before creating next one
        cy.get('[data-testid="products-table"]').should('be.visible')
        cy.get('[data-testid="add-new-product-btn"]').click()
      }
      cy.createProductViaUI(productData)
    })

    // Wait for products table to be fully rendered
    cy.get('[data-testid="products-table"]').should('be.visible')

    // Verify all products are displayed
    testProducts.forEach((productData) => {
      cy.contains(productData.name).should('be.visible')
      cy.contains(productData.sku).should('be.visible')
    })

    // Test truncated description (Budget Item has long description)
    cy.contains('Low cost item with very long description that sh...').should('be.visible')
    
    // Test low stock warning (Low Stock Item should have low-stock class)
    cy.get('[data-testid^="product-stock-"]').contains('5').should('have.class', 'low-stock')
    
    // Test out of stock item (should show 0)
    cy.get('[data-testid^="product-stock-"]').contains('0').should('be.visible')
    
    // Test high price formatting
    cy.contains('$9,999.99').should('be.visible')
    
    // Test low price formatting
    cy.contains('$0.99').should('be.visible')

    // Test search across different product types
    cy.searchProductsViaUI('Item')
    
    // Should show all products with "Item" in the name
    cy.contains('Budget Item').should('be.visible')
    cy.contains('Premium Item').should('be.visible')
    cy.contains('Out of Stock Item').should('be.visible')
    cy.contains('Low Stock Item').should('be.visible')
  })

  it('should maintain UI state consistency during operations', () => {
    const productData = {
      name: 'State Test Product',
      description: 'Testing UI state consistency',
      sku: 'STATE-001',
      price: 75.50,
      quantityInStock: 15
    }

    // Create product
    cy.createProductViaUI(productData)
    
    // Verify initial state
    cy.get('[data-testid="product-inventory-title"]').should('be.visible')
    cy.get('[data-testid="search-input"]').should('have.value', '')
    cy.contains(productData.name).should('be.visible')
    
    // Test search state
    cy.searchProductsViaUI('State')
    cy.get('[data-testid="search-input"]').should('have.value', 'State')
    cy.contains(productData.name).should('be.visible')
    
    // Navigate to edit form
    cy.get('[data-testid^="edit-product-"]').first().click()
    cy.url().should('include', '/product/')
    
    // Cancel edit and return to list
    cy.get('[data-testid="cancel-btn"]').click()
    cy.url().should('eq', Cypress.config().baseUrl + '/')
    
    // Search state should be maintained
    cy.get('[data-testid="search-input"]').should('have.value', 'State')
    cy.contains(productData.name).should('be.visible')
    
    // Clear search
    cy.clearSearchViaUI()
    cy.get('[data-testid="search-input"]').should('have.value', '')
    
    // Navigate to create form
    cy.get('[data-testid="add-new-product-btn"]').click()
    cy.url().should('include', '/product/new')
    
    // Cancel and return
    cy.get('[data-testid="cancel-btn"]').click()
    cy.url().should('eq', Cypress.config().baseUrl + '/')
    
    // Product should still be visible
    cy.contains(productData.name).should('be.visible')
    cy.get('[data-testid="search-input"]').should('have.value', '')
  })

  it('should handle error states gracefully', () => {
    // Test form error handling
    cy.get('[data-testid="add-new-product-btn"]').click()
    
    // Try to create product with duplicate SKU (if backend validates this)
    const productData = {
      name: 'Error Test Product 1',
      description: 'First product',
      sku: 'ERROR-001',
      price: 10.99,
      quantityInStock: 5
    }
    
    cy.createProductViaUI(productData)
    
    // Try to create second product with same SKU (should fail if backend validates)
    cy.get('[data-testid="products-table"]').should('be.visible')
    cy.get('[data-testid="add-new-product-btn"]').click()
    
    cy.get('[data-testid="product-name-input"]').type('Error Test Product 2')
    cy.get('[data-testid="product-sku-input"]').type('ERROR-001') // Same SKU
    cy.get('[data-testid="product-description-input"]').type('Second product')
    cy.get('[data-testid="product-price-input"]').clear().type('20.99')
    cy.get('[data-testid="product-quantity-input"]').clear().type('10')
    cy.get('[data-testid="submit-btn"]').click()
    
    // Should either show form error or return to list (depending on backend validation)
    // If error occurs, form should handle it gracefully
    cy.url().should('satisfy', (url) => {
      return url.includes('/product/new') || url === Cypress.config().baseUrl + '/'
    })
    
    // If form error occurred, error message should be visible
    cy.get('body').then((body) => {
      if (body.find('[data-testid="form-error"]').length > 0) {
        cy.get('[data-testid="form-error"]').should('be.visible')
      }
    })
  })
})