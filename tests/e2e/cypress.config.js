const { defineConfig } = require('cypress')
const { GenerateCtrfReport } = require('cypress-ctrf-json-reporter')

// Default to frontend development server (port 5173), but allow override via environment variables
const frontendBaseUrl = process.env.CYPRESS_FRONTEND_BASE_URL || 'http://localhost:5173'
const apiBaseUrl = process.env.CYPRESS_API_BASE_URL || 'http://localhost:5126'
const apiUrl = process.env.CYPRESS_API_URL || `${apiBaseUrl}/api`

module.exports = defineConfig({
  // Project ID for Cypress Dashboard recording
  projectId: process.env.CYPRESS_PROJECT_ID,
  
  e2e: {
    baseUrl: frontendBaseUrl,
    viewportWidth: 1280,
    viewportHeight: 720,
    
    // Video recording configuration
    video: process.env.CYPRESS_VIDEO === 'true' || false,
    videoCompression: 32,
    videosFolder: 'cypress/videos',
    
    // Screenshot configuration
    screenshotOnRunFailure: true,
    screenshotsFolder: 'cypress/screenshots',
    
    // Test file configuration
    supportFile: 'cypress/support/e2e.js',
    specPattern: 'cypress/e2e/**/*.cy.{js,jsx,ts,tsx}',
    
    // Recording and reporting setup
    setupNodeEvents(on, config) {
      // CTRF test reporting
      new GenerateCtrfReport({
        on,
        outputFile: 'ctrf-report.json',
        outputDir: 'cypress/reports'
      })
      
      // Additional configuration for recording
      if (process.env.CYPRESS_RECORD_KEY) {
        config.record = true
      }
      
      return config
    },
  },
  env: {
    apiUrl: apiUrl,
    frontendBaseUrl: frontendBaseUrl
  }
})