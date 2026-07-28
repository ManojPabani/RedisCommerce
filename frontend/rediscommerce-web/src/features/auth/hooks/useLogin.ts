import { useMutation } from '@tanstack/react-query'
import { authService } from '../services/authService'
import { setStoredSessionId } from '../../../core/constants/session'

export function useLogin() {
  return useMutation({
    mutationFn: authService.login,
    onSuccess: (session) => {
      setStoredSessionId(session.sessionId)
    },
  })
}
