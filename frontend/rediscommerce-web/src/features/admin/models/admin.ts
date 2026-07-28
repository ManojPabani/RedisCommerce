import type { Session } from '../../auth/models/session'

export interface ExpirationEvent {
  key: string
  eventType: string
  occurredAt: string
}

export interface AdminSessions {
  activeSessionCount: number
  activeSessions: Session[]
  recentExpirations: ExpirationEvent[]
}

export interface ActivitySummary {
  today: number
  yesterday: number
  last7Days: number
  last30Days: number
}

export interface VisitorAnalytics {
  dailyVisitors: number
  weeklyVisitors: number
  monthlyVisitors: number
  mergedLast7DaysVisitors: number
}

export interface MostActiveDay {
  date: string | null
  activeUserCount: number
}

export interface UserActivity {
  userId: number
  activeToday: boolean
}
