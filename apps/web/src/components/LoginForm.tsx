import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { LogIn, AlertCircle } from 'lucide-react';
import { authService } from '@/lib/services/authService';
import { useAuthStore } from '@/store/useAuthStore';
import { ApiError } from '@/lib/api';

// ─── Zod schema ──────────────────────────────────────────

const loginSchema = z.object({
  email: z.string().min(1, 'El email es obligatorio').email('Introduce un email válido'),
  password: z.string().min(1, 'La contraseña es obligatoria'),
});

type LoginFormData = z.infer<typeof loginSchema>;

// ─── Component ───────────────────────────────────────────

export default function LoginForm() {
  const setAuth = useAuthStore((s) => s.setAuth);
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormData) => {
    try {
      setServerError(null);
      const response = await authService.login({ email: data.email, password: data.password });
      setAuth(response);

      // Redirect based on mustChangePassword
      if (response.mustChangePassword) {
        window.location.href = '/profile?tab=security';
      } else {
        window.location.href = '/dashboard';
      }
    } catch (err) {
      if (err instanceof ApiError) {
        setServerError(err.message || 'Credenciales incorrectas');
      } else {
        setServerError('Error de conexión. Inténtalo de nuevo.');
      }
    }
  };

  return (
    <div className="w-full max-w-md space-y-8 p-8 md:p-12">
      <div className="space-y-2">
        <h1 className="text-4xl font-bold tracking-tight text-white flex items-center gap-2">
          <span className="text-brand-primary">E</span>DIFICIA
        </h1>
        <p className="text-gray-400 text-sm">
          Plataforma avanzada de redacción de memorias.
        </p>
      </div>

      {serverError && (
        <div className="flex items-center gap-2 bg-red-500/10 border border-red-500/20 rounded-lg px-4 py-3 text-red-400 text-sm">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{serverError}</span>
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <div className="space-y-4">
          <Input
            label="Email"
            type="email"
            placeholder="arquitecto@edificia.es"
            error={errors.email?.message}
            {...register('email')}
          />
          <Input
            label="Contraseña"
            type="password"
            placeholder="••••••••"
            error={errors.password?.message}
            {...register('password')}
          />
        </div>

        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-2">
            <input
              type="checkbox"
              id="remember"
              className="rounded border-white/10 bg-white/5 text-brand-primary focus:ring-brand-primary"
            />
            <label htmlFor="remember" className="text-xs text-gray-400">
              Recordar acceso
            </label>
          </div>
          <a href="#" className="text-xs text-brand-primary hover:underline">
            ¿Has olvidado la contraseña?
          </a>
        </div>

        <Button type="submit" className="w-full h-12" isLoading={isSubmitting}>
          <LogIn className="w-4 h-4 mr-2" />
          Iniciar Sesión
        </Button>
      </form>

      <div className="pt-6 border-t border-white/5">
        <p className="text-xs text-center text-gray-500">
          ¿No tienes cuenta?{' '}
          <a href="#" className="text-brand-primary hover:underline">
            Contacta con tu administrador
          </a>
        </p>
      </div>
    </div>
  );
}
