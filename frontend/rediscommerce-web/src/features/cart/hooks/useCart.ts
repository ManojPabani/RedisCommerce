import { useQuery } from '@tanstack/react-query'
import { cartService } from '../services/cartService'

export const cartQueryKey = (userId: number) => ['cart', userId] as const

export function useCart(userId: number) {
  return useQuery({
    queryKey: cartQueryKey(userId),
    queryFn: () => cartService.getCart(userId),
  })
}
