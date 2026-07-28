import { useQuery } from '@tanstack/react-query'
import { adminService } from '../services/adminService'

export function useActivitySummary() {
  return useQuery({
    queryKey: ['admin', 'activity-summary'] as const,
    queryFn: adminService.getActivitySummary,
    refetchInterval: 30_000,
  })
}
