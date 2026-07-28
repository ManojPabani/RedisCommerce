import { Link, useLocation, useParams } from 'react-router-dom'
import { CheckCircle2, Package } from 'lucide-react'
import { formatCurrency } from '../../../shared/utils/formatCurrency'
import type { Order } from '../models/order'
import { PageHeader } from '../../../shared/components/PageHeader'
import { SurfaceCard } from '../../../shared/components/SurfaceCard'
import { Badge } from '../../../shared/components/Badge'
import { Button } from '../../../shared/components/Button'

const STATUS_STEPS = ['Pending', 'Processing', 'Completed'] as const

function statusVariant(status: string): 'warning' | 'info' | 'success' | 'neutral' {
  if (status === 'Completed') return 'success'
  if (status === 'Processing') return 'info'
  if (status === 'Pending') return 'warning'
  return 'neutral'
}

export function OrderConfirmationPage() {
  const { id } = useParams<{ id: string }>()
  const location = useLocation()
  const order = location.state as Order | undefined
  const currentStatus = order?.status ?? 'Pending'
  const activeIndex = Math.max(
    0,
    STATUS_STEPS.findIndex((s) => s.toLowerCase() === currentStatus.toLowerCase()),
  )

  return (
    <div className="mx-auto max-w-xl">
      <PageHeader
        title="Order placed"
        description="Your order is in the Redis List processing queue."
        breadcrumbs={[{ label: 'Catalog', to: '/' }, { label: `Order #${id}` }]}
      />

      <SurfaceCard>
        <div className="flex items-start gap-3">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-success-soft text-success">
            <CheckCircle2 className="h-5 w-5" />
          </div>
          <div>
            <p className="font-semibold text-ink">Order #{id} created</p>
            <p className="mt-1 text-sm text-ink-muted">
              A background worker picks up queued orders and processes them within a few seconds.
            </p>
          </div>
        </div>

        <div className="mt-6">
          <ol className="flex items-center justify-between gap-2">
            {STATUS_STEPS.map((step, index) => {
              const done = index <= activeIndex
              return (
                <li key={step} className="flex flex-1 flex-col items-center gap-2 text-center">
                  <span
                    className={`flex h-8 w-8 items-center justify-center rounded-full border text-xs font-semibold ${
                      done
                        ? 'border-accent bg-accent text-accent-fg'
                        : 'border-border bg-surface-muted text-ink-subtle'
                    }`}
                  >
                    {index + 1}
                  </span>
                  <span className={`text-xs font-medium ${done ? 'text-ink' : 'text-ink-subtle'}`}>{step}</span>
                </li>
              )
            })}
          </ol>
        </div>

        {order && (
          <div className="mt-6 border-t border-border pt-5">
            <div className="flex items-center justify-between gap-3">
              <span className="text-sm text-ink-muted">Status</span>
              <Badge variant={statusVariant(order.status)}>{order.status}</Badge>
            </div>
            <div className="mt-3 flex items-center justify-between gap-3">
              <span className="text-sm text-ink-muted">Total</span>
              <span className="text-base font-semibold tabular-nums text-ink">
                {formatCurrency(order.totalAmount)}
              </span>
            </div>
            <ul className="mt-4 space-y-2 border-t border-border pt-4">
              {order.items.map((item) => (
                <li key={item.productId} className="flex justify-between gap-3 text-sm">
                  <span className="text-ink-muted">
                    {item.quantity} × {item.productName}
                  </span>
                  <span className="tabular-nums text-ink">
                    {formatCurrency(item.unitPrice * item.quantity)}
                  </span>
                </li>
              ))}
            </ul>
          </div>
        )}

        <div className="mt-6">
          <Link to="/">
            <Button variant="outline" className="w-full">
              <Package className="h-4 w-4" />
              Continue shopping
            </Button>
          </Link>
        </div>
      </SurfaceCard>
    </div>
  )
}
