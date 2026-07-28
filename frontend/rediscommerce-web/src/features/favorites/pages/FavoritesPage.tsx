import { Heart } from 'lucide-react'
import { Link } from 'react-router-dom'
import { CURRENT_USER_ID } from '../../../core/constants/currentUser'
import { useFavorites } from '../hooks/useFavorites'
import { FavoriteButton } from '../components/FavoriteButton'
import { ProductMedia } from '../../products/components/ProductMedia'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'
import { EmptyState } from '../../../shared/components/EmptyState'
import { PageHeader } from '../../../shared/components/PageHeader'
import { formatCurrency } from '../../../shared/utils/formatCurrency'

export function FavoritesPage() {
  const { data: favorites, isLoading, isError } = useFavorites(CURRENT_USER_ID)

  if (isLoading) {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    )
  }

  if (isError || !favorites) {
    return <ErrorMessage message="Failed to load favorites." />
  }

  return (
    <div>
      <PageHeader
        title="Favorites"
        description="Redis Set-backed wishlist — duplicate-free membership"
      />

      {favorites.products.length === 0 ? (
        <EmptyState
          icon={Heart}
          title="No favorites yet"
          description="Tap the heart on any product to save it here."
          actionLabel="Browse catalog"
          actionTo="/"
        />
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {favorites.products.map((product) => (
            <div
              key={product.productId}
              className="flex items-center gap-3 rounded-xl border border-border bg-surface p-4 shadow-card transition-all hover:-translate-y-0.5 hover:border-border-strong hover:shadow-elevated"
            >
              <Link to={`/products/${product.productId}`} className="flex min-w-0 flex-1 items-center gap-3">
                <ProductMedia
                  name={product.name}
                  className="h-12 w-12 shrink-0 rounded-lg"
                  textClassName="text-sm"
                />
                <div className="min-w-0">
                  <p className="truncate font-semibold text-ink">{product.name}</p>
                  <p className="mt-0.5 text-sm tabular-nums text-ink-muted">
                    {formatCurrency(product.price)}
                  </p>
                </div>
              </Link>
              <FavoriteButton productId={product.productId} name={product.name} price={product.price} />
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
