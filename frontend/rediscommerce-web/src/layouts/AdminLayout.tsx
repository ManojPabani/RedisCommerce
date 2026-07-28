import { NavLink, Outlet, Link } from 'react-router-dom'
import {
  Activity,
  ArrowLeft,
  BarChart3,
  LayoutDashboard,
  Users,
} from 'lucide-react'
import { cn } from '../shared/utils/cn'
import { ThemeToggle } from '../shared/components/ThemeToggle'
import { BrandLogo } from '../shared/components/BrandLogo'

const adminLinks = [
  { to: '/admin/dashboard', label: 'Dashboard', icon: LayoutDashboard, end: true },
  { to: '/admin/sessions', label: 'Sessions', icon: Users },
  { to: '/admin/visitors', label: 'Visitors', icon: BarChart3 },
  { to: '/admin/activity', label: 'Activity', icon: Activity },
]

export function AdminLayout() {
  return (
    <div className="min-h-screen bg-canvas">
      <header className="sticky top-0 z-40 border-b border-border bg-surface/90 backdrop-blur-md">
        <div className="mx-auto flex h-14 max-w-6xl items-center justify-between gap-4 px-4 sm:px-6">
          <div className="flex items-center gap-3">
            <Link
              to="/"
              className="inline-flex items-center gap-1.5 text-sm font-medium text-ink-muted transition-colors hover:text-ink"
            >
              <ArrowLeft className="h-4 w-4" />
              <span className="hidden sm:inline">Store</span>
            </Link>
            <span className="text-border-strong">|</span>
            <span className="text-sm font-semibold text-ink">Redline Analytics</span>
          </div>
          <div className="flex items-center gap-2">
            <ThemeToggle />
            <BrandLogo showWordmark={false} size="sm" />
          </div>
        </div>
      </header>

      <div className="mx-auto flex max-w-6xl flex-col gap-6 px-4 py-6 sm:px-6 lg:flex-row lg:gap-8 lg:py-8">
        <aside className="lg:w-52 lg:shrink-0">
          <nav
            aria-label="Admin"
            className="flex gap-1 overflow-x-auto pb-1 lg:flex-col lg:overflow-visible lg:pb-0"
          >
            {adminLinks.map(({ to, label, icon: Icon, end }) => (
              <NavLink
                key={to}
                to={to}
                end={end}
                className={({ isActive }) =>
                  cn(
                    'inline-flex shrink-0 items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                    isActive
                      ? 'bg-ink text-canvas'
                      : 'text-ink-muted hover:bg-surface-muted hover:text-ink',
                  )
                }
              >
                <Icon className="h-4 w-4" strokeWidth={1.75} />
                {label}
              </NavLink>
            ))}
          </nav>
        </aside>

        <main className="min-w-0 flex-1">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
