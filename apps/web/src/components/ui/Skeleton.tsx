import { cn } from '@/lib/utils';

interface SkeletonProps {
  className?: string;
}

/** Animated placeholder block that pulses while content loads. */
export function Skeleton({ className }: Readonly<SkeletonProps>) {
  return (
    <div
      className={cn(
        'animate-pulse rounded-lg bg-white/5',
        className,
      )}
    />
  );
}

// ── Composed skeletons ───────────────────────────────────

/** Skeleton matching the shape of a ProjectCard in the dashboard grid. */
export function ProjectCardSkeleton() {
  return (
    <div className="rounded-2xl border border-white/5 bg-dark-card/60 p-6 space-y-4">
      <div className="flex items-center justify-between">
        <Skeleton className="h-5 w-40" />
        <Skeleton className="h-5 w-20 rounded-full" />
      </div>
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-4 w-3/4" />
      <div className="flex items-center gap-3 pt-2">
        <Skeleton className="h-4 w-24" />
        <Skeleton className="h-4 w-24" />
      </div>
    </div>
  );
}

/** Grid of ProjectCardSkeletons (default 6 cards). */
export function ProjectGridSkeleton({ count = 6 }: Readonly<{ count?: number }>) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {Array.from({ length: count }).map((_, i) => (
        <ProjectCardSkeleton key={i} />
      ))}
    </div>
  );
}

/** Skeleton for the admin project table rows. */
export function TableRowSkeleton() {
  return (
    <div className="flex items-center gap-4 px-6 py-4 border-b border-white/5">
      <Skeleton className="h-4 w-48" />
      <Skeleton className="h-4 w-24" />
      <Skeleton className="h-5 w-20 rounded-full" />
      <div className="ml-auto flex gap-2">
        <Skeleton className="h-8 w-8 rounded-lg" />
        <Skeleton className="h-8 w-8 rounded-lg" />
      </div>
    </div>
  );
}

/** Skeleton for the editor view (sidebar + editor area). */
export function EditorSkeleton() {
  return (
    <div className="flex-1 flex h-full bg-dark-bg/20 overflow-hidden">
      {/* Sidebar skeleton */}
      <div className="w-72 border-r border-white/5 p-4 space-y-3">
        <Skeleton className="h-6 w-32 mb-6" />
        {Array.from({ length: 5 }).map((_, i) => (
          <div key={i} className="space-y-2 pl-2">
            <Skeleton className="h-4 w-40" />
            {i < 3 && <Skeleton className="h-3 w-32 ml-4" />}
          </div>
        ))}
      </div>
      {/* Editor area skeleton */}
      <div className="flex-1 flex flex-col">
        <div className="h-14 border-b border-white/5 px-6 flex items-center gap-4">
          <Skeleton className="h-6 w-32" />
          <div className="ml-auto flex gap-2">
            <Skeleton className="h-9 w-28 rounded-lg" />
            <Skeleton className="h-9 w-28 rounded-lg" />
          </div>
        </div>
        <div className="flex-1 p-10 space-y-4">
          <Skeleton className="h-8 w-64" />
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-4 w-3/4" />
          <Skeleton className="h-4 w-5/6" />
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-4 w-2/3" />
        </div>
      </div>
    </div>
  );
}
