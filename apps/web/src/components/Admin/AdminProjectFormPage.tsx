import { useState, useEffect } from 'react';
import { ArrowLeft, Loader2, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { ProjectForm } from '@/components/Admin/ProjectForm';
import { projectService } from '@/lib/services';
import type { ProjectResponse } from '@/lib/types';

interface AdminProjectFormPageProps {
  /** When provided, the page loads this project and operates in edit mode. */
  projectId?: string;
}

export default function AdminProjectFormPage({
  projectId,
}: AdminProjectFormPageProps) {
  const isEditMode = Boolean(projectId);

  const [project, setProject] = useState<ProjectResponse | null>(null);
  const [isLoading, setIsLoading] = useState(isEditMode);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!projectId) return;

    const load = async () => {
      setIsLoading(true);
      setError(null);
      try {
        const data = await projectService.getById(projectId);
        setProject(data);
      } catch {
        setError('No se pudo cargar el proyecto para editar.');
      } finally {
        setIsLoading(false);
      }
    };

    load();
  }, [projectId]);

  const handleSubmit = (createdId?: string) => {
    if (isEditMode) {
      window.location.href = `/admin/projects/${projectId}`;
    } else {
      // Redirect to the newly created project detail
      window.location.href = createdId ? `/projects/${createdId}` : '/dashboard';
    }
  };

  // ── Loading ────────────────────────────────────────────────
  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="flex flex-col items-center gap-4 text-gray-400">
          <Loader2 size={36} className="animate-spin text-brand-primary" />
          <p className="text-sm">Cargando datos del proyecto…</p>
        </div>
      </div>
    );
  }

  // ── Error ────────────────────────────────────────────────
  if (error) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh] gap-6 text-center">
        <AlertCircle size={48} className="text-red-400" />
        <div>
          <p className="text-white font-semibold mb-1">Error al cargar</p>
          <p className="text-gray-400 text-sm">{error}</p>
        </div>
        <Button
          variant="outline"
          onClick={() => window.location.href = '/admin/projects'}
        >
          <ArrowLeft size={16} className="mr-2" />
          Volver al listado
        </Button>
      </div>
    );
  }

  const backHref = isEditMode
    ? `/admin/projects/${projectId}`
    : '/dashboard';

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
      {/* Breadcrumb */}
      <nav className="flex items-center gap-2 text-sm text-gray-500">
        <a
          href={isEditMode ? '/admin/projects' : '/dashboard'}
          className="hover:text-white transition-colors flex items-center gap-1.5"
        >
          <ArrowLeft size={14} />
          {isEditMode ? 'Proyectos' : 'Inicio'}
        </a>
        {isEditMode && project && (
          <>
            <span>/</span>
            <a
              href={`/admin/projects/${projectId}`}
              className="hover:text-white transition-colors truncate max-w-xs"
            >
              {project.title}
            </a>
          </>
        )}
        <span>/</span>
        <span className="text-gray-300">
          {isEditMode ? 'Editar datos' : 'Nuevo proyecto'}
        </span>
      </nav>

      {/* Page header */}
      <div>
        <h1 className="text-3xl font-bold text-white tracking-tight">
          {isEditMode ? 'Editar Proyecto' : 'Nuevo Proyecto Técnico'}
        </h1>
        <p className="text-gray-400 mt-1">
          {isEditMode
            ? 'Modifica los datos generales del proyecto.'
            : 'Completa los campos para crear un nuevo proyecto técnico.'}
        </p>
      </div>

      {/* Form card */}
      <div className="bg-dark-card border border-white/5 rounded-3xl p-8 relative overflow-hidden">
        {/* Decorative background element */}
        <div className="absolute top-0 right-0 p-12 opacity-5 pointer-events-none text-brand-primary">
          <svg width="120" height="120" viewBox="0 0 24 24" fill="currentColor">
            <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" />
          </svg>
        </div>

        <ProjectForm
          project={project ?? undefined}
          onSubmit={handleSubmit}
        />
      </div>

      {/* Cancel link */}
      <div className="flex justify-start">
        <Button
          variant="outline"
          onClick={() => { window.location.href = backHref; }}
          className="h-10"
        >
          <ArrowLeft size={14} className="mr-2" />
          Cancelar y volver
        </Button>
      </div>
    </div>
  );
}
