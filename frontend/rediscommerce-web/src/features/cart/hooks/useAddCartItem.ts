import { useMutation, useQueryClient } from '@tanstack/react-query'
import { cartService } from '../services/cartService'
import type { AddCartItemRequest } from '../models/cart'
import { cartQueryKey } from './useCart'

export function useAddCartItem(userId: number) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: AddCartItemRequest) => cartService.addItem(userId, request),
    onSuccess: (cart) => {
      queryClient.setQueryData(cartQueryKey(userId), cart)
    },
  })
}
