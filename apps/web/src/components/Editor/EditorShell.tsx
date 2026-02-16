import { useEditor(EditorContent) } from '@tiptap/react';
import StarterKit from '@tiptap/starter-kit';
import Underline from '@tiptap/extension-underline';
import { useEditorStore } from '@/store/useEditorStore';
import { useEffect } from 'react';
import { Loader2, Sparkles, Save } from 'lucide-react';
import { Button } from '@/components/ui/Button';

export default function EditorShell() {
  const { activeSectionId, content, updateContent, isSaving } = useEditorStore();

  const editor = useEditor({
    extensions: [
      StarterKit,
      Underline,
    ],
    content: activeSectionId ? content[activeSectionId] || '<p>Selecciona una sección para comenzar a redactar...</p>' : '<p>Selecciona una sección...</p>',
    editorProps: {
      attributes: {
        class: 'prose prose-invert prose-sm sm:prose-base lg:prose-lg xl:prose-2xl focus:outline-none max-w-none min-h-[500px] p-8',
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
      editor.commands.setContent(content[activeSectionId] || '');
    }
  }, [activeSectionId, editor]);

  if (!activeSectionId) {
    return (
      <div className="flex-1 flex flex-col items-center justify-center text-gray-500 space-y-4 bg-dark-bg/20">
        <div className="p-4 bg-white/5 rounded-full">
          <FileText size={48} className="opacity-20" />
        </div>
        <p className="text-sm">Selecciona un capítulo de la barra lateral para empezar a redactar.</p>
      </div>
    );
  }

  return (
    <div className="flex-1 flex flex-col h-full bg-dark-bg/20 overflow-hidden">
      <!-- Toolbar Superior -->
      <div className="h-12 border-b border-white/5 bg-dark-card/50 backdrop-blur-md flex items-center justify-between px-6">
        <div className="flex items-center gap-2">
          {isSaving ? (
            <span className="flex items-center text-[10px] uppercase tracking-widest text-gray-500">
              <Loader2 size={12} className="animate-spin mr-2" /> Guardando...
            </span>
          ) : (
            <span className="flex items-center text-[10px] uppercase tracking-widest text-emerald-500">
              <Save size={12} className="mr-2" /> Guardado en local
            </span>
          )}
        </div>

        <div className="flex items-center gap-4">
           <Button variant="ghost" size="sm" className="h-8 text-brand-primary bg-brand-primary/10 hover:bg-brand-primary/20">
             <Sparkles size={14} className="mr-2" />
             Asistente IA
           </Button>
        </div>
      </div>

      <!-- Editor Content -->
      <div className="flex-1 overflow-y-auto">
        <EditorContent editor={editor} />
      </div>
    </div>
  );
}
