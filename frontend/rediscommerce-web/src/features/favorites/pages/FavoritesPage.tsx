import { Link } from 'react-router-dom'
import { CURRENT_USER_ID } from '../../../core/constants/currentUser'
import { useFavorites } from '../hooks/useFavorites'
import { FavoriteButton } from '../components/FavoriteButton'
import { formatCurrency } from '../../../shared/utils/formatCurrency'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'

export function FavoritesPage() {
  const { data: favorites, isLoading, isError } = useFavorites(CURRENT_USER_ID)

  if (isLoading) {
    return (
      <div className="flex justify-center py-12">
        <Spinner />
      </div>
    )
  }

  if (isError || !favorites) {
    return <ErrorMessage message="Failed to load favorites." />
  }

  if (favorites.products.length === 0) {
    return <p className="text-sm text-slate-500">You haven't favorited any products yet.</p>
  }

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {favorites.products.map((product) => (
        <div
          key={product.productId}
          className="flex items-center justify-between rounded-lg border border-slate-200 bg-white p-4 shadow-sm"
        >
          <Link to={`/products/${product.productId}`} className="min-w-0">
            <p className="truncate font-medium text-slate-900">{product.name}</p>
            <p className="text-sm text-slate-500">{formatCurrency(product.price)}</p>
          </Link>
          <FavoriteButton productId={product.productId} name={product.name} price={product.price} />
        </div>
      ))}
    </div>
  )
}
