import { useEditor, EditorContent } from '@tiptap/react';
import StarterKit from '@tiptap/starter-kit';
import Underline from '@tiptap/extension-underline';
import Link from '@tiptap/extension-link';
import { Table } from '@tiptap/extension-table';
import { TableRow } from '@tiptap/extension-table-row';
import { TableCell } from '@tiptap/extension-table-cell';
import { TableHeader } from '@tiptap/extension-table-header';
import { useEditorStore } from '@/store/useEditorStore';
import { useEffect } from 'react';
import { EditorToolbar } from './EditorToolbar.js';
import AiAssistantPanel from './AiAssistantPanel.js';
import { EditorContextBar } from './EditorContextBar.js';
import { EditorEmptyState } from './EditorEmptyState.js';
import { useEditorActions } from '@/lib/hooks/useEditorActions.js';
import { sanitizeRichHtml } from '@/lib/sanitizeHtml';

// ── Editor Shell ─────────────────────────────────────────

export default function EditorShell() {
  const {
    activeSectionId,
    content,
    updateContent,
    aiPanelOpen,
    setAiPanelOpen,
    activePath,
    syncStatus,
    pendingCount,
  } = useEditorStore();

  const editor = useEditor({
    extensions: [
      StarterKit,
      Underline,
      Link.configure({
        openOnClick: true,
        autolink: true,
        protocols: ['http', 'https', 'mailto', 'tel'],
      }),
      Table.configure({ resizable: true }),
      TableRow,
      TableHeader,
      TableCell,
    ],
    content: activeSectionId ? content[activeSectionId] || '' : '',
    editorProps: {
      attributes: {
        class:
          'prose prose-invert prose-brand focus:outline-none max-w-none min-h-[70vh] p-8 sm:p-12 lg:p-16 bg-dark-card/30 rounded-3xl border border-white/5 shadow-2xl transition-all duration-300',
      },
    },
    onUpdate: ({ editor }) => {
      if (activeSectionId) {
        updateContent(activeSectionId, editor.getHTML());
      }
    },
  });

  // Sincronizar contenido del editor cuando cambia la sección activa
  useEffect(() => {
    if (editor && activeSectionId) {
      const newContent = sanitizeRichHtml(content[activeSectionId] || '');
      if (editor.getHTML() !== newContent) {
        editor.commands.setContent(newContent);
      }
    }
  }, [activeSectionId, editor, content]);

  const {
    exporting,
    submittingReview,
    canSubmitForReview,
    isReadonly,
    projectId,
    handleAiInsertContent,
    handleExport,
    handleSubmitForReview,
  } = useEditorActions(editor);

  if (!activeSectionId) {
    return <EditorEmptyState />;
  }

  return (
    <div className="flex-1 flex h-full bg-dark-bg/20 overflow-hidden relative">
      {/* Área principal de edición */}
      <div className="flex-1 flex flex-col h-full overflow-hidden">
        <EditorContextBar
          activePath={activePath}
          syncStatus={syncStatus}
          pendingCount={pendingCount}
          isReadonly={isReadonly}
          canSubmitForReview={canSubmitForReview}
          submittingReview={submittingReview}
          aiPanelOpen={aiPanelOpen}
          exporting={exporting}
          projectId={projectId}
          onSubmitForReview={handleSubmitForReview}
          onToggleAi={() => setAiPanelOpen(!aiPanelOpen)}
          onExport={handleExport}
        />

        {/* Área de trabajo */}
        <div className="flex-1 overflow-y-auto p-6 lg:p-10 scroll-smooth">
          <div className="max-w-4xl mx-auto space-y-6">
            <EditorToolbar editor={editor} />
            <EditorContent editor={editor} className="editor-container" />
          </div>
        </div>
      </div>

      {/* Panel IA (desliza desde la derecha) */}
      <AiAssistantPanel
        isOpen={aiPanelOpen}
        onClose={() => setAiPanelOpen(false)}
        onInsertContent={handleAiInsertContent}
        sectionTitle={activePath[activePath.length - 1]}
      />
    </div>
  );
}
