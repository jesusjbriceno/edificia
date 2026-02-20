import { useState, useEffect, useCallback } from 'react';
import { ProjectRow } from './ProjectRow.js';
import { ProjectForm } from './ProjectForm.js';
import ProjectCard from '@/components/ProjectCard';
import { ProjectDetailsModal } from '@/components/ProjectDetailsModal';
import { DeleteProjectModal } from '@/components/DeleteProjectModal';
import Modal from '@/components/ui/Modal';
import { Button } from '@/components/ui/Button';
import { Plus, Search, Filter, AlertCircle, LayoutGrid, List } from 'lucide-react';
import { Select } from '@/components/ui/Select';
import { TableRowSkeleton, ProjectGridSkeleton } from '@/components/ui/Skeleton';
import { ErrorBoundary } from '@/components/ui/ErrorBoundary';
import { projectService } from '@/lib/services';
import type { ProjectResponse, PagedResponse } from '@/lib/types';
import { ApiError } from '@/lib/api';
import { useToastStore } from '@/store/useToastStore';
import { cn } from '@/lib/utils';

const PAGE_SIZE = 10;

export default function ProjectManagement() {
  const [projects, setProjects] = useState<ProjectResponse[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isAdding, setIsAdding] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [viewMode, setViewMode] = useState<'list' | 'grid'>('list');
  const { addToast } = useToastStore();

  // Modal states
  const [selectedProject, setSelectedProject] = useState<ProjectResponse | null>(null);
  const [isViewOpen, setIsViewOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  const fetchProjects = useCallback(async (p: number, status?: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const data: PagedResponse<ProjectResponse> = await projectService.list({
        page: p,
        pageSize: PAGE_SIZE,
        status,
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
    fetchProjects(page, statusFilter);
  }, [page, statusFilter, fetchProjects]);

  // Action handlers
  const handleView = (project: ProjectResponse) => {
    setSelectedProject(project);
    setIsViewOpen(true);
  };

  const handleEdit = (project: ProjectResponse) => {
    setSelectedProject(project);
    setIsEditOpen(true);
  };

  const handleDeleteClick = (project: ProjectResponse) => {
    setSelectedProject(project);
    setIsDeleteOpen(true);
  };

  const handleCompleteMemory = (project: ProjectResponse) => {
    window.location.href = `/projects/${project.id}`;
  };

  const confirmDelete = async (project: ProjectResponse) => {
    setIsDeleting(true);
    try {
      await projectService.delete(project.id);
      addToast('Proyecto eliminado correctamente', 'success');
      setIsDeleteOpen(false);
      fetchProjects(page, statusFilter);
    } catch (err) {
      addToast('Error al eliminar el proyecto', 'error');
    } finally {
      setIsDeleting(false);
    }
  };

  const handleCreated = async () => {
    setIsAdding(false);
    await fetchProjects(1, statusFilter);
    setPage(1);
    addToast('Proyecto creado correctamente', 'success');
  };

  const filteredProjects = searchTerm
    ? projects.filter(
        p =>
          p.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
          (p.description ?? '').toLowerCase().includes(searchTerm.toLowerCase()),
      )
    : projects;

  return (
    <ErrorBoundary>
    <div className="space-y-8">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold text-white tracking-tight">
            Gestión de Proyectos
          </h1>
          <p className="text-gray-400 mt-1">
            Supervise y gestione todas las obras y proyectos técnicos.
          </p>
        </div>

        <div className="flex items-center gap-3">
          {/* View Toggle */}
          <div className="flex bg-white/5 rounded-xl border border-white/5 p-1 mr-2">
            <button
              onClick={() => setViewMode('grid')}
              className={cn(
                "p-2 rounded-lg transition-all",
                viewMode === 'grid' ? "bg-brand-primary/20 text-brand-primary" : "text-gray-500 hover:text-gray-300"
              )}
              title="Vista Cuadrícula"
            >
              <LayoutGrid size={20} />
            </button>
            <button
              onClick={() => setViewMode('list')}
              className={cn(
                "p-2 rounded-lg transition-all",
                viewMode === 'list' ? "bg-brand-primary/20 text-brand-primary" : "text-gray-500 hover:text-gray-300"
              )}
              title="Vista Lista"
            >
              <List size={20} />
            </button>
          </div>

          <Button onClick={() => setIsAdding(!isAdding)} className="h-12 px-6">
            <Plus size={18} className="mr-2" />
            {isAdding ? 'Volver al Listado' : 'Nuevo Proyecto'}
          </Button>
        </div>
      </div>

      {isAdding ? (
        <div className="animate-in fade-in slide-in-from-bottom-4 duration-500">
          <div className="bg-dark-card border border-white/5 p-8 rounded-3xl relative overflow-hidden">
            <div className="absolute top-0 right-0 p-12 opacity-5 pointer-events-none">
              <Plus size={120} />
            </div>
            <h2 className="text-xl font-bold text-white mb-6">
              Crear Nuevo Proyecto Técnico
            </h2>
            <ProjectForm onSubmit={handleCreated} />
          </div>
        </div>
      ) : (
        <div className="space-y-6">
          {/* Search & filter bar */}
          <div className="flex items-center gap-4 p-2 bg-white/5 rounded-2xl border border-white/5">
            <div className="flex-1 relative">
              <Search
                className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500"
                size={18}
              />
              <input
                type="text"
                placeholder="Buscar por título o descripción..."
                className="w-full bg-transparent border-none focus:ring-0 text-white pl-12 py-3"
                value={searchTerm}
                onChange={e => setSearchTerm(e.target.value)}
              />
            </div>
            <Select
              containerClassName="w-auto"
              className="border-none bg-transparent pr-8 py-0 h-full text-gray-400 text-sm focus:ring-0"
              value={statusFilter ?? ''}
              onChange={(e) => {
                setStatusFilter(e.target.value || undefined);
                setPage(page);
              }}
              options={[
                { value: '', label: 'Todos los estados' },
                { value: 'Draft', label: 'Borrador' },
                { value: 'InProgress', label: 'En redacción' },
                { value: 'Completed', label: 'Completado' },
                { value: 'Archived', label: 'Archivado' },
              ]}
            />

            <Button
              variant="ghost"
              className="hidden sm:flex text-gray-400 hover:text-white"
              onClick={() => {
                setSearchTerm('');
                setStatusFilter(undefined);
                setPage(1);
              }}
            >
              <Filter size={18} className="mr-2" />
              Limpiar
            </Button>
          </div>

          {/* Loading */}
          {isLoading && (
            <div className="space-y-1">
              {viewMode === 'list' ? (
                Array.from({ length: 5 }).map((_, i) => (
                  <TableRowSkeleton key={i} />
                ))
              ) : (
                <ProjectGridSkeleton />
              )}
            </div>
          )}

          {/* Error */}
          {!isLoading && error && (
            <div className="flex flex-col items-center gap-4 py-20 text-center">
              <AlertCircle size={40} className="text-red-400" />
              <p className="text-red-400">{error}</p>
              <Button
                variant="outline"
                size="sm"
                onClick={() => fetchProjects(page, statusFilter)}
              >
                Reintentar
              </Button>
            </div>
          )}

          {/* Project list */}
          {!isLoading && !error && (
            <>
              {viewMode === 'list' ? (
                <div className="grid gap-4">
                  {filteredProjects.length > 0 ? (
                    filteredProjects.map(project => (
                      <ProjectRow
                        key={project.id}
                        project={project}
                        onView={handleView}
                        onEdit={handleEdit}
                        onDelete={handleDeleteClick}
                        onCompleteMemory={handleCompleteMemory}
                      />
                    ))
                  ) : (
                    <EmptyState searchTerm={searchTerm} onClearSearch={() => setSearchTerm('')} />
                  )}
                </div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                   {filteredProjects.length > 0 ? (
                    filteredProjects.map(project => (
                      <ProjectCard
                        key={project.id}
                        project={project}
                        onClick={() => handleView(project)}
                        onView={handleView}
                        onEdit={handleEdit}
                        onDelete={handleDeleteClick}
                        onCompleteMemory={handleCompleteMemory}
                      />
                    ))
                  ) : (
                    <div className="col-span-full">
                      <EmptyState searchTerm={searchTerm} onClearSearch={() => setSearchTerm('')} />
                    </div>
                  )}
                </div>
              )}

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
        </div>
      )}

      {/* Modals shared with Dashboard */}
      <ProjectDetailsModal
        isOpen={isViewOpen}
        onClose={() => setIsViewOpen(false)}
        project={selectedProject}
        onCompleteMemory={handleCompleteMemory}
      />

      <Modal
        isOpen={isEditOpen}
        onClose={() => setIsEditOpen(false)}
        title="Editar Datos del Proyecto"
        className="max-w-xl"
      >
        <ProjectForm
          project={selectedProject || undefined}
          onSubmit={() => {
            setIsEditOpen(false);
            fetchProjects(page, statusFilter);
            addToast('Proyecto actualizado correctamente', 'success');
          }}
        />
      </Modal>

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

function EmptyState({ searchTerm, onClearSearch }: { searchTerm: string, onClearSearch: () => void }) {
  return (
    <div className="py-20 text-center space-y-4 bg-white/2 rounded-3xl border border-dashed border-white/10">
      <p className="text-gray-500">
        No se han encontrado proyectos.
      </p>
      {searchTerm && (
        <Button
          variant="outline"
          onClick={onClearSearch}
          size="sm"
        >
          Limpiar búsqueda
        </Button>
      )}
    </div>
  );
}
