import { useQuery } from '@tanstack/react-query'
import { productService } from '../services/productService'

export const productsQueryKey = ['products'] as const

export function useProducts() {
  return useQuery({
    queryKey: productsQueryKey,
    queryFn: productService.getProducts,
  })
}
