import { axiosClient } from '../../../core/api/axiosClient'
import { API_ROUTES } from '../../../core/constants/apiRoutes'
import type { ActivitySummary, AdminSessions, MostActiveDay, UserActivity, VisitorAnalytics } from '../models/admin'

export const adminService = {
  async getSessions(): Promise<AdminSessions> {
    const { data } = await axiosClient.get<AdminSessions>(API_ROUTES.adminSessions)
    return data
  },

  async getActivitySummary(): Promise<ActivitySummary> {
    const { data } = await axiosClient.get<ActivitySummary>(API_ROUTES.adminActivitySummary)
    return data
  },

  async getMostActiveDay(): Promise<MostActiveDay> {
    const { data } = await axiosClient.get<MostActiveDay>(API_ROUTES.adminMostActiveDay)
    return data
  },

  async getVisitorAnalytics(): Promise<VisitorAnalytics> {
    const { data } = await axiosClient.get<VisitorAnalytics>(API_ROUTES.adminVisitors)
    return data
  },

  async getUserActivity(userId: number): Promise<UserActivity> {
    const { data } = await axiosClient.get<UserActivity>(API_ROUTES.adminActivityForUser(userId))
    return data
  },
}
