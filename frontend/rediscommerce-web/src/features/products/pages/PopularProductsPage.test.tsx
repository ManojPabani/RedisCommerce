import { screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { PopularProductsPage } from './PopularProductsPage'
import { renderWithProviders } from '../../../shared/utils/testUtils'

const { getPopularProducts } = vi.hoisted(() => ({ getPopularProducts: vi.fn() }))

vi.mock('../services/productService', () => ({
  productService: {
    getPopularProducts,
    getProducts: vi.fn(),
    getProductById: vi.fn(),
    searchProducts: vi.fn(),
    createProduct: vi.fn(),
    updateProduct: vi.fn(),
    deleteProduct: vi.fn(),
  },
}))

describe('PopularProductsPage', () => {
  it('renders a consistent rank badge for every product', async () => {
    getPopularProducts.mockResolvedValue([
      { productId: 1, name: 'Mechanical Keyboard', price: 89.99, viewCount: 3 },
      { productId: 2, name: 'USB-C Dock', price: 89.99, viewCount: 2 },
      { productId: 3, name: 'Wireless Mouse', price: 29.99, viewCount: 2 },
      { productId: 4, name: 'Headphones', price: 199.99, viewCount: 1 },
    ])

    renderWithProviders(<PopularProductsPage />)

    expect(await screen.findByText('Mechanical Keyboard')).toBeInTheDocument()
    expect(screen.getByText('1')).toBeInTheDocument()
    expect(screen.getByText('4')).toBeInTheDocument()
    expect(screen.getByText('3 views')).toBeInTheDocument()
    expect(screen.getByText('1 view')).toBeInTheDocument()
  })

  it('shows an empty state when there are no views', async () => {
    getPopularProducts.mockResolvedValue([])

    renderWithProviders(<PopularProductsPage />)

    expect(await screen.findByText(/no views recorded yet/i)).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /browse catalog/i })).toHaveAttribute('href', '/')
  })
})
