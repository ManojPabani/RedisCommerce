import { useQuery } from '@tanstack/react-query'
import { adminService } from '../services/adminService'

export function useMostActiveDay() {
  return useQuery({
    queryKey: ['admin', 'most-active-day'] as const,
    queryFn: adminService.getMostActiveDay,
    refetchInterval: 30_000,
  })
}
