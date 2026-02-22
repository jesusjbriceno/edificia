import React, { useState, useRef, useEffect } from 'react';
import { Badge } from '@/components/ui/Badge';
import { MoreHorizontal, UserCheck, UserX, KeyRound, Pencil, Trash2 } from 'lucide-react';
import { Dropdown } from '@/components/ui/Dropdown';

export interface User {
  id: string;
  name: string;
  email: string;
  role: string;
  status: 'Active' | 'Inactive';
  lastAccess: string;
}

interface UserRowProps {
  user: User;
  onToggleStatus: (user: User) => void;
  onEdit: (user: User) => void;
  onDelete: (user: User) => void;
  onResetPassword: (user: User) => void;
}

export function UserRow({ user, onToggleStatus, onEdit, onDelete, onResetPassword }: UserRowProps) {
  return (
    <tr className="hover:bg-white/5 transition-colors group">
      <td className="px-6 py-4">
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 rounded-full bg-brand-primary/20 flex items-center justify-center text-brand-primary font-bold text-xs">
            {user.name.charAt(0).toUpperCase()}
          </div>
          <div>
            <p className="font-medium text-white group-hover:text-brand-primary transition-colors">{user.name}</p>
            <p className="text-xs text-gray-500">{user.email}</p>
          </div>
        </div>
      </td>
      <td className="px-6 py-4">
        <span className="text-gray-300">{user.role}</span>
      </td>
      <td className="px-6 py-4">
        <Badge variant={user.status === 'Active' ? 'success' : 'error'}>
          {user.status === 'Active' ? 'Activo' : 'Desactivado'}
        </Badge>
      </td>
      <td className="px-6 py-4 text-gray-400 italic">
        {user.lastAccess}
      </td>
      <td className="px-6 py-4 text-right">
        <div className="flex justify-end gap-1">
          {/* Botón de restablecer contraseña */}
          <button
            title="Restablecer contraseña"
            onClick={() => onResetPassword(user)}
            className="p-1.5 text-gray-500 hover:text-white transition-colors rounded-lg hover:bg-white/5"
          >
            <KeyRound size={16} />
          </button>

          {/* Botón activar/desactivar */}
          <button
            title={user.status === 'Active' ? 'Desactivar usuario' : 'Activar usuario'}
            onClick={() => onToggleStatus(user)}
            className="p-1.5 text-gray-500 hover:text-brand-primary transition-colors rounded-lg hover:bg-white/5"
          >
            {user.status === 'Active' ? <UserX size={16} /> : <UserCheck size={16} />}
          </button>

          {/* Menú de 3 puntos */}
          <Dropdown
            trigger={
              <button
                title="Más opciones"
                className="p-1.5 text-gray-500 hover:text-white transition-colors rounded-lg hover:bg-white/5"
              >
                <MoreHorizontal size={16} />
              </button>
            }
          >
            <div className="flex flex-col">
              <button
                className="w-full flex items-center gap-2.5 px-4 py-2.5 text-sm text-gray-300 hover:bg-white/5 hover:text-white transition-colors"
                onClick={() => onEdit(user)}
              >
                <Pencil size={14} />
                Editar usuario
              </button>
              <button
                className="w-full flex items-center gap-2.5 px-4 py-2.5 text-sm text-red-400 hover:bg-red-500/10 hover:text-red-300 transition-colors"
                onClick={() => onDelete(user)}
              >
                <Trash2 size={14} />
                Eliminar usuario
              </button>
            </div>
          </Dropdown>

        </div>
      </td>
    </tr>
  );
}
