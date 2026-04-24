/**
 * DigestCard Component
 * 
 * Requirements: 7.4
 * Task: 22.2
 * 
 * Displays AI-generated daily digest with task summary
 */

interface DigestCardProps {
  dueTodayCount: number;
  overdueCount: number;
  loading?: boolean;
}

export function DigestCard({ dueTodayCount, overdueCount, loading }: DigestCardProps) {
  if (loading) {
    return (
      <div className="bg-gradient-to-r from-indigo-500 to-purple-600 rounded-lg shadow-lg p-6 mb-8 text-white">
        <div className="flex items-start">
          <div className="flex-shrink-0">
            <span className="text-3xl">🤖</span>
          </div>
          <div className="ml-4 flex-1">
            <h3 className="text-lg font-semibold mb-2">AI Daily Digest</h3>
            <div className="h-24 bg-white/10 rounded animate-pulse"></div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-gradient-to-r from-indigo-500 to-purple-600 rounded-lg shadow-lg p-6 mb-8 text-white">
      <div className="flex items-start">
        <div className="flex-shrink-0">
          <span className="text-3xl">🤖</span>
        </div>
        <div className="ml-4 flex-1">
          <h3 className="text-lg font-semibold mb-2">AI Daily Digest</h3>
          <p className="text-indigo-100 text-sm mb-3">
            Good morning! Here's your personalized summary for today.
          </p>
          <div className="bg-white/10 rounded p-3 text-sm">
            <p className="mb-2">
              📊 You have <strong>{dueTodayCount}</strong> task{dueTodayCount !== 1 ? 's' : ''} due today
            </p>
            <p className="mb-2">
              ⚠️ <strong>{overdueCount}</strong> overdue task{overdueCount !== 1 ? 's' : ''} need attention
            </p>
            <p>
              💪 Keep up the great work! Your productivity is on track.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
