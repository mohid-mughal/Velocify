/**
 * AiAssistantDrawer Component
 * 
 * Requirements: 8.1-8.7, 12.1-12.7, 27.2
 * 
 * Slide-in panel from right side providing AI-powered features:
 * - Natural language task input with Parse button
 * - Semantic search input
 * - Today's digest display
 * - Quick actions (decompose, workload suggestions for admin)
 * - Floating action button (AI icon) accessible from all pages
 */

import React, { useState } from 'react';
import { clsx } from 'clsx';
import { Button } from '../ui/Button';
import { Input } from '../ui/Input';
import { Spinner } from '../ui/Spinner';
import { Badge } from '../ui/Badge';
import { useAiParse, useSemanticSearch, useDigest, useWorkloadSuggestions } from '../../hooks/useAi';
import { useUserRole } from '../../store/authStore';
import { useNavigate } from 'react-router-dom';
import type { CreateTaskRequest } from '../../api/types';

export interface AiAssistantDrawerProps {
  isOpen: boolean;
  onClose: () => void;
}

/**
 * AiAssistantDrawer Component
 * 
 * Requirement 27.2: Create AI assistant drawer with natural language input, semantic search, digest, and quick actions
 * Requirement 8.1-8.7: Natural language task parsing
 * Requirement 12.1-12.7: Semantic search
 */
export const AiAssistantDrawer: React.FC<AiAssistantDrawerProps> = ({ isOpen, onClose }) => {
  const navigate = useNavigate();
  const userRole = useUserRole();
  const isAdmin = userRole === 'Admin' || userRole === 'SuperAdmin';

  // Tab state
  const [activeTab, setActiveTab] = useState<'parse' | 'search' | 'digest' | 'actions'>('parse');

  // Natural language task input state
  const [taskInput, setTaskInput] = useState('');
  const [parsedTask, setParsedTask] = useState<CreateTaskRequest | null>(null);

  // Semantic search state
  const [searchQuery, setSearchQuery] = useState('');
  const [searchEnabled, setSearchEnabled] = useState(false);

  // AI hooks
  const parseTaskMutation = useAiParse();
  const { data: digestData, isLoading: digestLoading } = useDigest(isOpen && activeTab === 'digest');
  const { data: workloadData, isLoading: workloadLoading } = useWorkloadSuggestions(
    isOpen && activeTab === 'actions' && isAdmin
  );
  const { data: searchResults, isLoading: searchLoading } = useSemanticSearch(
    searchQuery,
    true, // Always use semantic search in AI assistant
    searchEnabled
  );

  // Handle natural language task parsing
  const handleParseTask = async () => {
    if (!taskInput.trim()) return;

    parseTaskMutation.mutate(taskInput, {
      onSuccess: (data) => {
        setParsedTask(data);
      },
    });
  };

  // Handle creating task from parsed data
  const handleCreateTask = () => {
    if (parsedTask) {
      // Navigate to task form with pre-filled data
      navigate('/tasks/new', { state: { parsedTask } });
      onClose();
    }
  };

  // Handle semantic search
  const handleSearch = () => {
    if (searchQuery.trim()) {
      setSearchEnabled(true);
    }
  };

  // Handle search input change
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchQuery(e.target.value);
    if (!e.target.value.trim()) {
      setSearchEnabled(false);
    }
  };

  // Close panel when clicking outside
  const handleBackdropClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  // Reset state when closing
  const handleClose = () => {
    setTaskInput('');
    setParsedTask(null);
    setSearchQuery('');
    setSearchEnabled(false);
    onClose();
  };

  return (
    <>
      {/* Backdrop */}
      {isOpen && (
        <div
          className="fixed inset-0 bg-black bg-opacity-50 z-40 transition-opacity"
          onClick={handleBackdropClick}
          aria-hidden="true"
        />
      )}

      {/* Slide-in drawer */}
      <div
        className={clsx(
          'fixed top-0 right-0 h-full w-full sm:w-[480px] bg-white shadow-xl z-50 transform transition-transform duration-300 ease-in-out',
          isOpen ? 'translate-x-0' : 'translate-x-full'
        )}
        role="dialog"
        aria-modal="true"
        aria-labelledby="ai-assistant-title"
      >
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-neutral-200 bg-gradient-to-r from-primary-50 to-secondary-50">
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 bg-gradient-to-br from-primary-500 to-secondary-500 rounded-lg flex items-center justify-center">
              <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
              </svg>
            </div>
            <h2 id="ai-assistant-title" className="text-lg font-semibold text-neutral-900">
              AI Assistant
            </h2>
          </div>
          <button
            onClick={handleClose}
            className="p-2 rounded-md text-neutral-500 hover:bg-neutral-100 focus:outline-none focus:ring-2 focus:ring-primary-500"
            aria-label="Close AI assistant"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Tabs */}
        <div className="flex border-b border-neutral-200 bg-neutral-50">
          <button
            onClick={() => setActiveTab('parse')}
            className={clsx(
              'flex-1 px-4 py-3 text-sm font-medium transition-colors',
              activeTab === 'parse'
                ? 'text-primary-600 border-b-2 border-primary-600 bg-white'
                : 'text-neutral-600 hover:text-neutral-900 hover:bg-neutral-100'
            )}
          >
            Parse Task
          </button>
          <button
            onClick={() => setActiveTab('search')}
            className={clsx(
              'flex-1 px-4 py-3 text-sm font-medium transition-colors',
              activeTab === 'search'
                ? 'text-primary-600 border-b-2 border-primary-600 bg-white'
                : 'text-neutral-600 hover:text-neutral-900 hover:bg-neutral-100'
            )}
          >
            Search
          </button>
          <button
            onClick={() => setActiveTab('digest')}
            className={clsx(
              'flex-1 px-4 py-3 text-sm font-medium transition-colors',
              activeTab === 'digest'
                ? 'text-primary-600 border-b-2 border-primary-600 bg-white'
                : 'text-neutral-600 hover:text-neutral-900 hover:bg-neutral-100'
            )}
          >
            Digest
          </button>
          {isAdmin && (
            <button
              onClick={() => setActiveTab('actions')}
              className={clsx(
                'flex-1 px-4 py-3 text-sm font-medium transition-colors',
                activeTab === 'actions'
                  ? 'text-primary-600 border-b-2 border-primary-600 bg-white'
                  : 'text-neutral-600 hover:text-neutral-900 hover:bg-neutral-100'
              )}
            >
              Actions
            </button>
          )}
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto h-[calc(100vh-10rem)] p-6">
          {/* Parse Task Tab */}
          {activeTab === 'parse' && (
            <div className="space-y-4">
              <div>
                <label htmlFor="task-input" className="block text-sm font-medium text-neutral-700 mb-2">
                  Describe your task in natural language
                </label>
                <textarea
                  id="task-input"
                  value={taskInput}
                  onChange={(e) => setTaskInput(e.target.value)}
                  placeholder="e.g., Create a high priority design task for John due next Friday to redesign the homepage"
                  className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 min-h-[120px]"
                  disabled={parseTaskMutation.isPending}
                />
              </div>

              <Button
                onClick={handleParseTask}
                isLoading={parseTaskMutation.isPending}
                disabled={!taskInput.trim()}
                fullWidth
              >
                Parse Task
              </Button>

              {parseTaskMutation.isError && (
                <div className="p-3 bg-danger-50 border border-danger-200 rounded-md">
                  <p className="text-sm text-danger-700">
                    Failed to parse task. Please try again.
                  </p>
                </div>
              )}

              {parsedTask && (
                <div className="p-4 bg-primary-50 border border-primary-200 rounded-md space-y-3">
                  <h3 className="text-sm font-semibold text-neutral-900">Parsed Task</h3>
                  
                  <div className="space-y-2 text-sm">
                    {parsedTask.title && (
                      <div>
                        <span className="font-medium text-neutral-700">Title:</span>
                        <p className="text-neutral-900">{parsedTask.title}</p>
                      </div>
                    )}
                    
                    {parsedTask.description && (
                      <div>
                        <span className="font-medium text-neutral-700">Description:</span>
                        <p className="text-neutral-900">{parsedTask.description}</p>
                      </div>
                    )}
                    
                    {parsedTask.priority && (
                      <div>
                        <span className="font-medium text-neutral-700">Priority:</span>
                        <Badge variant="primary" size="sm" className="ml-2">
                          {parsedTask.priority}
                        </Badge>
                      </div>
                    )}
                    
                    {parsedTask.category && (
                      <div>
                        <span className="font-medium text-neutral-700">Category:</span>
                        <Badge variant="secondary" size="sm" className="ml-2">
                          {parsedTask.category}
                        </Badge>
                      </div>
                    )}
                    
                    {parsedTask.dueDate && (
                      <div>
                        <span className="font-medium text-neutral-700">Due Date:</span>
                        <p className="text-neutral-900">
                          {new Date(parsedTask.dueDate).toLocaleDateString()}
                        </p>
                      </div>
                    )}
                  </div>

                  <Button onClick={handleCreateTask} fullWidth size="sm">
                    Create Task
                  </Button>
                </div>
              )}
            </div>
          )}

          {/* Semantic Search Tab */}
          {activeTab === 'search' && (
            <div className="space-y-4">
              <div>
                <label htmlFor="search-input" className="block text-sm font-medium text-neutral-700 mb-2">
                  Search tasks using natural language
                </label>
                <div className="flex gap-2">
                  <Input
                    id="search-input"
                    value={searchQuery}
                    onChange={handleSearchChange}
                    placeholder="e.g., tasks about user authentication"
                    fullWidth
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') {
                        handleSearch();
                      }
                    }}
                  />
                  <Button onClick={handleSearch} disabled={!searchQuery.trim()}>
                    Search
                  </Button>
                </div>
                <p className="text-xs text-neutral-500 mt-1">
                  Using AI-powered semantic search to find conceptually similar tasks
                </p>
              </div>

              {searchLoading && (
                <div className="flex items-center justify-center py-8">
                  <Spinner size="lg" />
                </div>
              )}

              {searchResults && searchResults.length > 0 && (
                <div className="space-y-3">
                  <h3 className="text-sm font-semibold text-neutral-900">
                    Found {searchResults.length} results
                  </h3>
                  {searchResults.map((task) => (
                    <div
                      key={task.id}
                      onClick={() => {
                        navigate(`/tasks/${task.id}`);
                        handleClose();
                      }}
                      className="p-3 border border-neutral-200 rounded-md hover:bg-neutral-50 cursor-pointer transition-colors"
                    >
                      <h4 className="font-medium text-neutral-900 mb-1">{task.title}</h4>
                      {task.description && (
                        <p className="text-sm text-neutral-600 line-clamp-2 mb-2">
                          {task.description}
                        </p>
                      )}
                      <div className="flex gap-2">
                        <Badge variant="primary" size="sm">{task.priority}</Badge>
                        <Badge variant="secondary" size="sm">{task.status}</Badge>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {searchEnabled && searchResults && searchResults.length === 0 && !searchLoading && (
                <div className="text-center py-8">
                  <p className="text-sm text-neutral-500">No tasks found</p>
                </div>
              )}
            </div>
          )}

          {/* Digest Tab */}
          {activeTab === 'digest' && (
            <div className="space-y-4">
              <div>
                <h3 className="text-sm font-semibold text-neutral-900 mb-2">Today's Digest</h3>
                <p className="text-xs text-neutral-500 mb-4">
                  AI-generated summary of your tasks and priorities
                </p>
              </div>

              {digestLoading && (
                <div className="flex items-center justify-center py-8">
                  <Spinner size="lg" />
                </div>
              )}

              {digestData && (
                <div className="p-4 bg-gradient-to-br from-primary-50 to-secondary-50 border border-primary-200 rounded-md">
                  <div className="prose prose-sm max-w-none">
                    <p className="text-neutral-700 whitespace-pre-wrap">{digestData}</p>
                  </div>
                </div>
              )}

              {!digestLoading && !digestData && (
                <div className="text-center py-8">
                  <p className="text-sm text-neutral-500">No digest available</p>
                </div>
              )}
            </div>
          )}

          {/* Quick Actions Tab (Admin only) */}
          {activeTab === 'actions' && isAdmin && (
            <div className="space-y-4">
              <div>
                <h3 className="text-sm font-semibold text-neutral-900 mb-2">Workload Suggestions</h3>
                <p className="text-xs text-neutral-500 mb-4">
                  AI-powered recommendations for balancing team workload
                </p>
              </div>

              {workloadLoading && (
                <div className="flex items-center justify-center py-8">
                  <Spinner size="lg" />
                </div>
              )}

              {workloadData && workloadData.length > 0 && (
                <div className="space-y-3">
                  {workloadData.map((suggestion, index) => (
                    <div
                      key={index}
                      className="p-4 border border-neutral-200 rounded-md bg-white"
                    >
                      <div className="flex items-start justify-between mb-2">
                        <h4 className="font-medium text-neutral-900">Task Reassignment</h4>
                        <Badge variant="warning" size="sm">Suggestion</Badge>
                      </div>
                      <p className="text-sm text-neutral-600 mb-3">{suggestion.reason}</p>
                      <Button
                        size="sm"
                        onClick={() => {
                          navigate(`/tasks/${suggestion.taskId}`);
                          handleClose();
                        }}
                      >
                        View Task
                      </Button>
                    </div>
                  ))}
                </div>
              )}

              {!workloadLoading && workloadData && workloadData.length === 0 && (
                <div className="text-center py-8">
                  <svg
                    className="w-16 h-16 mx-auto text-neutral-300 mb-4"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={1.5}
                      d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                    />
                  </svg>
                  <p className="text-sm text-neutral-500">Workload is balanced!</p>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </>
  );
};

export default AiAssistantDrawer;
