import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { toast } from 'sonner'
import { MoreHorizontal, Pencil, ShoppingCart, Trash2 } from 'lucide-react'
import { useProduct } from '../hooks/useProduct'
import { useDeleteProduct } from '../hooks/useDeleteProduct'
import { DeleteConfirmDialog } from '../components/DeleteConfirmDialog'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'
import { Button } from '../../../shared/components/Button'
import { Badge } from '../../../shared/components/Badge'
import { PageHeader } from '../../../shared/components/PageHeader'
import { SurfaceCard } from '../../../shared/components/SurfaceCard'
import { ProductMedia } from '../components/ProductMedia'
import { FavoriteButton } from '../../favorites/components/FavoriteButton'
import { useAddCartItem } from '../../cart/hooks/useAddCartItem'
import { CURRENT_USER_ID } from '../../../core/constants/currentUser'
import { formatCurrency } from '../../../shared/utils/formatCurrency'

export function ProductDetailsPage() {
  const { id } = useParams<{ id: string }>()
  const productId = Number(id)
  const navigate = useNavigate()
  const [showDeleteDialog, setShowDeleteDialog] = useState(false)
  const [showManage, setShowManage] = useState(false)

  const { data: product, isLoading, isError, error } = useProduct(productId)
  const deleteProduct = useDeleteProduct()
  const addCartItem = useAddCartItem(CURRENT_USER_ID)

  if (isLoading) {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    )
  }

  if (isError || !product) {
    return <ErrorMessage message={error instanceof Error ? error.message : 'Product not found.'} />
  }

  const outOfStock = product.stockQuantity <= 0
  const productName = product.name

  function handleDelete() {
    deleteProduct.mutate(productId, {
      onSuccess: () => {
        toast.success('Product deleted')
        navigate('/')
      },
      onError: () => toast.error('Failed to delete product'),
    })
  }

  function handleAddToCart() {
    addCartItem.mutate(
      { productId, quantity: 1 },
      {
        onSuccess: () => toast.success(`Added ${productName} to cart`),
        onError: () => toast.error('Could not add to cart'),
      },
    )
  }

  return (
    <div>
      <PageHeader
        title={product.name}
        breadcrumbs={[
          { label: 'Catalog', to: '/' },
          { label: product.name },
        ]}
        actions={
          <div className="relative">
            <Button variant="outline" size="sm" onClick={() => setShowManage((v) => !v)} aria-expanded={showManage}>
              <MoreHorizontal className="h-4 w-4" />
              Manage
            </Button>
            {showManage && (
              <div className="absolute right-0 z-10 mt-2 w-44 overflow-hidden rounded-lg border border-border bg-surface shadow-elevated">
                <button
                  type="button"
                  className="flex w-full items-center gap-2 px-3 py-2.5 text-left text-sm text-ink hover:bg-surface-muted"
                  onClick={() => navigate(`/products/${product.id}/edit`)}
                >
                  <Pencil className="h-4 w-4 text-ink-muted" />
                  Edit
                </button>
                <button
                  type="button"
                  className="flex w-full items-center gap-2 px-3 py-2.5 text-left text-sm text-danger hover:bg-danger-soft"
                  onClick={() => {
                    setShowManage(false)
                    setShowDeleteDialog(true)
                  }}
                >
                  <Trash2 className="h-4 w-4" />
                  Delete
                </button>
              </div>
            )}
          </div>
        }
      />

      <div className="grid gap-6 lg:grid-cols-2">
        <SurfaceCard padding={false} className="overflow-hidden">
          <ProductMedia name={product.name} className="aspect-[4/3]" textClassName="text-5xl" />
        </SurfaceCard>

        <SurfaceCard>
          <div className="flex items-start justify-between gap-3">
            <div>
              <p className="text-3xl font-semibold tabular-nums tracking-tight text-ink">
                {formatCurrency(product.price)}
              </p>
              <div className="mt-2">
                {outOfStock ? (
                  <Badge variant="danger">Out of stock</Badge>
                ) : product.stockQuantity < 20 ? (
                  <Badge variant="warning">{product.stockQuantity} left in stock</Badge>
                ) : (
                  <Badge variant="success">{product.stockQuantity} in stock</Badge>
                )}
              </div>
            </div>
            <FavoriteButton productId={product.id} name={product.name} price={product.price} />
          </div>

          <p className="mt-5 text-sm leading-relaxed text-ink-muted">{product.description}</p>

          <div className="mt-8 flex flex-col gap-3 sm:flex-row">
            <Button
              className="flex-1"
              disabled={addCartItem.isPending || outOfStock}
              onClick={handleAddToCart}
            >
              <ShoppingCart className="h-4 w-4" />
              {outOfStock ? 'Out of stock' : addCartItem.isPending ? 'Adding...' : 'Add to Cart'}
            </Button>
            <Link to="/cart" className="sm:w-auto">
              <Button variant="outline" className="w-full">
                View cart
              </Button>
            </Link>
          </div>
        </SurfaceCard>
      </div>

      {showDeleteDialog && (
        <DeleteConfirmDialog
          productName={product.name}
          isDeleting={deleteProduct.isPending}
          onConfirm={handleDelete}
          onCancel={() => setShowDeleteDialog(false)}
        />
      )}
    </div>
  )
}
