import { useNavigate, useParams } from 'react-router-dom'
import { toast } from 'sonner'
import { useProduct } from '../hooks/useProduct'
import { useCreateProduct } from '../hooks/useCreateProduct'
import { useUpdateProduct } from '../hooks/useUpdateProduct'
import { ProductForm } from '../components/ProductForm'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'
import { PageHeader } from '../../../shared/components/PageHeader'
import { SurfaceCard } from '../../../shared/components/SurfaceCard'
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
      <div className="flex justify-center py-16">
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
        onSuccess: (updated) => {
          toast.success('Product updated')
          navigate(`/products/${updated.id}`)
        },
        onError: () => toast.error('Failed to save product'),
      })
    } else {
      createProduct.mutate(request, {
        onSuccess: (created) => {
          toast.success('Product created')
          navigate(`/products/${created.id}`)
        },
        onError: () => toast.error('Failed to create product'),
      })
    }
  }

  return (
    <div className="mx-auto max-w-xl">
      <PageHeader
        title={isEditMode ? 'Edit Product' : 'Add Product'}
        description={
          isEditMode
            ? 'Updates invalidate the Redis product cache on save.'
            : 'New products are stored in SQL Server and cached on first view.'
        }
        breadcrumbs={[
          { label: 'Catalog', to: '/' },
          { label: isEditMode ? 'Edit' : 'Add Product' },
        ]}
      />
      <SurfaceCard>
        <ProductForm
          initialValue={product}
          submitLabel={isEditMode ? 'Save Changes' : 'Create Product'}
          isSubmitting={createProduct.isPending || updateProduct.isPending}
          onSubmit={handleSubmit}
          onCancel={() => navigate(isEditMode && product ? `/products/${product.id}` : '/')}
        />
        {(createProduct.isError || updateProduct.isError) && (
          <div className="mt-4">
            <ErrorMessage message="Something went wrong while saving the product." />
          </div>
        )}
      </SurfaceCard>
    </div>
  )
}
