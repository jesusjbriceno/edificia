import { useEditorStore } from '@/store/useEditorStore';
import { ChevronRight, FileText, ChevronDown, Folder, Users } from 'lucide-react';
import { useState } from 'react';
import { cn } from '@/lib/utils';
import type { ContentTreeNode } from '@/lib/types';

interface SidebarNavigationProps {
  /** Filtered tree nodes to render. */
  tree: ContentTreeNode[];
}

export default function SidebarNavigation({ tree }: Readonly<SidebarNavigationProps>) {
  const { activeSectionId, setActiveSection } = useEditorStore();

  return (
    <div className="w-80 border-r border-white/5 bg-dark-bg/50 backdrop-blur-xl overflow-y-auto">
      <div className="p-4 border-b border-white/5">
        <h3 className="text-xs font-bold uppercase tracking-widest text-gray-500">
          Capítulos de la Memoria
        </h3>
      </div>
      <nav className="p-2 space-y-1">
        {tree.length === 0 ? (
          <p className="px-3 py-4 text-sm text-gray-500 italic">
            No hay capítulos disponibles.
          </p>
        ) : (
          tree.map((node) => (
            <TreeNode
              key={node.id}
              node={node}
              activeId={activeSectionId}
              onSelect={setActiveSection}
            />
          ))
        )}
      </nav>

      <div className="mt-8 p-4 border-t border-white/5">
        <h3 className="text-xs font-bold uppercase tracking-widest text-gray-500 mb-4 px-2">
          Administración
        </h3>
        <div className="space-y-1">
          <a
            href="/admin/projects"
            className="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-400 hover:text-white hover:bg-white/5 rounded-xl transition-all group"
          >
            <div className="p-2 bg-brand-primary/10 rounded-lg group-hover:bg-brand-primary/20 transition-colors">
              <Folder size={16} className="text-brand-primary" />
            </div>
            <span>Proyectos</span>
          </a>
          <a
            href="/admin/users"
            className="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-400 hover:text-white hover:bg-white/5 rounded-xl transition-all group"
          >
            <div className="p-2 bg-emerald-500/10 rounded-lg group-hover:bg-emerald-500/20 transition-colors">
              <Users size={16} className="text-emerald-500" />
            </div>
            <span>Usuarios</span>
          </a>
        </div>
      </div>
    </div>
  );
}

function TreeNode({
  node,
  activeId,
  onSelect,
}: Readonly<{
  node: ContentTreeNode;
  activeId: string | null;
  onSelect: (id: string) => void;
}>) {
  const [isOpen, setIsOpen] = useState(true);
  const hasChildren = node.sections.length > 0;
  const isActive = activeId === node.id;

  return (
    <div className="space-y-1">
      <button
        onClick={() => {
          if (hasChildren) setIsOpen(!isOpen);
          onSelect(node.id);
        }}
        className={cn(
          'w-full flex items-center gap-2 px-3 py-2 text-sm rounded-lg transition-all group',
          isActive
            ? 'bg-brand-primary/10 text-brand-primary font-medium'
            : 'text-gray-400 hover:text-white hover:bg-white/5',
        )}
      >
        {!hasChildren && <FileText size={14} className="ml-3.5" />}
        {hasChildren && isOpen && <ChevronDown size={14} />}
        {hasChildren && !isOpen && <ChevronRight size={14} />}
        <span className="truncate">{node.title}</span>
      </button>

      {hasChildren && isOpen && (
        <div className="ml-4 space-y-1 border-l border-white/5 pl-2">
          {node.sections.map((child) => (
            <TreeNode
              key={child.id}
              node={child}
              activeId={activeId}
              onSelect={onSelect}
            />
          ))}
        </div>
      )}
    </div>
  );
}
