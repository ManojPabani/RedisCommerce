import { screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { CartPage } from './CartPage'
import { renderWithProviders } from '../../../shared/utils/testUtils'

const { getCart } = vi.hoisted(() => ({ getCart: vi.fn() }))
const { getProductById } = vi.hoisted(() => ({ getProductById: vi.fn() }))

vi.mock('../services/cartService', () => ({
  cartService: {
    getCart,
    addItem: vi.fn(),
    updateItem: vi.fn(),
    removeItem: vi.fn(),
    clearCart: vi.fn(),
  },
}))

vi.mock('../../products/services/productService', () => ({
  productService: {
    getProductById,
    getProducts: vi.fn(),
    getPopularProducts: vi.fn(),
    createProduct: vi.fn(),
    updateProduct: vi.fn(),
    deleteProduct: vi.fn(),
  },
}))

describe('CartPage', () => {
  it('shows an empty-cart message when there are no items', async () => {
    getCart.mockResolvedValue({ userId: 1001, items: [] })

    renderWithProviders(<CartPage />)

    expect(await screen.findByText(/your cart is empty/i)).toBeInTheDocument()
  })

  it('renders cart items with the product name and quantity', async () => {
    getCart.mockResolvedValue({ userId: 1001, items: [{ productId: 10, quantity: 2 }] })
    getProductById.mockResolvedValue({
      id: 10,
      name: 'Wireless Mouse',
      description: 'Ergonomic wireless mouse.',
      price: 29.99,
      stockQuantity: 300,
      createdDate: '2026-01-01T00:00:00Z',
      updatedDate: '2026-01-01T00:00:00Z',
    })

    renderWithProviders(<CartPage />)

    expect(await screen.findByText('Wireless Mouse')).toBeInTheDocument()
    expect(screen.getByLabelText('Increase quantity').previousElementSibling).toHaveTextContent('2')
    expect(screen.getByRole('button', { name: 'Checkout' })).toBeInTheDocument()
  })
})
