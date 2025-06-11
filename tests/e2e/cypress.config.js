const { defineConfig } = require('cypress')
const { GenerateCtrfReport } = require('cypress-ctrf-json-reporter')

module.exports = defineConfig({
  e2e: {
    baseUrl: 'http://localhost:5126',
    viewportWidth: 1280,
    viewportHeight: 720,
    video: false,
    screenshotOnRunFailure: true,
    supportFile: 'cypress/support/e2e.js',
    specPattern: 'cypress/e2e/**/*.cy.{js,jsx,ts,tsx}',
    setupNodeEvents(on, config) {
      // implement node event listeners here
      new GenerateCtrfReport({
        on,
        outputFile: 'ctrf-report.json',
        outputDir: 'cypress/reports'
      })
    },
  },
  env: {
    apiUrl: 'http://localhost:5126/api'
  }
})