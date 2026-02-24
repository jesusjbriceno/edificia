import { Loader2, Check, Save, WifiOff, AlertTriangle } from 'lucide-react';
import type { SyncStatus } from '@/lib/syncManager';

export interface SyncBadgeProps {
  status: SyncStatus;
  pendingCount: number;
}

const BASE =
  'flex items-center text-[10px] font-bold uppercase tracking-[0.15em] px-4 py-2 rounded-lg border transition-all duration-300';

/**
 * Indicador visual del estado de sincronización del editor.
 * Refleja si el contenido está sincronizado, en proceso, con cambios
 * pendientes, en modo offline o en error.
 */
export function SyncBadge({ status, pendingCount }: Readonly<SyncBadgeProps>) {
  switch (status) {
    case 'syncing':
      return (
        <span
          role="status"
          aria-label="Sincronizando contenido con el servidor"
          className={`${BASE} text-blue-400 bg-blue-400/5 border-blue-400/10`}
        >
          <Loader2 size={12} className="animate-spin mr-2" aria-hidden="true" />
          Sincronizando...
        </span>
      );
    case 'synced':
      return (
        <span
          role="status"
          aria-label="Contenido sincronizado"
          className={`${BASE} text-emerald-400 bg-emerald-400/5 border-emerald-400/10 shadow-[0_0_15px_-5px_rgba(52,211,153,0.2)]`}
        >
          <Check size={12} className="mr-2" aria-hidden="true" />
          Sincronizado
        </span>
      );
    case 'modified':
      return (
        <span
          role="status"
          aria-label={`${pendingCount} cambio${pendingCount === 1 ? '' : 's'} pendiente${pendingCount === 1 ? '' : 's'} de sincronizar`}
          className={`${BASE} text-amber-400 bg-amber-400/5 border-amber-400/10`}
        >
          <Save size={12} className="mr-2" aria-hidden="true" />
          {pendingCount} pendiente{pendingCount === 1 ? '' : 's'}
        </span>
      );
    case 'offline':
      return (
        <span
          role="status"
          aria-label="Modo local: sin conexión al servidor"
          className={`${BASE} text-orange-400 bg-orange-400/5 border-orange-400/10`}
        >
          <WifiOff size={12} className="mr-2" aria-hidden="true" />
          Modo Local
        </span>
      );
    case 'error':
      return (
        <span
          role="alert"
          aria-label="Error al sincronizar el contenido"
          className={`${BASE} text-red-400 bg-red-400/5 border-red-400/10`}
        >
          <AlertTriangle size={12} className="mr-2" aria-hidden="true" />
          Error
        </span>
      );
    default:
      return (
        <span
          role="status"
          aria-label="Borrador guardado localmente"
          className={`${BASE} text-gray-300 bg-white/5 border-white/10`}
        >
          <Save size={12} className="mr-2" aria-hidden="true" />
          Borrador Local
        </span>
      );
  }
}
