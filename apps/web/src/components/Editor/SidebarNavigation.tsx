import { useEditorStore } from '@/store/useEditorStore';
import { ChevronRight, FileText, ChevronDown, Folder, Users, Search } from 'lucide-react';
import { useState, useMemo } from 'react';
import { cn } from '@/lib/utils';
import type { ContentTreeNode } from '@/lib/types';

interface SidebarNavigationProps {
  /** Original tree nodes to render. */
  tree: ContentTreeNode[];
}

/** Recursively filters the tree based on title search. */
function searchTree(nodes: ContentTreeNode[], query: string): ContentTreeNode[] {
  if (!query) return nodes;
  
  const lowerQuery = query.toLowerCase();
  
  return nodes.reduce<ContentTreeNode[]>((acc, node) => {
    const matchesQuery = node.title.toLowerCase().includes(lowerQuery);
    const filteredSections = searchTree(node.sections, query);
    
    if (matchesQuery || filteredSections.length > 0) {
      acc.push({
        ...node,
        sections: filteredSections
      });
    }
    return acc;
  }, []);
}


export default function SidebarNavigation({ tree }: Readonly<SidebarNavigationProps>) {
  const { activeSectionId, setActiveSection } = useEditorStore();
  const [searchQuery, setSearchQuery] = useState('');

  const filteredTree = useMemo(() => searchTree(tree, searchQuery), [tree, searchQuery]);

  return (
    <div className="w-full h-full border-r border-white/5 bg-dark-bg/50 backdrop-blur-xl flex flex-col overflow-hidden">
      {/* Search area */}
      <div className="p-4 space-y-3 border-b border-white/5">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={14} />
          <input
            type="text"
            placeholder="Buscar apartado..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full bg-white/5 border border-white/10 rounded-xl py-2 pl-9 pr-4 text-xs text-white placeholder:text-gray-600 focus:outline-none focus:ring-1 focus:ring-brand-primary/50 transition-all"
          />
        </div>
      </div>

      <div className="flex-1 overflow-y-auto custom-scrollbar">
        <div className="p-4 border-b border-white/5">
          <h3 className="text-[10px] font-bold uppercase tracking-widest text-gray-600">
            Capítulos de la Memoria
          </h3>
        </div>
        <nav className="p-2 space-y-1">
          {filteredTree.length === 0 ? (
            <p className="px-3 py-6 text-xs text-gray-500 italic text-center">
              No se han encontrado apartados.
            </p>
          ) : (
            filteredTree.map((node) => (
              <TreeNode
                key={node.id}
                node={node}
                activeId={activeSectionId}
                onSelect={setActiveSection}
                autoOpen={searchQuery !== ''}
              />
            ))
          )}
        </nav>
      </div>

      {/* Basic Admin links relegated to bottom */}
      <div className="p-3 border-t border-white/5 bg-white/[0.02]">
        <div className="flex items-center gap-2">
          <a
            href="/admin/projects"
            className="flex-1 flex items-center justify-center p-2 text-gray-500 hover:text-white hover:bg-white/5 rounded-lg transition-all"
            title="Proyectos"
          >
            <Folder size={16} />
          </a>
          <a
            href="/admin/users"
            className="flex-1 flex items-center justify-center p-2 text-gray-500 hover:text-white hover:bg-white/5 rounded-lg transition-all"
            title="Usuarios"
          >
            <Users size={16} />
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
  autoOpen = false,
}: Readonly<{
  node: ContentTreeNode;
  activeId: string | null;
  onSelect: (id: string) => void;
  autoOpen?: boolean;
}>) {
  const [isOpen, setIsOpen] = useState(true);
  const hasChildren = node.sections.length > 0;
  const isActive = activeId === node.id;

  // Si estamos buscando, forzamos que esté abierto para mostrar resultados
  const effectiveOpen = autoOpen || isOpen;

  return (
    <div className="space-y-1">
      <button
        onClick={() => {
          if (hasChildren) setIsOpen(!isOpen);
          onSelect(node.id);
        }}
        className={cn(
          'w-full flex items-start gap-2 px-3 py-2 text-xs rounded-lg transition-all group text-left',
          isActive
            ? 'bg-brand-primary/10 text-brand-primary font-medium'
            : 'text-gray-400 hover:text-white hover:bg-white/5',
        )}
      >
        <div className="mt-0.5 shrink-0">
          {!hasChildren && <FileText size={14} className="ml-3.5" />}
          {hasChildren && effectiveOpen && <ChevronDown size={14} />}
          {hasChildren && !effectiveOpen && <ChevronRight size={14} />}
        </div>
        <span className="leading-relaxed break-words">{node.title}</span>
      </button>

      {hasChildren && effectiveOpen && (
        <div className="ml-4 space-y-1 border-l border-white/5 pl-2">
          {node.sections.map((child) => (
            <TreeNode
              key={child.id}
              node={child}
              activeId={activeId}
              onSelect={onSelect}
              autoOpen={autoOpen}
            />
          ))}
        </div>
      )}
    </div>
  );
}
