import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useProduct } from '../hooks/useProduct'
import { useDeleteProduct } from '../hooks/useDeleteProduct'
import { DeleteConfirmDialog } from '../components/DeleteConfirmDialog'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'
import { Button } from '../../../shared/components/Button'
import { formatCurrency } from '../../../shared/utils/formatCurrency'

export function ProductDetailsPage() {
  const { id } = useParams<{ id: string }>()
  const productId = Number(id)
  const navigate = useNavigate()
  const [showDeleteDialog, setShowDeleteDialog] = useState(false)

  const { data: product, isLoading, isError, error } = useProduct(productId)
  const deleteProduct = useDeleteProduct()

  if (isLoading) {
    return (
      <div className="flex justify-center py-12">
        <Spinner />
      </div>
    )
  }

  if (isError || !product) {
    return <ErrorMessage message={error instanceof Error ? error.message : 'Product not found.'} />
  }

  function handleDelete() {
    deleteProduct.mutate(productId, {
      onSuccess: () => navigate('/'),
    })
  }

  return (
    <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <Link to="/" className="text-sm text-blue-600 hover:text-blue-700">
        &larr; Back to products
      </Link>

      <h1 className="mt-4 text-2xl font-semibold text-slate-900">{product.name}</h1>
      <p className="mt-2 text-slate-600">{product.description}</p>

      <div className="mt-4 flex gap-8">
        <div>
          <p className="text-xs uppercase text-slate-400">Price</p>
          <p className="text-lg font-semibold text-slate-900">{formatCurrency(product.price)}</p>
        </div>
        <div>
          <p className="text-xs uppercase text-slate-400">Stock</p>
          <p className="text-lg font-semibold text-slate-900">{product.stockQuantity}</p>
        </div>
      </div>

      <div className="mt-6 flex gap-3">
        <Button variant="secondary" onClick={() => navigate(`/products/${product.id}/edit`)}>
          Edit
        </Button>
        <Button variant="danger" onClick={() => setShowDeleteDialog(true)}>
          Delete
        </Button>
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
