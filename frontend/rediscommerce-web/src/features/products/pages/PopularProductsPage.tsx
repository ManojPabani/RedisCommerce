import { Link } from 'react-router-dom'
import { TrendingUp } from 'lucide-react'
import { usePopularProducts } from '../hooks/usePopularProducts'
import { formatCurrency } from '../../../shared/utils/formatCurrency'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'
import { EmptyState } from '../../../shared/components/EmptyState'
import { PageHeader } from '../../../shared/components/PageHeader'
import { SurfaceCard } from '../../../shared/components/SurfaceCard'
import { Badge } from '../../../shared/components/Badge'
import { cn } from '../../../shared/utils/cn'

export function PopularProductsPage() {
  const { data: products, isLoading, isError } = usePopularProducts()

  if (isLoading) {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    )
  }

  if (isError || !products) {
    return <ErrorMessage message="Failed to load popular products." />
  }

  const maxViews = Math.max(...products.map((p) => p.viewCount), 1)

  return (
    <div>
      <PageHeader
        title="Popular Products"
        description="Top products by view count — Redis Sorted Set leaderboard"
      />

      {products.length === 0 ? (
        <EmptyState
          icon={TrendingUp}
          title="No views recorded yet"
          description="Open a few product pages to populate the popularity leaderboard."
          actionLabel="Browse catalog"
          actionTo="/"
        />
      ) : (
        <SurfaceCard padding={false} className="overflow-hidden">
          <ol>
            {products.map((product, index) => {
              const rank = index + 1
              const width = Math.max(6, (product.viewCount / maxViews) * 100)
              return (
                <li key={product.productId} className="border-b border-border last:border-b-0">
                  <Link
                    to={`/products/${product.productId}`}
                    className="relative flex items-center gap-4 overflow-hidden px-4 py-4 transition-colors hover:bg-surface-muted sm:px-5"
                  >
                    <div
                      className="pointer-events-none absolute inset-y-0 left-0 bg-surface-muted"
                      style={{ width: `${width}%` }}
                      aria-hidden
                    />
                    <span
                      className={cn(
                        'relative z-10 flex h-8 w-8 shrink-0 items-center justify-center rounded-full',
                        'border border-border bg-surface text-sm font-semibold tabular-nums text-ink',
                      )}
                    >
                      {rank}
                    </span>
                    <div className="relative z-10 min-w-0 flex-1">
                      <p className="truncate font-semibold text-ink">{product.name}</p>
                      <p className="text-sm tabular-nums text-ink-muted">{formatCurrency(product.price)}</p>
                    </div>
                    <Badge variant="neutral" className="relative z-10 tabular-nums">
                      {product.viewCount} {product.viewCount === 1 ? 'view' : 'views'}
                    </Badge>
                  </Link>
                </li>
              )
            })}
          </ol>
        </SurfaceCard>
      )}
    </div>
  )
}
