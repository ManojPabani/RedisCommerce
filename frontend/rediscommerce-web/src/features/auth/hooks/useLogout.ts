import { useMutation } from '@tanstack/react-query'
import { authService } from '../services/authService'
import { clearStoredSessionId } from '../../../core/constants/session'

export function useLogout() {
  return useMutation({
    mutationFn: authService.logout,
    onSuccess: () => {
      clearStoredSessionId()
    },
  })
}
