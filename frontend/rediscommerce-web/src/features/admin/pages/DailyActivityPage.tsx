import { useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { useActivitySummary } from '../hooks/useActivitySummary'
import { adminService } from '../services/adminService'
import { StatCard } from '../components/StatCard'
import { Button } from '../../../shared/components/Button'
import { Spinner } from '../../../shared/components/Spinner'
import { ErrorMessage } from '../../../shared/components/ErrorMessage'

export function DailyActivityPage() {
  const { data, isLoading, isError } = useActivitySummary()
  const [userIdInput, setUserIdInput] = useState('')
  const checkUser = useMutation({
    mutationFn: (userId: number) => adminService.getUserActivity(userId),
  })

  if (isLoading) {
    return (
      <div className="flex justify-center py-12">
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
    <div className="space-y-6">
      <div>
        <h1 className="text-xl font-semibold text-slate-900">Daily Activity</h1>
        <p className="mt-1 text-sm text-slate-500">
          Backed by a Redis Bitmap per day (<code>activity:yyyyMMdd</code>) — one bit per user.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard label="Today" value={data.today} hint="BITCOUNT activity:today" />
        <StatCard label="Yesterday" value={data.yesterday} hint="BITCOUNT activity:yesterday" />
        <StatCard label="Last 7 Days" value={data.last7Days} hint="BITOP OR across 7 days" />
        <StatCard label="Last 30 Days" value={data.last30Days} hint="BITOP OR across 30 days" />
      </div>

      <div className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
        <h2 className="text-base font-semibold text-slate-900">Check a specific user</h2>
        <div className="mt-3 flex gap-2">
          <input
            type="number"
            value={userIdInput}
            onChange={(e) => setUserIdInput(e.target.value)}
            placeholder="User ID"
            className="w-40 rounded-md border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none"
          />
          <Button variant="secondary" onClick={handleCheckUser} disabled={checkUser.isPending}>
            Check
          </Button>
        </div>
        {checkUser.data && (
          <p className="mt-3 text-sm text-slate-600">
            User <span className="font-medium">{checkUser.data.userId}</span> is{' '}
            <span className="font-medium">{checkUser.data.activeToday ? 'active' : 'not active'}</span> today.
          </p>
        )}
      </div>
    </div>
  )
}
