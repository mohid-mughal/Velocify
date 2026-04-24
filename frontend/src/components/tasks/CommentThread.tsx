/**
 * CommentThread Component
 * 
 * Displays a list of comments with a form to add new comments.
 * 
 * Requirements: 23.4
 */

import React, { useState } from 'react';
import { Input } from '../ui/Input';
import { Button } from '../ui/Button';
import { Spinner } from '../ui/Spinner';
import { CommentItem } from './CommentItem';
import type { CommentDto } from '../../api/types';

export interface CommentThreadProps {
  comments: CommentDto[];
  isLoading: boolean;
  isCreating: boolean;
  isDeleting: boolean;
  currentUserId: string | undefined;
  isAdmin: boolean;
  onCreateComment: (content: string) => void;
  onDeleteComment: (commentId: string) => void;
}

export const CommentThread: React.FC<CommentThreadProps> = ({
  comments,
  isLoading,
  isCreating,
  isDeleting,
  currentUserId,
  isAdmin,
  onCreateComment,
  onDeleteComment,
}) => {
  const [commentContent, setCommentContent] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!commentContent.trim()) return;
    
    onCreateComment(commentContent);
    setCommentContent('');
  };

  return (
    <div className="bg-white rounded-lg shadow-sm p-6">
      <h2 className="text-xl font-semibold mb-4">Comments</h2>
      
      {/* Comment Form */}
      <form onSubmit={handleSubmit} className="mb-6">
        <Input
          value={commentContent}
          onChange={(e) => setCommentContent(e.target.value)}
          placeholder="Add a comment..."
          className="mb-2"
        />
        <Button
          type="submit"
          size="sm"
          isLoading={isCreating}
          disabled={!commentContent.trim()}
        >
          Post Comment
        </Button>
      </form>

      {/* Comments List */}
      {isLoading ? (
        <div className="flex justify-center py-4">
          <Spinner />
        </div>
      ) : comments.length === 0 ? (
        <p className="text-neutral-500 text-center py-4">No comments yet</p>
      ) : (
        <div className="space-y-4">
          {comments.map((comment) => (
            <CommentItem
              key={comment.id}
              comment={comment}
              canDelete={isAdmin || comment.user.id === currentUserId}
              isDeleting={isDeleting}
              onDelete={onDeleteComment}
            />
          ))}
        </div>
      )}
    </div>
  );
};
