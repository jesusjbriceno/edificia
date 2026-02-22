import { Badge } from '@/components/ui/Badge';
import { Calendar, FileText } from 'lucide-react';
import { ProjectActionsDropdown } from './ProjectActionsDropdown';
import {
  ProjectStatus,
  ProjectStatusLabels,
  InterventionTypeLabels,
} from '@/lib/types';
import type { ProjectResponse, InterventionType } from '@/lib/types';

interface ProjectCardProps {
  project: ProjectResponse;
  onClick?: (id: string) => void;
  onView?: (project: ProjectResponse) => void;
  onEdit?: (project: ProjectResponse) => void;
  onCompleteMemory?: (project: ProjectResponse) => void;
  onDelete?: (project: ProjectResponse) => void;
  onApprove?: (project: ProjectResponse) => void;
  onReject?: (project: ProjectResponse) => void;
  isAdmin?: boolean;
}

const statusVariant: Record<string, 'default' | 'info' | 'success' | 'warning'> = {
  [ProjectStatus.Draft]: 'default',
  [ProjectStatus.InProgress]: 'info',
  [ProjectStatus.Completed]: 'success',
  [ProjectStatus.Archived]: 'warning',
  [ProjectStatus.PendingReview]: 'warning',
};

export default function ProjectCard({ 
  project, 
  onClick,
  onView,
  onEdit,
  onCompleteMemory,
  onDelete,
  onApprove,
  onReject,
  isAdmin = false
}: Readonly<ProjectCardProps>) {
  const label =
    ProjectStatusLabels[project.status as ProjectStatus] ?? project.status;
  const variant = statusVariant[project.status] ?? 'default';

  const typeLabel =
    InterventionTypeLabels[
      Number(project.interventionType) as InterventionType
    ] ?? project.interventionType;

  const formattedDate = project.updatedAt
    ? new Date(project.updatedAt).toLocaleDateString('es-ES', {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
      })
    : new Date(project.createdAt).toLocaleDateString('es-ES', {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
      });

  return (
    <div
      onClick={() => onClick?.(project.id)}
      className="glass-card group p-6 rounded-xl transition-all duration-300 hover:border-brand-primary/30 hover:shadow-brand-primary/5 cursor-pointer flex flex-col justify-between h-48 text-left w-full relative"
    >
      <div className="flex justify-between items-start">
        <div className="p-2 bg-brand-primary/10 rounded-lg text-brand-primary">
          <FileText className="w-5 h-5" />
        </div>
        <ProjectActionsDropdown
          project={project}
          onView={onView}
          onEdit={onEdit}
          onCompleteMemory={onCompleteMemory}
          onDelete={onDelete}
          onApprove={onApprove}
          onReject={onReject}
          isAdmin={isAdmin}
        />
      </div>

      <div className="space-y-1">
        <h3 className="font-semibold text-lg text-white group-hover:text-brand-primary transition-colors truncate">
          {project.title}
        </h3>
        <p className="text-xs text-gray-500 uppercase tracking-widest font-medium">
          {typeLabel}
        </p>
      </div>

      <div className="flex items-center justify-between pt-4 border-t border-white/5">
        <div className="flex items-center text-xs text-gray-500">
          <Calendar className="w-3.5 h-3.5 mr-1.5" />
          {formattedDate}
        </div>
        <Badge variant={variant}>{label}</Badge>
      </div>
    </div>
  );
}
