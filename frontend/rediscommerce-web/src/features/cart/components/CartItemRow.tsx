import { Link } from 'react-router-dom'
import { Trash2 } from 'lucide-react'
import { useProduct } from '../../products/hooks/useProduct'
import { formatCurrency } from '../../../shared/utils/formatCurrency'
import { Button } from '../../../shared/components/Button'
import { QuantityStepper } from '../../../shared/components/QuantityStepper'
import { Skeleton } from '../../../shared/components/Skeleton'
import { ProductMedia } from '../../products/components/ProductMedia'

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
      <div className="flex items-center gap-4 py-4">
        <Skeleton className="h-16 w-16 shrink-0 rounded-lg" />
        <div className="flex-1 space-y-2">
          <Skeleton className="h-4 w-40" />
          <Skeleton className="h-3 w-24" />
        </div>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-4 py-4 sm:flex-row sm:items-center sm:justify-between">
      <div className="flex min-w-0 items-center gap-3">
        <ProductMedia
          name={product.name}
          className="h-14 w-14 shrink-0 rounded-lg"
          textClassName="text-sm"
        />
        <div className="min-w-0">
          <Link to={`/products/${product.id}`} className="font-medium text-ink hover:text-accent">
            {product.name}
          </Link>
          <p className="mt-0.5 text-sm text-ink-muted">{formatCurrency(product.price)} each</p>
        </div>
      </div>

      <div className="flex items-center justify-between gap-3 sm:justify-end">
        <QuantityStepper value={quantity} onChange={onQuantityChange} disabled={isUpdating} min={1} />
        <span className="w-20 text-right text-sm font-semibold tabular-nums text-ink">
          {formatCurrency(product.price * quantity)}
        </span>
        <Button
          variant="ghost"
          size="icon"
          className="text-ink-muted hover:text-danger"
          disabled={isUpdating}
          onClick={onRemove}
          aria-label={`Remove ${product.name}`}
        >
          <Trash2 className="h-4 w-4" />
        </Button>
      </div>
    </div>
  )
}
