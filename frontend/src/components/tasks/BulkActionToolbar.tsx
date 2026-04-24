/**
 * BulkActionToolbar Component
 * 
 * Requirements: 22.7
 * Task: 23.2
 * 
 * Toolbar for bulk actions on selected tasks (Admin only)
 */

import { Button } from '../ui/Button';

interface BulkActionToolbarProps {
  selectedCount: number;
  onChangeStatus: () => void;
  onReassign: () => void;
  onDelete: () => void;
}

export function BulkActionToolbar({
  selectedCount,
  onChangeStatus,
  onReassign,
  onDelete,
}: BulkActionToolbarProps) {
  if (selectedCount === 0) {
    return null;
  }

  return (
    <div className="bg-primary-50 border border-primary-200 rounded-lg p-4 flex items-center justify-between">
      <span className="text-sm font-medium text-primary-900">
        {selectedCount} task{selectedCount !== 1 ? 's' : ''} selected
      </span>
      <div className="flex gap-2">
        <Button size="sm" variant="secondary" onClick={onChangeStatus}>
          Change Status
        </Button>
        <Button size="sm" variant="secondary" onClick={onReassign}>
          Reassign
        </Button>
        <Button size="sm" variant="danger" onClick={onDelete}>
          Delete
        </Button>
      </div>
    </div>
  );
}
