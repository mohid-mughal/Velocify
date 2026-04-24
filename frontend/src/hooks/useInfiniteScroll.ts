/**
 * Infinite Scroll Hook
 * 
 * Custom hook for handling infinite scroll pagination.
 * Used for task lists and other paginated content.
 */

import { useState, useEffect, useRef, useCallback } from 'react';

/**
 * Options for the infinite scroll hook
 */
interface UseInfiniteScrollOptions {
  /** Callback to load more items */
  onLoadMore: () => void;
  /** Whether there are more items to load */
  hasMore: boolean;
  /** Whether data is currently loading */
  loading: boolean;
  /** Threshold in pixels from bottom to trigger load (default: 200) */
  threshold?: number;
  /** Whether to use intersection observer (default: true) */
  useIntersectionObserver?: boolean;
}

/**
 * Return type for the infinite scroll hook
 */
interface UseInfiniteScrollReturn {
  /** Ref to attach to the last element or sentinel */
  lastElementRef: (node: HTMLElement | null) => void;
  /** Ref to attach to the scroll container */
  scrollContainerRef: (node: HTMLElement | null) => void;
  /** Whether we're currently loading more items */
  isLoadingMore: boolean;
}

/**
 * Hook to handle infinite scroll pagination
 * 
 * Uses Intersection Observer API for efficient scroll detection.
 * Triggers onLoadMore when user scrolls near the bottom.
 * 
 * @param options - Configuration options
 * @returns Object with refs and loading state
 * 
 * @example
 * const { data, fetchNextPage, hasNextPage, isFetchingNextPage } = useInfiniteQuery({...});
 * 
 * const { lastElementRef, scrollContainerRef } = useInfiniteScroll({
 *   onLoadMore: () => fetchNextPage(),
 *   hasMore: hasNextPage ?? false,
 *   loading: isFetchingNextPage,
 * });
 * 
 * return (
 *   <div ref={scrollContainerRef} className="overflow-auto">
 *     {items.map((item, index) => (
 *       <div key={item.id} ref={index === items.length - 1 ? lastElementRef : null}>
 *         {item.content}
 *       </div>
 *     ))}
 *   </div>
 * );
 */
export function useInfiniteScroll({
  onLoadMore,
  hasMore,
  loading,
  threshold = 200,
  useIntersectionObserver = true,
}: UseInfiniteScrollOptions): UseInfiniteScrollReturn {
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const observerRef = useRef<IntersectionObserver | null>(null);
  const scrollContainerRef = useRef<HTMLElement | null>(null);

  // Callback ref for the last element (for intersection observer)
  const lastElementRef = useCallback(
    (node: HTMLElement | null) => {
      if (loading || !hasMore) return;

      if (observerRef.current) {
        observerRef.current.disconnect();
      }

      if (!useIntersectionObserver) return;

      observerRef.current = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting && hasMore && !loading) {
            setIsLoadingMore(true);
            onLoadMore();
          }
        },
        {
          root: scrollContainerRef.current,
          rootMargin: `${threshold}px`,
          threshold: 0,
        }
      );

      if (node) {
        observerRef.current.observe(node);
      }
    },
    [loading, hasMore, onLoadMore, threshold, useIntersectionObserver]
  );

  // Reset loading state when loading completes
  useEffect(() => {
    if (!loading) {
      setIsLoadingMore(false);
    }
  }, [loading]);

  // Fallback: scroll event listener for containers without intersection observer
  const scrollContainerCallback = useCallback(
    (node: HTMLElement | null) => {
      scrollContainerRef.current = node;

      if (!node || useIntersectionObserver) return;

      const handleScroll = () => {
        if (loading || !hasMore) return;

        const { scrollTop, scrollHeight, clientHeight } = node;
        const distanceFromBottom = scrollHeight - scrollTop - clientHeight;

        if (distanceFromBottom < threshold) {
          setIsLoadingMore(true);
          onLoadMore();
        }
      };

      node.addEventListener('scroll', handleScroll);
      
      // Cleanup
      return () => {
        node.removeEventListener('scroll', handleScroll);
      };
    },
    [loading, hasMore, onLoadMore, threshold, useIntersectionObserver]
  );

  // Cleanup observer on unmount
  useEffect(() => {
    return () => {
      if (observerRef.current) {
        observerRef.current.disconnect();
      }
    };
  }, []);

  return {
    lastElementRef,
    scrollContainerRef: scrollContainerCallback,
    isLoadingMore,
  };
}
