export interface CartItem {
  productId: number
  quantity: number
}

export interface Cart {
  userId: number
  items: CartItem[]
}

export interface AddCartItemRequest {
  productId: number
  quantity: number
}

export interface UpdateCartItemRequest {
  quantity: number
}
