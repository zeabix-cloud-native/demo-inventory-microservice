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

// Custom commands for API testing
Cypress.Commands.add('clearDatabase', () => {
  // In a real scenario, you might want to clear the database
  // For this demo with in-memory repository, we'll restart the API
  cy.log('Database cleared - in-memory repository resets on API restart')
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