import React, { useEffect, useState } from 'react';
import UserTable from '@/components/UserTable';
import { userService } from '@/lib/services/userService';
import type { UserResponse } from '@/lib/types';
import { Loader2, AlertCircle, Users } from 'lucide-react';

/**
 * Componente que carga los usuarios desde la API y los renderiza
 * en la tabla UserTable. Reemplaza los datos hardcodeados que
 * existían previamente en users.astro.
 */
export default function UserManagement() {
  const [users, setUsers] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadUsers();
  }, []);

  async function loadUsers() {
    try {
      setIsLoading(true);
      setError(null);
      const response = await userService.list({ page: 1, pageSize: 50 });

      // Mapear UserResponse del backend al formato que espera UserRow
      const mapped = response.items.map((u: UserResponse) => ({
        id: u.id,
        name: u.fullName,
        email: u.email,
        role: u.role,
        status: u.isActive ? 'Active' : 'Inactive',
        lastAccess: u.updatedAt
          ? formatRelativeDate(u.updatedAt)
          : formatRelativeDate(u.createdAt),
      }));

      setUsers(mapped);
    } catch (err: any) {
      console.error('Error cargando usuarios:', err);
      setError(err.message || 'No se pudieron cargar los usuarios');
    } finally {
      setIsLoading(false);
    }
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-20">
        <Loader2 className="w-8 h-8 text-brand-primary animate-spin" />
        <span className="ml-3 text-gray-400">Cargando usuarios…</span>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex flex-col items-center justify-center py-20 space-y-4">
        <AlertCircle className="w-12 h-12 text-red-400" />
        <p className="text-red-400 text-sm">{error}</p>
        <button
          onClick={loadUsers}
          className="px-4 py-2 text-sm bg-white/10 hover:bg-white/20 rounded-lg text-white transition-colors"
        >
          Reintentar
        </button>
      </div>
    );
  }

  if (users.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-20 space-y-4">
        <Users className="w-12 h-12 text-gray-600" />
        <p className="text-gray-400 text-sm">No hay usuarios registrados.</p>
      </div>
    );
  }

  return <UserTable users={users} />;
}

/** Convierte una fecha ISO a formato relativo legible. */
function formatRelativeDate(isoDate: string): string {
  try {
    const date = new Date(isoDate);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMin = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMin < 1) return 'Ahora mismo';
    if (diffMin < 60) return `Hace ${diffMin} min`;
    if (diffHours < 24) return `Hace ${diffHours}h`;
    if (diffDays === 1) return 'Ayer';
    if (diffDays < 7) return `Hace ${diffDays} días`;
    if (diffDays < 30) return `Hace ${Math.floor(diffDays / 7)} semanas`;
    return date.toLocaleDateString('es-ES', { day: 'numeric', month: 'short', year: 'numeric' });
  } catch {
    return '—';
  }
}
