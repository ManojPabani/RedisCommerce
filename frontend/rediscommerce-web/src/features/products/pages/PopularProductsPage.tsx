import { Link } from 'react-router-dom'
import { usePopularProducts } from '../hooks/usePopularProducts'
import { formatCurrency } from '../../../shared/utils/formatCurrency'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'

export function PopularProductsPage() {
  const { data: products, isLoading, isError } = usePopularProducts()

  if (isLoading) {
    return (
      <div className="flex justify-center py-12">
        <Spinner />
      </div>
    )
  }

  if (isError || !products) {
    return <ErrorMessage message="Failed to load popular products." />
  }

  if (products.length === 0) {
    return <p className="text-sm text-slate-500">No product views recorded yet.</p>
  }

  return (
    <div className="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm">
      <ol>
        {products.map((product, index) => (
          <li key={product.productId}>
            <Link
              to={`/products/${product.productId}`}
              className="flex items-center gap-4 border-b border-slate-200 p-4 last:border-b-0 hover:bg-slate-50"
            >
              <span className="w-6 text-sm font-semibold text-slate-400">{index + 1}</span>
              <div className="min-w-0 flex-1">
                <p className="truncate font-medium text-slate-900">{product.name}</p>
                <p className="text-sm text-slate-500">{formatCurrency(product.price)}</p>
              </div>
              <span className="text-sm text-slate-500">
                {product.viewCount} {product.viewCount === 1 ? 'view' : 'views'}
              </span>
            </Link>
          </li>
        ))}
      </ol>
    </div>
  )
}
