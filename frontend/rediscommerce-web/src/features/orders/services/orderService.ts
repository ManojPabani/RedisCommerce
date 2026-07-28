import { axiosClient } from '../../../core/api/axiosClient'
import { API_ROUTES } from '../../../core/constants/apiRoutes'
import type { CheckoutRequest, Order } from '../models/order'

export const orderService = {
  async checkout(request: CheckoutRequest): Promise<Order> {
    const { data } = await axiosClient.post<Order>(API_ROUTES.checkout, request)
    return data
  },
}
