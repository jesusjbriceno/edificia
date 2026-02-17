import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '@/components/ui/Button';

const projectSchema = z.object({
  title: z.string().min(1, 'El título del proyecto es obligatorio'),
  description: z.string().min(10, 'La descripción debe tener al menos 10 caracteres'),
  status: z.enum(['Active', 'Completed', 'OnHold']),
  budget: z.string().optional(),
});

type ProjectFormData = z.infer<typeof projectSchema>;

interface ProjectFormProps {
  initialData?: Partial<ProjectFormData>;
  onSubmit: (data: ProjectFormData) => void;
  isLoading?: boolean;
}

export function ProjectForm({ initialData, onSubmit, isLoading }: ProjectFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ProjectFormData>({
    resolver: zodResolver(projectSchema),
    defaultValues: {
      title: initialData?.title || '',
      description: initialData?.description || '',
      status: initialData?.status || 'Active',
      budget: initialData?.budget || '',
    },
  });

  return (
    <form 
      onSubmit={handleSubmit(onSubmit)} 
      className="space-y-4 bg-dark-card/50 p-6 rounded-xl border border-white/5 mx-auto max-w-lg"
    >
      <div className="space-y-2">
        <label htmlFor="title" className="text-sm font-medium text-gray-300">Nombre del Proyecto / Obra</label>
        <input
          id="title"
          {...register('title')}
          className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-white focus:border-brand-primary outline-none transition-colors"
          placeholder="Ej: Reforma Planta 3 - Sede Central"
        />
        {errors.title && <p className="text-xs text-red-500">{errors.title.message}</p>}
      </div>

      <div className="space-y-2">
        <label htmlFor="description" className="text-sm font-medium text-gray-300">Descripción Técnica</label>
        <textarea
          id="description"
          {...register('description')}
          className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-white focus:border-brand-primary outline-none transition-colors min-h-[100px]"
          placeholder="Describa brevemente los objetivos y alcance del proyecto..."
        />
        {errors.description && <p className="text-xs text-red-500">{errors.description.message}</p>}
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-2">
          <label htmlFor="status" className="text-sm font-medium text-gray-300">Estado Inicial</label>
          <select
            id="status"
            {...register('status')}
            className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-white focus:border-brand-primary outline-none transition-colors appearance-none"
          >
            <option value="Active">Activo / En proceso</option>
            <option value="OnHold">En espera</option>
            <option value="Completed">Finalizado</option>
          </select>
          {errors.status && <p className="text-xs text-red-500">{errors.status.message}</p>}
        </div>

        <div className="space-y-2">
          <label htmlFor="budget" className="text-sm font-medium text-gray-300">Presupuesto Est. (€)</label>
          <input
            id="budget"
            {...register('budget')}
            className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-white focus:border-brand-primary outline-none transition-colors"
            placeholder="Ej: 45.000"
          />
        </div>
      </div>

      <Button type="submit" className="w-full h-12 mt-6" isLoading={isLoading}>
        Crear Proyecto
      </Button>
    </form>
  );
}
