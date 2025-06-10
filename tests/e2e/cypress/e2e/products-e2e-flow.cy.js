describe('Products Complete E2E Flow Tests', () => {
  const apiUrl = Cypress.env('apiUrl')

  beforeEach(() => {
    // Wait for API to be ready
    cy.waitForApi()
  })

  it('should complete full product lifecycle - create, view, search, validate', () => {
    // Step 1: Verify initial state (empty products list)
    cy.getAllProducts().then((response) => {
      expect(response.status).to.eq(200)
      expect(response.body).to.be.an('array')
      expect(response.body).to.have.length(0)
    })

    // Step 2: Create first product
    const product1 = {
      name: 'E2E Test Laptop',
      description: 'High-performance laptop for testing',
      sku: 'E2E-LAPTOP-001',
      price: 1299.99,
      quantityInStock: 10
    }

    cy.createProduct(product1).then((createResponse1) => {
      expect(createResponse1.status).to.eq(201)
      expect(createResponse1.body).to.have.property('id')
      const product1Id = createResponse1.body.id

      // Step 3: Create second product
      const product2 = {
        name: 'E2E Test Mouse',
        description: 'Wireless mouse for testing',
        sku: 'E2E-MOUSE-001',
        price: 29.99,
        quantityInStock: 50
      }

      cy.createProduct(product2).then((createResponse2) => {
        expect(createResponse2.status).to.eq(201)
        expect(createResponse2.body).to.have.property('id')
        const product2Id = createResponse2.body.id

        // Step 4: Verify both products exist in the list
        cy.getAllProducts().then((allProductsResponse) => {
          expect(allProductsResponse.status).to.eq(200)
          expect(allProductsResponse.body).to.be.an('array')
          expect(allProductsResponse.body).to.have.length(2)
          
          const productIds = allProductsResponse.body.map(p => p.id)
          expect(productIds).to.include(product1Id)
          expect(productIds).to.include(product2Id)
        })

        // Step 5: Verify each product can be retrieved individually by ID
        cy.getProductById(product1Id).then((getProduct1Response) => {
          expect(getProduct1Response.status).to.eq(200)
          expect(getProduct1Response.body.name).to.eq(product1.name)
          expect(getProduct1Response.body.sku).to.eq(product1.sku)
          expect(getProduct1Response.body.price).to.eq(product1.price)
        })

        cy.getProductById(product2Id).then((getProduct2Response) => {
          expect(getProduct2Response.status).to.eq(200)
          expect(getProduct2Response.body.name).to.eq(product2.name)
          expect(getProduct2Response.body.sku).to.eq(product2.sku)
          expect(getProduct2Response.body.price).to.eq(product2.price)
        })

        // Step 6: Verify each product can be retrieved by SKU
        cy.request({
          method: 'GET',
          url: `${apiUrl}/products/sku/${product1.sku}`
        }).then((getBySkuResponse1) => {
          expect(getBySkuResponse1.status).to.eq(200)
          expect(getBySkuResponse1.body.id).to.eq(product1Id)
          expect(getBySkuResponse1.body.name).to.eq(product1.name)
        })

        cy.request({
          method: 'GET',
          url: `${apiUrl}/products/sku/${product2.sku}`
        }).then((getBySkuResponse2) => {
          expect(getBySkuResponse2.status).to.eq(200)
          expect(getBySkuResponse2.body.id).to.eq(product2Id)
          expect(getBySkuResponse2.body.name).to.eq(product2.name)
        })

        // Step 7: Test search functionality
        cy.request({
          method: 'GET',
          url: `${apiUrl}/products/search?searchTerm=E2E`
        }).then((searchE2EResponse) => {
          expect(searchE2EResponse.status).to.eq(200)
          expect(searchE2EResponse.body).to.be.an('array')
          expect(searchE2EResponse.body).to.have.length(2)
          
          const searchNames = searchE2EResponse.body.map(p => p.name)
          expect(searchNames).to.include(product1.name)
          expect(searchNames).to.include(product2.name)
        })

        // Step 8: Test specific search
        cy.request({
          method: 'GET',
          url: `${apiUrl}/products/search?searchTerm=Laptop`
        }).then((searchLaptopResponse) => {
          expect(searchLaptopResponse.status).to.eq(200)
          expect(searchLaptopResponse.body).to.be.an('array')
          expect(searchLaptopResponse.body).to.have.length(1)
          expect(searchLaptopResponse.body[0].name).to.eq(product1.name)
        })

        // Step 9: Test search with no results
        cy.request({
          method: 'GET',
          url: `${apiUrl}/products/search?searchTerm=NonExistentProduct`
        }).then((searchNoResultsResponse) => {
          expect(searchNoResultsResponse.status).to.eq(200)
          expect(searchNoResultsResponse.body).to.be.an('array')
          expect(searchNoResultsResponse.body).to.have.length(0)
        })

        // Step 10: Test 404 for non-existent product ID
        cy.request({
          method: 'GET',
          url: `${apiUrl}/products/999999`,
          failOnStatusCode: false
        }).then((notFoundResponse) => {
          expect(notFoundResponse.status).to.eq(404)
        })

        // Step 11: Test 404 for non-existent SKU
        cy.request({
          method: 'GET',
          url: `${apiUrl}/products/sku/NON-EXISTENT-SKU`,
          failOnStatusCode: false
        }).then((notFoundSkuResponse) => {
          expect(notFoundSkuResponse.status).to.eq(404)
        })
      })
    })
  })

  it('should handle product creation with various data types correctly', () => {
    const testProducts = [
      {
        name: 'Budget Item',
        description: 'Low cost item',
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
      }
    ]

    // Create all test products
    testProducts.forEach((productData, index) => {
      cy.createProduct(productData).then((response) => {
        expect(response.status).to.eq(201)
        expect(response.body.name).to.eq(productData.name)
        expect(response.body.sku).to.eq(productData.sku)
        expect(response.body.price).to.eq(productData.price)
        expect(response.body.quantityInStock).to.eq(productData.quantityInStock)
        expect(response.body).to.have.property('id')
        expect(response.body).to.have.property('createdAt')
        expect(response.body).to.have.property('updatedAt')
      })
    })

    // Verify all products can be retrieved
    cy.getAllProducts().then((response) => {
      expect(response.status).to.eq(200)
      expect(response.body).to.be.an('array')
      expect(response.body.length).to.be.at.least(testProducts.length)

      // Verify each test product exists in the response
      testProducts.forEach((productData) => {
        const foundProduct = response.body.find(p => p.sku === productData.sku)
        expect(foundProduct).to.exist
        expect(foundProduct.name).to.eq(productData.name)
        expect(foundProduct.price).to.eq(productData.price)
        expect(foundProduct.quantityInStock).to.eq(productData.quantityInStock)
      })
    })
  })

  it('should validate complete end-to-end workflow with data integrity', () => {
    const product = {
      name: 'Data Integrity Test Product',
      description: 'Testing data consistency across all operations',
      sku: 'DATA-INT-001',
      price: 123.45,
      quantityInStock: 25
    }

    // Create product and store the response
    cy.createProduct(product).then((createResponse) => {
      expect(createResponse.status).to.eq(201)
      const productId = createResponse.body.id
      const createdAt = createResponse.body.createdAt
      const updatedAt = createResponse.body.updatedAt

      // Verify data integrity across all retrieval methods
      cy.getProductById(productId).then((getByIdResponse) => {
        expect(getByIdResponse.status).to.eq(200)
        expect(getByIdResponse.body.id).to.eq(productId)
        expect(getByIdResponse.body.name).to.eq(product.name)
        expect(getByIdResponse.body.description).to.eq(product.description)
        expect(getByIdResponse.body.sku).to.eq(product.sku)
        expect(getByIdResponse.body.price).to.eq(product.price)
        expect(getByIdResponse.body.quantityInStock).to.eq(product.quantityInStock)
        expect(getByIdResponse.body.createdAt).to.eq(createdAt)
        expect(getByIdResponse.body.updatedAt).to.eq(updatedAt)
      })

      cy.request({
        method: 'GET',
        url: `${apiUrl}/products/sku/${product.sku}`
      }).then((getBySkuResponse) => {
        expect(getBySkuResponse.status).to.eq(200)
        expect(getBySkuResponse.body.id).to.eq(productId)
        expect(getBySkuResponse.body.name).to.eq(product.name)
        expect(getBySkuResponse.body.description).to.eq(product.description)
        expect(getBySkuResponse.body.sku).to.eq(product.sku)
        expect(getBySkuResponse.body.price).to.eq(product.price)
        expect(getBySkuResponse.body.quantityInStock).to.eq(product.quantityInStock)
        expect(getBySkuResponse.body.createdAt).to.eq(createdAt)
        expect(getBySkuResponse.body.updatedAt).to.eq(updatedAt)
      })

      cy.getAllProducts().then((getAllResponse) => {
        expect(getAllResponse.status).to.eq(200)
        const foundProduct = getAllResponse.body.find(p => p.id === productId)
        expect(foundProduct).to.exist
        expect(foundProduct.name).to.eq(product.name)
        expect(foundProduct.description).to.eq(product.description)
        expect(foundProduct.sku).to.eq(product.sku)
        expect(foundProduct.price).to.eq(product.price)
        expect(foundProduct.quantityInStock).to.eq(product.quantityInStock)
        expect(foundProduct.createdAt).to.eq(createdAt)
        expect(foundProduct.updatedAt).to.eq(updatedAt)
      })

      cy.request({
        method: 'GET',
        url: `${apiUrl}/products/search?searchTerm=${product.name}`
      }).then((searchResponse) => {
        expect(searchResponse.status).to.eq(200)
        expect(searchResponse.body).to.be.an('array')
        expect(searchResponse.body.length).to.be.at.least(1)
        const foundProduct = searchResponse.body.find(p => p.id === productId)
        expect(foundProduct).to.exist
        expect(foundProduct.name).to.eq(product.name)
        expect(foundProduct.description).to.eq(product.description)
        expect(foundProduct.sku).to.eq(product.sku)
        expect(foundProduct.price).to.eq(product.price)
        expect(foundProduct.quantityInStock).to.eq(product.quantityInStock)
        expect(foundProduct.createdAt).to.eq(createdAt)
        expect(foundProduct.updatedAt).to.eq(updatedAt)
      })
    })
  })
})