import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/components/ui/Button';
import { Lock, Eye, EyeOff, CheckCircle2, AlertCircle } from 'lucide-react';
import { authService } from '@/lib/services/authService';
import { useAuthStore } from '@/store/useAuthStore';
import { ApiError } from '@/lib/api';

// ─── Zod schema ──────────────────────────────────────────

const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, 'La contraseña actual es obligatoria'),
    newPassword: z
      .string()
      .min(8, 'Mínimo 8 caracteres')
      .regex(/[A-Z]/, 'Debe contener al menos una mayúscula')
      .regex(/[a-z]/, 'Debe contener al menos una minúscula')
      .regex(/[0-9]/, 'Debe contener al menos un número')
      .regex(/[^A-Za-z0-9]/, 'Debe contener al menos un carácter especial'),
    confirmPassword: z.string().min(1, 'Confirma la nueva contraseña'),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: 'Las contraseñas no coinciden',
    path: ['confirmPassword'],
  });

type ChangePasswordFormData = z.infer<typeof changePasswordSchema>;

// ─── Component ───────────────────────────────────────────

export function ChangePassword() {
  const [showPass, setShowPass] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useForm<ChangePasswordFormData>({
    resolver: zodResolver(changePasswordSchema),
  });

  const onSubmit = async (data: ChangePasswordFormData) => {
    try {
      setServerError(null);
      await authService.changePassword({
        currentPassword: data.currentPassword,
        newPassword: data.newPassword,
      });

      // Clear mustChangePassword flag if it was set
      const store = useAuthStore.getState();
      if (store.mustChangePassword) {
        useAuthStore.setState({ mustChangePassword: false });
      }

      setIsSuccess(true);
      reset();
    } catch (err) {
      if (err instanceof ApiError) {
        setServerError(err.message || 'No se pudo cambiar la contraseña');
      } else {
        setServerError('Error de conexión. Inténtalo de nuevo.');
      }
    }
  };

  if (isSuccess) {
    return (
      <div className="bg-emerald-500/10 border border-emerald-500/20 rounded-xl p-6 text-emerald-500 flex items-center gap-4">
        <CheckCircle2 size={24} />
        <div>
          <p className="font-bold">Contraseña actualizada</p>
          <p className="text-sm opacity-80">Tu nueva contraseña ha sido guardada correctamente.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-dark-card/50 border border-white/5 rounded-2xl p-8 backdrop-blur-xl">
      <h3 className="text-xl font-bold text-white mb-6">Cambiar Contraseña</h3>

      {serverError && (
        <div className="flex items-center gap-2 bg-red-500/10 border border-red-500/20 rounded-lg px-4 py-3 text-red-400 text-sm mb-6">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{serverError}</span>
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6 max-w-md">
        <div className="space-y-2">
          <label className="text-xs font-bold uppercase tracking-widest text-gray-500">
            Contraseña Actual
          </label>
          <div className="relative">
            <Lock className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 w-4 h-4" />
            <input
              type={showPass ? 'text' : 'password'}
              className="w-full bg-white/5 border border-white/10 rounded-lg pl-10 pr-10 py-3 text-white focus:border-brand-primary outline-none transition-colors"
              {...register('currentPassword')}
            />
          </div>
          {errors.currentPassword && (
            <p className="text-xs text-red-500">{errors.currentPassword.message}</p>
          )}
        </div>

        <div className="space-y-2">
          <label className="text-xs font-bold uppercase tracking-widest text-gray-500">
            Nueva Contraseña
          </label>
          <div className="relative">
            <Lock className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 w-4 h-4" />
            <input
              type={showPass ? 'text' : 'password'}
              className="w-full bg-white/5 border border-white/10 rounded-lg pl-10 pr-10 py-3 text-white focus:border-brand-primary outline-none transition-colors"
              {...register('newPassword')}
            />
            <button
              type="button"
              onClick={() => setShowPass(!showPass)}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-white transition-colors"
            >
              {showPass ? <EyeOff size={16} /> : <Eye size={16} />}
            </button>
          </div>
          {errors.newPassword && (
            <p className="text-xs text-red-500">{errors.newPassword.message}</p>
          )}
        </div>

        <div className="space-y-2">
          <label className="text-xs font-bold uppercase tracking-widest text-gray-500">
            Confirmar Nueva Contraseña
          </label>
          <div className="relative">
            <Lock className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 w-4 h-4" />
            <input
              type={showPass ? 'text' : 'password'}
              className="w-full bg-white/5 border border-white/10 rounded-lg pl-10 pr-10 py-3 text-white focus:border-brand-primary outline-none transition-colors"
              {...register('confirmPassword')}
            />
          </div>
          {errors.confirmPassword && (
            <p className="text-xs text-red-500">{errors.confirmPassword.message}</p>
          )}
        </div>

        <Button type="submit" className="h-12 px-8" isLoading={isSubmitting}>
          Actualizar Seguridad
        </Button>
      </form>
    </div>
  );
}
