import { useQuery } from '@tanstack/react-query'
import { productService } from '../services/productService'

export function usePopularProducts() {
  return useQuery({
    queryKey: ['products', 'popular'] as const,
    queryFn: productService.getPopularProducts,
  })
}
