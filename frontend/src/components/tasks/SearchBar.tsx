/**
 * SearchBar Component
 * 
 * Requirements: 22.5, 22.6
 * Task: 23.2
 * 
 * Search input with semantic search toggle
 */

import { Input } from '../ui/Input';

interface SearchBarProps {
  searchTerm: string;
  onSearchChange: (value: string) => void;
  useSemanticSearch: boolean;
  onSemanticToggle: (value: boolean) => void;
}

export function SearchBar({ 
  searchTerm, 
  onSearchChange, 
  useSemanticSearch, 
  onSemanticToggle 
}: SearchBarProps) {
  return (
    <div className="flex gap-4">
      <div className="flex-1">
        <Input
          type="text"
          placeholder="Search tasks..."
          value={searchTerm}
          onChange={(e) => onSearchChange(e.target.value)}
          fullWidth
        />
      </div>
      <label className="flex items-center gap-2 text-sm text-neutral-700">
        <input
          type="checkbox"
          checked={useSemanticSearch}
          onChange={(e) => onSemanticToggle(e.target.checked)}
          className="rounded border-neutral-300 text-primary-600 focus:ring-primary-500"
        />
        Semantic Search
      </label>
    </div>
  );
}
