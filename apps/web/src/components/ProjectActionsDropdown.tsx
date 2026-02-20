import { Eye, Edit2, FileEdit, Trash2, MoreVertical } from 'lucide-react';
import { Dropdown } from '@/components/ui/Dropdown';
import type { ProjectResponse } from '@/lib/types';
import { cn } from '@/lib/utils';

interface ProjectActionsDropdownProps {
  project: ProjectResponse;
  onView?: (project: ProjectResponse) => void;
  onEdit?: (project: ProjectResponse) => void;
  onCompleteMemory?: (project: ProjectResponse) => void;
  onDelete?: (project: ProjectResponse) => void;
  className?: string;
  triggerClassName?: string;
}

export function ProjectActionsDropdown({
  project,
  onView,
  onEdit,
  onCompleteMemory,
  onDelete,
  className,
  triggerClassName
}: ProjectActionsDropdownProps) {
  return (
    <Dropdown
      trigger={
        <button
          className={cn(
            "p-2 text-gray-400 hover:text-white hover:bg-white/10 rounded-lg transition-colors border border-transparent hover:border-white/10",
            triggerClassName
          )}
        >
          <MoreVertical size={18} />
        </button>
      }
      className={cn("w-56", className)}
      align="right"
    >
      <div className="py-1">
        <button
          onClick={() => onView?.(project)}
          className="w-full flex items-center gap-3 px-4 py-2 text-sm text-gray-300 hover:bg-white/5 hover:text-white transition-colors text-left"
        >
          <Eye size={16} className="text-blue-400" />
          <span>Visualizar</span>
        </button>

        <button
          onClick={() => onEdit?.(project)}
          className="w-full flex items-center gap-3 px-4 py-2 text-sm text-gray-300 hover:bg-white/5 hover:text-white transition-colors text-left"
        >
          <Edit2 size={16} className="text-amber-400" />
          <span>Editar Datos</span>
        </button>

        <button
          onClick={() => onCompleteMemory?.(project)}
          className="w-full flex items-center gap-3 px-4 py-2 text-sm text-gray-300 hover:bg-white/5 hover:text-white transition-colors text-left"
        >
          <FileEdit size={16} className="text-emerald-400" />
          <span>Completar Memoria</span>
        </button>

        <div className="my-1 border-t border-white/5" />

        <button
          onClick={() => onDelete?.(project)}
          className="w-full flex items-center gap-3 px-4 py-2 text-sm text-red-400 hover:bg-red-500/10 transition-colors text-left"
        >
          <Trash2 size={16} />
          <span>Borrar Proyecto</span>
        </button>
      </div>
    </Dropdown>
  );
}
