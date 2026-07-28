import { useQuery } from '@tanstack/react-query'
import { authService } from '../services/authService'
import { getStoredSessionId } from '../../../core/constants/session'

export function useSession() {
  return useQuery({
    queryKey: ['auth', 'session'] as const,
    queryFn: authService.getSession,
    enabled: getStoredSessionId() !== null,
    retry: false,
  })
}
