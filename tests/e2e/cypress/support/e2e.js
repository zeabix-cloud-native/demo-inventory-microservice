// ***********************************************************
// This example support/e2e.js is processed and
// loaded automatically before your test files.
//
// This is a great place to put global configuration and
// behavior that modifies Cypress.
//
// You can change the location of this file or turn off
// automatically serving support files with the
// 'supportFile' configuration option.
//
// You can read more here:
// https://on.cypress.io/configuration
// ***********************************************************

// Import commands.js using ES2015 syntax:
import './commands'

// Alternatively you can use CommonJS syntax:
// require('./commands')

// Custom commands for frontend E2E testing
Cypress.Commands.add('clearDatabase', () => {
  // Clear database by making API call to delete all products
  cy.request({
    method: 'GET',
    url: `${Cypress.env('apiUrl')}/products`,
    failOnStatusCode: false
  }).then((response) => {
    if (response.status === 200 && response.body.length > 0) {
      response.body.forEach((product) => {
        cy.request({
          method: 'DELETE',
          url: `${Cypress.env('apiUrl')}/products/${product.id}`,
          failOnStatusCode: false
        })
      })
    }
  })
})

Cypress.Commands.add('waitForApi', () => {
  cy.request({
    url: `${Cypress.env('apiUrl')}/products`,
    method: 'GET',
    failOnStatusCode: false
  }).then((response) => {
    expect(response.status).to.be.oneOf([200, 404])
  })
})

// Setup to run before each test
beforeEach(() => {
  // Ensure API is ready before running frontend tests
  cy.waitForApi()
  
  // Clear any existing data to ensure clean state
  cy.clearDatabase()
})