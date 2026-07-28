import { useMutation, useQueryClient } from '@tanstack/react-query'
import { cartService } from '../services/cartService'
import { cartQueryKey } from './useCart'

export function useRemoveCartItem(userId: number) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (productId: number) => cartService.removeItem(userId, productId),
    onSuccess: (cart) => {
      queryClient.setQueryData(cartQueryKey(userId), cart)
    },
  })
}
