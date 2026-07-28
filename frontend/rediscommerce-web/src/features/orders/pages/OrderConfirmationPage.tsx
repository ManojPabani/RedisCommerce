import { Link, useLocation, useParams } from 'react-router-dom'
import { formatCurrency } from '../../../shared/utils/formatCurrency'
import type { Order } from '../models/order'

export function OrderConfirmationPage() {
  const { id } = useParams<{ id: string }>()
  const location = useLocation()
  const order = location.state as Order | undefined

  return (
    <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <h1 className="text-xl font-semibold text-slate-900">Order Placed!</h1>
      <p className="mt-2 text-sm text-slate-600">
        Order <span className="font-medium">#{id}</span> has been created and is now in the processing queue.
      </p>
      <p className="mt-1 text-sm text-slate-500">
        A background worker picks up queued orders and processes them within a few seconds.
      </p>

      {order && (
        <div className="mt-4 border-t border-slate-200 pt-4">
          <p className="text-sm text-slate-500">
            Status: <span className="font-medium text-slate-900">{order.status}</span>
          </p>
          <p className="text-sm text-slate-500">
            Total: <span className="font-medium text-slate-900">{formatCurrency(order.totalAmount)}</span>
          </p>
          <ul className="mt-2 space-y-1 text-sm text-slate-600">
            {order.items.map((item) => (
              <li key={item.productId}>
                {item.quantity} × {item.productName} ({formatCurrency(item.unitPrice)} each)
              </li>
            ))}
          </ul>
        </div>
      )}

      <Link to="/" className="mt-6 inline-block text-sm text-blue-600 hover:text-blue-700">
        &larr; Continue shopping
      </Link>
    </div>
  )
}
