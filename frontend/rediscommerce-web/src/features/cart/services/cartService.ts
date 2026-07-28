import { axiosClient } from '../../../core/api/axiosClient'
import { API_ROUTES } from '../../../core/constants/apiRoutes'
import type { AddCartItemRequest, Cart, UpdateCartItemRequest } from '../models/cart'

export const cartService = {
  async getCart(userId: number): Promise<Cart> {
    const { data } = await axiosClient.get<Cart>(API_ROUTES.cart(userId))
    return data
  },

  async addItem(userId: number, request: AddCartItemRequest): Promise<Cart> {
    const { data } = await axiosClient.post<Cart>(API_ROUTES.cartItems(userId), request)
    return data
  },

  async updateItem(userId: number, productId: number, request: UpdateCartItemRequest): Promise<Cart> {
    const { data } = await axiosClient.put<Cart>(API_ROUTES.cartItem(userId, productId), request)
    return data
  },

  async removeItem(userId: number, productId: number): Promise<Cart> {
    const { data } = await axiosClient.delete<Cart>(API_ROUTES.cartItem(userId, productId))
    return data
  },

  async clearCart(userId: number): Promise<void> {
    await axiosClient.delete(API_ROUTES.cart(userId))
  },
}
