import { useQuery } from '@tanstack/react-query'
import { productService } from '../services/productService'

export function productQueryKey(id: number) {
  return ['products', id] as const
}

export function useProduct(id: number) {
  return useQuery({
    queryKey: productQueryKey(id),
    queryFn: () => productService.getProductById(id),
    enabled: Number.isFinite(id),
  })
}
