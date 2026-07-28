import { Link } from 'react-router-dom'
import type { LucideIcon } from 'lucide-react'
import { Button } from './Button'
import { cn } from '../utils/cn'

interface EmptyStateProps {
  icon?: LucideIcon
  title: string
  description: string
  actionLabel?: string
  actionTo?: string
  onAction?: () => void
  className?: string
}

export function EmptyState({
  icon: Icon,
  title,
  description,
  actionLabel,
  actionTo,
  onAction,
  className = '',
}: EmptyStateProps) {
  return (
    <div
      className={cn(
        'flex flex-col items-center justify-center rounded-xl border border-dashed border-border bg-surface px-6 py-16 text-center',
        className,
      )}
    >
      {Icon && (
        <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-accent-soft text-accent">
          <Icon className="h-6 w-6" strokeWidth={1.75} />
        </div>
      )}
      <h2 className="text-base font-semibold text-ink">{title}</h2>
      <p className="mt-1.5 max-w-sm text-sm text-ink-muted">{description}</p>
      {actionLabel && actionTo && (
        <Link to={actionTo} className="mt-5">
          <Button>{actionLabel}</Button>
        </Link>
      )}
      {actionLabel && onAction && !actionTo && (
        <Button className="mt-5" onClick={onAction}>
          {actionLabel}
        </Button>
      )}
    </div>
  )
}
