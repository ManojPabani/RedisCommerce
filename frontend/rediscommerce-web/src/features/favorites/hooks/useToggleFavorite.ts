import { useMutation, useQueryClient } from '@tanstack/react-query'
import { favoriteService } from '../services/favoriteService'
import { favoritesQueryKey } from './useFavorites'
import type { Favorites } from '../models/favorite'

interface ToggleFavoriteInput {
  productId: number
  name: string
  price: number
  isFavorite: boolean
}

export function useToggleFavorite(userId: number) {
  const queryClient = useQueryClient()
  const queryKey = favoritesQueryKey(userId)

  return useMutation({
    mutationFn: (input: ToggleFavoriteInput) =>
      input.isFavorite
        ? favoriteService.removeFavorite(userId, input.productId)
        : favoriteService.addFavorite(userId, input.productId),

    onMutate: async (input) => {
      await queryClient.cancelQueries({ queryKey })
      const previous = queryClient.getQueryData<Favorites>(queryKey)

      queryClient.setQueryData<Favorites>(queryKey, (current) => {
        const products = current?.products ?? []
        if (input.isFavorite) {
          return { userId, products: products.filter((p) => p.productId !== input.productId) }
        }
        return { userId, products: [...products, { productId: input.productId, name: input.name, price: input.price }] }
      })

      return { previous }
    },

    onError: (_error, _input, context) => {
      if (context?.previous) {
        queryClient.setQueryData(queryKey, context.previous)
      }
    },

    onSettled: () => {
      queryClient.invalidateQueries({ queryKey })
    },
  })
}
