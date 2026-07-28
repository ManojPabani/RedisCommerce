import { useState, type FormEvent } from 'react'
import { Button } from '../../../shared/components/Button'
import type { CreateProductRequest, Product } from '../models/product'

interface ProductFormProps {
  initialValue?: Product
  submitLabel: string
  isSubmitting?: boolean
  onSubmit: (request: CreateProductRequest) => void
}

interface FormErrors {
  name?: string
  description?: string
  price?: string
  stockQuantity?: string
}

export function ProductForm({ initialValue, submitLabel, isSubmitting, onSubmit }: ProductFormProps) {
  const [name, setName] = useState(initialValue?.name ?? '')
  const [description, setDescription] = useState(initialValue?.description ?? '')
  const [price, setPrice] = useState(initialValue?.price.toString() ?? '')
  const [stockQuantity, setStockQuantity] = useState(initialValue?.stockQuantity.toString() ?? '')
  const [errors, setErrors] = useState<FormErrors>({})

  function validate(): FormErrors {
    const nextErrors: FormErrors = {}
    if (!name.trim()) nextErrors.name = 'Name is required.'
    if (!description.trim()) nextErrors.description = 'Description is required.'
    if (!price || Number(price) <= 0) nextErrors.price = 'Price must be greater than 0.'
    if (!stockQuantity || Number(stockQuantity) < 0) nextErrors.stockQuantity = 'Stock quantity cannot be negative.'
    return nextErrors
  }

  function handleSubmit(event: FormEvent) {
    event.preventDefault()
    const nextErrors = validate()
    setErrors(nextErrors)
    if (Object.keys(nextErrors).length > 0) return

    onSubmit({
      name: name.trim(),
      description: description.trim(),
      price: Number(price),
      stockQuantity: Number(stockQuantity),
    })
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4" noValidate>
      <div>
        <label htmlFor="name" className="block text-sm font-medium text-slate-700">
          Name
        </label>
        <input
          id="name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none"
        />
        {errors.name && <p className="mt-1 text-sm text-red-600">{errors.name}</p>}
      </div>

      <div>
        <label htmlFor="description" className="block text-sm font-medium text-slate-700">
          Description
        </label>
        <textarea
          id="description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          rows={3}
          className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none"
        />
        {errors.description && <p className="mt-1 text-sm text-red-600">{errors.description}</p>}
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label htmlFor="price" className="block text-sm font-medium text-slate-700">
            Price
          </label>
          <input
            id="price"
            type="number"
            step="0.01"
            value={price}
            onChange={(e) => setPrice(e.target.value)}
            className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none"
          />
          {errors.price && <p className="mt-1 text-sm text-red-600">{errors.price}</p>}
        </div>

        <div>
          <label htmlFor="stockQuantity" className="block text-sm font-medium text-slate-700">
            Stock Quantity
          </label>
          <input
            id="stockQuantity"
            type="number"
            value={stockQuantity}
            onChange={(e) => setStockQuantity(e.target.value)}
            className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none"
          />
          {errors.stockQuantity && <p className="mt-1 text-sm text-red-600">{errors.stockQuantity}</p>}
        </div>
      </div>

      <Button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Saving...' : submitLabel}
      </Button>
    </form>
  )
}
