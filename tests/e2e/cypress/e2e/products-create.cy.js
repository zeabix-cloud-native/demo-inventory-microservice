describe('Product Create E2E Tests', () => {
  const apiUrl = Cypress.env('apiUrl')

  beforeEach(() => {
    // Wait for API to be ready
    cy.waitForApi()
  })

  it('should create a product with valid data', () => {
    const productData = {
      name: 'New Test Product',
      description: 'A brand new test product',
      sku: 'NEW-TEST-001',
      price: 29.99,
      quantityInStock: 200
    }

    cy.createProduct(productData).then((response) => {
      expect(response.status).to.eq(201)
      expect(response.body).to.have.property('id')
      expect(response.body.id).to.be.a('number')
      expect(response.body.name).to.eq(productData.name)
      expect(response.body.description).to.eq(productData.description)
      expect(response.body.sku).to.eq(productData.sku)
      expect(response.body.price).to.eq(productData.price)
      expect(response.body.quantityInStock).to.eq(productData.quantityInStock)
      expect(response.body).to.have.property('createdAt')
      expect(response.body).to.have.property('updatedAt')
      
      // Verify dates are valid ISO strings
      expect(new Date(response.body.createdAt)).to.be.instanceOf(Date)
      expect(new Date(response.body.updatedAt)).to.be.instanceOf(Date)
    })
  })

  it('should create multiple products successfully', () => {
    const products = [
      {
        name: 'Product A',
        description: 'Description A',
        sku: 'PROD-A-001',
        price: 10.00,
        quantityInStock: 50
      },
      {
        name: 'Product B',
        description: 'Description B',
        sku: 'PROD-B-001',
        price: 20.00,
        quantityInStock: 30
      },
      {
        name: 'Product C',
        description: 'Description C',
        sku: 'PROD-C-001',
        price: 30.00,
        quantityInStock: 40
      }
    ]

    // Create all products
    products.forEach((productData, index) => {
      cy.createProduct(productData).then((response) => {
        expect(response.status).to.eq(201)
        expect(response.body.name).to.eq(productData.name)
        expect(response.body.sku).to.eq(productData.sku)
        expect(response.body.price).to.eq(productData.price)
      })
    })

    // Verify all products exist by getting all products
    cy.getAllProducts().then((response) => {
      expect(response.status).to.eq(200)
      expect(response.body).to.be.an('array')
      expect(response.body.length).to.be.at.least(3)
      
      // Check that our created products exist
      const skus = response.body.map(p => p.sku)
      expect(skus).to.include('PROD-A-001')
      expect(skus).to.include('PROD-B-001')
      expect(skus).to.include('PROD-C-001')
    })
  })

  it('should create product with minimum required fields', () => {
    const minimalProduct = {
      name: 'Minimal Product',
      description: 'Minimal description',
      sku: 'MIN-001',
      price: 1.00,
      quantityInStock: 1
    }

    cy.createProduct(minimalProduct).then((response) => {
      expect(response.status).to.eq(201)
      expect(response.body.name).to.eq(minimalProduct.name)
      expect(response.body.description).to.eq(minimalProduct.description)
      expect(response.body.sku).to.eq(minimalProduct.sku)
      expect(response.body.price).to.eq(minimalProduct.price)
      expect(response.body.quantityInStock).to.eq(minimalProduct.quantityInStock)
    })
  })

  it('should create product with zero quantity', () => {
    const productData = {
      name: 'Out of Stock Product',
      description: 'This product is out of stock',
      sku: 'OOS-001',
      price: 15.99,
      quantityInStock: 0
    }

    cy.createProduct(productData).then((response) => {
      expect(response.status).to.eq(201)
      expect(response.body.quantityInStock).to.eq(0)
    })
  })

  it('should create product with high price', () => {
    const expensiveProduct = {
      name: 'Expensive Product',
      description: 'Very expensive item',
      sku: 'EXP-001',
      price: 9999.99,
      quantityInStock: 1
    }

    cy.createProduct(expensiveProduct).then((response) => {
      expect(response.status).to.eq(201)
      expect(response.body.price).to.eq(9999.99)
    })
  })

  it('should handle product creation with decimal prices correctly', () => {
    const products = [
      {
        name: 'Product with 2 decimals',
        description: 'Price with 2 decimal places',
        sku: 'DEC-2-001',
        price: 12.34,
        quantityInStock: 10
      },
      {
        name: 'Product with 1 decimal',
        description: 'Price with 1 decimal place',
        sku: 'DEC-1-001',
        price: 15.5,
        quantityInStock: 20
      }
    ]

    products.forEach((productData) => {
      cy.createProduct(productData).then((response) => {
        expect(response.status).to.eq(201)
        expect(response.body.price).to.eq(productData.price)
      })
    })
  })

  it('should create product and verify it can be retrieved', () => {
    const productData = {
      name: 'Retrievable Product',
      description: 'Product that can be retrieved after creation',
      sku: 'RETR-001',
      price: 45.67,
      quantityInStock: 25
    }

    // Create product
    cy.createProduct(productData).then((createResponse) => {
      expect(createResponse.status).to.eq(201)
      const productId = createResponse.body.id

      // Retrieve the same product by ID
      cy.getProductById(productId).then((getResponse) => {
        expect(getResponse.status).to.eq(200)
        expect(getResponse.body.id).to.eq(productId)
        expect(getResponse.body.name).to.eq(productData.name)
        expect(getResponse.body.description).to.eq(productData.description)
        expect(getResponse.body.sku).to.eq(productData.sku)
        expect(getResponse.body.price).to.eq(productData.price)
        expect(getResponse.body.quantityInStock).to.eq(productData.quantityInStock)
      })

      // Retrieve the same product by SKU
      cy.request({
        method: 'GET',
        url: `${apiUrl}/products/sku/${productData.sku}`
      }).then((getBySkuResponse) => {
        expect(getBySkuResponse.status).to.eq(200)
        expect(getBySkuResponse.body.id).to.eq(productId)
        expect(getBySkuResponse.body.sku).to.eq(productData.sku)
      })
    })
  })

  it('should validate product creation flow end-to-end', () => {
    const testProduct = {
      name: 'E2E Test Product',
      description: 'End-to-end test product for full validation',
      sku: 'E2E-FULL-001',
      price: 99.99,
      quantityInStock: 100
    }

    // Step 1: Verify product doesn't exist initially
    cy.request({
      method: 'GET',
      url: `${apiUrl}/products/sku/${testProduct.sku}`,
      failOnStatusCode: false
    }).then((response) => {
      expect(response.status).to.eq(404)
    })

    // Step 2: Create the product
    cy.createProduct(testProduct).then((createResponse) => {
      expect(createResponse.status).to.eq(201)
      const productId = createResponse.body.id

      // Step 3: Verify product appears in all products list
      cy.getAllProducts().then((allProductsResponse) => {
        expect(allProductsResponse.status).to.eq(200)
        const createdProduct = allProductsResponse.body.find(p => p.id === productId)
        expect(createdProduct).to.exist
        expect(createdProduct.name).to.eq(testProduct.name)
      })

      // Step 4: Verify product can be found by search
      cy.request({
        method: 'GET',
        url: `${apiUrl}/products/search?searchTerm=E2E`
      }).then((searchResponse) => {
        expect(searchResponse.status).to.eq(200)
        expect(searchResponse.body).to.be.an('array')
        expect(searchResponse.body.length).to.be.at.least(1)
        const foundProduct = searchResponse.body.find(p => p.id === productId)
        expect(foundProduct).to.exist
      })
    })
  })
})