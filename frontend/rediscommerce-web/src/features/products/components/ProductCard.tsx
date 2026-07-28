import { Link } from 'react-router-dom'
import type { MouseEvent } from 'react'
import { toast } from 'sonner'
import { ShoppingCart } from 'lucide-react'
import type { Product } from '../models/product'
import { formatCurrency } from '../../../shared/utils/formatCurrency'
import { Button } from '../../../shared/components/Button'
import { Badge } from '../../../shared/components/Badge'
import { ProductMedia } from './ProductMedia'
import { FavoriteButton } from '../../favorites/components/FavoriteButton'
import { useAddCartItem } from '../../cart/hooks/useAddCartItem'
import { CURRENT_USER_ID } from '../../../core/constants/currentUser'

interface ProductCardProps {
  product: Product
}

function stockBadge(stockQuantity: number) {
  if (stockQuantity <= 0) return <Badge variant="danger">Out of stock</Badge>
  if (stockQuantity < 20) return <Badge variant="warning">{stockQuantity} left</Badge>
  return <Badge variant="success">{stockQuantity} in stock</Badge>
}

export function ProductCard({ product }: ProductCardProps) {
  const addCartItem = useAddCartItem(CURRENT_USER_ID)
  const outOfStock = product.stockQuantity <= 0

  function handleAddToCart(event: MouseEvent) {
    event.preventDefault()
    event.stopPropagation()
    addCartItem.mutate(
      { productId: product.id, quantity: 1 },
      {
        onSuccess: () => toast.success(`Added ${product.name} to cart`),
        onError: () => toast.error('Could not add to cart'),
      },
    )
  }

  return (
    <article className="group flex h-full flex-col overflow-hidden rounded-xl border border-border bg-surface shadow-card transition-all duration-150 hover:-translate-y-0.5 hover:border-border-strong hover:shadow-elevated">
      <Link to={`/products/${product.id}`} className="block">
        <div className="relative">
          <ProductMedia
            name={product.name}
            className="aspect-[4/3]"
            textClassName="text-3xl transition-transform duration-150 group-hover:scale-105"
          />
          <div className="absolute right-3 top-3" onClick={(e) => e.preventDefault()}>
            <FavoriteButton productId={product.id} name={product.name} price={product.price} />
          </div>
        </div>
      </Link>

      <div className="flex flex-1 flex-col p-4">
        <Link to={`/products/${product.id}`} className="min-w-0">
          <h3 className="truncate text-base font-semibold text-ink transition-colors group-hover:text-accent">
            {product.name}
          </h3>
          <p className="mt-1 line-clamp-2 text-sm text-ink-muted">{product.description}</p>
        </Link>

        <div className="mt-auto flex items-center justify-between gap-2 pt-4">
          <span className="text-lg font-semibold tabular-nums text-ink">{formatCurrency(product.price)}</span>
          {stockBadge(product.stockQuantity)}
        </div>

        <Button
          variant="secondary"
          className="mt-3 w-full"
          disabled={addCartItem.isPending || outOfStock}
          onClick={handleAddToCart}
        >
          <ShoppingCart className="h-4 w-4" />
          {outOfStock ? 'Out of stock' : addCartItem.isPending ? 'Adding...' : 'Add to Cart'}
        </Button>
      </div>
    </article>
  )
}
