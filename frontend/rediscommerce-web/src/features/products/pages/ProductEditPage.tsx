import { useNavigate, useParams } from 'react-router-dom'
import { useProduct } from '../hooks/useProduct'
import { useCreateProduct } from '../hooks/useCreateProduct'
import { useUpdateProduct } from '../hooks/useUpdateProduct'
import { ProductForm } from '../components/ProductForm'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'
import type { CreateProductRequest } from '../models/product'

export function ProductEditPage() {
  const { id } = useParams<{ id: string }>()
  const isEditMode = id !== undefined
  const productId = Number(id)
  const navigate = useNavigate()

  const { data: product, isLoading, isError } = useProduct(productId)
  const createProduct = useCreateProduct()
  const updateProduct = useUpdateProduct(productId)

  if (isEditMode && isLoading) {
    return (
      <div className="flex justify-center py-12">
        <Spinner />
      </div>
    )
  }

  if (isEditMode && (isError || !product)) {
    return <ErrorMessage message="Product not found." />
  }

  function handleSubmit(request: CreateProductRequest) {
    if (isEditMode) {
      updateProduct.mutate(request, {
        onSuccess: (updated) => navigate(`/products/${updated.id}`),
      })
    } else {
      createProduct.mutate(request, {
        onSuccess: (created) => navigate(`/products/${created.id}`),
      })
    }
  }

  return (
    <div className="max-w-lg rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <h1 className="mb-6 text-xl font-semibold text-slate-900">
        {isEditMode ? 'Edit Product' : 'Add Product'}
      </h1>
      <ProductForm
        initialValue={product}
        submitLabel={isEditMode ? 'Save Changes' : 'Create Product'}
        isSubmitting={createProduct.isPending || updateProduct.isPending}
        onSubmit={handleSubmit}
      />
      {(createProduct.isError || updateProduct.isError) && (
        <div className="mt-4">
          <ErrorMessage message="Something went wrong while saving the product." />
        </div>
      )}
    </div>
  )
}
