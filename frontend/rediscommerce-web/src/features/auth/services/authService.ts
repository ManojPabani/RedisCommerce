import { axiosClient } from '../../../core/api/axiosClient'
import { API_ROUTES } from '../../../core/constants/apiRoutes'
import type { LoginRequest, Session } from '../models/session'

export const authService = {
  async login(request: LoginRequest): Promise<Session> {
    const { data } = await axiosClient.post<Session>(API_ROUTES.login, request)
    return data
  },

  async logout(): Promise<void> {
    await axiosClient.post(API_ROUTES.logout)
  },

  async getSession(): Promise<Session> {
    const { data } = await axiosClient.get<Session>(API_ROUTES.session)
    return data
  },
}
