import { useState, useEffect, useCallback } from 'react';
import ProjectCard from '@/components/ProjectCard';
import ProjectWizard from '@/components/ProjectWizard';
import { DeleteProjectModal } from '@/components/DeleteProjectModal';
import { Button } from '@/components/ui/Button';
import { Plus, AlertCircle, FolderOpen } from 'lucide-react';
import { ProjectGridSkeleton } from '@/components/ui/Skeleton';
import { ErrorBoundary } from '@/components/ui/ErrorBoundary';
import { projectService } from '@/lib/services';
import type { ProjectResponse, PagedResponse } from '@/lib/types';
import { ApiError } from '@/lib/api';
import { useToastStore } from '@/store/useToastStore';

const PAGE_SIZE = 9;

export default function DashboardProjects() {
  const [projects, setProjects] = useState<ProjectResponse[]>([]);
  const [totalPages, setTotalPages] = useState(0);
  const [page, setPage] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [wizardOpen, setWizardOpen] = useState(false);
  const { addToast } = useToastStore();

  // Delete state (kept as modal — destructive action)
  const [selectedProject, setSelectedProject] = useState<ProjectResponse | null>(null);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  const fetchProjects = useCallback(async (p: number) => {
    setIsLoading(true);
    setError(null);
    try {
      const data: PagedResponse<ProjectResponse> = await projectService.list({
        page: p,
        pageSize: PAGE_SIZE,
      });
      setProjects(data.items);
      setTotalPages(data.totalPages);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message);
      } else {
        setError('No se pudieron cargar los proyectos.');
      }
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchProjects(page);
  }, [page, fetchProjects]);

  const handleCreated = (projectId: string) => {
    window.location.href = `/projects/${projectId}`;
  };

  const handleCardClick = (id: string) => {
    window.location.href = `/projects/${id}`;
  };

  // Action handlers — navigate to full pages
  const handleView = (project: ProjectResponse) => {
    window.location.href = `/projects/${project.id}`;
  };

  const handleEdit = (project: ProjectResponse) => {
    window.location.href = `/projects/${project.id}/edit`;
  };

  const handleDeleteClick = (project: ProjectResponse) => {
    setSelectedProject(project);
    setIsDeleteOpen(true);
  };

  const handleCompleteMemory = (project: ProjectResponse) => {
    window.location.href = `/projects/${project.id}/editor`;
  };

  const confirmDelete = async (project: ProjectResponse) => {
    setIsDeleting(true);
    try {
      await projectService.delete(project.id);
      addToast('Proyecto eliminado correctamente', 'success');
      setIsDeleteOpen(false);
      fetchProjects(page);
    } catch (err) {
      addToast('Error al eliminar el proyecto', 'error');
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <ErrorBoundary>
    <div className="space-y-8">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-white tracking-tight">Mis Proyectos</h1>
          <p className="text-gray-400 mt-1">Gestiona y redacta tus memorias de proyecto.</p>
        </div>
        <Button onClick={() => setWizardOpen(true)} className="h-12 px-6">
          <Plus size={18} className="mr-2" />
          Nuevo Proyecto
        </Button>
      </div>

      {/* Loading skeleton */}
      {isLoading && <ProjectGridSkeleton />}

      {/* Error */}
      {!isLoading && error && (
        <div className="flex flex-col items-center gap-4 py-20 text-center">
          <AlertCircle size={40} className="text-red-400" />
          <p className="text-red-400">{error}</p>
          <Button variant="outline" size="sm" onClick={() => fetchProjects(page)}>
            Reintentar
          </Button>
        </div>
      )}

      {/* Empty state */}
      {!isLoading && !error && projects.length === 0 && (
        <div className="flex flex-col items-center gap-4 py-20 text-center rounded-2xl border border-dashed border-white/10 bg-white/2">
          <FolderOpen size={48} className="text-gray-600" />
          <p className="text-gray-400">Aún no tienes ningún proyecto.</p>
          <Button onClick={() => setWizardOpen(true)} size="sm">
            <Plus size={16} className="mr-1" />
            Crear primer proyecto
          </Button>
        </div>
      )}

      {/* Project grid */}
      {!isLoading && !error && projects.length > 0 && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {projects.map(project => (
              <ProjectCard
                key={project.id}
                project={project}
                onClick={handleCardClick}
                onView={handleView}
                onEdit={handleEdit}
                onDelete={handleDeleteClick}
                onCompleteMemory={handleCompleteMemory}
              />
            ))}
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-center gap-2 pt-4">
              <Button
                variant="ghost"
                size="sm"
                disabled={page === 1}
                onClick={() => setPage(p => p - 1)}
              >
                Anterior
              </Button>
              <span className="text-sm text-gray-400">
                Página {page} de {totalPages}
              </span>
              <Button
                variant="ghost"
                size="sm"
                disabled={page >= totalPages}
                onClick={() => setPage(p => p + 1)}
              >
                Siguiente
              </Button>
            </div>
          )}
        </>
      )}

      {/* ProjectWizard (create new) */}
      <ProjectWizard
        isOpen={wizardOpen}
        onClose={() => setWizardOpen(false)}
        onCreated={handleCreated}
      />

      {/* Delete confirmation — kept as modal (destructive action) */}
      <DeleteProjectModal
        isOpen={isDeleteOpen}
        onClose={() => setIsDeleteOpen(false)}
        project={selectedProject}
        onConfirm={confirmDelete}
        isLoading={isDeleting}
      />
    </div>
    </ErrorBoundary>
  );
}
