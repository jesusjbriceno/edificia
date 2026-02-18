import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import Modal from '@/components/ui/Modal';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import {
  ChevronRight,
  ChevronLeft,
  Building2,
  HardHat,
  Maximize2,
  FileCheck,
  AlertCircle,
  Loader2,
} from 'lucide-react';
import { InterventionType, InterventionTypeLabels } from '@/lib/types';
import type { CreateProjectRequest, ProjectResponse } from '@/lib/types';
import { projectService } from '@/lib/services';
import { ApiError } from '@/lib/api';

// ─── Zod Schema ──────────────────────────────────────────
const wizardSchema = z.object({
  title: z.string().min(1, 'El título del proyecto es obligatorio'),
  cadastralReference: z.string().max(20, 'Máximo 20 caracteres').optional().or(z.literal('')),
  description: z.string().optional().or(z.literal('')),
  address: z.string().optional().or(z.literal('')),
  interventionType: z.number().min(0).max(2, 'Selecciona un tipo de intervención'),
  isLoeRequired: z.boolean(),
  localRegulations: z.string().optional().or(z.literal('')),
});

type WizardFormData = z.infer<typeof wizardSchema>;

// ─── Types ───────────────────────────────────────────────
interface ProjectWizardProps {
  isOpen: boolean;
  onClose: () => void;
  onCreated?: (project: ProjectResponse) => void;
}

const INTERVENTION_OPTIONS = [
  {
    value: InterventionType.NewConstruction,
    label: InterventionTypeLabels[InterventionType.NewConstruction],
    icon: Building2,
    color: 'brand-primary',
    bgClass: 'bg-brand-primary/10 text-brand-primary',
  },
  {
    value: InterventionType.Reform,
    label: InterventionTypeLabels[InterventionType.Reform],
    icon: HardHat,
    color: 'emerald-500',
    bgClass: 'bg-emerald-500/10 text-emerald-500',
  },
  {
    value: InterventionType.Extension,
    label: InterventionTypeLabels[InterventionType.Extension],
    icon: Maximize2,
    color: 'violet-500',
    bgClass: 'bg-violet-500/10 text-violet-500',
  },
] as const;

// ─── Component ───────────────────────────────────────────
export default function ProjectWizard({ isOpen, onClose, onCreated }: ProjectWizardProps) {
  const [step, setStep] = useState(1);
  const [apiError, setApiError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const {
    register,
    setValue,
    watch,
    trigger,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<WizardFormData>({
    resolver: zodResolver(wizardSchema),
    defaultValues: {
      title: '',
      cadastralReference: '',
      description: '',
      address: '',
      interventionType: -1,
      isLoeRequired: true,
      localRegulations: '',
    },
  });

  const selectedType = watch('interventionType');
  const isLoeRequired = watch('isLoeRequired');

  const handleClose = () => {
    reset();
    setStep(1);
    setApiError(null);
    onClose();
  };

  const nextStep = async () => {
    if (step === 1) {
      const valid = await trigger(['title', 'cadastralReference', 'description', 'address']);
      if (!valid) return;
    }
    if (step === 2) {
      const valid = await trigger('interventionType');
      if (!valid) return;
    }
    setStep(s => s + 1);
  };

  const prevStep = () => setStep(s => s - 1);

  const onSubmit = async (data: WizardFormData) => {
    setApiError(null);
    setIsSubmitting(true);
    try {
      const payload: CreateProjectRequest = {
        title: data.title,
        interventionType: data.interventionType as InterventionType,
        isLoeRequired: data.isLoeRequired,
        description: data.description || null,
        address: data.address || null,
        cadastralReference: data.cadastralReference || null,
        localRegulations: data.localRegulations || null,
      };
      const created = await projectService.create(payload);
      onCreated?.(created);
      handleClose();
    } catch (err) {
      if (err instanceof ApiError) {
        setApiError(err.message);
      } else {
        setApiError('Error inesperado al crear el proyecto.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title={`Nuevo Proyecto — Paso ${step} de 3`}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
        {/* ── Step 1: Datos Básicos ─────────────────────── */}
        {step === 1 && (
          <div className="space-y-6 animate-in slide-in-from-right-4 duration-300">
            <div className="space-y-2">
              <h4 className="text-lg font-medium text-white">Datos Básicos</h4>
              <p className="text-sm text-gray-400">
                Introduce el nombre, la dirección y la referencia catastral del proyecto.
              </p>
            </div>
            <div className="space-y-4">
              <div className="space-y-2">
                <Input
                  label="Título del Proyecto *"
                  placeholder="Ej: Residencial Los Almendros"
                  {...register('title')}
                />
                {errors.title && (
                  <p className="text-xs text-red-500">{errors.title.message}</p>
                )}
              </div>
              <div className="space-y-2">
                <Input
                  label="Descripción (Opcional)"
                  placeholder="Breve descripción del alcance del proyecto"
                  {...register('description')}
                />
              </div>
              <div className="space-y-2">
                <Input
                  label="Dirección (Opcional)"
                  placeholder="Ej: Calle Mayor 12, Madrid"
                  {...register('address')}
                />
              </div>
              <div className="space-y-2">
                <Input
                  label="Referencia Catastral (Opcional)"
                  placeholder="20 caracteres alfanuméricos"
                  {...register('cadastralReference')}
                />
                {errors.cadastralReference && (
                  <p className="text-xs text-red-500">{errors.cadastralReference.message}</p>
                )}
              </div>
            </div>
          </div>
        )}

        {/* ── Step 2: Tipo de Intervención ─────────────── */}
        {step === 2 && (
          <div className="space-y-6 animate-in slide-in-from-right-4 duration-300">
            <div className="space-y-2">
              <h4 className="text-lg font-medium text-white">Tipo de Intervención</h4>
              <p className="text-sm text-gray-400">¿Qué tipo de obra se va a realizar?</p>
            </div>
            <div className="grid grid-cols-3 gap-4">
              {INTERVENTION_OPTIONS.map(opt => {
                const Icon = opt.icon;
                const isSelected = selectedType === opt.value;
                return (
                  <button
                    key={opt.value}
                    type="button"
                    onClick={() => setValue('interventionType', opt.value, { shouldValidate: true })}
                    className={`p-5 rounded-xl border transition-all text-left space-y-3 ${
                      isSelected
                        ? 'border-brand-primary bg-brand-primary/5'
                        : 'border-white/5 bg-white/5 hover:border-white/10'
                    }`}
                  >
                    <div className={`p-2 rounded-lg w-fit ${opt.bgClass}`}>
                      <Icon size={24} />
                    </div>
                    <div className="font-semibold text-white">{opt.label}</div>
                  </button>
                );
              })}
            </div>
            {errors.interventionType && (
              <p className="text-xs text-red-500">{errors.interventionType.message}</p>
            )}
          </div>
        )}

        {/* ── Step 3: Normativa y LOE ──────────────────── */}
        {step === 3 && (
          <div className="space-y-6 animate-in slide-in-from-right-4 duration-300">
            <div className="space-y-2">
              <h4 className="text-lg font-medium text-white">Normativa y LOE</h4>
              <p className="text-sm text-gray-400">Configuración del árbol de normativa.</p>
            </div>
            <div className="space-y-4">
              <div className="p-4 bg-white/5 rounded-xl border border-white/5 flex items-start gap-4">
                <FileCheck className="text-brand-primary shrink-0" size={24} />
                <div>
                  <p className="text-sm text-white font-medium">CTE 2024 — Auto-seleccionado</p>
                  <p className="text-xs text-gray-500">
                    Se cargarán los capítulos obligatorios según el Código Técnico de la Edificación.
                  </p>
                </div>
              </div>

              {/* LOE toggle */}
              <label className="flex items-center justify-between p-4 bg-white/5 rounded-xl border border-white/5 cursor-pointer hover:border-white/10 transition-colors">
                <div>
                  <p className="text-sm text-white font-medium">¿Aplica LOE?</p>
                  <p className="text-xs text-gray-500">
                    La Ley de Ordenación de la Edificación se aplica por defecto en Obra Nueva y Ampliación.
                  </p>
                </div>
                <input
                  type="checkbox"
                  checked={isLoeRequired}
                  onChange={e => setValue('isLoeRequired', e.target.checked)}
                  className="w-5 h-5 rounded border-white/20 bg-white/5 text-brand-primary focus:ring-brand-primary focus:ring-offset-0"
                />
              </label>

              <div className="space-y-2">
                <Input
                  label="Normativa Local (Opcional)"
                  placeholder="Ej: PGOU Madrid, POUM Barcelona..."
                  {...register('localRegulations')}
                />
              </div>
            </div>
          </div>
        )}

        {/* ── API Error ────────────────────────────────── */}
        {apiError && (
          <div className="flex items-center gap-2 p-3 bg-red-500/10 border border-red-500/20 rounded-lg text-red-400 text-sm">
            <AlertCircle size={16} className="shrink-0" />
            {apiError}
          </div>
        )}

        {/* ── Navigation ───────────────────────────────── */}
        <div className="flex items-center justify-between pt-4 border-t border-white/5">
          <Button type="button" variant="ghost" onClick={step === 1 ? handleClose : prevStep}>
            {step === 1 ? (
              'Cancelar'
            ) : (
              <>
                <ChevronLeft size={18} className="mr-1" /> Anterior
              </>
            )}
          </Button>

          {step < 3 ? (
            <Button type="button" onClick={nextStep}>
              Siguiente <ChevronRight size={18} className="ml-1" />
            </Button>
          ) : (
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? (
                <>
                  <Loader2 size={18} className="mr-2 animate-spin" /> Creando…
                </>
              ) : (
                'Crear Proyecto'
              )}
            </Button>
          )}
        </div>
      </form>
    </Modal>
  );
}
