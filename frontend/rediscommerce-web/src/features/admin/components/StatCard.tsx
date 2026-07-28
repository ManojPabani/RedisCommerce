import type { LucideIcon } from 'lucide-react'
import { cn } from '../../../shared/utils/cn'

interface StatCardProps {
  label: string
  value: string | number
  hint?: string
  icon?: LucideIcon
  className?: string
}

export function StatCard({ label, value, hint, icon: Icon, className = '' }: StatCardProps) {
  return (
    <div
      className={cn(
        'rounded-xl border border-border bg-surface p-5 shadow-card transition-shadow hover:shadow-elevated',
        className,
      )}
    >
      <div className="flex items-start justify-between gap-3">
        <p className="text-xs font-medium uppercase tracking-wide text-ink-subtle">{label}</p>
        {Icon && (
          <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-accent-soft text-accent">
            <Icon className="h-4 w-4" strokeWidth={1.75} />
          </span>
        )}
      </div>
      <p className="mt-2 text-2xl font-semibold tracking-tight tabular-nums text-ink">{value}</p>
      {hint && <p className="mt-1.5 text-xs text-ink-subtle">{hint}</p>}
    </div>
  )
}
