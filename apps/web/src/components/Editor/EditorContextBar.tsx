import { Loader2, Download, Sparkles, SendHorizonal, ChevronRight } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { SyncBadge } from './SyncBadge.js';
import { cn } from '@/lib/utils';
import type { SyncStatus } from '@/lib/syncManager';

export interface EditorContextBarProps {
  /** Ruta de migas de pan hacia la sección activa. */
  activePath: string[];
  /** Estado de sincronización actual. */
  syncStatus: SyncStatus;
  /** Número de cambios pendientes de sincronizar. */
  pendingCount: number;
  /** Si el proyecto está en modo solo lectura (PendingReview, Completed, Archived). */
  isReadonly: boolean;
  /** Si el proyecto puede enviarse a revisión (Draft o InProgress). */
  canSubmitForReview: boolean;
  /** Si el envío a revisión está en progreso. */
  submittingReview: boolean;
  /** Si el panel de IA está abierto. */
  aiPanelOpen: boolean;
  /** Si la exportación a DOCX está en progreso. */
  exporting: boolean;
  /** ID del proyecto activo. Sin él, la exportación se deshabilita. */
  projectId: string | null;
  onSubmitForReview: () => void;
  onToggleAi: () => void;
  onExport: () => void;
}

/**
 * Barra contextual del editor que muestra:
 * - Migas de pan hacia la sección activa
 * - Estado de sincronización (`SyncBadge`)
 * - Acciones principales: Enviar a Revisión, IA, Exportar
 *
 * Cumple nivel AA de WCAG en contrastes y atributos ARIA.
 */
export function EditorContextBar({
  activePath,
  syncStatus,
  pendingCount,
  isReadonly,
  canSubmitForReview,
  submittingReview,
  aiPanelOpen,
  exporting,
  projectId,
  onSubmitForReview,
  onToggleAi,
  onExport,
}: Readonly<EditorContextBarProps>) {
  return (
    <div className="px-6 py-4 flex flex-col gap-4 border-b border-white/5 bg-dark-card/30 backdrop-blur-md">
      {/* Línea 1: migas de pan */}
      <nav aria-label="Ruta de la sección activa" className="flex items-center gap-1.5 overflow-x-auto scrollbar-hide py-1">
        {activePath.map((item, idx) => (
          <div key={`${item}-${idx}`} className="flex items-center gap-1.5 shrink-0">
            <span
              className={cn(
                'text-[11px] font-medium transition-colors',
                idx === activePath.length - 1
                  ? 'text-blue-500'
                  : 'text-gray-500 hover:text-gray-300',
              )}
              aria-current={idx === activePath.length - 1 ? 'page' : undefined}
            >
              {item}
            </span>
            {idx < activePath.length - 1 && (
              <ChevronRight size={10} className="text-gray-800" aria-hidden="true" />
            )}
          </div>
        ))}
      </nav>

      {/* Línea 2: estado + acciones */}
      <div className="flex items-center justify-between">
        {/* Estado izquierda */}
        <div className="flex items-center gap-3">
          <SyncBadge status={syncStatus} pendingCount={pendingCount} />
          {isReadonly && (
            <span
              role="status"
              aria-label="El proyecto está en modo de solo lectura"
              className="text-[10px] font-bold uppercase tracking-[0.15em] px-4 py-2 rounded-lg border text-amber-400 bg-amber-400/5 border-amber-400/10"
            >
              Solo lectura
            </span>
          )}
        </div>

        {/* Acciones derecha */}
        <div className="flex items-center gap-2" role="toolbar" aria-label="Acciones del editor">
          {/* ── Enviar a Revisión ── */}
          {canSubmitForReview && (
            <Button
              size="sm"
              aria-label={submittingReview ? 'Enviando a revisión…' : 'Enviar memoria a revisión'}
              aria-busy={submittingReview}
              className="h-9 px-4 bg-emerald-500/10 hover:bg-emerald-500/20 text-emerald-400 border border-emerald-500/20 font-bold transition-all"
              onClick={onSubmitForReview}
              disabled={submittingReview}
            >
              {submittingReview ? (
                <>
                  <Loader2 size={14} className="mr-2 animate-spin" aria-hidden="true" />
                  <span>Enviando...</span>
                  <span className="sr-only">Por favor espera, enviando a revisión</span>
                </>
              ) : (
                <>
                  <SendHorizonal size={14} className="mr-2" aria-hidden="true" />
                  Enviar a Revisión
                </>
              )}
            </Button>
          )}

          {/* ── Panel IA (toggle) ── */}
          <Button
            variant="ghost"
            size="sm"
            aria-label={aiPanelOpen ? 'Cerrar asistente de IA' : 'Abrir asistente de IA'}
            aria-pressed={aiPanelOpen}
            aria-expanded={aiPanelOpen}
            className={cn(
              'h-9 px-4 text-blue-400 bg-blue-400/5 hover:bg-blue-400/15 border border-blue-400/10',
              aiPanelOpen && 'ring-1 ring-blue-400/50 bg-blue-400/10',
            )}
            onClick={onToggleAi}
          >
            <Sparkles size={14} className="mr-2 text-blue-400" aria-hidden="true" />
            IA
          </Button>

          {/* ── Exportar DOCX ── */}
          <Button
            size="sm"
            aria-label={
              !projectId
                ? 'Exportar a DOCX (no disponible: proyecto no guardado)'
                : exporting
                  ? 'Exportando documento…'
                  : 'Exportar memoria a DOCX'
            }
            aria-busy={exporting}
            title={!projectId ? 'El proyecto debe estar guardado para poder exportar' : undefined}
            className="h-9 px-6 bg-brand-primary hover:bg-brand-primary-hover text-white shadow-lg shadow-brand-primary/20 transition-all font-bold"
            onClick={onExport}
            disabled={exporting || !projectId}
          >
            {exporting ? (
              <>
                <Loader2 size={14} className="mr-2 animate-spin" aria-hidden="true" />
                <span>Exportando...</span>
                <span className="sr-only">Generando el documento DOCX, por favor espera</span>
              </>
            ) : (
              <>
                <Download size={14} className="mr-2" aria-hidden="true" />
                Exportar
              </>
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}
