import type { ReactNode } from 'react'
import { cn } from '../utils/cn'

type BadgeVariant = 'neutral' | 'success' | 'warning' | 'danger' | 'accent' | 'info'

interface BadgeProps {
  children: ReactNode
  variant?: BadgeVariant
  className?: string
}

const variantClasses: Record<BadgeVariant, string> = {
  neutral: 'bg-surface-muted text-ink-muted border border-border',
  success: 'bg-success-soft text-success',
  warning: 'bg-warning-soft text-warning',
  danger: 'bg-danger-soft text-danger',
  accent: 'bg-accent-soft text-accent',
  info: 'bg-info-soft text-info',
}

export function Badge({ children, variant = 'neutral', className = '' }: BadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-md px-2 py-0.5 text-xs font-medium',
        variantClasses[variant],
        className,
      )}
    >
      {children}
    </span>
  )
}
