import { useMutation, useQueryClient } from '@tanstack/react-query'
import { productService } from '../services/productService'
import type { UpdateProductRequest } from '../models/product'
import { productsQueryKey } from './useProducts'

export function useUpdateProduct(id: number) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: UpdateProductRequest) => productService.updateProduct(id, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: productsQueryKey })
      queryClient.invalidateQueries({ queryKey: ['products', id] })
    },
  })
}
