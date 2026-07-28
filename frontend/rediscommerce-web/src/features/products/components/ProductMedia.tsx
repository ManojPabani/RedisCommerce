import { cn } from '../../../shared/utils/cn'

/** Shared product media placeholder — token-safe in light and dark mode. */
export function ProductMedia({
  name,
  className = '',
  textClassName = '',
}: {
  name: string
  className?: string
  textClassName?: string
}) {
  const initials = name
    .split(/\s+/)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? '')
    .join('')

  return (
    <div
      className={cn(
        'flex items-center justify-center bg-surface-muted text-ink-subtle',
        className,
      )}
    >
      <span className={cn('font-semibold tracking-tight', textClassName)}>{initials}</span>
    </div>
  )
}
