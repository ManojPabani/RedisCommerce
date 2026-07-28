import { useMutation, useQueryClient } from '@tanstack/react-query'
import { orderService } from '../services/orderService'
import type { CheckoutRequest } from '../models/order'
import { cartQueryKey } from '../../cart/hooks/useCart'

export function useCheckout() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: CheckoutRequest) => orderService.checkout(request),
    onSuccess: (_order, request) => {
      queryClient.invalidateQueries({ queryKey: cartQueryKey(request.userId) })
    },
  })
}
