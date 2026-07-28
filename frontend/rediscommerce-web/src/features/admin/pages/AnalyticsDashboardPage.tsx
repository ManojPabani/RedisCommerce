import { Link } from 'react-router-dom'
import {
  Activity,
  ArrowRight,
  BarChart3,
  CalendarDays,
  Percent,
  TrendingUp,
  Users,
} from 'lucide-react'
import { useAdminSessions } from '../hooks/useAdminSessions'
import { useActivitySummary } from '../hooks/useActivitySummary'
import { useVisitorAnalytics } from '../hooks/useVisitorAnalytics'
import { useMostActiveDay } from '../hooks/useMostActiveDay'
import { usePopularProducts } from '../../products/hooks/usePopularProducts'
import { StatCard } from '../components/StatCard'
import { LazyBarChart } from '../components/LazyBarChart'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'
import { PageHeader } from '../../../shared/components/PageHeader'
import { SurfaceCard } from '../../../shared/components/SurfaceCard'
import { Badge } from '../../../shared/components/Badge'
import { StatCardSkeleton } from '../../../shared/components/Skeleton'

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

  const activeSessionCount = sessions.data?.activeSessionCount ?? 0
  const recentSessionExpirations =
    sessions.data?.recentExpirations.filter((e) => e.eventType === 'Session Expired').length ?? 0
  const expirationRate =
    activeSessionCount + recentSessionExpirations === 0
      ? 0
      : Math.round((recentSessionExpirations / (activeSessionCount + recentSessionExpirations)) * 100)

  const topProducts = (popularProducts.data ?? []).slice(0, 5)

  return (
    <div>
      <PageHeader
        title="Analytics Dashboard"
        description="Live Redis metrics · auto-refreshes every 30 seconds"
        actions={<Badge variant="info">Live</Badge>}
      />

      {isError && <ErrorMessage message="Failed to load the analytics dashboard." className="mb-6" />}

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {isLoading ? (
          Array.from({ length: 6 }).map((_, i) => <StatCardSkeleton key={i} />)
        ) : (
          <>
            <StatCard
              label="Today's Active Users"
              value={activity.data?.today ?? 0}
              hint="Redis Bitmap"
              icon={Activity}
            />
            <StatCard
              label="Unique Visitors Today"
              value={visitors.data?.dailyVisitors ?? 0}
              hint="Redis HyperLogLog"
              icon={Users}
            />
            <StatCard
              label="Current Sessions"
              value={activeSessionCount}
              hint="Redis String, sliding TTL"
              icon={BarChart3}
            />
            <StatCard
              label="Session Expiration Rate"
              value={`${expirationRate}%`}
              hint={`${recentSessionExpirations} recent expirations`}
              icon={Percent}
            />
            <StatCard
              label="Most Active Day"
              value={formatMostActiveDay(mostActiveDay.data?.date ?? null)}
              hint={`${mostActiveDay.data?.activeUserCount ?? 0} active users`}
              icon={CalendarDays}
            />
            <StatCard
              label="Popular Products Tracked"
              value={popularProducts.data?.length ?? 0}
              hint="Redis Sorted Set"
              icon={TrendingUp}
            />
          </>
        )}
      </div>

      <div className="mt-8 grid gap-6 lg:grid-cols-2">
        <SurfaceCard>
          <div className="flex items-center justify-between gap-3">
            <h2 className="text-base font-semibold text-ink">Top Products</h2>
            <Link to="/products/popular" className="text-sm font-medium text-accent hover:text-accent-hover">
              View all
            </Link>
          </div>
          {topProducts.length === 0 ? (
            <p className="mt-4 text-sm text-ink-subtle">No product views yet.</p>
          ) : (
            <div className="mt-2">
              <LazyBarChart
                data={topProducts.map((p) => ({
                  name: p.name.length > 14 ? `${p.name.slice(0, 14)}…` : p.name,
                  value: p.viewCount,
                }))}
                valueLabel="Views"
              />
            </div>
          )}
        </SurfaceCard>

        <SurfaceCard>
          <h2 className="text-base font-semibold text-ink">Activity windows</h2>
          <p className="mt-1 text-xs text-ink-subtle">Active users across Bitmap OR windows</p>
          <div className="mt-2">
            <LazyBarChart
              data={[
                { name: 'Today', value: activity.data?.today ?? 0 },
                { name: 'Yesterday', value: activity.data?.yesterday ?? 0 },
                { name: '7 days', value: activity.data?.last7Days ?? 0 },
                { name: '30 days', value: activity.data?.last30Days ?? 0 },
              ]}
              valueLabel="Users"
              color="#64748b"
            />
          </div>
        </SurfaceCard>
      </div>

      <SurfaceCard className="mt-6">
        <h2 className="text-base font-semibold text-ink">Explore</h2>
        <div className="mt-4 grid gap-2 sm:grid-cols-3">
          {[
            { to: '/admin/sessions', label: 'User sessions', desc: 'Active sessions & expirations' },
            { to: '/admin/visitors', label: 'Visitor analytics', desc: 'HyperLogLog unique counts' },
            { to: '/admin/activity', label: 'Daily activity', desc: 'Bitmap active-user windows' },
          ].map((item) => (
            <Link
              key={item.to}
              to={item.to}
              className="flex items-center justify-between rounded-lg border border-border px-4 py-3 transition-colors hover:bg-surface-muted"
            >
              <div>
                <p className="text-sm font-medium text-ink">{item.label}</p>
                <p className="text-xs text-ink-subtle">{item.desc}</p>
              </div>
              <ArrowRight className="h-4 w-4 shrink-0 text-ink-subtle" />
            </Link>
          ))}
        </div>
      </SurfaceCard>
    </div>
  )
}
