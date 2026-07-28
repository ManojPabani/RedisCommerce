import { useQueries } from '@tanstack/react-query'
import { productService } from '../../products/services/productService'
import { productQueryKey } from '../../products/hooks/useProduct'
import type { CartItem } from '../models/cart'
import { formatCurrency } from '../../../shared/utils/formatCurrency'

export function CartOrderSummary({ items }: { items: CartItem[] }) {
  const queries = useQueries({
    queries: items.map((item) => ({
      queryKey: productQueryKey(item.productId),
      queryFn: () => productService.getProductById(item.productId),
    })),
  })

  const loading = queries.some((q) => q.isLoading)
  const subtotal = queries.reduce((sum, q, index) => {
    if (!q.data) return sum
    return sum + q.data.price * items[index].quantity
  }, 0)
  const itemCount = items.reduce((sum, item) => sum + item.quantity, 0)

  return (
    <div className="space-y-3 text-sm">
      <div className="flex justify-between text-ink-muted">
        <span>Items</span>
        <span className="tabular-nums">{itemCount}</span>
      </div>
      <div className="flex justify-between border-t border-border pt-3 text-base font-semibold text-ink">
        <span>Subtotal</span>
        <span className="tabular-nums">{loading ? '…' : formatCurrency(subtotal)}</span>
      </div>
    </div>
  )
}
