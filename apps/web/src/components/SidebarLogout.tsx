import { LogOut } from 'lucide-react';
import { useAuthStore } from '@/store/useAuthStore';

export default function SidebarLogout() {
  const handleLogout = () => {
    useAuthStore.getState().logout();
    globalThis.location.href = '/';
  };

  return (
    <button
      type="button"
      onClick={handleLogout}
      className="flex items-center gap-3 px-4 py-3 text-sm font-medium text-red-400 hover:bg-red-500/10 rounded-lg transition-all w-full text-left"
    >
      <LogOut size={18} />
      Cerrar Sesión
    </button>
  );
}
