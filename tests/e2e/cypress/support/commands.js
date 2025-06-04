// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************

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