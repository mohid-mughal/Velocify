/**
 * DecompositionModal Component
 * 
 * Modal for displaying AI-generated subtask suggestions and allowing user to select
 * which ones to create.
 * 
 * Requirements: 23.3
 */

import React, { useState } from 'react';
import { Modal } from '../ui/Modal';
import { Button } from '../ui/Button';
import { Spinner } from '../ui/Spinner';
import type { SubtaskSuggestion } from '../../api/types';

export interface DecompositionModalProps {
  isOpen: boolean;
  isLoading: boolean;
  isCreating: boolean;
  suggestions: SubtaskSuggestion[] | undefined;
  onClose: () => void;
  onCreateSubtasks: (selectedIndices: number[]) => void;
}

export const DecompositionModal: React.FC<DecompositionModalProps> = ({
  isOpen,
  isLoading,
  isCreating,
  suggestions,
  onClose,
  onCreateSubtasks,
}) => {
  const [selectedSuggestions, setSelectedSuggestions] = useState<Set<number>>(new Set());

  const handleClose = () => {
    setSelectedSuggestions(new Set());
    onClose();
  };

  const handleCreate = () => {
    onCreateSubtasks(Array.from(selectedSuggestions));
    setSelectedSuggestions(new Set());
  };

  const toggleSelection = (index: number) => {
    const newSet = new Set(selectedSuggestions);
    if (newSet.has(index)) {
      newSet.delete(index);
    } else {
      newSet.add(index);
    }
    setSelectedSuggestions(newSet);
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title="AI Task Decomposition"
      size="lg"
    >
      {isLoading ? (
        <div className="flex justify-center py-8">
          <Spinner size="lg" />
        </div>
      ) : suggestions && suggestions.length > 0 ? (
        <div>
          <p className="text-neutral-600 mb-4">
            Select the subtasks you want to create:
          </p>
          <div className="space-y-3 mb-6">
            {suggestions.map((suggestion, idx) => (
              <label
                key={idx}
                className="flex items-start gap-3 p-3 border border-neutral-200 rounded-lg hover:bg-neutral-50 cursor-pointer"
              >
                <input
                  type="checkbox"
                  checked={selectedSuggestions.has(idx)}
                  onChange={() => toggleSelection(idx)}
                  className="mt-1 rounded border-neutral-300 text-primary-600 focus:ring-primary-500"
                />
                <div className="flex-1">
                  <p className="font-medium text-neutral-900">{suggestion.title}</p>
                  {suggestion.estimatedHours && (
                    <p className="text-sm text-neutral-500 mt-1">
                      Estimated: {suggestion.estimatedHours}h
                    </p>
                  )}
                </div>
              </label>
            ))}
          </div>
          <div className="flex gap-2 justify-end">
            <Button
              variant="secondary"
              onClick={handleClose}
            >
              Cancel
            </Button>
            <Button
              variant="primary"
              onClick={handleCreate}
              disabled={selectedSuggestions.size === 0 || isCreating}
              isLoading={isCreating}
            >
              Create {selectedSuggestions.size} Subtask{selectedSuggestions.size !== 1 ? 's' : ''}
            </Button>
          </div>
        </div>
      ) : (
        <div className="text-center py-8">
          <p className="text-neutral-600">No suggestions available</p>
        </div>
      )}
    </Modal>
  );
};
