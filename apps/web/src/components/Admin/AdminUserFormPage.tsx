import { useState, useEffect } from 'react';
import { ArrowLeft, Loader2, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { UserForm } from '@/components/Admin/UserForm';
import { userService } from '@/lib/services/userService';
import type { UserResponse } from '@/lib/types';
import { ApiError } from '@/lib/api';

interface AdminUserFormPageProps {
  /** When provided, the page loads this user and operates in edit mode. */
  userId?: string;
}

export default function AdminUserFormPage({ userId }: AdminUserFormPageProps) {
  const isEditMode = Boolean(userId);

  const [user, setUser] = useState<UserResponse | null>(null);
  const [isLoading, setIsLoading] = useState(isEditMode);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saveError, setSaveError] = useState<string | null>(null);

  useEffect(() => {
    if (!userId) return;

    const load = async () => {
      setIsLoading(true);
      setError(null);
      try {
        const data = await userService.getById(userId);
        setUser(data);
      } catch {
        setError('No se pudo cargar el usuario para editar.');
      } finally {
        setIsLoading(false);
      }
    };

    load();
  }, [userId]);

  const handleSubmit = async (formData: {
    name: string;
    email: string;
    role: string;
  }) => {
    setIsSaving(true);
    setSaveError(null);
    try {
      if (isEditMode && userId) {
        await userService.update(userId, {
          fullName: formData.name,
          role: formData.role,
        });
        window.location.href = `/admin/users`;
      } else {
        await userService.create({
          fullName: formData.name,
          email: formData.email,
          role: formData.role,
        });
        window.location.href = '/admin/users';
      }
    } catch (err) {
      if (err instanceof ApiError) {
        setSaveError(err.message);
      } else {
        setSaveError('Error inesperado al guardar el usuario.');
      }
    } finally {
      setIsSaving(false);
    }
  };

  // ── Loading ────────────────────────────────────────────────
  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="flex flex-col items-center gap-4 text-gray-400">
          <Loader2 size={36} className="animate-spin text-brand-primary" />
          <p className="text-sm">Cargando datos del usuario…</p>
        </div>
      </div>
    );
  }

  // ── Error fetching ─────────────────────────────────────────
  if (error) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh] gap-6 text-center">
        <AlertCircle size={48} className="text-red-400" />
        <div>
          <p className="text-white font-semibold mb-1">Error al cargar</p>
          <p className="text-gray-400 text-sm">{error}</p>
        </div>
        <Button
          variant="outline"
          onClick={() => (window.location.href = '/admin/users')}
        >
          <ArrowLeft size={16} className="mr-2" />
          Volver al listado
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
      {/* Breadcrumb */}
      <nav className="flex items-center gap-2 text-sm text-gray-500">
        <a
          href="/admin/users"
          className="hover:text-white transition-colors flex items-center gap-1.5"
        >
          <ArrowLeft size={14} />
          Usuarios
        </a>
        <span>/</span>
        <span className="text-gray-300">
          {isEditMode ? 'Editar usuario' : 'Nuevo usuario'}
        </span>
      </nav>

      {/* Page header */}
      <div>
        <h1 className="text-3xl font-bold text-white tracking-tight">
          {isEditMode ? 'Editar Usuario' : 'Nuevo Usuario'}
        </h1>
        <p className="text-gray-400 mt-1">
          {isEditMode
            ? `Modifica los datos de ${user?.fullName ?? 'este usuario'}.`
            : 'Rellena los campos para dar de alta un nuevo usuario.'}
        </p>
      </div>

      {/* Save error */}
      {saveError && (
        <div className="flex items-start gap-3 p-4 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400 text-sm">
          <AlertCircle size={18} className="shrink-0 mt-0.5" />
          <p>{saveError}</p>
        </div>
      )}

      {/* Form card */}
      <div className="bg-dark-card border border-white/5 rounded-3xl p-8 relative overflow-hidden">
        {/* Decorative */}
        <div className="absolute top-0 right-0 p-12 opacity-5 pointer-events-none text-brand-primary">
          <svg width="120" height="120" viewBox="0 0 24 24" fill="currentColor">
            <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2M12 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8z" />
          </svg>
        </div>

        <UserForm
          initialData={
            user
              ? {
                  name: user.fullName,
                  email: user.email,
                  role: user.role as any,
                }
              : undefined
          }
          onSubmit={handleSubmit}
          isLoading={isSaving}
        />
      </div>

      {/* Cancel link */}
      <div className="flex justify-start">
        <a
          href="/admin/users"
          className="text-sm text-gray-500 hover:text-white transition-colors flex items-center gap-1.5"
        >
          <ArrowLeft size={14} />
          Cancelar y volver
        </a>
      </div>
    </div>
  );
}
