import { Link } from 'react-router-dom'
import { PackagePlus, Search } from 'lucide-react'
import { useState } from 'react'
import { useProducts } from '../hooks/useProducts'
import { useProductSearch } from '../hooks/useProductSearch'
import { useDebouncedValue } from '../../../shared/hooks/useDebouncedValue'
import { ProductCard } from '../components/ProductCard'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'
import { EmptyState } from '../../../shared/components/EmptyState'
import { PageHeader } from '../../../shared/components/PageHeader'
import { ProductCardSkeleton } from '../../../shared/components/Skeleton'
import { Button } from '../../../shared/components/Button'
import { Input } from '../../../shared/components/Input'
import { Badge } from '../../../shared/components/Badge'
import { BRAND } from '../../../shared/brand'

export function ProductListPage() {
  const [query, setQuery] = useState('')
  const debouncedQuery = useDebouncedValue(query, 300)
  const isSearching = debouncedQuery.trim().length > 0

  const catalog = useProducts()
  const search = useProductSearch(debouncedQuery)

  const products = isSearching ? search.data : catalog.data
  const isLoading = isSearching ? search.isLoading : catalog.isLoading
  const isError = isSearching ? search.isError : catalog.isError
  const error = isSearching ? search.error : catalog.error

  return (
    <div>
      <PageHeader
        title="Catalog"
        description={
          isSearching
            ? 'Search tracks activity in Redis Bitmap'
            : catalog.data
              ? `${catalog.data.length} product${catalog.data.length === 1 ? '' : 's'} available`
              : BRAND.tagline
        }
        actions={
          <Link to="/products/new">
            <Button>
              <PackagePlus className="h-4 w-4" />
              Add Product
            </Button>
          </Link>
        }
      />

      <div className="mb-6 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <div className="relative max-w-md flex-1">
          <label htmlFor="catalog-search" className="sr-only">
            Search products
          </label>
          <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-ink-subtle" />
          <Input
            id="catalog-search"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Search by name..."
            className="pl-9"
          />
        </div>
        {isSearching && (
          <Badge variant="info" className="self-start sm:self-auto">
            API search · tracks Search activity
          </Badge>
        )}
      </div>

      {isLoading && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <ProductCardSkeleton key={i} />
          ))}
        </div>
      )}

      {isError && (
        <ErrorMessage message={error instanceof Error ? error.message : 'Failed to load products.'} />
      )}

      {!isLoading && !isError && !isSearching && products && products.length === 0 && (
        <EmptyState
          icon={PackagePlus}
          title="No products yet"
          description="Add your first product to get started with the catalog and Redis cache."
          actionLabel="Add Product"
          actionTo="/products/new"
        />
      )}

      {!isLoading && !isError && isSearching && products && products.length === 0 && (
        <EmptyState
          icon={Search}
          title="No matches"
          description={`Nothing matched “${debouncedQuery.trim()}”. Try a different search.`}
          actionLabel="Clear search"
          onAction={() => setQuery('')}
        />
      )}

      {!isLoading && !isError && products && products.length > 0 && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {products.map((product) => (
            <ProductCard key={product.id} product={product} />
          ))}
        </div>
      )}
    </div>
  )
}
