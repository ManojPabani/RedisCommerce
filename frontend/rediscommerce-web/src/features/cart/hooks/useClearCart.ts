import { useMutation, useQueryClient } from '@tanstack/react-query'
import { cartService } from '../services/cartService'
import { cartQueryKey } from './useCart'

export function useClearCart(userId: number) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => cartService.clearCart(userId),
    onSuccess: () => {
      queryClient.setQueryData(cartQueryKey(userId), { userId, items: [] })
    },
  })
}
