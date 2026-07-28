import { useQuery } from '@tanstack/react-query'
import { adminService } from '../services/adminService'

export function useVisitorAnalytics() {
  return useQuery({
    queryKey: ['admin', 'visitors'] as const,
    queryFn: adminService.getVisitorAnalytics,
    refetchInterval: 30_000,
  })
}
