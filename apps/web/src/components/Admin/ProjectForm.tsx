import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '@/components/ui/Button';
import { Select } from '@/components/ui/Select';
import { AlertCircle } from 'lucide-react';
import { InterventionType, InterventionTypeLabels } from '@/lib/types';
import type { CreateProjectRequest, ProjectResponse } from '@/lib/types';
import { projectService } from '@/lib/services';
import { ApiError } from '@/lib/api';

const projectSchema = z.object({
  title: z.string().min(1, 'El título del proyecto es obligatorio'),
  description: z.string().optional().or(z.literal('')),
  address: z.string().optional().or(z.literal('')),
  interventionType: z.coerce.number().min(0).max(2, 'Selecciona un tipo válido'),
  isLoeRequired: z.boolean(),
  cadastralReference: z.string().max(20, 'Máximo 20 caracteres').optional().or(z.literal('')),
  localRegulations: z.string().optional().or(z.literal('')),
});

type ProjectFormData = z.infer<typeof projectSchema>;

interface ProjectFormProps {
  onSubmit: () => void;
  project?: ProjectResponse;
  isLoading?: boolean;
}

export function ProjectForm({ onSubmit, project, isLoading: externalLoading }: Readonly<ProjectFormProps>) {
  const [apiError, setApiError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ProjectFormData>({
    resolver: zodResolver(projectSchema),
    defaultValues: {
      title: project?.title || '',
      description: project?.description || '',
      address: project?.address || '',
      interventionType: project?.interventionType !== undefined 
        ? Number(project.interventionType) 
        : InterventionType.NewConstruction,
      isLoeRequired: project?.isLoeRequired ?? true,
      cadastralReference: project?.cadastralReference || '',
      localRegulations: project?.localRegulations || '',
    },
  });

  const handleFormSubmit = async (data: ProjectFormData) => {
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

      if (project?.id) {
        await projectService.update(project.id, payload);
      } else {
        await projectService.create(payload);
      }
      
      onSubmit();
    } catch (err) {
      if (err instanceof ApiError) {
        setApiError(err.message);
      } else {
        setApiError('Error inesperado al guardar el proyecto.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const loading = externalLoading || isSubmitting;

  return (
    <form
      onSubmit={handleSubmit(handleFormSubmit)}
      className="space-y-4 bg-dark-card/50 p-6 rounded-xl border border-white/5 mx-auto max-w-lg"
      data-testid="project-form"
    >
      <div className="space-y-2">
        <label htmlFor="title" className="text-sm font-medium text-gray-300">
          Título del Proyecto *
        </label>
        <input
          id="title"
          {...register('title')}
          className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-white focus:border-brand-primary outline-none transition-colors"
          placeholder="Ej: Reforma Planta 3 - Sede Central"
        />
        {errors.title && (
          <p className="text-xs text-red-500">{errors.title.message}</p>
        )}
      </div>

      <div className="space-y-2">
        <label htmlFor="description" className="text-sm font-medium text-gray-300">
          Descripción (Opcional)
        </label>
        <textarea
          id="description"
          {...register('description')}
          className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-white focus:border-brand-primary outline-none transition-colors min-h-20"
          placeholder="Breve descripción del alcance del proyecto."
        />
      </div>

      <div className="space-y-2">
        <label htmlFor="address" className="text-sm font-medium text-gray-300">
          Dirección (Opcional)
        </label>
        <input
          id="address"
          {...register('address')}
          className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-white focus:border-brand-primary outline-none transition-colors"
          placeholder="Ej: Calle Mayor 12, Madrid"
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-2">
          <Select
            id="interventionType"
            label="Tipo de Intervención"
            options={Object.entries(InterventionTypeLabels).map(([value, label]) => ({
              value,
              label,
            }))}
            {...register('interventionType')}
            error={errors.interventionType?.message}
          />

        </div>

        <div className="space-y-2">
          <label htmlFor="cadastralReference" className="text-sm font-medium text-gray-300">
            Ref. Catastral (Opcional)
          </label>
          <input
            id="cadastralReference"
            {...register('cadastralReference')}
            className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-white focus:border-brand-primary outline-none transition-colors"
            placeholder="20 caracteres"
          />
          {errors.cadastralReference && (
            <p className="text-xs text-red-500">{errors.cadastralReference.message}</p>
          )}
        </div>
      </div>

      <div className="flex items-center gap-3 p-3 bg-white/5 rounded-lg border border-white/5">
        <input
          type="checkbox"
          id="isLoeRequired"
          {...register('isLoeRequired')}
          className="w-4 h-4 rounded border-white/20 bg-white/5 text-brand-primary focus:ring-brand-primary focus:ring-offset-0"
        />
        <label htmlFor="isLoeRequired" className="text-sm text-gray-300 cursor-pointer">
          ¿Aplica LOE? (Ley de Ordenación de la Edificación)
        </label>
      </div>

      <div className="space-y-2">
        <label htmlFor="localRegulations" className="text-sm font-medium text-gray-300">
          Normativa Local (Opcional)
        </label>
        <input
          id="localRegulations"
          {...register('localRegulations')}
          className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-white focus:border-brand-primary outline-none transition-colors"
          placeholder="Ej: PGOU Madrid"
        />
      </div>

      {apiError && (
        <div className="flex items-center gap-2 p-3 bg-red-500/10 border border-red-500/20 rounded-lg text-red-400 text-sm">
          <AlertCircle size={16} className="shrink-0" />
          {apiError}
        </div>
      )}

      <Button type="submit" className="w-full h-12 mt-6" isLoading={loading}>
        {project?.id ? 'Actualizar Proyecto' : 'Crear Proyecto'}
      </Button>
    </form>
  );
}
