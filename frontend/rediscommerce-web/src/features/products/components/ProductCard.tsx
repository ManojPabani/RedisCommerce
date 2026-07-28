import { Link } from 'react-router-dom'
import type { MouseEvent } from 'react'
import type { Product } from '../models/product'
import { formatCurrency } from '../../../shared/utils/formatCurrency'
import { Button } from '../../../shared/components/Button'
import { FavoriteButton } from '../../favorites/components/FavoriteButton'
import { useAddCartItem } from '../../cart/hooks/useAddCartItem'
import { CURRENT_USER_ID } from '../../../core/constants/currentUser'

interface ProductCardProps {
  product: Product
}

export function ProductCard({ product }: ProductCardProps) {
  const addCartItem = useAddCartItem(CURRENT_USER_ID)

  function handleAddToCart(event: MouseEvent) {
    event.preventDefault()
    event.stopPropagation()
    addCartItem.mutate({ productId: product.id, quantity: 1 })
  }

  return (
    <Link
      to={`/products/${product.id}`}
      className="block rounded-lg border border-slate-200 bg-white p-4 shadow-sm transition-shadow hover:shadow-md"
    >
      <div className="flex items-start justify-between gap-2">
        <h3 className="text-base font-semibold text-slate-900">{product.name}</h3>
        <FavoriteButton productId={product.id} name={product.name} price={product.price} />
      </div>
      <p className="mt-1 line-clamp-2 text-sm text-slate-500">{product.description}</p>
      <div className="mt-3 flex items-center justify-between">
        <span className="text-lg font-semibold text-slate-900">{formatCurrency(product.price)}</span>
        <span className="text-xs text-slate-500">{product.stockQuantity} in stock</span>
      </div>
      <Button
        variant="secondary"
        className="mt-3 w-full"
        disabled={addCartItem.isPending}
        onClick={handleAddToCart}
      >
        {addCartItem.isPending ? 'Adding...' : 'Add to Cart'}
      </Button>
    </Link>
  )
}
