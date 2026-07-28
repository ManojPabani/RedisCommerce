const VISITOR_ID_STORAGE_KEY = 'rediscommerce_visitor_id'

export function getOrCreateVisitorId(): string {
  const existing = localStorage.getItem(VISITOR_ID_STORAGE_KEY)
  if (existing) {
    return existing
  }

  const visitorId = crypto.randomUUID()
  localStorage.setItem(VISITOR_ID_STORAGE_KEY, visitorId)
  return visitorId
}
