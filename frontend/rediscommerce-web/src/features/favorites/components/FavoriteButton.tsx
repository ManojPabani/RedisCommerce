import type { MouseEvent } from 'react'
import { CURRENT_USER_ID } from '../../../core/constants/currentUser'
import { useFavorites } from '../hooks/useFavorites'
import { useToggleFavorite } from '../hooks/useToggleFavorite'

interface FavoriteButtonProps {
  productId: number
  name: string
  price: number
}

export function FavoriteButton({ productId, name, price }: FavoriteButtonProps) {
  const { data: favorites } = useFavorites(CURRENT_USER_ID)
  const toggleFavorite = useToggleFavorite(CURRENT_USER_ID)

  const isFavorite = favorites?.products.some((p) => p.productId === productId) ?? false

  function handleClick(event: MouseEvent) {
    event.preventDefault()
    event.stopPropagation()
    toggleFavorite.mutate({ productId, name, price, isFavorite })
  }

  return (
    <button
      type="button"
      onClick={handleClick}
      aria-label={isFavorite ? 'Remove from favorites' : 'Add to favorites'}
      aria-pressed={isFavorite}
      disabled={toggleFavorite.isPending}
      className="rounded-full p-1.5 leading-none hover:bg-slate-100 disabled:opacity-50"
    >
      <svg
        viewBox="0 0 24 24"
        className={`h-5 w-5 ${isFavorite ? 'fill-red-500 stroke-red-500' : 'fill-none stroke-slate-400'}`}
        strokeWidth={1.5}
      >
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          d="M12 21s-6.716-4.35-9.428-8.06C.94 10.42 1.2 6.9 4.05 5.1c2.35-1.48 5.06-.68 6.95 1.44 1.89-2.12 4.6-2.92 6.95-1.44 2.85 1.8 3.11 5.32 1.48 7.84C18.716 16.65 12 21 12 21z"
        />
      </svg>
    </button>
  )
}
