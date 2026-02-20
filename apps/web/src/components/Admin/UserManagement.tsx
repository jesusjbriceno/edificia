import React, { useEffect, useState } from 'react';
import UserTable from '@/components/UserTable';
import { userService } from '@/lib/services/userService';
import type { UserResponse } from '@/lib/types';
import { Loader2, AlertCircle, Users, UserPlus } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import Modal from '@/components/ui/Modal';
import { UserForm } from '@/components/Admin/UserForm';
import type { User } from '@/components/Admin/UserRow';

/**
 * Componente completo de gestión de usuarios.
 * Incluye: carga desde API, añadir, editar, activar/desactivar y eliminar.
 */
export default function UserManagement() {
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Modal de crear/editar
  const [modalOpen, setModalOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  // Modal de confirmación de eliminación
  const [deleteTarget, setDeleteTarget] = useState<User | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    loadUsers();
  }, []);

  async function loadUsers() {
    try {
      setIsLoading(true);
      setError(null);
      const response = await userService.list({ page: 1, pageSize: 50 });

      const mapped: User[] = response.items.map((u: UserResponse) => ({
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

  // ── Añadir / Editar ──────────────────────────────────────

  function handleOpenCreate() {
    setEditingUser(null);
    setModalOpen(true);
  }

  function handleOpenEdit(user: User) {
    setEditingUser(user);
    setModalOpen(true);
  }

  async function handleSaveUser(formData: { name: string; email: string; role: string }) {
    setIsSaving(true);
    try {
      if (editingUser) {
        await userService.update(editingUser.id, {
          fullName: formData.name,
          role: formData.role,
        });
      } else {
        await userService.create({
          fullName: formData.name,
          email: formData.email,
          role: formData.role,
        });
      }
      setModalOpen(false);
      await loadUsers();
    } catch (err: any) {
      console.error('Error guardando usuario:', err);
    } finally {
      setIsSaving(false);
    }
  }

  // ── Activar / Desactivar ─────────────────────────────────

  async function handleToggleStatus(user: User) {
    try {
      if (user.status === 'Active') {
        await userService.deactivate(user.id);
      } else {
        await userService.activate(user.id);
      }
      await loadUsers();
    } catch (err: any) {
      console.error('Error cambiando estado del usuario:', err);
    }
  }

  // ── Eliminar ─────────────────────────────────────────────

  async function handleConfirmDelete() {
    if (!deleteTarget) return;
    setIsDeleting(true);
    try {
      await userService.remove(deleteTarget.id);
      setDeleteTarget(null);
      await loadUsers();
    } catch (err: any) {
      console.error('Error eliminando usuario:', err);
    } finally {
      setIsDeleting(false);
    }
  }

  // ── Render ───────────────────────────────────────────────

  return (
    <>
      {/* Header con botón Añadir */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8">
        <div>
          <h1 className="text-2xl md:text-3xl font-bold text-white tracking-tight">Gestión de Usuarios</h1>
          <p className="text-gray-400 mt-1 text-sm md:text-base">
            Administra los accesos y roles de los arquitectos y colaboradores.
          </p>
        </div>
        <Button
          className="h-12 bg-white text-dark-bg hover:bg-gray-100 border-none w-full sm:w-auto"
          onClick={handleOpenCreate}
        >
          <UserPlus className="w-5 h-5 mr-2" />
          Añadir Usuario
        </Button>
      </div>

      {/* Contenido principal */}
      {isLoading && (
        <div className="flex items-center justify-center py-20">
          <Loader2 className="w-8 h-8 text-brand-primary animate-spin" />
          <span className="ml-3 text-gray-400">Cargando usuarios…</span>
        </div>
      )}

      {!isLoading && error && (
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
      )}

      {!isLoading && !error && users.length === 0 && (
        <div className="flex flex-col items-center justify-center py-20 space-y-4">
          <Users className="w-12 h-12 text-gray-600" />
          <p className="text-gray-400 text-sm">No hay usuarios registrados.</p>
          <button
            onClick={handleOpenCreate}
            className="px-4 py-2 text-sm bg-brand-primary/20 hover:bg-brand-primary/30 rounded-lg text-brand-primary transition-colors"
          >
            Crear el primer usuario
          </button>
        </div>
      )}

      {!isLoading && !error && users.length > 0 && (
        <UserTable
          users={users}
          onToggleStatus={handleToggleStatus}
          onEdit={handleOpenEdit}
          onDelete={setDeleteTarget}
        />
      )}

      {/* Modal Crear / Editar Usuario */}
      <Modal
        isOpen={modalOpen}
        onClose={() => setModalOpen(false)}
        title={editingUser ? 'Editar Usuario' : 'Nuevo Usuario'}
      >
        <UserForm
          initialData={editingUser ? { name: editingUser.name, email: editingUser.email, role: editingUser.role as any } : undefined}
          onSubmit={handleSaveUser}
          isLoading={isSaving}
        />
      </Modal>

      {/* Modal de confirmación de eliminación */}
      <Modal
        isOpen={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        title="¿Eliminar usuario?"
      >
        <div className="space-y-4">
          <p className="text-gray-300 text-sm">
            Estás a punto de eliminar a <span className="font-semibold text-white">{deleteTarget?.name}</span>.
            Esta acción no se puede deshacer.
          </p>
          <div className="flex gap-3 justify-end pt-2">
            <Button
              onClick={() => setDeleteTarget(null)}
              className="bg-white/10 hover:bg-white/20 border-none text-white"
            >
              Cancelar
            </Button>
            <Button
              onClick={handleConfirmDelete}
              isLoading={isDeleting}
              className="bg-red-500 hover:bg-red-600 border-none text-white"
            >
              Sí, eliminar
            </Button>
          </div>
        </div>
      </Modal>
    </>
  );
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
