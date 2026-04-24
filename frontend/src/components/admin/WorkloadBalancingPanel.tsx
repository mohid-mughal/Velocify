/**
 * Workload Balancing Panel Component
 * 
 * Requirements: 27.3
 * Task: 26.3
 * 
 * Shows AI workload suggestions with Accept buttons that actually reassign tasks.
 */

import { useWorkloadSuggestions } from '../../hooks/useAi';
import { useUpdateTask } from '../../hooks/useTasks';
import { useQueryClient } from '@tanstack/react-query';
import { taskKeys } from '../../api/queryKeys';
import { aiKeys } from '../../api/queryKeys';

export default function WorkloadBalancingPanel() {
  const { data: suggestions, isLoading } = useWorkloadSuggestions(true);
  const updateTaskMutation = useUpdateTask();
  const queryClient = useQueryClient();

  const handleAcceptSuggestion = async (taskId: string, suggestedAssigneeId: string) => {
    try {
      await updateTaskMutation.mutateAsync({
        taskId: taskId,
        data: {
          assignedToUserId: suggestedAssigneeId,
        },
      });

      // Invalidate workload suggestions to refresh the list
      queryClient.invalidateQueries({ queryKey: aiKeys.workloadSuggestions() });
      
      // Invalidate task lists to show updated assignments
      queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
    } catch (error) {
      console.error('Failed to accept suggestion:', error);
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-4">
        {[1, 2, 3].map(i => (
          <div key={i} className="h-20 bg-gray-200 animate-pulse rounded"></div>
        ))}
      </div>
    );
  }

  if (!suggestions || suggestions.length === 0) {
    return (
      <div className="text-center py-8 text-gray-500">
        <p>No workload balancing suggestions at this time</p>
        <p className="text-sm mt-2">The AI will generate suggestions when workload imbalances are detected</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {suggestions.map((suggestion, index) => (
        <div
          key={index}
          className="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:border-blue-300 transition-colors"
        >
          <div className="flex-1">
            <p className="text-sm font-medium text-gray-900">Task ID: {suggestion.taskId}</p>
            <p className="text-sm text-gray-600 mt-1">{suggestion.reason}</p>
            <p className="text-xs text-gray-500 mt-1">
              Suggested Assignee: {suggestion.suggestedAssigneeId}
            </p>
          </div>
          <button
            className="ml-4 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:bg-gray-400 disabled:cursor-not-allowed"
            onClick={() => handleAcceptSuggestion(suggestion.taskId, suggestion.suggestedAssigneeId)}
            disabled={updateTaskMutation.isPending}
          >
            {updateTaskMutation.isPending ? 'Accepting...' : 'Accept'}
          </button>
        </div>
      ))}
    </div>
  );
}
