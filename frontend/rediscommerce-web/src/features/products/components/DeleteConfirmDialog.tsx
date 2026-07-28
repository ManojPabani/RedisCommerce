import { Button } from '../../../shared/components/Button'

interface DeleteConfirmDialogProps {
  productName: string
  isDeleting?: boolean
  onConfirm: () => void
  onCancel: () => void
}

export function DeleteConfirmDialog({ productName, isDeleting, onConfirm, onCancel }: DeleteConfirmDialogProps) {
  return (
    <div className="fixed inset-0 flex items-center justify-center bg-black/40 px-4">
      <div className="w-full max-w-sm rounded-lg bg-white p-6 shadow-lg">
        <h2 className="text-base font-semibold text-slate-900">Delete product</h2>
        <p className="mt-2 text-sm text-slate-600">
          Are you sure you want to delete <span className="font-medium">{productName}</span>? This action cannot be
          undone.
        </p>
        <div className="mt-6 flex justify-end gap-3">
          <Button variant="secondary" onClick={onCancel} disabled={isDeleting}>
            Cancel
          </Button>
          <Button variant="danger" onClick={onConfirm} disabled={isDeleting}>
            {isDeleting ? 'Deleting...' : 'Delete'}
          </Button>
        </div>
      </div>
    </div>
  )
}
