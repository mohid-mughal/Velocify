/**
 * CommentItem Component
 * 
 * Displays a single comment with sentiment indicator, user info, and delete action.
 * 
 * Requirements: 23.4
 */

import React from 'react';
import { clsx } from 'clsx';
import { format } from 'date-fns';
import { Avatar } from '../ui/Avatar';
import { Button } from '../ui/Button';
import type { CommentDto } from '../../api/types';

export interface CommentItemProps {
  comment: CommentDto;
  canDelete: boolean;
  isDeleting: boolean;
  onDelete: (commentId: string) => void;
}

export const CommentItem: React.FC<CommentItemProps> = ({
  comment,
  canDelete,
  isDeleting,
  onDelete,
}) => {
  const getSentimentColor = (score: number | null) => {
    if (score === null) return 'text-neutral-500';
    if (score >= 0.7) return 'text-success-600';
    if (score >= 0.4) return 'text-warning-600';
    return 'text-danger-600';
  };

  const getSentimentEmoji = (score: number | null) => {
    if (score === null) return '😐';
    if (score >= 0.7) return '😊';
    if (score >= 0.4) return '😐';
    return '😞';
  };

  return (
    <div className="border-l-4 border-neutral-200 pl-4">
      <div className="flex items-start justify-between gap-2">
        <div className="flex items-center gap-2">
          <Avatar
            name={`${comment.user.firstName} ${comment.user.lastName}`}
            size="sm"
          />
          <div>
            <p className="font-medium text-neutral-900">
              {comment.user.firstName} {comment.user.lastName}
            </p>
            <p className="text-xs text-neutral-500">
              {format(new Date(comment.createdAt), 'MMM dd, yyyy HH:mm')}
            </p>
          </div>
          {comment.sentimentScore !== null && (
            <span
              className={clsx('text-lg', getSentimentColor(comment.sentimentScore))}
              title={`Sentiment: ${Math.round(comment.sentimentScore * 100)}%`}
            >
              {getSentimentEmoji(comment.sentimentScore)}
            </span>
          )}
        </div>
        
        {canDelete && (
          <Button
            variant="danger"
            size="sm"
            onClick={() => onDelete(comment.id)}
            disabled={isDeleting}
          >
            Delete
          </Button>
        )}
      </div>
      <p className="text-neutral-700 mt-2 whitespace-pre-wrap">{comment.content}</p>
    </div>
  );
};
