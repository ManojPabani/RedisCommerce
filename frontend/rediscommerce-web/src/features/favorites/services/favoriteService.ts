import { axiosClient } from '../../../core/api/axiosClient'
import { API_ROUTES } from '../../../core/constants/apiRoutes'
import type { Favorites } from '../models/favorite'

export const favoriteService = {
  async getFavorites(userId: number): Promise<Favorites> {
    const { data } = await axiosClient.get<Favorites>(API_ROUTES.favorites(userId))
    return data
  },

  async addFavorite(userId: number, productId: number): Promise<void> {
    await axiosClient.post(API_ROUTES.favorite(userId, productId))
  },

  async removeFavorite(userId: number, productId: number): Promise<void> {
    await axiosClient.delete(API_ROUTES.favorite(userId, productId))
  },
}
