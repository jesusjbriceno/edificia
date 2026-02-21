import { useEditor, EditorContent } from '@tiptap/react';
import StarterKit from '@tiptap/starter-kit';
import Underline from '@tiptap/extension-underline';
import { useEditorStore } from '@/store/useEditorStore';
import { useEffect, useState, useCallback } from 'react';
import { Loader2, FileText, Download, Sparkles, Check, Save, WifiOff, AlertTriangle, ChevronRight, SendHorizonal } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { EditorToolbar } from './EditorToolbar.js';
import AiAssistantPanel from './AiAssistantPanel.js';
import { projectService } from '@/lib/services/projectService.js';
import { toast } from '@/store/useToastStore';
import { cn } from '@/lib/utils';
import { ProjectStatus } from '@/lib/types';
import type { SyncStatus } from '@/lib/syncManager';

// ── Sync Status Badge ────────────────────────────────────

function SyncBadge({ status, pendingCount }: Readonly<{ status: SyncStatus; pendingCount: number }>) {
  const base = 'flex items-center text-[10px] font-bold uppercase tracking-[0.15em] px-4 py-2 rounded-lg border transition-all duration-300';

  switch (status) {
    case 'syncing':
      return (
        <span className={`${base} text-blue-400 bg-blue-400/5 border-blue-400/10`}>
          <Loader2 size={12} className="animate-spin mr-2" /> Sincronizando...
        </span>
      );
    case 'synced':
      return (
        <span className={`${base} text-emerald-400 bg-emerald-400/5 border-emerald-400/10 shadow-[0_0_15px_-5px_rgba(52,211,153,0.2)]`}>
          <Check size={12} className="mr-2" /> Sincronizado
        </span>
      );
    case 'modified':
      return (
        <span className={`${base} text-amber-400 bg-amber-400/5 border-amber-400/10`}>
          <Save size={12} className="mr-2" /> {pendingCount} pendiente{pendingCount === 1 ? '' : 's'}
        </span>
      );
    case 'offline':
      return (
        <span className={`${base} text-orange-400 bg-orange-400/5 border-orange-400/10`}>
          <WifiOff size={12} className="mr-2" /> Modo Local
        </span>
      );
    case 'error':
      return (
        <span className={`${base} text-red-400 bg-red-400/5 border-red-400/10`}>
          <AlertTriangle size={12} className="mr-2" /> Error
        </span>
      );
    default:
      return (
        <span className={`${base} text-gray-500 bg-white/5 border-white/5`}>
          <Save size={12} className="mr-2" /> Borrador Local
        </span>
      );
  }
}

// ── Editor Shell ─────────────────────────────────────────

export default function EditorShell() {
  const { 
    activeSectionId, 
    content, 
    updateContent, 
    projectId,
    aiPanelOpen,
    setAiPanelOpen,
    activePath,
    syncStatus,
    pendingCount,
    projectStatus,
    setProjectStatus
  } = useEditorStore();

  const [exporting, setExporting] = useState(false);
  const [submittingReview, setSubmittingReview] = useState(false);

  const canSubmitForReview = projectStatus === ProjectStatus.Draft || projectStatus === ProjectStatus.InProgress;
  const isReadonly = projectStatus === ProjectStatus.PendingReview || projectStatus === ProjectStatus.Completed || projectStatus === ProjectStatus.Archived;

  const editor = useEditor({
    extensions: [
      StarterKit,
      Underline,
    ],
    content: activeSectionId ? content[activeSectionId] || '' : '',
    editorProps: {
      attributes: {
        class: 'prose prose-invert prose-brand focus:outline-none max-w-none min-h-[70vh] p-8 sm:p-12 lg:p-16 bg-dark-card/30 rounded-3xl border border-white/5 shadow-2xl transition-all duration-300',
      },
    },
    onUpdate: ({ editor }) => {
      if (activeSectionId) {
        updateContent(activeSectionId, editor.getHTML());
      }
    },
  });

  // Sincronizar editor cuando cambia la sección activa
  useEffect(() => {
    if (editor && activeSectionId) {
      const currentContent = editor.getHTML();
      const newContent = content[activeSectionId] || '';
      if (currentContent !== newContent) {
        editor.commands.setContent(newContent);
      }
    }
  }, [activeSectionId, editor]);

  // ── AI content insertion handler ──
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

  // ── Export DOCX handler ──
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

  // ── Submit for Review handler ──
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

  if (!activeSectionId) {
    return (
      <div className="flex-1 flex flex-col items-center justify-center text-gray-500 space-y-6 bg-dark-bg/20 backdrop-blur-sm">
        <div className="p-8 bg-white/5 rounded-full animate-pulse border border-white/5">
          <FileText size={64} className="opacity-10" />
        </div>
        <div className="text-center space-y-2">
          <h3 className="text-white/80 font-bold text-lg">Editor de Memoria</h3>
          <p className="text-sm max-w-70">Selecciona un capítulo de la barra lateral para empezar el proceso de redacción técnica.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 flex h-full bg-dark-bg/20 overflow-hidden relative">
      {/* Main Editor Area */}
      <div className="flex-1 flex flex-col h-full overflow-hidden">
        {/* Context Toolbar - As per the sketch */}
        <div className="px-6 py-4 flex flex-col gap-4 border-b border-white/5 bg-dark-card/30 backdrop-blur-md">
          {/* Line 1: Breadcrumbs */}
          <nav className="flex items-center gap-1.5 overflow-x-auto scrollbar-hide py-1">
            {activePath.map((item, idx) => (
              <div key={`${item}-${idx}`} className="flex items-center gap-1.5 shrink-0">
                <span className={cn(
                  "text-[11px] font-medium transition-colors",
                  idx === activePath.length - 1 ? "text-blue-500" : "text-gray-500 hover:text-gray-300"
                )}>
                  {item}
                </span>
                {idx < activePath.length - 1 && (
                  <ChevronRight size={10} className="text-gray-800" />
                )}
              </div>
            ))}
          </nav>

          {/* Line 2: Actions */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <SyncBadge status={syncStatus} pendingCount={pendingCount} />
              {isReadonly && (
                <span className="text-[10px] font-bold uppercase tracking-[0.15em] px-4 py-2 rounded-lg border text-amber-400 bg-amber-400/5 border-amber-400/10">
                  Solo lectura
                </span>
              )}
            </div>
            
            <div className="flex items-center gap-2">
              {canSubmitForReview && (
                <Button
                  size="sm"
                  className="h-9 px-4 bg-emerald-500/10 hover:bg-emerald-500/20 text-emerald-400 border border-emerald-500/20 font-bold transition-all"
                  onClick={handleSubmitForReview}
                  disabled={submittingReview}
                >
                  {submittingReview
                    ? <><Loader2 size={14} className="mr-2 animate-spin" /> Enviando...</>
                    : <><SendHorizonal size={14} className="mr-2" /> Enviar a Revisión</>
                  }
                </Button>
              )}
              <Button
                variant="ghost"
                size="sm"
                className={cn(
                  "h-9 px-4 text-brand-primary bg-brand-primary/5 hover:bg-brand-primary/15 border border-brand-primary/10",
                  aiPanelOpen && "ring-1 ring-brand-primary/40"
                )}
                onClick={() => setAiPanelOpen(!aiPanelOpen)}
              >
                <Sparkles size={14} className="mr-2 text-brand-primary" />
                IA
              </Button>
              <Button
                size="sm"
                className="h-9 px-6 bg-brand-primary hover:bg-brand-primary-hover text-white shadow-lg shadow-brand-primary/20 transition-all font-bold"
                onClick={handleExport}
                disabled={exporting || !projectId}
              >
                {exporting
                  ? <><Loader2 size={14} className="mr-2 animate-spin" /> Exportando...</>
                  : <><Download size={14} className="mr-2" /> Exportar</>
                }
              </Button>
            </div>
          </div>
        </div>

        {/* Area de Trabajo */}
        <div className="flex-1 overflow-y-auto p-6 lg:p-10 scroll-smooth">
          <div className="max-w-4xl mx-auto space-y-6">
            <EditorToolbar editor={editor} />
            <EditorContent editor={editor} className="editor-container" />
          </div>
        </div>
      </div>

      {/* AI Assistant Panel (slide-in from right) */}
      <AiAssistantPanel
        isOpen={aiPanelOpen}
        onClose={() => setAiPanelOpen(false)}
        onInsertContent={handleAiInsertContent}
        sectionTitle={activePath[activePath.length - 1]}
      />
    </div>
  );
}
