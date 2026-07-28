export interface Session {
  sessionId: string
  userId: number
  loginTime: string
  lastActivity: string
  ipAddress: string
  browser: string
  device: string
}

export interface LoginRequest {
  userId: number
}
