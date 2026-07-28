import { Link, Outlet } from 'react-router-dom'

export function MainLayout() {
  return (
    <div className="min-h-screen bg-slate-50">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-5xl items-center justify-between px-4 py-4">
          <Link to="/" className="text-lg font-semibold text-slate-900">
            RedisCommerce
          </Link>
          <nav className="flex items-center gap-4 text-sm font-medium">
            <Link to="/products/popular" className="text-slate-600 hover:text-slate-900">
              Popular
            </Link>
            <Link to="/favorites" className="text-slate-600 hover:text-slate-900">
              Favorites
            </Link>
            <Link to="/cart" className="text-slate-600 hover:text-slate-900">
              Cart
            </Link>
            <Link to="/products/new" className="text-blue-600 hover:text-blue-700">
              + Add Product
            </Link>
          </nav>
        </div>
      </header>
      <main className="mx-auto max-w-5xl px-4 py-8">
        <Outlet />
      </main>
    </div>
  )
}
