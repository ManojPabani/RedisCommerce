export interface FavoriteProduct {
  productId: number
  name: string
  price: number
}

export interface Favorites {
  userId: number
  products: FavoriteProduct[]
}
