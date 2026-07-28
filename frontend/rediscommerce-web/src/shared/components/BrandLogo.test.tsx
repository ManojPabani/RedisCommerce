import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { BrandLogo } from './BrandLogo'

describe('BrandLogo', () => {
  it('renders the Redline wordmark and links home', () => {
    render(
      <MemoryRouter>
        <BrandLogo />
      </MemoryRouter>,
    )

    expect(screen.getByRole('link', { name: /redline home/i })).toHaveAttribute('href', '/')
    expect(screen.getByText('Redline')).toBeInTheDocument()
  })

  it('can hide the wordmark', () => {
    render(
      <MemoryRouter>
        <BrandLogo showWordmark={false} />
      </MemoryRouter>,
    )

    expect(screen.queryByText('Redline')).not.toBeInTheDocument()
    expect(screen.getByRole('link', { name: /redline home/i })).toBeInTheDocument()
  })
})
