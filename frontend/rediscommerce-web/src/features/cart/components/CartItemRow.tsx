import { useProduct } from '../../products/hooks/useProduct'
import { formatCurrency } from '../../../shared/utils/formatCurrency'
import { Button } from '../../../shared/components/Button'
import { Spinner } from '../../../shared/components/Spinner'

interface CartItemRowProps {
  productId: number
  quantity: number
  onQuantityChange: (quantity: number) => void
  onRemove: () => void
  isUpdating?: boolean
}

export function CartItemRow({ productId, quantity, onQuantityChange, onRemove, isUpdating }: CartItemRowProps) {
  const { data: product, isLoading } = useProduct(productId)

  if (isLoading || !product) {
    return (
      <div className="flex items-center justify-center py-4">
        <Spinner />
      </div>
    )
  }

  return (
    <div className="flex items-center justify-between gap-4 border-b border-slate-200 py-4 last:border-b-0">
      <div>
        <p className="font-medium text-slate-900">{product.name}</p>
        <p className="text-sm text-slate-500">{formatCurrency(product.price)} each</p>
      </div>

      <div className="flex items-center gap-3">
        <div className="flex items-center gap-2">
          <Button
            variant="secondary"
            className="px-2 py-1"
            disabled={isUpdating}
            onClick={() => onQuantityChange(quantity - 1)}
          >
            −
          </Button>
          <span className="w-6 text-center text-sm">{quantity}</span>
          <Button
            variant="secondary"
            className="px-2 py-1"
            disabled={isUpdating}
            onClick={() => onQuantityChange(quantity + 1)}
          >
            +
          </Button>
        </div>

        <span className="w-20 text-right font-medium text-slate-900">
          {formatCurrency(product.price * quantity)}
        </span>

        <Button variant="danger" className="px-2 py-1 text-xs" disabled={isUpdating} onClick={onRemove}>
          Remove
        </Button>
      </div>
    </div>
  )
}
