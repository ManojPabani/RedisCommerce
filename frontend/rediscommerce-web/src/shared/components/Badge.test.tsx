import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Badge } from './Badge'

describe('Badge', () => {
  it('renders children with the neutral variant by default', () => {
    render(<Badge>3 views</Badge>)

    expect(screen.getByText('3 views')).toBeInTheDocument()
  })

  it('supports semantic variants', () => {
    render(<Badge variant="success">In stock</Badge>)

    expect(screen.getByText('In stock')).toBeInTheDocument()
  })
})
