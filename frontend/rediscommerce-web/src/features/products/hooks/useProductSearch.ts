import { useQuery } from '@tanstack/react-query'
import { productService } from '../services/productService'

export function productSearchQueryKey(query: string) {
  return ['products', 'search', query] as const
}

export function useProductSearch(query: string) {
  const trimmed = query.trim()

  return useQuery({
    queryKey: productSearchQueryKey(trimmed),
    queryFn: () => productService.searchProducts(trimmed),
    enabled: trimmed.length > 0,
  })
}
