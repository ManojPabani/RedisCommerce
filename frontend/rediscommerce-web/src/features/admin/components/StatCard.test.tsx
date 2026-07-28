import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { StatCard } from './StatCard'

describe('StatCard', () => {
  it('renders the label and value', () => {
    render(<StatCard label="Today's Active Users" value={42} />)

    expect(screen.getByText("Today's Active Users")).toBeInTheDocument()
    expect(screen.getByText('42')).toBeInTheDocument()
  })

  it('renders an optional hint', () => {
    render(<StatCard label="Unique Visitors" value={10} hint="Redis HyperLogLog" />)

    expect(screen.getByText('Redis HyperLogLog')).toBeInTheDocument()
  })

  it('omits the hint when not provided', () => {
    render(<StatCard label="Current Sessions" value={3} />)

    expect(screen.queryByText(/redis/i)).not.toBeInTheDocument()
  })
})
