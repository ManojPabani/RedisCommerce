export interface OrderItem {
  productId: number
  productName: string
  unitPrice: number
  quantity: number
}

export interface Order {
  id: number
  userId: number
  status: string
  totalAmount: number
  items: OrderItem[]
  createdDate: string
}

export interface CheckoutRequest {
  userId: number
}
