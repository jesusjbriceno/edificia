import { useState, useEffect, useCallback } from 'react';
import { ProjectRow } from './ProjectRow.js';
import { ProjectForm } from './ProjectForm.js';
import { Button } from '@/components/ui/Button';
import { Plus, Search, Filter, AlertCircle } from 'lucide-react';
import { TableRowSkeleton } from '@/components/ui/Skeleton';
import { ErrorBoundary } from '@/components/ui/ErrorBoundary';
import { projectService } from '@/lib/services';
import type { ProjectResponse, PagedResponse } from '@/lib/types';
import { ApiError } from '@/lib/api';

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

  const handleAddProject = async () => {
    setIsAdding(false);
    // Refresh list after creation
    await fetchProjects(1, statusFilter);
    setPage(1);
  };

  // Client-side search filter (within current page)
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

        <Button onClick={() => setIsAdding(!isAdding)} className="h-12 px-6">
          <Plus size={18} className="mr-2" />
          {isAdding ? 'Volver al Listado' : 'Nuevo Proyecto'}
        </Button>
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
            <ProjectForm onSubmit={handleAddProject} />
          </div>
        </div>
      ) : (
        <div className="space-y-6">
          {/* Search & filter bar */}
          <div className="flex gap-4 p-2 bg-white/5 rounded-2xl border border-white/5">
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
            <select
              value={statusFilter ?? ''}
              onChange={e => {
                setStatusFilter(e.target.value || undefined);
                setPage(1);
              }}
              className="bg-transparent border-none text-gray-400 text-sm focus:ring-0 appearance-none pr-6 cursor-pointer"
            >
              <option value="">Todos los estados</option>
              <option value="Draft">Borrador</option>
              <option value="InProgress">En redacción</option>
              <option value="Completed">Completado</option>
              <option value="Archived">Archivado</option>
            </select>
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
              {Array.from({ length: 5 }).map((_, i) => (
                <TableRowSkeleton key={i} />
              ))}
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
              <div className="grid gap-4">
                {filteredProjects.length > 0 ? (
                  filteredProjects.map(project => (
                    <ProjectRow
                      key={project.id}
                      project={project}
                      onView={id => (globalThis.location.href = `/projects/${id}`)}
                      onEdit={id => (globalThis.location.href = `/projects/${id}`)}
                    />
                  ))
                ) : (
                  <div className="py-20 text-center space-y-4 bg-white/2 rounded-3xl border border-dashed border-white/10">
                    <p className="text-gray-500">
                      No se han encontrado proyectos.
                    </p>
                    {searchTerm && (
                      <Button
                        variant="outline"
                        onClick={() => setSearchTerm('')}
                        size="sm"
                      >
                        Limpiar búsqueda
                      </Button>
                    )}
                  </div>
                )}
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
        </div>
      )}
    </div>
    </ErrorBoundary>
  );
}
