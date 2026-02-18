import { MoreVertical, Folder, Calendar } from 'lucide-react';
import { cn } from '@/lib/utils';
import {
  ProjectStatus,
  ProjectStatusLabels,
  InterventionTypeLabels,
} from '@/lib/types';
import type { ProjectResponse, InterventionType } from '@/lib/types';

interface ProjectRowProps {
  project: ProjectResponse;
  onEdit?: (id: string) => void;
  onView?: (id: string) => void;
}

const statusStyles: Record<string, string> = {
  [ProjectStatus.Draft]:
    'bg-gray-400/10 text-gray-400 border-gray-400/20',
  [ProjectStatus.InProgress]:
    'bg-emerald-400/10 text-emerald-400 border-emerald-400/20 shadow-[0_0_10px_-2px_rgba(52,211,153,0.2)]',
  [ProjectStatus.Completed]:
    'bg-blue-400/10 text-blue-400 border-blue-400/20',
  [ProjectStatus.Archived]:
    'bg-amber-400/10 text-amber-400 border-amber-400/20',
};

export function ProjectRow({ project, onEdit, onView }: ProjectRowProps) {
  const statusLabel =
    ProjectStatusLabels[project.status as ProjectStatus] ?? project.status;
  const typeLabel =
    InterventionTypeLabels[Number(project.interventionType) as InterventionType] ??
    project.interventionType;

  return (
    <div
      className="group flex items-center gap-6 p-4 bg-white/2 rounded-2xl border border-white/5 hover:bg-white/5 hover:border-brand-primary/20 transition-all duration-300 cursor-pointer"
      onClick={() => onView?.(project.id)}
    >
      <div className="h-14 w-14 rounded-2xl bg-brand-primary/10 flex items-center justify-center border border-brand-primary/10 group-hover:scale-110 transition-transform">
        <Folder className="text-brand-primary" size={24} />
      </div>

      <div className="flex-1 min-w-0">
        <h3 className="text-white font-bold truncate group-hover:text-brand-primary transition-colors">
          {project.title}
        </h3>
        <p className="text-sm text-gray-500 truncate mt-1">
          {project.description || typeLabel}
        </p>
      </div>

      <div className="hidden md:flex items-center gap-6 text-sm text-gray-400">
        <div className="flex items-center gap-2">
          <Calendar size={14} className="opacity-50" />
          <span>{new Date(project.createdAt).toLocaleDateString('es-ES')}</span>
        </div>
        <span className="text-xs text-gray-500 uppercase tracking-wider">
          {typeLabel}
        </span>
      </div>

      <div className="flex items-center gap-4">
        <span
          className={cn(
            'px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-wider border',
            statusStyles[project.status] ?? statusStyles[ProjectStatus.Draft],
          )}
        >
          {statusLabel}
        </span>

        <button
          onClick={e => {
            e.stopPropagation();
            onEdit?.(project.id);
          }}
          className="p-2 text-gray-500 hover:text-white hover:bg-white/10 rounded-lg transition-colors"
        >
          <MoreVertical size={18} />
        </button>
      </div>
    </div>
  );
}
