import { ArrowLeft } from 'lucide-react';
import { useEditorStore } from '@/store/useEditorStore';
import { InterventionTypeLabels } from '@/lib/types';

export default function EditorHeader() {
  const { 
    projectId,
    projectTitle, 
    interventionType, 
  } = useEditorStore();

  const backHref = projectId ? `/projects/${projectId}` : '/dashboard';

  return (
    <header className="h-20 border-b border-white/5 bg-dark-bg flex items-center px-6 shrink-0 z-50 relative">
      <div className="flex items-center gap-6">
        {/* Back Button */}
        <a 
          href={backHref} 
          className="p-2 -ml-2 text-gray-400 hover:text-white transition-all group"
          title="Volver al detalle del proyecto"
        >
          <ArrowLeft size={22} className="group-hover:-translate-x-0.5 transition-transform" />
        </a>
        
        {/* Separator */}
        <div className="h-10 w-px bg-white/10"></div>

        {/* Project Info */}
        <div className="flex flex-col gap-0.5">
          <div className="flex items-center gap-3">
            {/* Logo/Title Placeholder - The sketch shows a stylized "fff" */}
            <h1 className="text-2xl font-black text-white tracking-tighter">
              {projectTitle || '---'}
            </h1>
            {interventionType !== null && (
              <span className="text-lg font-medium text-gray-500">
                ({InterventionTypeLabels[interventionType as any]})
              </span>
            )}
          </div>
          
          <p className="text-[10px] text-gray-500 font-bold uppercase tracking-[0.2em]">
            Memoria de proyecto de ejecución
          </p>
        </div>
      </div>
    </header>
  );
}
