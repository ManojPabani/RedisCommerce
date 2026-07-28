import type { ReactNode } from 'react'
import { cn } from '../utils/cn'

interface SurfaceCardProps {
  children: ReactNode
  className?: string
  padding?: boolean
}

export function SurfaceCard({ children, className = '', padding = true }: SurfaceCardProps) {
  return (
    <div
      className={cn(
        'rounded-xl border border-border bg-surface shadow-card',
        padding && 'p-5 sm:p-6',
        className,
      )}
    >
      {children}
    </div>
  )
}
