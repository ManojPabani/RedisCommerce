import { screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { UserSessionsPage } from './UserSessionsPage'
import { renderWithProviders } from '../../../shared/utils/testUtils'

const { getSessions } = vi.hoisted(() => ({ getSessions: vi.fn() }))

vi.mock('../services/adminService', () => ({
  adminService: {
    getSessions,
    getActivitySummary: vi.fn(),
    getMostActiveDay: vi.fn(),
    getVisitorAnalytics: vi.fn(),
    getUserActivity: vi.fn(),
  },
}))

describe('UserSessionsPage', () => {
  it('renders active sessions in a table', async () => {
    getSessions.mockResolvedValue({
      activeSessionCount: 1,
      activeSessions: [
        {
          sessionId: 'abc123',
          userId: 1001,
          loginTime: new Date().toISOString(),
          lastActivity: new Date().toISOString(),
          ipAddress: '127.0.0.1',
          browser: 'Chrome',
          device: 'Desktop',
        },
      ],
      recentExpirations: [],
    })

    renderWithProviders(<UserSessionsPage />)

    expect(await screen.findByText('1001')).toBeInTheDocument()
    expect(screen.getByText('Chrome')).toBeInTheDocument()
    expect(screen.getByText('No expirations recorded yet.')).toBeInTheDocument()
  })

  it('shows an empty state when there are no active sessions', async () => {
    getSessions.mockResolvedValue({ activeSessionCount: 0, activeSessions: [], recentExpirations: [] })

    renderWithProviders(<UserSessionsPage />)

    expect(await screen.findByText('No active sessions.')).toBeInTheDocument()
  })
})
