import { useNavigate } from 'react-router-dom'
import { CURRENT_USER_ID } from '../../../core/constants/currentUser'
import { useCart } from '../hooks/useCart'
import { useUpdateCartItem } from '../hooks/useUpdateCartItem'
import { useRemoveCartItem } from '../hooks/useRemoveCartItem'
import { useCheckout } from '../../orders/hooks/useCheckout'
import { CartItemRow } from '../components/CartItemRow'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'
import { Button } from '../../../shared/components/Button'

export function CartPage() {
  const navigate = useNavigate()
  const { data: cart, isLoading, isError } = useCart(CURRENT_USER_ID)
  const updateItem = useUpdateCartItem(CURRENT_USER_ID)
  const removeItem = useRemoveCartItem(CURRENT_USER_ID)
  const checkout = useCheckout()

  if (isLoading) {
    return (
      <div className="flex justify-center py-12">
        <Spinner />
      </div>
    )
  }

  if (isError || !cart) {
    return <ErrorMessage message="Failed to load your cart." />
  }

  if (cart.items.length === 0) {
    return <p className="text-sm text-slate-500">Your cart is empty. Add a product to get started.</p>
  }

  function handleCheckout() {
    checkout.mutate(
      { userId: CURRENT_USER_ID },
      { onSuccess: (order) => navigate(`/orders/${order.id}`, { state: order }) },
    )
  }

  return (
    <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <h1 className="mb-4 text-xl font-semibold text-slate-900">Your Cart</h1>

      <div>
        {cart.items.map((item) => (
          <CartItemRow
            key={item.productId}
            productId={item.productId}
            quantity={item.quantity}
            isUpdating={updateItem.isPending || removeItem.isPending}
            onQuantityChange={(quantity) => updateItem.mutate({ productId: item.productId, quantity })}
            onRemove={() => removeItem.mutate(item.productId)}
          />
        ))}
      </div>

      {checkout.isError && (
        <div className="mt-4">
          <ErrorMessage message="Checkout failed. Please try again." />
        </div>
      )}

      <div className="mt-6 flex justify-end">
        <Button onClick={handleCheckout} disabled={checkout.isPending}>
          {checkout.isPending ? 'Placing order...' : 'Checkout'}
        </Button>
      </div>
    </div>
  )
}
