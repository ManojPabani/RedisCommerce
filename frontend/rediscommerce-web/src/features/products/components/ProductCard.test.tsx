import { screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { ProductCard } from './ProductCard'
import type { Product } from '../models/product'
import { renderWithProviders } from '../../../shared/utils/testUtils'

vi.mock('../../favorites/services/favoriteService', () => ({
  favoriteService: {
    getFavorites: vi.fn().mockResolvedValue({ userId: 1001, products: [] }),
    addFavorite: vi.fn(),
    removeFavorite: vi.fn(),
  },
}))

vi.mock('../../cart/services/cartService', () => ({
  cartService: {
    getCart: vi.fn().mockResolvedValue({ userId: 1001, items: [] }),
    addItem: vi.fn(),
    updateItem: vi.fn(),
    removeItem: vi.fn(),
    clearCart: vi.fn(),
  },
}))

const product: Product = {
  id: 1,
  name: 'Mechanical Keyboard',
  description: 'RGB backlit mechanical keyboard.',
  price: 79.99,
  stockQuantity: 150,
  createdDate: '2026-01-01T00:00:00Z',
  updatedDate: '2026-01-01T00:00:00Z',
}

describe('ProductCard', () => {
  it('renders the product name, price, and stock quantity', () => {
    renderWithProviders(<ProductCard product={product} />)

    expect(screen.getByText('Mechanical Keyboard')).toBeInTheDocument()
    expect(screen.getByText('$79.99')).toBeInTheDocument()
    expect(screen.getByText('150 in stock')).toBeInTheDocument()
  })

  it('links to the product details page', () => {
    renderWithProviders(<ProductCard product={product} />)

    expect(screen.getByRole('link')).toHaveAttribute('href', '/products/1')
  })

  it('renders an Add to Cart button', () => {
    renderWithProviders(<ProductCard product={product} />)

    expect(screen.getByRole('button', { name: 'Add to Cart' })).toBeInTheDocument()
  })
})
