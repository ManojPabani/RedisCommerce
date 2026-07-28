import { Link } from 'react-router-dom'
import { useAdminSessions } from '../hooks/useAdminSessions'
import { useActivitySummary } from '../hooks/useActivitySummary'
import { useVisitorAnalytics } from '../hooks/useVisitorAnalytics'
import { useMostActiveDay } from '../hooks/useMostActiveDay'
import { usePopularProducts } from '../../products/hooks/usePopularProducts'
import { StatCard } from '../components/StatCard'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'

function formatMostActiveDay(date: string | null): string {
  if (!date) return 'No data yet'
  return `${date.slice(0, 4)}-${date.slice(4, 6)}-${date.slice(6, 8)}`
}

export function AnalyticsDashboardPage() {
  const sessions = useAdminSessions()
  const activity = useActivitySummary()
  const visitors = useVisitorAnalytics()
  const mostActiveDay = useMostActiveDay()
  const popularProducts = usePopularProducts()

  const isLoading =
    sessions.isLoading || activity.isLoading || visitors.isLoading || mostActiveDay.isLoading || popularProducts.isLoading
  const isError = sessions.isError || activity.isError || visitors.isError || mostActiveDay.isError || popularProducts.isError

  if (isLoading) {
    return (
      <div className="flex justify-center py-12">
        <Spinner />
      </div>
    )
  }

  if (isError) {
    return <ErrorMessage message="Failed to load the analytics dashboard." />
  }

  const activeSessionCount = sessions.data?.activeSessionCount ?? 0
  const recentSessionExpirations =
    sessions.data?.recentExpirations.filter((e) => e.eventType === 'Session Expired').length ?? 0
  const expirationRate =
    activeSessionCount + recentSessionExpirations === 0
      ? 0
      : Math.round((recentSessionExpirations / (activeSessionCount + recentSessionExpirations)) * 100)

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-xl font-semibold text-slate-900">Analytics Dashboard</h1>
        <p className="mt-1 text-sm text-slate-500">Auto-refreshes every 30 seconds.</p>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <StatCard label="Today's Active Users" value={activity.data?.today ?? 0} hint="Redis Bitmap" />
        <StatCard label="Unique Visitors Today" value={visitors.data?.dailyVisitors ?? 0} hint="Redis HyperLogLog" />
        <StatCard label="Current Sessions" value={activeSessionCount} hint="Redis String, sliding TTL" />
        <StatCard
          label="Session Expiration Rate"
          value={`${expirationRate}%`}
          hint={`${recentSessionExpirations} recent expirations`}
        />
        <StatCard
          label="Most Active Day"
          value={formatMostActiveDay(mostActiveDay.data?.date ?? null)}
          hint={`${mostActiveDay.data?.activeUserCount ?? 0} active users`}
        />
        <StatCard label="Popular Products Tracked" value={popularProducts.data?.length ?? 0} hint="Redis Sorted Set" />
      </div>

      <div className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
        <h2 className="text-base font-semibold text-slate-900">Top Products</h2>
        <ul className="mt-3 space-y-2 text-sm text-slate-600">
          {(popularProducts.data ?? []).slice(0, 5).map((product) => (
            <li key={product.productId} className="flex items-center justify-between">
              <span>{product.name}</span>
              <span className="text-slate-400">{product.viewCount} views</span>
            </li>
          ))}
        </ul>
      </div>

      <div className="flex gap-4 text-sm">
        <Link to="/admin/sessions" className="text-blue-600 hover:text-blue-700">
          View user sessions &rarr;
        </Link>
        <Link to="/admin/visitors" className="text-blue-600 hover:text-blue-700">
          View visitor analytics &rarr;
        </Link>
        <Link to="/admin/activity" className="text-blue-600 hover:text-blue-700">
          View daily activity &rarr;
        </Link>
      </div>
    </div>
  )
}
