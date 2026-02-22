import Modal from '@/components/ui/Modal';
import { Button } from '@/components/ui/Button';
import { Calendar, MapPin, Hash, BookOpen, Clock, FileText } from 'lucide-react';
import type { ProjectResponse } from '@/lib/types';
import { InterventionTypeStringLabels, ProjectStatusLabels } from '@/lib/types';

interface ProjectDetailsModalProps {
  isOpen: boolean;
  onClose: () => void;
  project: ProjectResponse | null;
  onCompleteMemory: (project: ProjectResponse) => void;
}

const DetailItem = ({ icon: Icon, label, value }: { icon: any, label: string, value: string | null | undefined }) => (
  <div className="flex gap-4 p-4 rounded-xl bg-white/5 border border-white/5">
    <div className="p-2 h-fit bg-brand-primary/10 rounded-lg text-brand-primary">
      <Icon size={18} />
    </div>
    <div>
      <p className="text-xs text-gray-500 uppercase tracking-widest font-medium mb-1">{label}</p>
      <p className="text-white font-medium">{value || 'No especificado'}</p>
    </div>
  </div>
);

export function ProjectDetailsModal({
  isOpen,
  onClose,
  project,
  onCompleteMemory
}: ProjectDetailsModalProps) {
  if (!project) return null;

  const typeLabel =
    InterventionTypeStringLabels[project.interventionType] ??
    project.interventionType;
  
  const statusLabel = ProjectStatusLabels[project.status as keyof typeof ProjectStatusLabels] ?? project.status;

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Detalles del Proyecto" className="max-w-3xl">
      <div className="space-y-6">
        {/* Header Summary */}
        <div className="flex justify-between items-start gap-4">
          <div>
            <h2 className="text-2xl font-bold text-white mb-2">{project.title}</h2>
            <p className="text-gray-400 leading-relaxed">
              {project.description || 'Sin descripción adicional.'}
            </p>
          </div>
          <div className="px-3 py-1 rounded-full bg-brand-primary/20 text-brand-primary border border-brand-primary/20 text-xs font-bold uppercase overflow-hidden whitespace-nowrap">
            {statusLabel}
          </div>
        </div>

        {/* Grid of Details */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <DetailItem 
            icon={BookOpen} 
            label="Tipo de Intervención" 
            value={typeLabel} 
          />
          <DetailItem 
            icon={Calendar} 
            label="Fecha de Creación" 
            value={new Date(project.createdAt).toLocaleDateString('es-ES', { day: '2-digit', month: 'long', year: 'numeric' })} 
          />
          <DetailItem 
            icon={MapPin} 
            label="Ubicación / Dirección" 
            value={project.address} 
          />
          <DetailItem 
            icon={Hash} 
            label="Referencia Catastral" 
            value={project.cadastralReference} 
          />
          <DetailItem 
            icon={Clock} 
            label="Última Actualización" 
            value={project.updatedAt ? new Date(project.updatedAt).toLocaleDateString('es-ES', { day: '2-digit', month: 'long', year: 'numeric' }) : null} 
          />
          <DetailItem 
            icon={FileText} 
            label="Normativa Aplicable" 
            value={project.localRegulations} 
          />
        </div>

        {/* Action area */}
        <div className="pt-6 border-t border-white/5 flex gap-4">
          <Button 
            onClick={() => onCompleteMemory(project)} 
            className="flex-1 h-12 gap-2"
          >
            <FileText size={18} />
            Continuar con la Memoria
          </Button>
          <Button variant="outline" onClick={onClose} className="h-12 px-8">
            Cerrar
          </Button>
        </div>
      </div>
    </Modal>
  );
}
