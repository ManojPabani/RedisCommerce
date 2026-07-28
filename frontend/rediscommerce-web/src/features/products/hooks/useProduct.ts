import { useQuery } from '@tanstack/react-query'
import { productService } from '../services/productService'

export function useProduct(id: number) {
  return useQuery({
    queryKey: ['products', id] as const,
    queryFn: () => productService.getProductById(id),
    enabled: Number.isFinite(id),
  })
}
