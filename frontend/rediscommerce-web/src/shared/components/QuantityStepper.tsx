import { Minus, Plus } from 'lucide-react'
import { Button } from './Button'
import { cn } from '../utils/cn'

interface QuantityStepperProps {
  value: number
  onChange: (value: number) => void
  disabled?: boolean
  min?: number
  className?: string
}

export function QuantityStepper({
  value,
  onChange,
  disabled,
  min = 1,
  className = '',
}: QuantityStepperProps) {
  return (
    <div className={cn('inline-flex items-center rounded-lg border border-border bg-surface', className)}>
      <Button
        variant="ghost"
        size="icon"
        className="h-9 w-9 rounded-r-none"
        disabled={disabled || value <= min}
        onClick={() => onChange(value - 1)}
        aria-label="Decrease quantity"
      >
        <Minus className="h-3.5 w-3.5" />
      </Button>
      <span className="min-w-8 text-center text-sm font-medium tabular-nums text-ink" aria-live="polite">
        {value}
      </span>
      <Button
        variant="ghost"
        size="icon"
        className="h-9 w-9 rounded-l-none"
        disabled={disabled}
        onClick={() => onChange(value + 1)}
        aria-label="Increase quantity"
      >
        <Plus className="h-3.5 w-3.5" />
      </Button>
    </div>
  )
}
