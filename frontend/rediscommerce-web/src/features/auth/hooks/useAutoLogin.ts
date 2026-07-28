import { useEffect, useRef } from 'react'
import { useLogin } from './useLogin'
import { getStoredSessionId } from '../../../core/constants/session'
import { CURRENT_USER_ID } from '../../../core/constants/currentUser'

/**
 * Establishes a session for the demo user on first load if one isn't already stored.
 * There's no login form to build here — UserId-only login has no real credentials to
 * check — this just makes sure the rest of the app (activity tracking, sliding session
 * TTL, the admin dashboard) has a session to observe.
 */
export function useAutoLogin() {
  const login = useLogin()
  const hasAttempted = useRef(false)

  useEffect(() => {
    if (hasAttempted.current || getStoredSessionId() !== null) {
      return
    }

    hasAttempted.current = true
    login.mutate({ userId: CURRENT_USER_ID })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])
}
