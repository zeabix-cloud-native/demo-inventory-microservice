const { defineConfig } = require('cypress')
const { GenerateCtrfReport } = require('cypress-ctrf-json-reporter')

// Default to CI environment (port 5126), but allow override via environment variables
const apiBaseUrl = process.env.CYPRESS_API_BASE_URL || 'http://localhost:5126'
const apiUrl = process.env.CYPRESS_API_URL || `${apiBaseUrl}/api`

module.exports = defineConfig({
  e2e: {
    baseUrl: apiBaseUrl,
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
    apiUrl: apiUrl
  }
})