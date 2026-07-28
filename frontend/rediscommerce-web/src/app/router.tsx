import { createBrowserRouter } from 'react-router-dom'
import { MainLayout } from '../layouts/MainLayout'
import { ProductListPage } from '../features/products/pages/ProductListPage'
import { ProductDetailsPage } from '../features/products/pages/ProductDetailsPage'
import { ProductEditPage } from '../features/products/pages/ProductEditPage'
import { PopularProductsPage } from '../features/products/pages/PopularProductsPage'
import { CartPage } from '../features/cart/pages/CartPage'
import { FavoritesPage } from '../features/favorites/pages/FavoritesPage'
import { OrderConfirmationPage } from '../features/orders/pages/OrderConfirmationPage'

export const router = createBrowserRouter([
  {
    path: '/',
    element: <MainLayout />,
    children: [
      { index: true, element: <ProductListPage /> },
      { path: 'products/new', element: <ProductEditPage /> },
      { path: 'products/popular', element: <PopularProductsPage /> },
      { path: 'products/:id', element: <ProductDetailsPage /> },
      { path: 'products/:id/edit', element: <ProductEditPage /> },
      { path: 'cart', element: <CartPage /> },
      { path: 'favorites', element: <FavoritesPage /> },
      { path: 'orders/:id', element: <OrderConfirmationPage /> },
    ],
  },
])
