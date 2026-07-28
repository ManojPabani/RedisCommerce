import { useVisitorAnalytics } from '../hooks/useVisitorAnalytics'
import { StatCard } from '../components/StatCard'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'

export function VisitorAnalyticsPage() {
  const { data, isLoading, isError } = useVisitorAnalytics()

  if (isLoading) {
    return (
      <div className="flex justify-center py-12">
        <Spinner />
      </div>
    )
  }

  if (isError || !data) {
    return <ErrorMessage message="Failed to load visitor analytics." />
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-xl font-semibold text-slate-900">Visitor Analytics</h1>
        <p className="mt-1 text-sm text-slate-500">
          Approximate unique-visitor counts powered by Redis HyperLogLog &mdash; small margin of error, constant
          memory regardless of visitor volume.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard label="Daily Visitors" value={data.dailyVisitors} hint="PFCOUNT visitors:daily:*" />
        <StatCard label="Weekly Visitors" value={data.weeklyVisitors} hint="PFCOUNT visitors:weekly:*" />
        <StatCard label="Monthly Visitors" value={data.monthlyVisitors} hint="PFCOUNT visitors:monthly:*" />
        <StatCard
          label="Merged (Last 7 Days)"
          value={data.mergedLast7DaysVisitors}
          hint="PFCOUNT across 7 daily keys"
        />
      </div>
    </div>
  )
}
