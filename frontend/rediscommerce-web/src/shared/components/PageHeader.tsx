import { Link } from 'react-router-dom'
import { cn } from '../utils/cn'

interface PageHeaderProps {
  title: string
  description?: string
  actions?: React.ReactNode
  breadcrumbs?: Array<{ label: string; to?: string }>
  className?: string
}

export function PageHeader({ title, description, actions, breadcrumbs, className = '' }: PageHeaderProps) {
  return (
    <div className={cn('mb-8', className)}>
      {breadcrumbs && breadcrumbs.length > 0 && (
        <nav aria-label="Breadcrumb" className="mb-3 flex flex-wrap items-center gap-1.5 text-sm text-ink-subtle">
          {breadcrumbs.map((crumb, index) => (
            <span key={`${crumb.label}-${index}`} className="flex items-center gap-1.5">
              {index > 0 && <span aria-hidden>/</span>}
              {crumb.to ? (
                <Link to={crumb.to} className="hover:text-ink transition-colors">
                  {crumb.label}
                </Link>
              ) : (
                <span className="text-ink-muted">{crumb.label}</span>
              )}
            </span>
          ))}
        </nav>
      )}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0">
          <h1 className="text-2xl font-semibold tracking-tight text-ink">{title}</h1>
          {description && <p className="mt-1.5 text-sm text-ink-muted">{description}</p>}
        </div>
        {actions && <div className="flex shrink-0 flex-wrap items-center gap-2">{actions}</div>}
      </div>
    </div>
  )
}
