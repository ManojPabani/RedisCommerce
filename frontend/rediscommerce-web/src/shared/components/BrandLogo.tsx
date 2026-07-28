import { Link } from 'react-router-dom'
import { cn } from '../utils/cn'

interface BrandLogoProps {
  to?: string
  showWordmark?: boolean
  size?: 'sm' | 'md'
  className?: string
  onClick?: () => void
}

function Mark({ size }: { size: 'sm' | 'md' }) {
  const dim = size === 'sm' ? 'h-7 w-7' : 'h-8 w-8'
  return (
    <span
      className={cn(
        'relative inline-flex items-center justify-center overflow-hidden rounded-lg bg-accent text-accent-fg shadow-sm',
        dim,
      )}
      aria-hidden
    >
      <svg viewBox="0 0 32 32" className="h-full w-full" fill="none">
        <path
          d="M6 21.5h8.2L18.5 10H26"
          stroke="currentColor"
          strokeWidth="2.4"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        <path
          d="M6 14.5h5.5L14.8 10"
          stroke="currentColor"
          strokeWidth="2.4"
          strokeLinecap="round"
          strokeLinejoin="round"
          opacity="0.55"
        />
        <circle cx="23.5" cy="21.5" r="2.2" fill="currentColor" />
      </svg>
    </span>
  )
}

export function BrandLogo({
  to = '/',
  showWordmark = true,
  size = 'md',
  className = '',
  onClick,
}: BrandLogoProps) {
  const content = (
    <>
      <Mark size={size} />
      {showWordmark && (
        <span
          className={cn(
            'font-semibold tracking-tight text-ink',
            size === 'sm' ? 'text-sm' : 'text-base',
          )}
        >
          Redline
        </span>
      )}
    </>
  )

  if (!to) {
    return <span className={cn('inline-flex items-center gap-2.5', className)}>{content}</span>
  }

  return (
    <Link
      to={to}
      onClick={onClick}
      className={cn('inline-flex items-center gap-2.5 transition-opacity hover:opacity-90', className)}
      aria-label="Redline home"
    >
      {content}
    </Link>
  )
}
