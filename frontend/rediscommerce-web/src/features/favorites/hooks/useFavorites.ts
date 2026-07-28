import { useQuery } from '@tanstack/react-query'
import { favoriteService } from '../services/favoriteService'

export const favoritesQueryKey = (userId: number) => ['favorites', userId] as const

export function useFavorites(userId: number) {
  return useQuery({
    queryKey: favoritesQueryKey(userId),
    queryFn: () => favoriteService.getFavorites(userId),
  })
}
