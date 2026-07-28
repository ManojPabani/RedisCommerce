import { Dialog } from '../../../shared/components/Dialog'
import { Button } from '../../../shared/components/Button'

interface DeleteConfirmDialogProps {
  productName: string
  isDeleting?: boolean
  onConfirm: () => void
  onCancel: () => void
}

export function DeleteConfirmDialog({ productName, isDeleting, onConfirm, onCancel }: DeleteConfirmDialogProps) {
  return (
    <Dialog
      title="Delete product"
      onClose={onCancel}
      footer={
        <>
          <Button variant="secondary" onClick={onCancel} disabled={isDeleting}>
            Cancel
          </Button>
          <Button variant="danger" onClick={onConfirm} disabled={isDeleting}>
            {isDeleting ? 'Deleting...' : 'Delete'}
          </Button>
        </>
      }
    >
      <p>
        Are you sure you want to delete <span className="font-medium text-ink">{productName}</span>? This action
        cannot be undone.
      </p>
    </Dialog>
  )
}
