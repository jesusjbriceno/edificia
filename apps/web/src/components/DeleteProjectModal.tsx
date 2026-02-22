import { useState, useEffect } from 'react';
import Modal from '@/components/ui/Modal';
import { Button } from '@/components/ui/Button';
import { AlertTriangle, Trash2 } from 'lucide-react';
import type { ProjectResponse } from '@/lib/types';

interface DeleteProjectModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: (project: ProjectResponse) => void;
  project: ProjectResponse | null;
  isLoading?: boolean;
}

export function DeleteProjectModal({
  isOpen,
  onClose,
  onConfirm,
  project,
  isLoading
}: DeleteProjectModalProps) {
  const [confirmationName, setConfirmationName] = useState('');

  // Reset confirmation field when modal opens
  useEffect(() => {
    if (isOpen) {
      setConfirmationName('');
    }
  }, [isOpen]);

  if (!project) return null;

  const isConfirmed = confirmationName === project.title;

  const handleConfirm = () => {
    if (isConfirmed) {
      onConfirm(project);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="¿Eliminar Proyecto?" className="max-w-md">
      <div className="space-y-6">
        <div className="flex gap-4 p-4 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400">
          <AlertTriangle className="shrink-0" size={24} />
          <div className="space-y-1">
            <p className="font-bold">Acción irreversible</p>
            <p className="text-sm">Se eliminarán todos los datos y la memoria asociada a este proyecto. No podrás deshacer esta acción.</p>
          </div>
        </div>

        <div className="space-y-3">
          <p className="text-sm text-gray-400">
            Para confirmar, escribe <span className="text-white font-bold select-all">{project.title}</span> a continuación:
          </p>
          <input
            type="text"
            value={confirmationName}
            onChange={(e) => setConfirmationName(e.target.value)}
            className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-white focus:border-red-500 outline-none transition-colors"
            placeholder="Introduce el nombre del proyecto"
          />
        </div>

        <div className="flex gap-3">
          <Button 
            variant="outline" 
            onClick={onClose} 
            className="flex-1 h-12"
            disabled={isLoading}
          >
            Cancelar
          </Button>
          <Button 
            onClick={handleConfirm} 
            disabled={!isConfirmed || isLoading}
            isLoading={isLoading}
            className="flex-1 h-12 bg-red-600 hover:bg-red-700 text-white border-transparent"
          >
            <Trash2 size={18} className="mr-2" />
            Eliminar Proyecto
          </Button>
        </div>
      </div>
    </Modal>
  );
}
