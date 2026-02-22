import { useState, useEffect } from 'react';
import {
  ArrowLeft,
  Calendar,
  MapPin,
  Hash,
  BookOpen,
  Clock,
  FileText,
  Edit2,
  Trash2,
  CheckCircle,
  XCircle,
  AlertTriangle,
  Loader2,
  AlertCircle,
  FileEdit,
} from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { DeleteProjectModal } from '@/components/DeleteProjectModal';
import { projectService } from '@/lib/services';
import type { ProjectResponse } from '@/lib/types';
import {
  InterventionTypeStringLabels,
  ProjectStatus,
  ProjectStatusLabels,
} from '@/lib/types';
import { useAuthStore } from '@/store/useAuthStore';
import { useToastStore } from '@/store/useToastStore';

interface AdminProjectDetailPageProps {
  projectId: string;
}

const statusColorMap: Record<string, string> = {
  Draft: 'text-gray-400 bg-gray-500/10 border-gray-500/20',
  InProgress: 'text-blue-400 bg-blue-500/10 border-blue-500/20',
  PendingReview: 'text-amber-400 bg-amber-500/10 border-amber-500/20',
  Completed: 'text-emerald-400 bg-emerald-500/10 border-emerald-500/20',
  Archived: 'text-gray-500 bg-gray-600/10 border-gray-600/20',
};

const DetailItem = ({
  icon: Icon,
  label,
  value,
}: {
  icon: React.ElementType;
  label: string;
  value: string | null | undefined;
}) => (
  <div className="flex gap-4 p-4 rounded-xl bg-white/5 border border-white/5 transition-colors hover:bg-white/8">
    <div className="p-2 h-fit bg-brand-primary/10 rounded-lg text-brand-primary shrink-0">
      <Icon size={18} />
    </div>
    <div className="min-w-0">
      <p className="text-xs text-gray-500 uppercase tracking-widest font-medium mb-1">
        {label}
      </p>
      <p className="text-white font-medium break-words">
        {value || <span className="text-gray-600 italic">No especificado</span>}
      </p>
    </div>
  </div>
);

export default function AdminProjectDetailPage({
  projectId,
}: AdminProjectDetailPageProps) {
  const [project, setProject] = useState<ProjectResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [isRejectOpen, setIsRejectOpen] = useState(false);
  const [rejectReason, setRejectReason] = useState('');
  const [isProcessingReview, setIsProcessingReview] = useState(false);

  const { user } = useAuthStore();
  const { addToast } = useToastStore();

  const isAdmin =
    user?.roles?.some((r) => r === 'Admin' || r === 'Root') ?? false;

  useEffect(() => {
    const load = async () => {
      setIsLoading(true);
      setError(null);
      try {
        const data = await projectService.getById(projectId);
        setProject(data);
      } catch {
        setError('No se pudo cargar el proyecto. Comprueba que existe y tienes acceso.');
      } finally {
        setIsLoading(false);
      }
    };
    load();
  }, [projectId]);

  const handleDelete = async (p: ProjectResponse) => {
    setIsDeleting(true);
    try {
      await projectService.delete(p.id);
      addToast('Proyecto eliminado correctamente', 'success');
      window.location.href = '/admin/projects';
    } catch {
      addToast('Error al eliminar el proyecto', 'error');
    } finally {
      setIsDeleting(false);
    }
  };

  const handleApprove = async () => {
    if (!project) return;
    setIsProcessingReview(true);
    try {
      await projectService.approveProject(project.id);
      addToast('Memoria aprobada correctamente', 'success');
      const updated = await projectService.getById(project.id);
      setProject(updated);
    } catch {
      addToast('Error al aprobar la memoria', 'error');
    } finally {
      setIsProcessingReview(false);
    }
  };

  const handleConfirmReject = async () => {
    if (!project || !rejectReason.trim()) return;
    setIsProcessingReview(true);
    try {
      await projectService.rejectProject(project.id, rejectReason.trim());
      addToast('Memoria rechazada correctamente', 'success');
      setIsRejectOpen(false);
      const updated = await projectService.getById(project.id);
      setProject(updated);
    } catch {
      addToast('Error al rechazar la memoria', 'error');
    } finally {
      setIsProcessingReview(false);
    }
  };

  // ── Loading ────────────────────────────────────────────────
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

  // ── Error ────────────────────────────────────────────────
  if (error || !project) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh] gap-6 text-center">
        <AlertCircle size={48} className="text-red-400" />
        <div>
          <p className="text-white font-semibold mb-1">No se pudo cargar el proyecto</p>
          <p className="text-gray-400 text-sm">{error}</p>
        </div>
        <Button variant="outline" onClick={() => window.location.href = '/admin/projects'}>
          <ArrowLeft size={16} className="mr-2" />
          Volver al listado
        </Button>
      </div>
    );
  }

  const typeLabel =
    InterventionTypeStringLabels[project.interventionType] ??
    String(project.interventionType);
  const statusLabel =
    ProjectStatusLabels[project.status as keyof typeof ProjectStatusLabels] ??
    project.status;
  const statusColors =
    statusColorMap[project.status] ?? statusColorMap['Draft'];
  const isPendingReview = project.status === ProjectStatus.PendingReview;

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
      {/* Breadcrumb + Back */}
      <nav className="flex items-center gap-2 text-sm text-gray-500">
        <a
          href="/admin/projects"
          className="hover:text-white transition-colors flex items-center gap-1.5"
        >
          <ArrowLeft size={14} />
          Proyectos
        </a>
        <span>/</span>
        <span className="text-gray-300 truncate max-w-xs">{project.title}</span>
      </nav>

      {/* Title row */}
      <div className="flex flex-col lg:flex-row lg:items-start justify-between gap-6">
        <div className="min-w-0">
          <div className="flex items-center gap-3 mb-2 flex-wrap">
            <span
              className={`px-3 py-1 rounded-full border text-xs font-bold uppercase ${statusColors}`}
            >
              {statusLabel}
            </span>
          </div>
          <h1 className="text-3xl font-bold text-white tracking-tight leading-tight">
            {project.title}
          </h1>
          {project.description && (
            <p className="mt-3 text-gray-400 leading-relaxed max-w-2xl">
              {project.description}
            </p>
          )}
        </div>

        {/* Actions */}
        <div className="flex flex-wrap gap-3 shrink-0">
          {isAdmin && isPendingReview && (
            <>
              <Button
                onClick={handleApprove}
                disabled={isProcessingReview}
                className="bg-emerald-600 hover:bg-emerald-700 text-white border-transparent gap-2"
              >
                <CheckCircle size={16} />
                Aprobar
              </Button>
              <Button
                onClick={() => { setRejectReason(''); setIsRejectOpen(true); }}
                disabled={isProcessingReview}
                className="bg-orange-500 hover:bg-orange-600 text-white border-transparent gap-2"
              >
                <XCircle size={16} />
                Rechazar
              </Button>
            </>
          )}

          <Button
            variant="outline"
            onClick={() => window.location.href = `/projects/${project.id}/editor`}
            className="gap-2"
          >
            <FileEdit size={16} />
            Completar Memoria
          </Button>

          <Button
            variant="outline"
            onClick={() => window.location.href = `/admin/projects/${project.id}/edit`}
            className="gap-2"
          >
            <Edit2 size={16} />
            Editar Datos
          </Button>

          <Button
            variant="ghost"
            onClick={() => setIsDeleteOpen(true)}
            className="bg-red-600/10 hover:bg-red-600/20 text-red-400 border border-red-500/20 gap-2"
          >
            <Trash2 size={16} />
            Eliminar
          </Button>
        </div>
      </div>

      {/* Details grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
        <DetailItem
          icon={BookOpen}
          label="Tipo de Intervención"
          value={typeLabel}
        />
        <DetailItem
          icon={Calendar}
          label="Fecha de Creación"
          value={new Date(project.createdAt).toLocaleDateString('es-ES', {
            day: '2-digit',
            month: 'long',
            year: 'numeric',
          })}
        />
        <DetailItem
          icon={Clock}
          label="Última Actualización"
          value={
            project.updatedAt
              ? new Date(project.updatedAt).toLocaleDateString('es-ES', {
                  day: '2-digit',
                  month: 'long',
                  year: 'numeric',
                })
              : null
          }
        />
        <DetailItem
          icon={MapPin}
          label="Ubicación / Dirección"
          value={project.address}
        />
        <DetailItem
          icon={Hash}
          label="Referencia Catastral"
          value={project.cadastralReference}
        />
        <DetailItem
          icon={FileText}
          label="Normativa Aplicable"
          value={project.localRegulations}
        />
      </div>

      {/* LOE badge */}
      <div className="flex items-center gap-3 p-4 rounded-xl bg-white/5 border border-white/5 w-fit">
        <div
          className={`w-3 h-3 rounded-full ${project.isLoeRequired ? 'bg-blue-400' : 'bg-gray-500'}`}
        />
        <span className="text-sm text-gray-300">
          LOE:{' '}
          <span className={project.isLoeRequired ? 'text-blue-400 font-medium' : 'text-gray-500'}>
            {project.isLoeRequired ? 'Aplicable' : 'Exento (reforma menor)'}
          </span>
        </span>
      </div>

      {/* Reject dialog inline */}
      {isRejectOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-in fade-in duration-200">
          <div className="bg-dark-card border border-white/10 rounded-2xl p-6 w-full max-w-lg space-y-4 shadow-2xl animate-in zoom-in-95 duration-200">
            <h3 className="text-lg font-bold text-white">Rechazar Memoria</h3>
            <p className="text-sm text-gray-400">
              Indica el motivo del rechazo. El proyecto volverá al estado de borrador para correcciones.
            </p>
            <div>
              <label className="block text-sm font-medium text-gray-300 mb-2">
                Motivo del rechazo
              </label>
              <textarea
                rows={4}
                maxLength={2000}
                className="w-full rounded-xl bg-white/5 border border-white/10 text-white placeholder-gray-500 px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-brand-primary/40 focus:border-brand-primary/40 transition-all resize-none"
                placeholder="Describa las correcciones necesarias…"
                value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
              />
              <p className="text-xs text-gray-500 mt-1 text-right">
                {rejectReason.length}/2000
              </p>
            </div>
            <div className="flex justify-end gap-3 pt-2">
              <Button
                variant="ghost"
                onClick={() => setIsRejectOpen(false)}
                disabled={isProcessingReview}
              >
                Cancelar
              </Button>
              <Button
                className="bg-orange-500 hover:bg-orange-600 text-white border-transparent"
                onClick={handleConfirmReject}
                disabled={!rejectReason.trim() || isProcessingReview}
                isLoading={isProcessingReview}
              >
                Confirmar Rechazo
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Delete modal */}
      <DeleteProjectModal
        isOpen={isDeleteOpen}
        onClose={() => setIsDeleteOpen(false)}
        project={project}
        onConfirm={handleDelete}
        isLoading={isDeleting}
      />
    </div>
  );
}
