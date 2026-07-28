import { cn } from '../utils/cn'

interface SkeletonProps {
  className?: string
}

export function Skeleton({ className = '' }: SkeletonProps) {
  return <div className={cn('animate-pulse rounded-lg bg-surface-muted', className)} aria-hidden />
}

export function ProductCardSkeleton() {
  return (
    <div className="overflow-hidden rounded-xl border border-border bg-surface shadow-card">
      <Skeleton className="aspect-[4/3] rounded-none" />
      <div className="space-y-3 p-4">
        <Skeleton className="h-5 w-3/4" />
        <Skeleton className="h-4 w-full" />
        <Skeleton className="h-4 w-2/3" />
        <div className="flex items-center justify-between pt-1">
          <Skeleton className="h-6 w-20" />
          <Skeleton className="h-5 w-16" />
        </div>
        <Skeleton className="h-10 w-full" />
      </div>
    </div>
  )
}

export function StatCardSkeleton() {
  return (
    <div className="rounded-xl border border-border bg-surface p-5 shadow-card">
      <Skeleton className="h-3 w-28" />
      <Skeleton className="mt-3 h-8 w-16" />
      <Skeleton className="mt-2 h-3 w-24" />
    </div>
  )
}
