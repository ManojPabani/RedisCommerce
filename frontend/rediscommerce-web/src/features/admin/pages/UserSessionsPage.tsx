import { useAdminSessions } from '../hooks/useAdminSessions'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'

function formatDuration(loginTime: string): string {
  const minutes = Math.max(0, Math.round((Date.now() - new Date(loginTime).getTime()) / 60_000))
  if (minutes < 60) return `${minutes}m`
  return `${Math.floor(minutes / 60)}h ${minutes % 60}m`
}

export function UserSessionsPage() {
  const { data, isLoading, isError } = useAdminSessions()

  if (isLoading) {
    return (
      <div className="flex justify-center py-12">
        <Spinner />
      </div>
    )
  }

  if (isError || !data) {
    return <ErrorMessage message="Failed to load sessions." />
  }

  return (
    <div className="space-y-6">
      <h1 className="text-xl font-semibold text-slate-900">User Sessions ({data.activeSessionCount} active)</h1>

      <div className="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm">
        <table className="min-w-full divide-y divide-slate-200 text-sm">
          <thead className="bg-slate-50 text-left text-xs font-medium uppercase text-slate-500">
            <tr>
              <th className="px-4 py-2">User</th>
              <th className="px-4 py-2">Browser</th>
              <th className="px-4 py-2">Device</th>
              <th className="px-4 py-2">IP</th>
              <th className="px-4 py-2">Session Duration</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {data.activeSessions.map((session) => (
              <tr key={session.sessionId}>
                <td className="px-4 py-2 font-medium text-slate-900">{session.userId}</td>
                <td className="px-4 py-2 text-slate-600">{session.browser}</td>
                <td className="px-4 py-2 text-slate-600">{session.device}</td>
                <td className="px-4 py-2 text-slate-600">{session.ipAddress}</td>
                <td className="px-4 py-2 text-slate-600">{formatDuration(session.loginTime)}</td>
              </tr>
            ))}
            {data.activeSessions.length === 0 && (
              <tr>
                <td colSpan={5} className="px-4 py-6 text-center text-slate-400">
                  No active sessions.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      <div>
        <h2 className="text-base font-semibold text-slate-900">Recent Expirations</h2>
        <ul className="mt-3 space-y-1 text-sm text-slate-600">
          {data.recentExpirations.map((event, index) => (
            <li key={index} className="flex justify-between">
              <span>
                {event.eventType} &mdash; {event.key}
              </span>
              <span className="text-slate-400">{new Date(event.occurredAt).toLocaleTimeString()}</span>
            </li>
          ))}
          {data.recentExpirations.length === 0 && <li className="text-slate-400">No expirations recorded yet.</li>}
        </ul>
      </div>
    </div>
  )
}
