import { useMutation, useQueryClient } from '@tanstack/react-query'
import { cartService } from '../services/cartService'
import { cartQueryKey } from './useCart'

export function useUpdateCartItem(userId: number) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ productId, quantity }: { productId: number; quantity: number }) =>
      cartService.updateItem(userId, productId, { quantity }),
    onSuccess: (cart) => {
      queryClient.setQueryData(cartQueryKey(userId), cart)
    },
  })
}
