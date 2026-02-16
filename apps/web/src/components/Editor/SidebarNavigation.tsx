import { useEditorStore } from "@/store/useEditorStore";
import { ChevronRight, FileText, ChevronDown } from "lucide-react";
import { useState } from "react";
import { cn } from "@/lib/utils";

interface Node {
  id: string;
  title: string;
  children?: Node[];
}

const treeData: Node[] = [
  {
    id: "1",
    title: "1. Memoria Descriptiva",
    children: [
      { id: "1.1", title: "1.1 Agentes" },
      { id: "1.2", title: "1.2 Información previa" },
      { id: "1.3", title: "1.3 Descripción del proyecto" },
    ],
  },
  {
    id: "2",
    title: "2. Memoria Constructiva",
    children: [
      { id: "2.1", title: "2.1 Sustentación del edificio" },
      { id: "2.2", title: "2.2 Sistema envolvente" },
    ],
  },
];

export default function SidebarNavigation() {
  const { activeSectionId, setActiveSection } = useEditorStore();

  return (
    <div className="w-80 border-r border-white/5 bg-dark-bg/50 backdrop-blur-xl overflow-y-auto">
      <div className="p-4 border-b border-white/5">
        <h3 className="text-xs font-bold uppercase tracking-widest text-gray-500">Capítulos de la Memoria</h3>
      </div>
      <nav className="p-2 space-y-1">
        {treeData.map((node) => (
          <TreeNode 
            key={node.id} 
            node={node} 
            activeId={activeSectionId} 
            onSelect={setActiveSection} 
          />
        ))}
      </nav>
    </div>
  );
}

function TreeNode({ node, activeId, onSelect }: { node: Node, activeId: string | null, onSelect: (id: string) => void }) {
  const [isOpen, setIsOpen] = useState(true);
  const hasChildren = node.children && node.children.length > 0;
  const isActive = activeId === node.id;

  return (
    <div className="space-y-1">
      <button
        onClick={() => {
          if (hasChildren) setIsOpen(!isOpen);
          onSelect(node.id);
        }}
        className={cn(
          "w-full flex items-center gap-2 px-3 py-2 text-sm rounded-lg transition-all group",
          isActive ? "bg-brand-primary/10 text-brand-primary font-medium" : "text-gray-400 hover:text-white hover:bg-white/5"
        )}
      >
        {hasChildren ? (
          isOpen ? <ChevronDown size={14} /> : <ChevronRight size={14} />
        ) : (
          <FileText size={14} className="ml-3.5" />
        )}
        <span className="truncate">{node.title}</span>
      </button>

      {hasChildren && isOpen && (
        <div className="ml-4 space-y-1 border-l border-white/5 pl-2">
          {node.children!.map((child) => (
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
