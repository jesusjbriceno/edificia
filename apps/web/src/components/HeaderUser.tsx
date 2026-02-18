import { useState, useRef, useEffect } from 'react';
import { useAuthStore } from '@/store/useAuthStore';
import { RoleLabels, type Role } from '@/lib/types';
import { User, LogOut, ChevronDown } from 'lucide-react';

export default function HeaderUser() {
  const user = useAuthStore((s) => s.user);
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  // Close dropdown on outside click
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false);
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  const handleLogout = () => {
    useAuthStore.getState().logout();
    globalThis.location.href = '/';
  };

  if (!user) {
    return (
      <div className="flex items-center gap-3 pl-4 border-l border-white/10 animate-pulse">
        <div className="text-right space-y-1">
          <div className="h-4 w-24 bg-white/10 rounded" />
          <div className="h-3 w-16 bg-white/5 rounded" />
        </div>
        <div className="w-10 h-10 rounded-full bg-white/10" />
      </div>
    );
  }

  const roleLabel = RoleLabels[user.roles?.[0] as Role] ?? user.roles?.[0] ?? '';
  const initials = user.fullName
    .split(' ')
    .map((w) => w[0])
    .join('')
    .slice(0, 2)
    .toUpperCase();

  return (
    <div className="relative" ref={ref}>
      {/* Trigger */}
      <button
        type="button"
        onClick={() => setOpen((v) => !v)}
        className="flex items-center gap-2 md:gap-3 pl-2 md:pl-4 border-l border-white/10 cursor-pointer hover:opacity-90 transition-opacity"
      >
        <div className="text-right hidden sm:block">
          <p className="text-sm font-medium text-white">{user.fullName}</p>
          <p className="text-xs text-gray-500">{roleLabel}</p>
        </div>
        <div className="w-9 h-9 md:w-10 md:h-10 rounded-full bg-linear-to-br from-brand-primary to-blue-600 flex items-center justify-center font-bold text-white text-xs md:text-sm">
          {initials}
        </div>
        <ChevronDown
          size={14}
          className={`text-gray-400 transition-transform duration-200 ${open ? 'rotate-180' : ''}`}
        />
      </button>

      {/* Dropdown */}
      {open && (
        <div className="absolute right-0 top-full mt-2 w-48 md:w-52 bg-dark-card border border-white/10 rounded-xl shadow-xl py-1 z-50 animate-in fade-in slide-in-from-top-2 duration-150">
          <a
            href="/profile"
            className="flex items-center gap-3 px-4 py-3 text-sm text-gray-300 hover:bg-white/5 hover:text-white transition-colors"
          >
            <User size={16} />
            Mi Perfil
          </a>
          <div className="border-t border-white/5 mx-2" />
          <button
            type="button"
            onClick={handleLogout}
            className="flex items-center gap-3 px-4 py-3 text-sm text-red-400 hover:bg-red-500/10 transition-colors w-full text-left"
          >
            <LogOut size={16} />
            Cerrar Sesión
          </button>
        </div>
      )}
    </div>
  );
}
