import { Link, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { Package, ShoppingBag } from 'lucide-react'
import { CURRENT_USER_ID } from '../../../core/constants/currentUser'
import { useCart } from '../hooks/useCart'
import { useUpdateCartItem } from '../hooks/useUpdateCartItem'
import { useRemoveCartItem } from '../hooks/useRemoveCartItem'
import { useCheckout } from '../../orders/hooks/useCheckout'
import { CartItemRow } from '../components/CartItemRow'
import { CartOrderSummary } from '../components/CartOrderSummary'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'
import { Button } from '../../../shared/components/Button'
import { EmptyState } from '../../../shared/components/EmptyState'
import { PageHeader } from '../../../shared/components/PageHeader'
import { SurfaceCard } from '../../../shared/components/SurfaceCard'

export function CartPage() {
  const navigate = useNavigate()
  const { data: cart, isLoading, isError } = useCart(CURRENT_USER_ID)
  const updateItem = useUpdateCartItem(CURRENT_USER_ID)
  const removeItem = useRemoveCartItem(CURRENT_USER_ID)
  const checkout = useCheckout()

  if (isLoading) {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    )
  }

  if (isError || !cart) {
    return <ErrorMessage message="Failed to load your cart." />
  }

  if (cart.items.length === 0) {
    return (
      <div>
        <PageHeader title="Your Cart" description="Redis Hash-backed shopping cart" />
        <EmptyState
          icon={ShoppingBag}
          title="Your cart is empty"
          description="Add a product to get started. Cart items live in Redis with a 24-hour TTL."
          actionLabel="Browse catalog"
          actionTo="/"
        />
      </div>
    )
  }

  function handleCheckout() {
    checkout.mutate(
      { userId: CURRENT_USER_ID },
      {
        onSuccess: (order) => {
          toast.success('Order placed')
          navigate(`/orders/${order.id}`, { state: order })
        },
        onError: () => toast.error('Checkout failed'),
      },
    )
  }

  return (
    <div>
      <PageHeader
        title="Your Cart"
        description={`${cart.items.length} line item${cart.items.length === 1 ? '' : 's'} · Redis Hash`}
      />

      <div className="grid gap-6 lg:grid-cols-[1fr_320px]">
        <SurfaceCard padding={false} className="overflow-hidden">
          <div className="divide-y divide-border px-4 sm:px-5">
            {cart.items.map((item) => (
              <CartItemRow
                key={item.productId}
                productId={item.productId}
                quantity={item.quantity}
                isUpdating={updateItem.isPending || removeItem.isPending}
                onQuantityChange={(quantity) => {
                  if (quantity < 1) {
                    removeItem.mutate(item.productId)
                    return
                  }
                  updateItem.mutate({ productId: item.productId, quantity })
                }}
                onRemove={() => removeItem.mutate(item.productId)}
              />
            ))}
          </div>
        </SurfaceCard>

        <aside className="lg:sticky lg:top-24 lg:self-start">
          <SurfaceCard>
            <h2 className="text-base font-semibold text-ink">Order summary</h2>
            <div className="mt-4">
              <CartOrderSummary items={cart.items} />
            </div>
            {checkout.isError && (
              <div className="mt-4">
                <ErrorMessage message="Checkout failed. Please try again." />
              </div>
            )}
            <Button className="mt-6 w-full" onClick={handleCheckout} disabled={checkout.isPending}>
              <Package className="h-4 w-4" />
              {checkout.isPending ? 'Placing order...' : 'Checkout'}
            </Button>
            <Link to="/" className="mt-3 block text-center text-sm font-medium text-ink-muted hover:text-ink">
              Continue shopping
            </Link>
          </SurfaceCard>
        </aside>
      </div>
    </div>
  )
}
