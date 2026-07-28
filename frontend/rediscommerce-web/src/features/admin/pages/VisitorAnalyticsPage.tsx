import { Eye, CalendarRange, CalendarDays, Layers } from 'lucide-react'
import { useVisitorAnalytics } from '../hooks/useVisitorAnalytics'
import { StatCard } from '../components/StatCard'
import { LazyBarChart } from '../components/LazyBarChart'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'
import { PageHeader } from '../../../shared/components/PageHeader'
import { SurfaceCard } from '../../../shared/components/SurfaceCard'

export function VisitorAnalyticsPage() {
  const { data, isLoading, isError } = useVisitorAnalytics()

  if (isLoading) {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    )
  }

  if (isError || !data) {
    return <ErrorMessage message="Failed to load visitor analytics." />
  }

  return (
    <div>
      <PageHeader
        title="Visitor Analytics"
        description="Approximate unique-visitor counts via Redis HyperLogLog — ~0.81% error, constant memory."
      />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard label="Daily Visitors" value={data.dailyVisitors} hint="PFCOUNT visitors:daily:*" icon={Eye} />
        <StatCard
          label="Weekly Visitors"
          value={data.weeklyVisitors}
          hint="PFCOUNT visitors:weekly:*"
          icon={CalendarRange}
        />
        <StatCard
          label="Monthly Visitors"
          value={data.monthlyVisitors}
          hint="PFCOUNT visitors:monthly:*"
          icon={CalendarDays}
        />
        <StatCard
          label="Merged (Last 7 Days)"
          value={data.mergedLast7DaysVisitors}
          hint="PFCOUNT across 7 daily keys"
          icon={Layers}
        />
      </div>

      <SurfaceCard className="mt-8">
        <h2 className="text-base font-semibold text-ink">Visitor comparison</h2>
        <p className="mt-1 text-xs text-ink-subtle">HyperLogLog cardinalities across time windows</p>
        <div className="mt-2">
          <LazyBarChart
            data={[
              { name: 'Daily', value: data.dailyVisitors },
              { name: 'Weekly', value: data.weeklyVisitors },
              { name: 'Monthly', value: data.monthlyVisitors },
              { name: 'Merged 7d', value: data.mergedLast7DaysVisitors },
            ]}
            valueLabel="Visitors"
          />
        </div>
      </SurfaceCard>
    </div>
  )
}
