import { useMutation, useQueryClient } from '@tanstack/react-query'
import { productService } from '../services/productService'
import { productsQueryKey } from './useProducts'

export function useDeleteProduct() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) => productService.deleteProduct(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: productsQueryKey })
    },
  })
}
