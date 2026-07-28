import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { ProductForm } from './ProductForm'

describe('ProductForm', () => {
  it('shows validation errors and does not submit when fields are empty', async () => {
    const onSubmit = vi.fn()
    const user = userEvent.setup()

    render(<ProductForm submitLabel="Create Product" onSubmit={onSubmit} />)

    await user.click(screen.getByRole('button', { name: 'Create Product' }))

    expect(await screen.findByText('Name is required.')).toBeInTheDocument()
    expect(screen.getByText('Description is required.')).toBeInTheDocument()
    expect(onSubmit).not.toHaveBeenCalled()
  })

  it('calls onSubmit with the entered values when the form is valid', async () => {
    const onSubmit = vi.fn()
    const user = userEvent.setup()

    render(<ProductForm submitLabel="Create Product" onSubmit={onSubmit} />)

    await user.type(screen.getByLabelText('Name'), 'Wireless Mouse')
    await user.type(screen.getByLabelText('Description'), 'Ergonomic wireless mouse.')
    await user.type(screen.getByLabelText('Price'), '29.99')
    await user.type(screen.getByLabelText('Stock Quantity'), '300')
    await user.click(screen.getByRole('button', { name: 'Create Product' }))

    expect(onSubmit).toHaveBeenCalledWith({
      name: 'Wireless Mouse',
      description: 'Ergonomic wireless mouse.',
      price: 29.99,
      stockQuantity: 300,
    })
  })
})
