import type { InputHTMLAttributes, ReactNode, TextareaHTMLAttributes } from 'react'
import { cn } from '../utils/cn'

const fieldClass =
  'block w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm text-ink shadow-sm placeholder:text-ink-subtle transition-colors focus:border-accent focus:outline-none focus:ring-2 focus:ring-accent/20 disabled:cursor-not-allowed disabled:bg-surface-muted disabled:opacity-60'

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  invalid?: boolean
}

export function Input({ className = '', invalid, ...props }: InputProps) {
  return (
    <input
      className={cn(fieldClass, invalid && 'border-danger focus:border-danger focus:ring-danger/20', className)}
      aria-invalid={invalid || undefined}
      {...props}
    />
  )
}

interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  invalid?: boolean
}

export function Textarea({ className = '', invalid, ...props }: TextareaProps) {
  return (
    <textarea
      className={cn(fieldClass, 'min-h-20 resize-y', invalid && 'border-danger focus:border-danger focus:ring-danger/20', className)}
      aria-invalid={invalid || undefined}
      {...props}
    />
  )
}

interface LabelProps {
  htmlFor?: string
  children: ReactNode
  className?: string
}

export function Label({ htmlFor, children, className = '' }: LabelProps) {
  return (
    <label htmlFor={htmlFor} className={cn('mb-1.5 block text-sm font-medium text-ink', className)}>
      {children}
    </label>
  )
}

interface FieldProps {
  label: string
  htmlFor: string
  error?: string
  children: ReactNode
}

export function Field({ label, htmlFor, error, children }: FieldProps) {
  const errorId = error ? `${htmlFor}-error` : undefined
  return (
    <div>
      <Label htmlFor={htmlFor}>{label}</Label>
      {children}
      {error && (
        <p id={errorId} className="mt-1.5 text-sm text-danger" role="alert">
          {error}
        </p>
      )}
    </div>
  )
}
