import { lazy, Suspense } from 'react'
import { Skeleton } from '../../../shared/components/Skeleton'

const SimpleBarChart = lazy(() =>
  import('./SimpleBarChart').then((module) => ({ default: module.SimpleBarChart })),
)

interface ChartDatum {
  name: string
  value: number
}

interface LazyBarChartProps {
  data: ChartDatum[]
  valueLabel?: string
  height?: number
  color?: string
}

export function LazyBarChart(props: LazyBarChartProps) {
  return (
    <Suspense fallback={<Skeleton className="h-60 w-full" />}>
      <SimpleBarChart {...props} />
    </Suspense>
  )
}
