import { useState } from "react";
import Modal from "@/components/ui/Modal";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { ChevronRight, ChevronLeft, Building2, HardHat, FileCheck } from "lucide-react";

interface ProjectWizardProps {
  isOpen: boolean;
  onClose: () => void;
}

export default function ProjectWizard({ isOpen, onClose }: ProjectWizardProps) {
  const [step, setStep] = useState(1);
  const [formData, setFormData] = useState({
    title: "",
    interventionType: "",
    localNormative: "",
  });

  const nextStep = () => setStep(s => s + 1);
  const prevStep = () => setStep(s => s - 1);

  return (
    <Modal 
      isOpen={isOpen} 
      onClose={onClose} 
      title={`Nuevo Proyecto - Paso ${step} de 3`}
    >
      <div className="space-y-8">
        {step === 1 && (
          <div className="space-y-6 animate-in slide-in-from-right-4 duration-300">
            <div className="space-y-2">
              <h4 className="text-lg font-medium text-white">Datos Básicos</h4>
              <p className="text-sm text-gray-400">Introduce el nombre y la referencia catastral del proyecto.</p>
            </div>
            <div className="space-y-4">
              <Input 
                label="Título del Proyecto" 
                placeholder="Ej: Residencial Los Almendros" 
                value={formData.title}
                onChange={e => setFormData({...formData, title: e.target.value})}
              />
              <Input label="Referencia Catastral (Opcional)" placeholder="20 caracteres alfanuméricos" />
            </div>
          </div>
        )}

        {step === 2 && (
          <div className="space-y-6 animate-in slide-in-from-right-4 duration-300">
            <div className="space-y-2">
              <h4 className="text-lg font-medium text-white">Tipo de Intervención</h4>
              <p className="text-sm text-gray-400">¿Qué tipo de obra se va a realizar?</p>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <button 
                onClick={() => setFormData({...formData, interventionType: 'New'})}
                className={`p-6 rounded-xl border transition-all text-left space-y-3 ${formData.interventionType === 'New' ? 'border-brand-primary bg-brand-primary/5' : 'border-white/5 bg-white/5 hoverline:border-white/10'}`}
              >
                <div className="p-2 bg-brand-primary/10 rounded-lg w-fit text-brand-primary">
                  <Building2 size={24} />
                </div>
                <div className="font-semibold text-white text-lg">Obra Nueva</div>
              </button>
              <button 
                onClick={() => setFormData({...formData, interventionType: 'Reform'})}
                className={`p-6 rounded-xl border transition-all text-left space-y-3 ${formData.interventionType === 'Reform' ? 'border-brand-primary bg-brand-primary/5' : 'border-white/5 bg-white/5 hover:border-white/10'}`}
              >
                <div className="p-2 bg-emerald-500/10 rounded-lg w-fit text-emerald-500">
                  <HardHat size={24} />
                </div>
                <div className="font-semibold text-white text-lg">Reforma</div>
              </button>
            </div>
          </div>
        )}

        {step === 3 && (
          <div className="space-y-6 animate-in slide-in-from-right-4 duration-300">
            <div className="space-y-2">
              <h4 className="text-lg font-medium text-white">Normativa y LOE</h4>
              <p className="text-sm text-gray-400">Configuración final del árbol de normativa.</p>
            </div>
            <div className="space-y-4">
              <div className="p-4 bg-white/5 rounded-xl border border-white/5 flex items-start gap-4">
                <FileCheck className="text-brand-primary shrink-0" size={24} />
                <div>
                  <p className="text-sm text-white font-medium">Auto-seleccionar CTE 2024</p>
                  <p className="text-xs text-gray-500">La aplicación cargará automáticamente los capítulos obligatorios según el Código Técnico de la Edificación.</p>
                </div>
              </div>
              <Input label="Municipio / Normativa Local" placeholder="Ej: Madrid, Barcelona..." />
            </div>
          </div>
        )}

        <div className="flex items-center justify-between pt-4 border-t border-white/5">
          <Button 
            variant="ghost" 
            onClick={step === 1 ? onClose : prevStep}
          >
            {step === 1 ? 'Cancelar' : <><ChevronLeft size={18} className="mr-1" /> Anterior</>}
          </Button>
          
          <Button onClick={step === 3 ? onClose : nextStep}>
            {step === 3 ? 'Crear Proyecto' : <>Siguiente <ChevronRight size={18} className="ml-1" /></>}
          </Button>
        </div>
      </div>
    </Modal>
  );
}
