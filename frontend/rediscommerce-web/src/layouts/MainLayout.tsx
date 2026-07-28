import { useEffect, useState } from 'react'
import { NavLink, Link, Outlet } from 'react-router-dom'
import {
  BarChart3,
  Heart,
  Menu,
  Package,
  ShoppingCart,
  TrendingUp,
  X,
} from 'lucide-react'
import { CURRENT_USER_ID } from '../core/constants/currentUser'
import { useCart } from '../features/cart/hooks/useCart'
import { Button } from '../shared/components/Button'
import { BrandLogo } from '../shared/components/BrandLogo'
import { ThemeToggle } from '../shared/components/ThemeToggle'
import { cn } from '../shared/utils/cn'

const shopLinks = [
  { to: '/', label: 'Catalog', icon: Package, end: true },
  { to: '/products/popular', label: 'Popular', icon: TrendingUp },
  { to: '/favorites', label: 'Favorites', icon: Heart },
]

function NavItem({
  to,
  label,
  icon: Icon,
  end,
  onNavigate,
}: {
  to: string
  label: string
  icon: typeof Package
  end?: boolean
  onNavigate?: () => void
}) {
  return (
    <NavLink
      to={to}
      end={end}
      onClick={onNavigate}
      className={({ isActive }) =>
        cn(
          'inline-flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
          isActive ? 'bg-accent-soft text-accent' : 'text-ink-muted hover:bg-surface-muted hover:text-ink',
        )
      }
    >
      <Icon className="h-4 w-4" strokeWidth={1.75} />
      {label}
    </NavLink>
  )
}

export function MainLayout() {
  const [mobileOpen, setMobileOpen] = useState(false)
  const { data: cart } = useCart(CURRENT_USER_ID)
  const cartCount = cart?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0

  useEffect(() => {
    document.body.style.overflow = mobileOpen ? 'hidden' : ''
    return () => {
      document.body.style.overflow = ''
    }
  }, [mobileOpen])

  function closeMobile() {
    setMobileOpen(false)
  }

  return (
    <div className="min-h-screen bg-canvas">
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:absolute focus:left-4 focus:top-4 focus:z-50 focus:rounded-lg focus:bg-surface focus:px-3 focus:py-2 focus:shadow-elevated"
      >
        Skip to content
      </a>

      <header className="sticky top-0 z-40 border-b border-border bg-surface/90 backdrop-blur-md">
        <div className="mx-auto flex h-16 max-w-6xl items-center justify-between gap-4 px-4 sm:px-6">
          <div className="flex items-center gap-3">
            <Button
              variant="ghost"
              size="icon"
              className="md:hidden"
              aria-label={mobileOpen ? 'Close menu' : 'Open menu'}
              aria-expanded={mobileOpen}
              onClick={() => setMobileOpen((open) => !open)}
            >
              {mobileOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
            </Button>

            <BrandLogo onClick={closeMobile} />
          </div>

          <nav className="hidden items-center gap-1 md:flex" aria-label="Primary">
            {shopLinks.map((link) => (
              <NavItem key={link.to} {...link} />
            ))}
          </nav>

          <div className="flex items-center gap-1.5 sm:gap-2">
            <ThemeToggle />
            <NavLink
              to="/cart"
              className={({ isActive }) =>
                cn(
                  'relative inline-flex items-center gap-2 rounded-lg px-2.5 py-2 text-sm font-medium transition-colors sm:px-3',
                  isActive ? 'bg-accent-soft text-accent' : 'text-ink-muted hover:bg-surface-muted hover:text-ink',
                )
              }
            >
              <ShoppingCart className="h-4 w-4" strokeWidth={1.75} />
              <span className="hidden sm:inline">Cart</span>
              {cartCount > 0 && (
                <span className="absolute -right-0.5 -top-0.5 flex h-[18px] min-w-[18px] items-center justify-center rounded-full bg-accent px-1 text-[10px] font-semibold text-accent-fg sm:static sm:ml-0.5">
                  {cartCount > 99 ? '99+' : cartCount}
                </span>
              )}
            </NavLink>

            <NavLink
              to="/admin/dashboard"
              className={({ isActive }) =>
                cn(
                  'hidden items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors sm:inline-flex',
                  isActive ? 'bg-ink text-canvas' : 'text-ink-muted hover:bg-surface-muted hover:text-ink',
                )
              }
            >
              <BarChart3 className="h-4 w-4" strokeWidth={1.75} />
              Analytics
            </NavLink>

            <Link to="/products/new" className="hidden sm:block">
              <Button size="sm">Add Product</Button>
            </Link>
          </div>
        </div>
      </header>

      {mobileOpen && (
        <div className="fixed inset-0 z-30 md:hidden">
          <button type="button" className="absolute inset-0 bg-ink/40" aria-label="Close menu" onClick={closeMobile} />
          <nav
            aria-label="Mobile"
            className="absolute left-0 top-16 flex h-[calc(100vh-4rem)] w-72 flex-col gap-1 border-r border-border bg-surface p-4 shadow-elevated"
          >
            {shopLinks.map((link) => (
              <NavItem key={link.to} {...link} onNavigate={closeMobile} />
            ))}
            <NavItem to="/cart" label="Cart" icon={ShoppingCart} onNavigate={closeMobile} />
            <NavItem to="/admin/dashboard" label="Analytics" icon={BarChart3} onNavigate={closeMobile} />
            <div className="mt-3 border-t border-border pt-3">
              <Link to="/products/new" onClick={closeMobile}>
                <Button className="w-full">Add Product</Button>
              </Link>
            </div>
          </nav>
        </div>
      )}

      <main id="main-content" className="mx-auto max-w-6xl px-4 py-8 sm:px-6">
        <Outlet />
      </main>
    </div>
  )
}
