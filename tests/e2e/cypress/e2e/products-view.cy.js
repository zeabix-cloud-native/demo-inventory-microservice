describe('Product View E2E Tests', () => {
  const apiUrl = Cypress.env('apiUrl')

  beforeEach(() => {
    // Wait for API to be ready
    cy.waitForApi()
  })

  it('should get all products - empty list initially', () => {
    cy.getAllProducts().then((response) => {
      expect(response.status).to.eq(200)
      expect(response.body).to.be.an('array')
      expect(response.body).to.have.length(0)
    })
  })

  it('should get all products after creating some', () => {
    // Create test products
    const product1 = {
      name: 'Test Product 1',
      description: 'Test Description 1',
      sku: 'TEST-001',
      price: 10.99,
      quantityInStock: 100
    }

    const product2 = {
      name: 'Test Product 2',
      description: 'Test Description 2',
      sku: 'TEST-002',
      price: 20.99,
      quantityInStock: 50
    }

    // Create products
    cy.createProduct(product1).then((response) => {
      expect(response.status).to.eq(201)
      expect(response.body).to.have.property('id')
      expect(response.body.name).to.eq(product1.name)
    })

    cy.createProduct(product2).then((response) => {
      expect(response.status).to.eq(201)
      expect(response.body).to.have.property('id')
      expect(response.body.name).to.eq(product2.name)
    })

    // Get all products and verify
    cy.getAllProducts().then((response) => {
      expect(response.status).to.eq(200)
      expect(response.body).to.be.an('array')
      expect(response.body).to.have.length(2)
      
      // Check first product
      const firstProduct = response.body.find(p => p.sku === 'TEST-001')
      expect(firstProduct).to.exist
      expect(firstProduct.name).to.eq('Test Product 1')
      expect(firstProduct.price).to.eq(10.99)
      
      // Check second product
      const secondProduct = response.body.find(p => p.sku === 'TEST-002')
      expect(secondProduct).to.exist
      expect(secondProduct.name).to.eq('Test Product 2')
      expect(secondProduct.price).to.eq(20.99)
    })
  })

  it('should get product by ID', () => {
    const productData = {
      name: 'Single Test Product',
      description: 'Single Test Description',
      sku: 'SINGLE-001',
      price: 15.99,
      quantityInStock: 75
    }

    // Create product
    cy.createProduct(productData).then((createResponse) => {
      expect(createResponse.status).to.eq(201)
      const productId = createResponse.body.id

      // Get product by ID
      cy.getProductById(productId).then((getResponse) => {
        expect(getResponse.status).to.eq(200)
        expect(getResponse.body).to.have.property('id', productId)
        expect(getResponse.body.name).to.eq(productData.name)
        expect(getResponse.body.description).to.eq(productData.description)
        expect(getResponse.body.sku).to.eq(productData.sku)
        expect(getResponse.body.price).to.eq(productData.price)
        expect(getResponse.body.quantityInStock).to.eq(productData.quantityInStock)
        expect(getResponse.body).to.have.property('createdAt')
        expect(getResponse.body).to.have.property('updatedAt')
      })
    })
  })

  it('should return 404 for non-existent product', () => {
    cy.request({
      method: 'GET',
      url: `${apiUrl}/products/999`,
      failOnStatusCode: false
    }).then((response) => {
      expect(response.status).to.eq(404)
    })
  })

  it('should get product by SKU', () => {
    const productData = {
      name: 'SKU Test Product',
      description: 'SKU Test Description',
      sku: 'SKU-TEST-001',
      price: 25.99,
      quantityInStock: 30
    }

    // Create product
    cy.createProduct(productData).then((createResponse) => {
      expect(createResponse.status).to.eq(201)

      // Get product by SKU
      cy.request({
        method: 'GET',
        url: `${apiUrl}/products/sku/${productData.sku}`
      }).then((getResponse) => {
        expect(getResponse.status).to.eq(200)
        expect(getResponse.body.name).to.eq(productData.name)
        expect(getResponse.body.sku).to.eq(productData.sku)
        expect(getResponse.body.price).to.eq(productData.price)
      })
    })
  })

  it('should search products by name', () => {
    const product1 = {
      name: 'Laptop Computer',
      description: 'High performance laptop',
      sku: 'LAPTOP-001',
      price: 999.99,
      quantityInStock: 10
    }

    const product2 = {
      name: 'Desktop Computer',
      description: 'Powerful desktop',
      sku: 'DESKTOP-001',
      price: 1299.99,
      quantityInStock: 5
    }

    // Create products
    cy.createProduct(product1)
    cy.createProduct(product2)

    // Search for "Computer"
    cy.request({
      method: 'GET',
      url: `${apiUrl}/products/search?searchTerm=Computer`
    }).then((response) => {
      expect(response.status).to.eq(200)
      expect(response.body).to.be.an('array')
      expect(response.body).to.have.length(2)
      
      const names = response.body.map(p => p.name)
      expect(names).to.include('Laptop Computer')
      expect(names).to.include('Desktop Computer')
    })

    // Search for "Laptop"
    cy.request({
      method: 'GET',
      url: `${apiUrl}/products/search?searchTerm=Laptop`
    }).then((response) => {
      expect(response.status).to.eq(200)
      expect(response.body).to.be.an('array')
      expect(response.body).to.have.length(1)
      expect(response.body[0].name).to.eq('Laptop Computer')
    })
  })
})