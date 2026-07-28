import { useAdminSessions } from '../hooks/useAdminSessions'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'
import { PageHeader } from '../../../shared/components/PageHeader'
import { SurfaceCard } from '../../../shared/components/SurfaceCard'
import { Badge } from '../../../shared/components/Badge'
import { EmptyState } from '../../../shared/components/EmptyState'
import { Users } from 'lucide-react'

function formatDuration(loginTime: string): string {
  const minutes = Math.max(0, Math.round((Date.now() - new Date(loginTime).getTime()) / 60_000))
  if (minutes < 60) return `${minutes}m`
  return `${Math.floor(minutes / 60)}h ${minutes % 60}m`
}

export function UserSessionsPage() {
  const { data, isLoading, isError } = useAdminSessions()

  if (isLoading) {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    )
  }

  if (isError || !data) {
    return <ErrorMessage message="Failed to load sessions." />
  }

  return (
    <div>
      <PageHeader
        title="User Sessions"
        description={`${data.activeSessionCount} active · sliding TTL String keys`}
        actions={<Badge variant="success">{data.activeSessionCount} live</Badge>}
      />

      <SurfaceCard padding={false} className="overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-border text-sm">
            <thead className="bg-surface-muted text-left text-xs font-medium uppercase tracking-wide text-ink-subtle">
              <tr>
                <th className="px-4 py-3">User</th>
                <th className="px-4 py-3">Browser</th>
                <th className="px-4 py-3">Device</th>
                <th className="px-4 py-3">IP</th>
                <th className="px-4 py-3">Duration</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {data.activeSessions.map((session) => (
                <tr key={session.sessionId} className="hover:bg-surface-muted/60">
                  <td className="px-4 py-3 font-medium tabular-nums text-ink">{session.userId}</td>
                  <td className="px-4 py-3">
                    <Badge variant="neutral">{session.browser}</Badge>
                  </td>
                  <td className="px-4 py-3 text-ink-muted">{session.device}</td>
                  <td className="px-4 py-3 font-mono text-xs text-ink-muted">{session.ipAddress}</td>
                  <td className="px-4 py-3 tabular-nums text-ink-muted">{formatDuration(session.loginTime)}</td>
                </tr>
              ))}
              {data.activeSessions.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-10">
                    <EmptyState
                      icon={Users}
                      title="No active sessions."
                      description="Sessions appear here after login and refresh on each API request."
                      className="border-0 bg-transparent py-4 shadow-none"
                    />
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </SurfaceCard>

      <div className="mt-8">
        <h2 className="mb-3 text-base font-semibold text-ink">Recent Expirations</h2>
        <SurfaceCard padding={false}>
          <ul className="divide-y divide-border">
            {data.recentExpirations.map((event, index) => (
              <li key={index} className="flex items-center justify-between gap-4 px-4 py-3 text-sm">
                <div className="min-w-0">
                  <Badge variant={event.eventType.includes('Session') ? 'warning' : 'neutral'}>
                    {event.eventType}
                  </Badge>
                  <p className="mt-1 truncate font-mono text-xs text-ink-muted">{event.key}</p>
                </div>
                <span className="shrink-0 tabular-nums text-ink-subtle">
                  {new Date(event.occurredAt).toLocaleTimeString()}
                </span>
              </li>
            ))}
            {data.recentExpirations.length === 0 && (
              <li className="px-4 py-6 text-center text-sm text-ink-subtle">No expirations recorded yet.</li>
            )}
          </ul>
        </SurfaceCard>
      </div>
    </div>
  )
}
