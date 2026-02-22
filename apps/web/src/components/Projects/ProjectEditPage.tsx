import { useState, useEffect } from 'react';
import { ArrowLeft, Loader2, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { ProjectForm } from '@/components/Admin/ProjectForm';
import { projectService } from '@/lib/services';
import type { ProjectResponse } from '@/lib/types';

interface ProjectEditPageProps {
  projectId: string;
}

export default function ProjectEditPage({ projectId }: ProjectEditPageProps) {
  const [project, setProject] = useState<ProjectResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
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

  const handleSubmit = () => {
    window.location.href = `/projects/${projectId}`;
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="flex flex-col items-center gap-4 text-gray-400">
          <Loader2 size={36} className="animate-spin text-brand-primary" />
          <p className="text-sm">Cargando proyecto…</p>
        </div>
      </div>
    );
  }

  if (error || !project) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh] gap-6 text-center">
        <AlertCircle size={48} className="text-red-400" />
        <div>
          <p className="text-white font-semibold mb-1">Error al cargar</p>
          <p className="text-gray-400 text-sm">{error}</p>
        </div>
        <Button variant="outline" onClick={() => (window.location.href = `/projects/${projectId}`)}>
          <ArrowLeft size={16} className="mr-2" />
          Volver al proyecto
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
      {/* Breadcrumb */}
      <nav className="flex items-center gap-2 text-sm text-gray-500">
        <a
          href="/dashboard"
          className="hover:text-white transition-colors flex items-center gap-1.5"
        >
          <ArrowLeft size={14} />
          Mis Proyectos
        </a>
        <span>/</span>
        <a
          href={`/projects/${projectId}`}
          className="hover:text-white transition-colors truncate max-w-xs"
        >
          {project.title}
        </a>
        <span>/</span>
        <span className="text-gray-300">Editar datos</span>
      </nav>

      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-white tracking-tight">
          Editar Proyecto
        </h1>
        <p className="text-gray-400 mt-1">
          Modifica los datos generales de <span className="text-white">{project.title}</span>.
        </p>
      </div>

      {/* Form card */}
      <div className="bg-dark-card border border-white/5 rounded-3xl p-8 relative overflow-hidden">
        <div className="absolute top-0 right-0 p-12 opacity-5 pointer-events-none text-brand-primary">
          <svg width="120" height="120" viewBox="0 0 24 24" fill="currentColor">
            <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" />
          </svg>
        </div>
        <ProjectForm project={project} onSubmit={handleSubmit} />
      </div>

      {/* Cancel link */}
      <div className="flex justify-start">
        <a
          href={`/projects/${projectId}`}
          className="text-sm text-gray-500 hover:text-white transition-colors flex items-center gap-1.5"
        >
          <ArrowLeft size={14} />
          Cancelar y volver
        </a>
      </div>
    </div>
  );
}
