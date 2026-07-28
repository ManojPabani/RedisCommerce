import { useQuery } from '@tanstack/react-query'
import { adminService } from '../services/adminService'

export function useAdminSessions() {
  return useQuery({
    queryKey: ['admin', 'sessions'] as const,
    queryFn: adminService.getSessions,
    refetchInterval: 30_000,
  })
}
