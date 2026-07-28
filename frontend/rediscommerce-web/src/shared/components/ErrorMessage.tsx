import { AlertCircle } from 'lucide-react'
import { cn } from '../utils/cn'

interface ErrorMessageProps {
  message: string
  className?: string
}

export function ErrorMessage({ message, className = '' }: ErrorMessageProps) {
  return (
    <div
      role="alert"
      className={cn(
        'flex items-start gap-3 rounded-xl border border-danger/20 bg-danger-soft px-4 py-3 text-sm text-danger',
        className,
      )}
    >
      <AlertCircle className="mt-0.5 h-4 w-4 shrink-0" strokeWidth={2} />
      <p>{message}</p>
    </div>
  )
}
