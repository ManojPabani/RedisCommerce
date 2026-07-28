import { useProducts } from '../hooks/useProducts'
import { ProductCard } from '../components/ProductCard'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'

export function ProductListPage() {
  const { data: products, isLoading, isError, error } = useProducts()

  if (isLoading) {
    return (
      <div className="flex justify-center py-12">
        <Spinner />
      </div>
    )
  }

  if (isError) {
    return <ErrorMessage message={error instanceof Error ? error.message : 'Failed to load products.'} />
  }

  if (!products || products.length === 0) {
    return <p className="text-sm text-slate-500">No products yet. Add your first product to get started.</p>
  }

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {products.map((product) => (
        <ProductCard key={product.id} product={product} />
      ))}
    </div>
  )
}
