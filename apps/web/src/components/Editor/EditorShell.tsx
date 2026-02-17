import { useEditor, EditorContent } from '@tiptap/react';
import StarterKit from '@tiptap/starter-kit';
import Underline from '@tiptap/extension-underline';
import { useEditorStore } from '@/store/useEditorStore';
import { useEffect } from 'react';
import { Loader2, Sparkles, Save, FileText } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { EditorToolbar } from './EditorToolbar';

export default function EditorShell() {
  const { activeSectionId, content, updateContent, isSaving } = useEditorStore();

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

  if (!activeSectionId) {
    return (
      <div className="flex-1 flex flex-col items-center justify-center text-gray-500 space-y-6 bg-dark-bg/20 backdrop-blur-sm">
        <div className="p-8 bg-white/5 rounded-full animate-pulse border border-white/5">
          <FileText size={64} className="opacity-10" />
        </div>
        <div className="text-center space-y-2">
          <h3 className="text-white/80 font-bold text-lg">Editor de Memoria</h3>
          <p className="text-sm max-w-[280px]">Selecciona un capítulo de la barra lateral para empezar el proceso de redacción técnica.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 flex flex-col h-full bg-dark-bg/20 overflow-hidden">
      {/* Header del Editor */}
      <div className="h-14 border-b border-white/5 bg-dark-card/80 backdrop-blur-xl flex items-center justify-between px-6 z-10">
        <div className="flex items-center gap-4">
          <div className="h-8 w-px bg-white/10 mx-2" />
          {isSaving ? (
            <span className="flex items-center text-[10px] font-bold uppercase tracking-widest text-gray-500 bg-white/5 px-3 py-1.5 rounded-full border border-white/5">
              <Loader2 size={12} className="animate-spin mr-2" /> Sincronizando...
            </span>
          ) : (
            <span className="flex items-center text-[10px] font-bold uppercase tracking-widest text-emerald-400 bg-emerald-400/10 px-3 py-1.5 rounded-full border border-emerald-400/10 shadow-[0_0_15px_-5px_rgba(52,211,153,0.3)]">
              <Save size={12} className="mr-2" /> Borrador Local
            </span>
          )}
        </div>

        <div className="flex items-center gap-3">
           <Button variant="ghost" size="sm" className="h-9 px-4 text-brand-primary bg-brand-primary/5 hover:bg-brand-primary/15 border border-brand-primary/10">
             <Sparkles size={14} className="mr-2 text-brand-primary animate-pulse" />
             Optimizar con IA
           </Button>
           <Button size="sm" className="h-9 px-4 bg-white/10 hover:bg-white/20 text-white border border-white/10">
             Exportar PDF
           </Button>
        </div>
      </div>

      {/* Area de Trabajo con Toolbar Flotante o Fija */}
      <div className="flex-1 overflow-y-auto p-6 lg:p-10 scroll-smooth">
        <div className="max-w-4xl mx-auto space-y-6">
          <EditorToolbar editor={editor} />
          <EditorContent editor={editor} className="editor-container" />
        </div>
      </div>
    </div>
  );
}
