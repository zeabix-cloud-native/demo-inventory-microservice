export interface Product {
  id: number;
  name: string;
  description: string;
  sku: string;
  price: number;
  quantityInStock: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProductDto {
  name: string;
  description: string;
  sku: string;
  price: number;
  quantityInStock: number;
}

export interface UpdateProductDto {
  name: string;
  description: string;
  price: number;
  quantityInStock: number;
}