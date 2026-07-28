import { useState, type FormEvent } from 'react'
import { Button } from '../../../shared/components/Button'
import { Field, Input, Textarea } from '../../../shared/components/Input'
import type { CreateProductRequest, Product } from '../models/product'

interface ProductFormProps {
  initialValue?: Product
  submitLabel: string
  isSubmitting?: boolean
  onSubmit: (request: CreateProductRequest) => void
  onCancel?: () => void
}

interface FormErrors {
  name?: string
  description?: string
  price?: string
  stockQuantity?: string
}

export function ProductForm({
  initialValue,
  submitLabel,
  isSubmitting,
  onSubmit,
  onCancel,
}: ProductFormProps) {
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
    <form onSubmit={handleSubmit} className="space-y-5" noValidate>
      <Field label="Name" htmlFor="name" error={errors.name}>
        <Input
          id="name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          invalid={Boolean(errors.name)}
          aria-describedby={errors.name ? 'name-error' : undefined}
          placeholder="e.g. Mechanical Keyboard"
        />
      </Field>

      <Field label="Description" htmlFor="description" error={errors.description}>
        <Textarea
          id="description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          invalid={Boolean(errors.description)}
          aria-describedby={errors.description ? 'description-error' : undefined}
          rows={4}
          placeholder="What makes this product special?"
        />
      </Field>

      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2">
        <Field label="Price" htmlFor="price" error={errors.price}>
          <Input
            id="price"
            type="number"
            step="0.01"
            value={price}
            onChange={(e) => setPrice(e.target.value)}
            invalid={Boolean(errors.price)}
            aria-describedby={errors.price ? 'price-error' : undefined}
            placeholder="0.00"
          />
        </Field>

        <Field label="Stock Quantity" htmlFor="stockQuantity" error={errors.stockQuantity}>
          <Input
            id="stockQuantity"
            type="number"
            value={stockQuantity}
            onChange={(e) => setStockQuantity(e.target.value)}
            invalid={Boolean(errors.stockQuantity)}
            aria-describedby={errors.stockQuantity ? 'stockQuantity-error' : undefined}
            placeholder="0"
          />
        </Field>
      </div>

      <div className="flex flex-col-reverse gap-2 pt-2 sm:flex-row sm:justify-end">
        {onCancel && (
          <Button type="button" variant="outline" onClick={onCancel} disabled={isSubmitting}>
            Cancel
          </Button>
        )}
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Saving...' : submitLabel}
        </Button>
      </div>
    </form>
  )
}
