import { fireEvent, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { FavoriteButton } from './FavoriteButton'
import { renderWithProviders } from '../../../shared/utils/testUtils'

const { getFavorites, addFavorite, removeFavorite } = vi.hoisted(() => ({
  getFavorites: vi.fn(),
  addFavorite: vi.fn(),
  removeFavorite: vi.fn(),
}))

vi.mock('../services/favoriteService', () => ({
  favoriteService: { getFavorites, addFavorite, removeFavorite },
}))

describe('FavoriteButton', () => {
  it('optimistically marks the product as favorited before the request resolves', async () => {
    getFavorites.mockResolvedValue({ userId: 1001, products: [] })
    addFavorite.mockReturnValue(new Promise(() => {})) // never resolves during this test

    renderWithProviders(<FavoriteButton productId={1} name="Mechanical Keyboard" price={79.99} />)

    const button = await screen.findByRole('button')
    await waitFor(() => expect(button).toHaveAttribute('aria-pressed', 'false'))

    fireEvent.click(button)

    await waitFor(() => expect(button).toHaveAttribute('aria-pressed', 'true'))
    expect(addFavorite).toHaveBeenCalledWith(1001, 1)
  })

  it('rolls back to not-favorited if the request fails', async () => {
    getFavorites.mockResolvedValue({ userId: 1001, products: [] })
    addFavorite.mockRejectedValue(new Error('network error'))

    renderWithProviders(<FavoriteButton productId={1} name="Mechanical Keyboard" price={79.99} />)

    const button = await screen.findByRole('button')
    await waitFor(() => expect(button).toHaveAttribute('aria-pressed', 'false'))

    fireEvent.click(button)

    await waitFor(() => expect(button).toHaveAttribute('aria-pressed', 'false'))
  })
})
