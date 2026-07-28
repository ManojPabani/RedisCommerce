import { useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { Activity, CalendarClock, CalendarDays, CalendarRange } from 'lucide-react'
import { useActivitySummary } from '../hooks/useActivitySummary'
import { adminService } from '../services/adminService'
import { StatCard } from '../components/StatCard'
import { LazyBarChart } from '../components/LazyBarChart'
import { Button } from '../../../shared/components/Button'
import { Input } from '../../../shared/components/Input'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'
import { PageHeader } from '../../../shared/components/PageHeader'
import { SurfaceCard } from '../../../shared/components/SurfaceCard'
import { Badge } from '../../../shared/components/Badge'

export function DailyActivityPage() {
  const { data, isLoading, isError } = useActivitySummary()
  const [userIdInput, setUserIdInput] = useState('')
  const checkUser = useMutation({
    mutationFn: (userId: number) => adminService.getUserActivity(userId),
  })

  if (isLoading) {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    )
  }

  if (isError || !data) {
    return <ErrorMessage message="Failed to load activity data." />
  }

  function handleCheckUser() {
    const userId = Number(userIdInput)
    if (Number.isFinite(userId) && userId > 0) {
      checkUser.mutate(userId)
    }
  }

  return (
    <div>
      <PageHeader
        title="Daily Activity"
        description="Redis Bitmap per day (activity:yyyyMMdd) — one bit per user."
      />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard label="Today" value={data.today} hint="BITCOUNT activity:today" icon={Activity} />
        <StatCard label="Yesterday" value={data.yesterday} hint="BITCOUNT activity:yesterday" icon={CalendarClock} />
        <StatCard label="Last 7 Days" value={data.last7Days} hint="BITOP OR across 7 days" icon={CalendarRange} />
        <StatCard label="Last 30 Days" value={data.last30Days} hint="BITOP OR across 30 days" icon={CalendarDays} />
      </div>

      <SurfaceCard className="mt-8">
        <h2 className="text-base font-semibold text-ink">Activity trend</h2>
        <p className="mt-1 text-xs text-ink-subtle">Unique active users by Bitmap window</p>
        <div className="mt-2">
          <LazyBarChart
            data={[
              { name: 'Today', value: data.today },
              { name: 'Yesterday', value: data.yesterday },
              { name: '7 days', value: data.last7Days },
              { name: '30 days', value: data.last30Days },
            ]}
            valueLabel="Users"
            color="#64748b"
          />
        </div>
      </SurfaceCard>

      <SurfaceCard className="mt-6">
        <h2 className="text-base font-semibold text-ink">Check a specific user</h2>
        <p className="mt-1 text-sm text-ink-muted">Look up whether a userId bit is set for today.</p>
        <div className="mt-4 flex flex-col gap-2 sm:flex-row sm:items-center">
          <Input
            type="number"
            value={userIdInput}
            onChange={(e) => setUserIdInput(e.target.value)}
            placeholder="User ID"
            className="sm:w-48"
            onKeyDown={(e) => {
              if (e.key === 'Enter') handleCheckUser()
            }}
          />
          <Button variant="secondary" onClick={handleCheckUser} disabled={checkUser.isPending}>
            {checkUser.isPending ? 'Checking...' : 'Check'}
          </Button>
        </div>
        {checkUser.isError && (
          <div className="mt-4">
            <ErrorMessage message="Could not check user activity." />
          </div>
        )}
        {checkUser.data && (
          <div className="mt-4 flex items-center gap-2 rounded-lg border border-border bg-surface-muted px-4 py-3 text-sm">
            <span className="text-ink-muted">
              User <span className="font-medium text-ink">{checkUser.data.userId}</span> is
            </span>
            <Badge variant={checkUser.data.activeToday ? 'success' : 'warning'}>
              {checkUser.data.activeToday ? 'active today' : 'not active today'}
            </Badge>
          </div>
        )}
      </SurfaceCard>
    </div>
  )
}
