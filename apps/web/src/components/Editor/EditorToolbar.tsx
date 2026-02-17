import { type Editor } from '@tiptap/react';
import { 
  Bold, Italic, Underline, 
  List, ListOrdered, Quote, 
  Heading1, Heading2, Heading3, 
  Undo, Redo, Strikethrough
} from 'lucide-react';
import { cn } from '@/lib/utils';

interface EditorToolbarProps {
  editor: Editor | null;
}

export function EditorToolbar({ editor }: EditorToolbarProps) {
  if (!editor) return null;

  const toggleAction = (action: () => boolean, isActive: boolean) => (
    <button
      type="button"
      onClick={() => action()}
      className={cn(
        "p-2 rounded-lg transition-all duration-200",
        isActive 
          ? "bg-brand-primary/20 text-brand-primary shadow-[0_0_15px_-3px_rgba(var(--brand-primary-rgb),0.3)]" 
          : "text-gray-400 hover:bg-white/5 hover:text-white"
      )}
    >
      {/* Iconos se pasan dinámicamente o se definen aquí */}
    </button>
  );

  const tools = [
    { icon: Bold, action: () => editor.chain().focus().toggleBold().run(), active: editor.isActive('bold'), label: 'Negrita' },
    { icon: Italic, action: () => editor.chain().focus().toggleItalic().run(), active: editor.isActive('italic'), label: 'Cursiva' },
    { icon: Underline, action: () => editor.chain().focus().toggleUnderline().run(), active: editor.isActive('underline'), label: 'Subrayado' },
    { icon: Strikethrough, action: () => editor.chain().focus().toggleStrike().run(), active: editor.isActive('strike'), label: 'Tachado' },
    { separator: true },
    { icon: Heading1, action: () => editor.chain().focus().toggleHeading({ level: 1 }).run(), active: editor.isActive('heading', { level: 1 }), label: 'H1' },
    { icon: Heading2, action: () => editor.chain().focus().toggleHeading({ level: 2 }).run(), active: editor.isActive('heading', { level: 2 }), label: 'H2' },
    { icon: Heading3, action: () => editor.chain().focus().toggleHeading({ level: 3 }).run(), active: editor.isActive('heading', { level: 3 }), label: 'H3' },
    { separator: true },
    { icon: List, action: () => editor.chain().focus().toggleBulletList().run(), active: editor.isActive('bulletList'), label: 'Lista' },
    { icon: ListOrdered, action: () => editor.chain().focus().toggleOrderedList().run(), active: editor.isActive('orderedList'), label: 'Lista Ordenada' },
    { icon: Quote, action: () => editor.chain().focus().toggleBlockquote().run(), active: editor.isActive('blockquote'), label: 'Cita' },
    { separator: true },
    { icon: Undo, action: () => editor.chain().focus().undo().run(), disabled: !editor.can().undo(), label: 'Deshacer' },
    { icon: Redo, action: () => editor.chain().focus().redo().run(), disabled: !editor.can().redo(), label: 'Rehacer' },
  ];

  return (
    <div className="flex items-center gap-1 p-1 bg-white/5 rounded-xl border border-white/5 mb-4 overflow-x-auto no-scrollbar">
      {tools.map((tool, i) => {
        if ('separator' in tool) {
          return <div key={i} className="w-px h-4 bg-white/10 mx-1" />;
        }

        const Icon = tool.icon;
        return (
          <button
            key={i}
            type="button"
            title={tool.label}
            onClick={tool.action}
            disabled={'disabled' in tool ? tool.disabled : false}
            className={cn(
              "p-2 rounded-lg transition-all duration-200 flex items-center justify-center min-w-[36px]",
              'active' in tool && tool.active 
                ? "bg-brand-primary text-white shadow-lg shadow-brand-primary/20 scale-105" 
                : "text-gray-400 opt-hover:bg-white/5 hover:text-white"
            )}
          >
            <Icon size={18} />
          </button>
        );
      })}
    </div>
  );
}
