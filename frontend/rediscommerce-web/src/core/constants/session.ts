export const SESSION_HEADER_NAME = 'X-Session-Id'
export const VISITOR_HEADER_NAME = 'X-Visitor-Id'
export const SESSION_STORAGE_KEY = 'rediscommerce_session_id'

export function getStoredSessionId(): string | null {
  return localStorage.getItem(SESSION_STORAGE_KEY)
}

export function setStoredSessionId(sessionId: string): void {
  localStorage.setItem(SESSION_STORAGE_KEY, sessionId)
}

export function clearStoredSessionId(): void {
  localStorage.removeItem(SESSION_STORAGE_KEY)
}
