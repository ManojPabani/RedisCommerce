export interface Product {
  id: number
  name: string
  description: string
  price: number
  stockQuantity: number
  createdDate: string
  updatedDate: string
}

export interface CreateProductRequest {
  name: string
  description: string
  price: number
  stockQuantity: number
}

export type UpdateProductRequest = CreateProductRequest

export interface PopularProduct {
  productId: number
  name: string
  price: number
  viewCount: number
}
