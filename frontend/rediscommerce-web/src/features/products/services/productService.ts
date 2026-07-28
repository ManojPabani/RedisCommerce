import { axiosClient } from '../../../core/api/axiosClient'
import { API_ROUTES } from '../../../core/constants/apiRoutes'
import type { CreateProductRequest, PopularProduct, Product, UpdateProductRequest } from '../models/product'

export const productService = {
  async getProducts(): Promise<Product[]> {
    const { data } = await axiosClient.get<Product[]>(API_ROUTES.products)
    return data
  },

  async getPopularProducts(): Promise<PopularProduct[]> {
    const { data } = await axiosClient.get<PopularProduct[]>(API_ROUTES.popularProducts)
    return data
  },

  async searchProducts(query: string): Promise<Product[]> {
    const { data } = await axiosClient.get<Product[]>(API_ROUTES.productSearch, {
      params: { query },
    })
    return data
  },

  async getProductById(id: number): Promise<Product> {
    const { data } = await axiosClient.get<Product>(API_ROUTES.product(id))
    return data
  },

  async createProduct(request: CreateProductRequest): Promise<Product> {
    const { data } = await axiosClient.post<Product>(API_ROUTES.products, request)
    return data
  },

  async updateProduct(id: number, request: UpdateProductRequest): Promise<Product> {
    const { data } = await axiosClient.put<Product>(API_ROUTES.product(id), request)
    return data
  },

  async deleteProduct(id: number): Promise<void> {
    await axiosClient.delete(API_ROUTES.product(id))
  },
}
