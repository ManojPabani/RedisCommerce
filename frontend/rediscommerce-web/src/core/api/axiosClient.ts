import axios from 'axios'
import { SESSION_HEADER_NAME, VISITOR_HEADER_NAME, getStoredSessionId } from '../constants/session'
import { getOrCreateVisitorId } from '../utils/visitorId'

export const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

axiosClient.interceptors.request.use((config) => {
  const sessionId = getStoredSessionId()
  if (sessionId) {
    config.headers.set(SESSION_HEADER_NAME, sessionId)
  }

  config.headers.set(VISITOR_HEADER_NAME, getOrCreateVisitorId())

  return config
})
