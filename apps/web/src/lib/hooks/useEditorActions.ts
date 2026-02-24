import { useState, useCallback } from 'react';
import type { Editor } from '@tiptap/react';
import { useEditorStore } from '@/store/useEditorStore';
import { projectService } from '@/lib/services/projectService.js';
import { toast } from '@/store/useToastStore';
import { ProjectStatus } from '@/lib/types';

/**
 * Encapsula toda la lógica de acción del EditorShell:
 * - Inserción de contenido IA en el editor TipTap
 * - Exportación a DOCX
 * - Envío a revisión
 *
 * Separa la lógica de negocio de la presentación para facilitar
 * el testing independiente de cada handler.
 */
export function useEditorActions(editor: Editor | null) {
  const { projectId, activeSectionId, updateContent, projectStatus, setProjectStatus } =
    useEditorStore();

  const [exporting, setExporting] = useState(false);
  const [submittingReview, setSubmittingReview] = useState(false);

  const canSubmitForReview =
    projectStatus === ProjectStatus.Draft || projectStatus === ProjectStatus.InProgress;

  const isReadonly =
    projectStatus === ProjectStatus.PendingReview ||
    projectStatus === ProjectStatus.Completed ||
    projectStatus === ProjectStatus.Archived;

  /** Inserta contenido generado por IA en el editor activo. */
  const handleAiInsertContent = useCallback(
    (html: string, mode: 'replace' | 'append') => {
      if (!editor || !activeSectionId) return;

      if (mode === 'replace') {
        editor.commands.setContent(html);
      } else {
        editor.commands.focus('end');
        editor.commands.insertContent(html);
      }

      updateContent(activeSectionId, editor.getHTML());
    },
    [editor, activeSectionId, updateContent],
  );

  /** Solicita la exportación DOCX y dispara la descarga en el navegador. */
  const handleExport = useCallback(async () => {
    if (!projectId || exporting) return;

    setExporting(true);
    try {
      const { blob, fileName } = await projectService.exportDocx(projectId);

      const url = URL.createObjectURL(blob);
      const anchor = document.createElement('a');
      anchor.href = url;
      anchor.download = fileName;
      document.body.appendChild(anchor);
      anchor.click();
      anchor.remove();
      URL.revokeObjectURL(url);
      toast.success(`Documento "${fileName}" descargado correctamente.`);
    } catch {
      toast.error('No se pudo exportar el documento. Inténtalo de nuevo.');
    } finally {
      setExporting(false);
    }
  }, [projectId, exporting]);

  /** Cambia el estado del proyecto a PendingReview vía API. */
  const handleSubmitForReview = useCallback(async () => {
    if (!projectId || submittingReview || !canSubmitForReview) return;

    setSubmittingReview(true);
    try {
      await projectService.submitForReview(projectId);
      setProjectStatus(ProjectStatus.PendingReview);
      toast.success('Memoria enviada a revisión correctamente.');
    } catch {
      toast.error('No se pudo enviar a revisión. Inténtalo de nuevo.');
    } finally {
      setSubmittingReview(false);
    }
  }, [projectId, submittingReview, canSubmitForReview, setProjectStatus]);

  return {
    exporting,
    submittingReview,
    canSubmitForReview,
    isReadonly,
    projectId,
    handleAiInsertContent,
    handleExport,
    handleSubmitForReview,
  };
}
