// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************

// Custom command to create a product via UI
Cypress.Commands.add('createProductViaUI', (productData) => {
  cy.visit('/product/new')
  cy.get('[data-testid="product-name-input"]').type(productData.name)
  cy.get('[data-testid="product-sku-input"]').type(productData.sku)
  cy.get('[data-testid="product-description-input"]').type(productData.description)
  cy.get('[data-testid="product-price-input"]').clear().type(productData.price.toString())
  cy.get('[data-testid="product-quantity-input"]').clear().type(productData.quantityInStock.toString())
  cy.get('[data-testid="submit-btn"]').click()
})

// Custom command to search for products via UI
Cypress.Commands.add('searchProductsViaUI', (searchTerm) => {
  cy.get('[data-testid="search-input"]').clear().type(searchTerm)
  cy.get('[data-testid="search-btn"]').click()
})

// Custom command to clear search via UI
Cypress.Commands.add('clearSearchViaUI', () => {
  cy.get('[data-testid="clear-search-btn"]').click()
})

// Custom command to visit product list and wait for it to load
Cypress.Commands.add('visitProductList', () => {
  cy.visit('/')
  cy.get('[data-testid="product-inventory-title"]').should('be.visible')
})

// Custom command to delete a product via UI
Cypress.Commands.add('deleteProductViaUI', (productId) => {
  cy.get(`[data-testid="delete-product-${productId}"]`).click()
  // Handle confirmation dialog
  cy.window().then((win) => {
    cy.stub(win, 'confirm').returns(true)
  })
  cy.get(`[data-testid="delete-product-${productId}"]`).click()
})

// Legacy API commands for backward compatibility (if needed for setup/teardown)
// Custom command to create a product via API
Cypress.Commands.add('createProduct', (productData) => {
  return cy.request({
    method: 'POST',
    url: `${Cypress.env('apiUrl')}/products`,
    body: productData,
    headers: {
      'Content-Type': 'application/json'
    }
  })
})

// Custom command to get all products
Cypress.Commands.add('getAllProducts', () => {
  return cy.request({
    method: 'GET',
    url: `${Cypress.env('apiUrl')}/products`
  })
})

// Custom command to get product by ID
Cypress.Commands.add('getProductById', (id) => {
  return cy.request({
    method: 'GET',
    url: `${Cypress.env('apiUrl')}/products/${id}`
  })
})

// Custom command to delete product by ID
Cypress.Commands.add('deleteProduct', (id) => {
  return cy.request({
    method: 'DELETE',
    url: `${Cypress.env('apiUrl')}/products/${id}`,
    failOnStatusCode: false
  })
})

// Custom command to wait for frontend to be ready
Cypress.Commands.add('waitForFrontend', () => {
  cy.visit('/')
  cy.get('[data-testid="product-inventory-title"]').should('be.visible')
})