import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/components/ui/Button';
import { User, Mail, Shield, Save, CheckCircle2, AlertCircle, Hash } from 'lucide-react';
import { useAuthStore } from '@/store/useAuthStore';
import { authService } from '@/lib/services/authService';
import { RoleLabels, type Role } from '@/lib/types';
import { ApiError } from '@/lib/api';
import { ChangePassword } from './ChangePassword';

// ─── Zod schema ──────────────────────────────────────────

const profileSchema = z.object({
  fullName: z.string().min(1, 'El nombre es obligatorio').max(200),
  collegiateNumber: z.string().max(50).nullable().optional(),
});

type ProfileFormData = z.infer<typeof profileSchema>;

// ─── Component ───────────────────────────────────────────

export function ProfileView() {
  const { user, updateUser, mustChangePassword } = useAuthStore();
  const [serverError, setServerError] = useState<string | null>(null);
  const [saveSuccess, setSaveSuccess] = useState(false);
  const [activeTab, setActiveTab] = useState<'profile' | 'security'>(() => {
    if (typeof window !== 'undefined') {
      const params = new URLSearchParams(window.location.search);
      return params.get('tab') === 'security' ? 'security' : 'profile';
    }
    return 'profile';
  });

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting, isDirty },
    reset,
  } = useForm<ProfileFormData>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      fullName: user?.fullName ?? '',
      collegiateNumber: (user as any)?.collegiateNumber ?? null,
    },
  });

  // Reset form when user data changes (e.g. after initial load)
  useEffect(() => {
    if (user) {
      reset({
        fullName: user.fullName,
        collegiateNumber: (user as any)?.collegiateNumber ?? null,
      });
    }
  }, [user, reset]);

  const onSubmit = async (data: ProfileFormData) => {
    try {
      setServerError(null);
      setSaveSuccess(false);
      const updated = await authService.updateProfile({
        fullName: data.fullName,
        collegiateNumber: data.collegiateNumber || null,
      });
      updateUser({ fullName: updated.fullName });
      setSaveSuccess(true);
      reset(data); // so isDirty resets
      setTimeout(() => setSaveSuccess(false), 3000);
    } catch (err) {
      if (err instanceof ApiError) {
        setServerError(err.message || 'No se pudo actualizar el perfil');
      } else {
        setServerError('Error de conexión. Inténtalo de nuevo.');
      }
    }
  };

  if (!user) return null;

  const userRole = user.roles?.[0] as Role | undefined;

  return (
    <div className="max-w-4xl mx-auto space-y-8">
      {/* Force password change banner */}
      {mustChangePassword && (
        <div className="flex items-center gap-3 bg-amber-500/10 border border-amber-500/20 rounded-xl px-5 py-4 text-amber-400">
          <AlertCircle className="w-5 h-5 shrink-0" />
          <p className="text-sm font-medium">
            Tu contraseña temporal debe ser cambiada antes de continuar usando la plataforma.
          </p>
        </div>
      )}

      {/* Tab nav */}
      <div className="flex gap-1 bg-white/5 rounded-lg p-1 max-w-xs">
        <button
          type="button"
          onClick={() => setActiveTab('profile')}
          className={`flex-1 px-4 py-2 rounded-md text-sm font-medium transition-colors ${
            activeTab === 'profile'
              ? 'bg-brand-primary text-white'
              : 'text-gray-400 hover:text-white'
          }`}
        >
          Perfil
        </button>
        <button
          type="button"
          onClick={() => setActiveTab('security')}
          className={`flex-1 px-4 py-2 rounded-md text-sm font-medium transition-colors ${
            activeTab === 'security'
              ? 'bg-brand-primary text-white'
              : 'text-gray-400 hover:text-white'
          }`}
        >
          Seguridad
        </button>
      </div>

      {activeTab === 'profile' && (
        <div className="bg-dark-card/50 border border-white/5 rounded-2xl p-8 backdrop-blur-xl">
          <div className="flex items-center gap-6 mb-8">
            <div className="w-20 h-20 rounded-full bg-brand-primary/20 flex items-center justify-center text-brand-primary text-3xl font-bold border border-brand-primary/30">
              {user.fullName.charAt(0)}
            </div>
            <div>
              <h2 className="text-2xl font-bold text-white">{user.fullName}</h2>
              <p className="text-gray-400">
                Gestiona tu información personal y preferencias.
              </p>
            </div>
          </div>

          {serverError && (
            <div className="flex items-center gap-2 bg-red-500/10 border border-red-500/20 rounded-lg px-4 py-3 text-red-400 text-sm mb-6">
              <AlertCircle className="w-4 h-4 shrink-0" />
              <span>{serverError}</span>
            </div>
          )}

          {saveSuccess && (
            <div className="flex items-center gap-2 bg-emerald-500/10 border border-emerald-500/20 rounded-lg px-4 py-3 text-emerald-400 text-sm mb-6">
              <CheckCircle2 className="w-4 h-4 shrink-0" />
              <span>Perfil actualizado correctamente.</span>
            </div>
          )}

          <form onSubmit={handleSubmit(onSubmit)}>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-2">
                <label className="text-xs font-bold uppercase tracking-widest text-gray-500">
                  Nombre Completo
                </label>
                <div className="relative">
                  <User className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 w-4 h-4" />
                  <input
                    className="w-full bg-white/5 border border-white/10 rounded-lg pl-10 pr-4 py-3 text-white focus:border-brand-primary outline-none transition-colors"
                    {...register('fullName')}
                  />
                </div>
                {errors.fullName && (
                  <p className="text-xs text-red-500">{errors.fullName.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <label className="text-xs font-bold uppercase tracking-widest text-gray-500">
                  Email Profesional
                </label>
                <div className="relative">
                  <Mail className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 w-4 h-4" />
                  <input
                    defaultValue={user.email}
                    className="w-full bg-white/5 border border-white/10 rounded-lg pl-10 pr-4 py-3 text-white opacity-70 cursor-not-allowed"
                    readOnly
                  />
                </div>
              </div>

              <div className="space-y-2">
                <label className="text-xs font-bold uppercase tracking-widest text-gray-500">
                  Nº Colegiado
                </label>
                <div className="relative">
                  <Hash className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 w-4 h-4" />
                  <input
                    className="w-full bg-white/5 border border-white/10 rounded-lg pl-10 pr-4 py-3 text-white focus:border-brand-primary outline-none transition-colors"
                    placeholder="Ej: 12345-M"
                    {...register('collegiateNumber')}
                  />
                </div>
              </div>

              <div className="space-y-2">
                <label className="text-xs font-bold uppercase tracking-widest text-gray-500">
                  Rol asignado
                </label>
                <div className="flex items-center gap-3 bg-white/5 border border-white/10 rounded-lg px-4 py-3">
                  <Shield className="text-brand-primary w-4 h-4" />
                  <span className="text-white font-medium">
                    {userRole ? RoleLabels[userRole] ?? userRole : '—'}
                  </span>
                </div>
              </div>
            </div>

            <div className="mt-8 flex justify-end">
              <Button
                type="submit"
                className="h-12 px-8"
                isLoading={isSubmitting}
                disabled={!isDirty}
              >
                <Save className="w-4 h-4 mr-2" />
                Guardar Cambios
              </Button>
            </div>
          </form>
        </div>
      )}

      {activeTab === 'security' && <ChangePassword />}
    </div>
  );
}
