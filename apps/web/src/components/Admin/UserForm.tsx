import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '@/components/ui/Button';

const userSchema = z.object({
  name: z.string().min(1, 'El nombre es obligatorio'),
  email: z.string().email('Email inválido').min(1, 'El email es obligatorio'),
  role: z.enum(['Admin', 'Architect', 'Collaborator', 'Supervisor']),
});

type UserFormData = z.infer<typeof userSchema>;

interface UserFormProps {
  initialData?: Partial<UserFormData>;
  onSubmit: (data: UserFormData) => void;
  isLoading?: boolean;
}

export function UserForm({ initialData, onSubmit, isLoading }: UserFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<UserFormData>({
    resolver: zodResolver(userSchema),
    defaultValues: {
      name: initialData?.name || '',
      email: initialData?.email || '',
      role: initialData?.role || 'Architect',
    },
  });

  return (
    <form 
      onSubmit={handleSubmit(onSubmit)} 
      className="space-y-4 bg-dark-card/50 p-6 rounded-xl border border-white/5 mx-auto max-w-lg"
      data-testid="user-form"
    >
      <div className="space-y-2">
        <label htmlFor="name" className="text-sm font-medium text-gray-300">Nombre Completo</label>
        <input
          id="name"
          {...register('name')}
          className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-white focus:border-brand-primary outline-none transition-colors"
          placeholder="Ej: Ana Martínez"
        />
        {errors.name && <p className="text-xs text-red-500">{errors.name.message}</p>}
      </div>

      <div className="space-y-2">
        <label htmlFor="email" className="text-sm font-medium text-gray-300">Correo Electrónico</label>
        <input
          id="email"
          type="email"
          {...register('email')}
          className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-white focus:border-brand-primary outline-none transition-colors"
          placeholder="ana@edificia.es"
        />
        {errors.email && <p className="text-xs text-red-500">{errors.email.message}</p>}
      </div>

      <div className="space-y-2">
        <label htmlFor="role" className="text-sm font-medium text-gray-300">Rol del Usuario</label>
        <select
          id="role"
          {...register('role')}
          className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-white focus:border-brand-primary outline-none transition-colors appearance-none"
        >
          <option value="Architect">Arquitecto</option>
          <option value="Admin">Administrador</option>
          <option value="Collaborator">Colaborador</option>
          <option value="Supervisor">Supervisor</option>
        </select>
        {errors.role && <p className="text-xs text-red-500">{errors.role.message}</p>}
      </div>

      <Button type="submit" className="w-full h-12 mt-6" isLoading={isLoading}>
        Guardar Usuario
      </Button>
    </form>
  );
}
